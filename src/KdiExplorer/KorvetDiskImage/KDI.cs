using System.Text;

using KorvetDiskImage.Cpm22;
using KorvetDiskImage.Interfaces;
using KorvetDiskImage.Exceptions;

namespace KorvetDiskImage
{
    public class KDI
    {
        /*
         * Структура корветовского CP/M диска:
         * 
         * 
         * PHY : TRACK/SECTOR
         * LOG : TRACK/RECORD
         * CPM : CLUSTER
         * USR : USERID/FILENAME
         * 
         */

        public enum ReadOption
        {
            Auto,
            AsBinary,
            AsText
        }


        private FileInfo DiskImageInfo;

        private IFileSystem VFS;
        private IBlockDevice VDRIVE;
        private ByteStorage VMEDIA;

        // TODO add switch of codepage in the CLI
        // 20127              us-ascii                     US-ASCII    
        // 20866              koi8-r                       Cyrillic (KOI8-R)            
        // 21866              koi8-u Cyrillic(KOI8-U)
        public Encoding TextEncoding { get; private set; }



        public KDI(string encoding_name = "ASCII")
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            TextEncoding = Encoding.GetEncoding(encoding_name);

            DiskImageInfo = new FileInfo(Path.GetTempFileName());

            VMEDIA = new ByteStorage(800*1024, 0xE5);
            var dpb = new GenericDPB();
            VDRIVE = new KorvetBIOS(dpb, VMEDIA);
            VFS = new GenericFileSystem(VDRIVE);
        }

        // TODO a new kind of constructor 'create a new kdi'
        public KDI(FileInfo imagefile, string encoding_name = "ASCII")
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            TextEncoding = Encoding.GetEncoding(encoding_name);

            DiskImageInfo = imagefile;

            TextEncoding = Encoding.GetEncoding(20866);

            VMEDIA = new ByteStorage(DiskImageInfo.FullName);
            var dpb = new KorvetDPB(VMEDIA.GetRange(0, 128));
            VDRIVE = new KorvetBIOS(dpb, VMEDIA);
            VFS = new GenericFileSystem(VDRIVE);
        }


        public override string ToString()
        {
            return $"KDI: Loaded file '{DiskImageInfo.FullName}'";
        }

        public List<byte> ReadSector(int track, int sector)
        {
            VDRIVE.SETTRK(track);
            VDRIVE.SETSEC(sector);
            VDRIVE.READ();
            var sector_data = VDRIVE.BUFFER;
            return sector_data.AsList;
        }
        public List<byte> ReadCluster(int cluster)
        {
            var cluster_data = VFS.ReadCluster(cluster);
            return cluster_data.AsList;
        }




        // returns file content
        public List<byte> ReadFile(string name, int cluster=-1)
        {
            var found = ResolveName(name);
            if (found.Count == 0) throw new EntryNotFoundException($"can't find file '{name}'");

            // read the file contents
            var alloc = VFS.FindAllocatedClusters(found[0].User, found[0].Name, found[0].Type);
            if (cluster > 0 && cluster < alloc.Count)
            {
                alloc = alloc.GetRange(cluster, 1);
            }

            return VFS.ReadCluster(alloc).AsList;
        }


        public void ExportFile(string name, FileInfo target_file, bool respect_eof)
        {

            if (target_file.Exists) throw new Exception($"file '{target_file.FullName}' already exists");

            var bytes = ReadFile(name);

            if (respect_eof && bytes.Contains(26))
            {
                // cut off everything past the byte 26 (EOF tag, or Ctrl-Z character)
                bytes = bytes.GetRange(0, bytes.IndexOf(26));                
            }


            File.WriteAllBytes(target_file.FullName, bytes.ToArray());

        }






        private struct CpmFilename
        {
            public int User;
            public string Name;
            public string Type;
            public CpmFilename(int user, string name, string type)
            {
                User = user;
                Name = name;
                Type = type;
            }
        }

        private List<CpmFilename> ResolveName(string name)
        {
            // parse the name
            // full: UU/NNNNNNNN.TTT
            // short: NNNNNNNN.TTT

            var accum = new List<CpmFilename>();



            var dot_pos = name.LastIndexOf('.');
            var slash_pos = name.IndexOfAny(@"/\".ToCharArray());
            var type = string.Empty;
            var user = -1;

            if (dot_pos != -1)
            {
                type = name.Substring(dot_pos + 1);
                name = name.Substring(0, dot_pos);
            }

            if (slash_pos != -1)
            {
                try
                {
                    user = Convert.ToInt32(name.Substring(0, slash_pos), 16);
                }
                catch
                {
                    user = -1;
                }
                name = name.Substring(slash_pos + 1);
            }

            foreach (var de in VFS.DIRECTORY)
            {
                if (de.EntryNumber != 0) continue;
                if (user != -1 && user != de.UserNumber) continue;

                // TODO WILDCARDS

                if (de.Filename != name && de.Filename.ToUpperInvariant().Trim() != name.ToUpperInvariant().Trim()) continue;
                if (de.Filetype != type && de.Filetype.ToUpperInvariant().Trim() != type.ToUpperInvariant().Trim()) continue;

                accum.Add(new CpmFilename(de.UserNumber, de.Filename, de.Filetype));
            }

            return accum;

        }

            // find all files
            public List<string> FindFile(int details_level = 0, bool showDeleted = false, bool showHash = false, bool showTextScore=false)
        {
            var accum = new List<string>();

            // header
            if (details_level==1)
            {
                accum.Add($"{"File name",-20} {"ATR"}{"Length",10}       Allocation");
                accum.Add($"{"---------",-20} {"---"}{"------",10}       ----------");
            }
            else            if (details_level == 2)
            {
                accum.Add("ParentContainer,FullName,Length,Hidden,Writable,User,BaseName,Ext,Hash,TextScore,Clusters");
            }
            else if (details_level == 3)
            {
                accum.Add("ParentContainer,FullName,User,BaseName,Ext,Cluster,Hash");

            }


            foreach (var de in VFS.DIRECTORY)
            {
                if (de.EntryNumber != 0 | (!showDeleted & de.UserNumber > 15))
                {
                    continue;
                }

                var baseName = de.Filename.Trim();
                var extension = $".{de.Filetype.Trim()}";
                var name = $"{baseName}{extension}";
                var fullName = $"{de.UserNumber:X2}/{baseName}{extension}";

                if (details_level==0)
                {
                    accum.Add(fullName);
                    continue;
                }

                var alloc = VFS.FindAllocatedClusters(de.UserNumber, de.Filename, de.Filetype);
                var length = (128 << VFS.DiskParametersBlock.BSH) * alloc.Count;
                var s_alloc = string.Join(',', alloc);
                var s_attr = string.Format("{0}{1}", (de.IsHidden ? "h" : "-"), (de.IsReadOnly ? "r-" : "rw"));

                if (details_level == 1)
                {
                    // list with details
                    accum.Add($"{fullName,-20} {s_attr}{length,10} bytes [{s_alloc}]");
                    continue;
                }



                //var parent_container = Path.GetFileNameWithoutExtension(DiskImageInfo.Name);
                var parent_container = DiskImageInfo.Name;
                var exportName = SanitizeFilename($"{parent_container}_{de.UserNumber:X2}_{baseName}{extension}").ToLower();
                var s_hid = de.IsHidden ? "hidden" : "visible";
                var s_ro = de.IsReadOnly ? "read-only" : "writable";


                double text_score = 0;
                var hash = string.Empty;
                if (showHash | showTextScore)
                {
                    var content = VFS.ReadCluster(alloc);

                    if (showHash) hash = MD5(content.AsArray);
                    if (showTextScore) text_score = TextScore(content);
                }

                // CSV file hashes
                var info = $"\"{parent_container}\",\"{fullName}\",{length},\"{s_hid}\",\"{s_ro}\",{de.UserNumber},\"{baseName}\",\"{extension}\",\"{hash}\",\"{text_score}\", \"{s_alloc}\"";
                accum.Add(info);

                    //// CSV cluster hashes
                    //var cnt = 0;
                    //foreach (var c in alloc)
                    //{
                    //    var cluster_content = VFS.ReadCluster(c);
                    //    var cluster_hash = MD5(cluster_content.AsArray);
                    //    var info = $"\"{parent_container}\",\"{fullName}\",{de.UserNumber},\"{baseName}\",\"{extension}\",{cnt},\"{cluster_hash}\"";
                    //    accum.Add(info);
                    //    cnt++;
                    //}


            }
            return accum;
        }


        // find all files
        public void BulkExportClusters(string host_directory)
        {

            foreach (var de in VFS.DIRECTORY)
            {
                if (de.EntryNumber != 0 | de.UserNumber > 15)
                {
                    continue;
                }

                var alloc = VFS.FindAllocatedClusters(de.UserNumber, de.Filename, de.Filetype);
                var parent_container = Path.GetFileNameWithoutExtension(DiskImageInfo.Name);
                var s_alloc = string.Join(',', alloc);
                var baseName = de.Filename.Trim();
                var extension = de.Filetype.Trim();

                var cnt = 0;
                foreach (var c in alloc)
                {
                    var exportName = SanitizeFilename($"{parent_container}_{de.UserNumber:X2}_{baseName}_{extension}_{cnt:X2}.cluster").ToLower();
                    var content = VFS.ReadCluster(c);

                    var host_path = Path.Join(host_directory, exportName);

                    File.WriteAllBytes(host_path, content.AsArray);
                    cnt++;
                }



            }

        }

        // Text View Generator
        // - removes the 'tail'
        // - converts from koi8
        public string TextView(List<byte> dump)
        {

            if (dump.Contains(26))
            {
                // cut off everything past the byte 26 (EOF tag, or Ctrl-Z character)
                dump = dump.GetRange(0, dump.IndexOf(26));

            }

            return TextEncoding.GetString(dump.ToArray());
        }



        public string RawView(List<byte> dump)
        {
            return Encoding.ASCII.GetString(dump.ToArray());
        }



        // Hex View Generator:
        // 0000  00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00  ................
        // 0000  00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00  ................
        // 0000  00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00  ................
        // 0000  00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00  ................
        public List<string> HexView(List<byte> dump, int row_length=16)
        {
            if (row_length > dump.Count) row_length = dump.Count;

            if (row_length <= 0) row_length = 32;

            var accum = new List<string>();

            // determine the max length of an offset
            var offset_length = $"{dump.Count:X}".Length;
            var offsetFormat = $"X{offset_length}";

            for(int offset = 0; offset<dump.Count; offset+=row_length)
            {
                var row = new StringBuilder();
                var row_hex = new StringBuilder();
                var row_ascii = new StringBuilder();

                for (int i = 0; i<row_length; i++)
                {
                    var addr = offset + i;
                    var asc = dump[addr] < 32 ? "." : TextEncoding.GetString(dump.GetRange(addr,1).ToArray());
                    
                    row_hex.Append($"{dump[addr]:X2} ");
                    row_ascii.Append(asc);
                }

                row.Append($"{offset.ToString(offsetFormat)}  {row_hex}  {row_ascii}");
                accum.Add(row.ToString());
            }

            return accum;

        }



        





        private string SanitizeFilename(string filename)
        {
            var invalids = System.IO.Path.GetInvalidFileNameChars();
            var newName = String.Join("_", filename.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
            return newName;
        }



        public string MD5(byte[] bytes)
        {
            // calculate hash
            byte[] hash;
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                md5.TransformFinalBlock(bytes, 0, bytes.Length);
                hash = md5.Hash;
            }


            // convert it to a string
            StringBuilder result = new StringBuilder(hash.Length * 2);

            for (int i = 0; i < hash.Length; i++)
            {
                result.Append(hash[i].ToString("X2"));
            }

            return result.ToString();
        }


        // calculate how likely it's a text document
        private double TextScore(ByteStorage content)
        {
            // ASCII: [a-zA-Z0-9\n\r\s]; KOI8-R: [а-яА-Я]
            var CommonKoi8r = new List<byte>() { 9, 32, 65, 97, 66, 98, 67, 99, 68, 100, 69, 101, 70, 102, 71, 103, 72, 104, 73, 105, 74, 106, 75, 107, 76, 108, 77, 109, 78, 110, 79, 111, 80, 112, 81, 113, 82, 114, 83, 115, 84, 116, 85, 117, 86, 118, 87, 119, 88, 120, 89, 121, 90, 122, 194, 215, 199, 196, 214, 218, 203, 204, 205, 206, 208, 210, 211, 212, 198, 200, 195, 222, 219, 221, 193, 197, 201, 207, 213, 217, 220, 192, 209, 202, 216, 223, 226, 247, 231, 228, 246, 250, 235, 236, 237, 238, 240, 242, 243, 244, 230, 232, 227, 254, 251, 253, 225, 229, 233, 239, 245, 249, 252, 224, 241, 234, 248, 255, 49, 50, 51, 52, 53, 54, 55, 56, 57, 48, 13, 10 };

            var cnt = 1;
            var valid_bytes = 0;
            var invalid_bytes = 0;
            var whitespaces = 0;

            foreach (var value in content.AsList)
            {
                // stop counting on Ctrl-Z char
                if (value == 26)
                {
                    break;
                }

                if (value == 32 | value == 9)
                {
                    whitespaces++;
                }

                if (CommonKoi8r.Contains(value))
                {
                    valid_bytes++;
                }
                else
                {
                    invalid_bytes++;
                }

                cnt++;
            }

            // portion of data before 0xE5 byte
            double length_ratio = cnt / (double)(content.Count);

            // ratio of valid to all characters
            double valid_ratio = valid_bytes / (double)cnt;
            double invalid_ratio = invalid_bytes / (double)cnt;
            double valinval_ratio = valid_bytes / (double)invalid_bytes;

            // ratio whitespaces to all is about 0.2 in a regular text
            double whitespaces_ratio = whitespaces / (double)cnt;

            double text_prob_estim = Math.Round(valid_ratio * length_ratio, 2);

            return text_prob_estim;
        }


       























    } // KorvetDiskImage
} // namespace
