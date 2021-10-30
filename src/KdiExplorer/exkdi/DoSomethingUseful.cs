using System.Text;

using KorvetDiskImage;

namespace exkdi
{



    public class DoSomethingUseful
    {
        public DoSomethingUseful()
        {
            Console.OutputEncoding = Encoding.UTF8;

        }

        public void Root(FileInfo imagefile)
        {
            ////Console.WriteLine($"DSU ROOT {imagefile}");

            Console.WriteLine(@"
KDI Explorer, a tool for accessing data stored in CP/M disk images

Usage:
  exkdi <imagefile> [command [subcommand]] [<command-related arguments>]

Mandatory arguments:
  <imagefile>  path to a CP/M disk image file

All commands:
  DIR          display list of files from a CP/M directory
  TYPE <file>  display contents of a CP/M file
             
  EXPORT SYSTEM       save the system tracks (if present)
  EXPORT FILE <file>  save a CP/M file to the host computer
  EXPORT TEXT <file>  save a text file to the host computer in UTF-8

  PRINT STAT                     provides general statistical information about file storage and device assignment
  PRINT DPB                      print Disk Parameter Block values
  PRINT MAP                      print disk map
  PRINT CLUSTER <cluster>        print contents of a cluster
  PRINT SECTOR <track> <sector>  print contents of a 128-byte CP/M sector

For detailed information about the commands and subcommands, use:
  exkdi [command [subcommand]] -help");
           
        }


        public void Dir(FileInfo imagefile, DirOutputOptions f, bool d, bool m, bool t)
        {
            //Console.WriteLine($"DSU DIR {imagefile} {f} {d} {m} {t}");

            if (imagefile == null)
            {
                
            }

            var files = new List<string>();

            try
            {
                var kdi = new KDI(imagefile, "koi8-r");
                files = kdi.FindFile((int)f, d, m, t);

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }


            foreach (var s in files)
            {
                Console.WriteLine(s);
            }
        }


        public void Type(FileInfo imagefile, string file, TypeOutputOptions f)
        {
            //Console.WriteLine($"DSU TYPE {imagefile} {file} {f}");


            KDI kdi = new KDI("koi8-r");
            var bytes = new List<byte>();

            try
            {
                kdi = new KDI(imagefile, "koi8-r");
                bytes = kdi.ReadFile(file);

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }


            switch (f)
            {
                default:
                case TypeOutputOptions.text:
                    var text = kdi.TextView(bytes);
                    Console.WriteLine(text);
                    break;

                case TypeOutputOptions.hex:
                    var hvs = kdi.HexView(bytes);
                    foreach (var hv in hvs)
                    {
                        Console.WriteLine(hv);
                    }
                    break;


                case TypeOutputOptions.raw:
                    var raw_ascii = kdi.RawView(bytes);
                    Console.Write(raw_ascii);
                    break;
            }


        }


        public void ExportFile(FileInfo imagefile, string file, FileInfo to, bool eof)
        {
            //Console.WriteLine($"DSU INFO {imagefile} {file} {to} {eof}");

            var bytes = new List<byte>();

            try
            {
                var kdi = new KDI(imagefile, "koi8-r");
                kdi.ExportFile(file, to, eof);

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);            
            }

            Console.WriteLine($"Success. The file was exported to {to.FullName}");
            Environment.Exit(0);
        }

        public void ExportText(FileInfo imagefile, string file, FileInfo to)
        {
            Console.Error.WriteLine($"Not yet implemented, my pad'n.");
            Environment.Exit(1);

            Console.WriteLine($"Success. The text was exported to {to.FullName}");
            Environment.Exit(0);

        }

        public void ExportSystem(FileInfo imagefile, FileInfo to)
        {
            Console.Error.WriteLine($"Not yet implemented, my pad'n.");
            Environment.Exit(1);

            Console.WriteLine($"Success. The system tracks were exported to {to.FullName}");
            Environment.Exit(0);

        }
        public void PrintSector(FileInfo imagefile, int track, int sector, ClusterOutputOptions f)
        {

            var bytes = new List<byte>();
            var kdi = new KDI("koi8-r");

            try
            {
                //Console.WriteLine($"DSU SECTOR {imagefile} {s} {t} {f}");
                kdi = new KDI(imagefile, "koi8-r");
                bytes = kdi.ReadSector(track, sector);

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }


            switch (f)
            {
                default:
                case ClusterOutputOptions.hex:
                    var hvs = kdi.HexView(bytes);
                    foreach (var hv in hvs)
                    {
                        Console.WriteLine(hv);
                    }

                    break;

                case ClusterOutputOptions.hash:
                    Console.WriteLine(kdi.MD5(bytes.ToArray()));
                    break;


                case ClusterOutputOptions.raw:
                    var raw_ascii = kdi.RawView(bytes);
                    Console.Write(raw_ascii);
                    break;
            }


        }




        public void PrintCluster(FileInfo imagefile, int cluster, ClusterOutputOptions f)
        {
            //Console.WriteLine($"DSU CLUSTER {imagefile} {cluster} {f}");
            var kdi = new KDI("koi8-r");
            var bytes = new List<byte>();

            try
            {
                kdi = new KDI(imagefile, "koi8-r");
                bytes = kdi.ReadCluster(cluster);

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }


            switch (f)
            {
                default:
                case ClusterOutputOptions.hex:
                    var hvs = kdi.HexView(bytes);
                    foreach (var hv in hvs)
                    {
                        Console.WriteLine(hv);
                    }

                    break;

                case ClusterOutputOptions.hash:
                    Console.WriteLine(kdi.MD5(bytes.ToArray()));
                    break;


                case ClusterOutputOptions.raw:
                    var raw_ascii = kdi.RawView(bytes);
                    Console.Write(raw_ascii);
                    break;
            }
        }

        public void PrintMap(FileInfo imagename, bool csv)
        {
            Console.Error.WriteLine($"Not yet implemented, my pad'n.");
            Environment.Exit(1);

        }


        public void PrintDpb(FileInfo imagename, bool csv)
        {
            Console.Error.WriteLine($"Not yet implemented, my pad'n.");
            Environment.Exit(1);

            //DEFW spt; Number of 128 - byte records per track
            //DEFB    bsh; Block shift. 3 => 1k, 4 => 2k, 5 => 4k....DEFB    blm; Block mask. 7 => 1k, 0Fh => 2k, 1Fh => 4k...
            //DEFB exm; Extent mask, see later
            //DEFW    dsm; (no.of blocks on the disc)-1

            //DEFW drm; (no.of directory entries)-1

            //DEFB al0; Directory allocation bitmap, first byte
            //DEFB    al1; Directory allocation bitmap, second byte
            //DEFW    cks; Checksum vector size, 0 for a fixed disc
            //              ; No.directory entries/ 4, rounded up.
            //DEFW off; Offset, number of reserved tracks
        }


        public void PrintStat(FileInfo imagename, bool csv)
        {
            Console.Error.WriteLine($"Not yet implemented, my pad'n.");
            Environment.Exit(1);

            //   d: Drive Characteristics
            //65536: 128 Byte Record Capacity
            // 8192: Kilobyte Drive Capacity
            //  128: 32 Byte Directory Eritries
            //    0: Checked Directory Eritries
            // 1024: Records / Extent
            //  128: Records / BlocK
            //   58: Sectors / TracK
            //    2: Reserved TracKs
        }



    }

}