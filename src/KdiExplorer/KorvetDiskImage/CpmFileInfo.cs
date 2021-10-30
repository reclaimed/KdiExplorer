
using KorvetDiskImage.Interfaces;
using KorvetDiskImage.Exceptions;

namespace KorvetDiskImage
{
    public class CpmFileInfo
    {

        public int User { get; private set; }
        public string BaseName { get; private set; }
        public string Extension { get; private set; }
        public string Name { get; private set; }
        public string FullName { get; private set; }
        public string ExportName { get; private set; }
        public bool IsHidden { get; private set; }
        public bool IsReadOnly { get; private set; }
        public int Length { get; private set; }
        public string Hash { get; private set; }
        public List<int> Allocation { get; private set; }
        public string ParentContainer { get; private set; }
        //public string Directory;
        //public DateTime CreationTime;
        //public DateTime LastAccessTime;
        //public DateTime LastWriteTime;

        public CpmFileInfo(DirectoryEntry de, int length, string hash, string parent_container, List<int> alloc)
        {
            ParentContainer = parent_container;
            Hash = hash;
            Length = length;

            User = de.UserNumber;
            BaseName = de.Filename.Trim();
            Extension = $".{de.Filetype.Trim()}";
            Name = $"{BaseName}{Extension}";
            FullName = $"{User}/{BaseName}{Extension}";
            ExportName = SanitizeFilename($"{ParentContainer}_{User}_{BaseName}{Extension}").ToLower();

            IsReadOnly = de.IsReadOnly;
            IsHidden = de.IsHidden;
            //Length = de.Length;
            Allocation = alloc;
        }

        public string ToString()
        {
            var str_hid = IsHidden ? "H" : "-";
            var str_ro = IsReadOnly ? "R-" : "RW";
            var alloc_no = Allocation.Count;

            return $"{FullName,-20} {str_hid}{str_ro}{Length,10} bytes {alloc_no,4} clusters  {ParentContainer}";

        }


        public string ToCsv()
        {
            var str_hid = IsHidden ? "hidden" : "normal";
            var str_ro = IsReadOnly ? "read-only" : "writable";
            var alloc = string.Join(',', Allocation);

            return $"\"{FullName}\",{Length},\"{ParentContainer}\",\"{str_hid}\",\"{str_ro}\",{User},\"{BaseName}\",\"{Extension}\",\"{Hash}\",\"{alloc}\"\n";
        }


        private string SanitizeFilename(string nasty)
        {
            // windows invalid characters
            var invalid = new List<byte>() { 34, 60, 62, 124, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 58, 42, 63, 92, 47 };
            var safe = nasty;
            foreach (char c in invalid)
            {
                //char c = Encoding.ASCII.GetString(b);
                safe = safe.Replace(c, '_');
            }

            return safe;

        }





    }
}
