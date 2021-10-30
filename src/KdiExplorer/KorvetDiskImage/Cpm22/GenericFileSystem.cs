
using KorvetDiskImage.Interfaces;
using KorvetDiskImage.Exceptions;


namespace KorvetDiskImage.Cpm22
{
    internal class GenericFileSystem : IFileSystem
    {
        // records per cluster
        private IBlockDevice BIOS;
        private int RecordsPerCluster { get { return 1 << DiskParametersBlock.BSH;  } }


        public IDiskParameters DiskParametersBlock { get; private set; }
        public List<DirectoryEntry> DIRECTORY { get; private set; } = new List<DirectoryEntry>();


        public GenericFileSystem(IBlockDevice bios)
        {
            BIOS = bios;
            DiskParametersBlock = BIOS.SELDSK();

            var dir_alloc = DirectoryClusters();
            var dir_dump = ReadCluster(dir_alloc);
            DIRECTORY = ReadDirectoryEntries(dir_dump);
        }


        // read single cluster from a block device
        public ByteStorage ReadCluster(int cluster)
        {
            var accum = new ByteStorage();

            var n_trk = ClusterToTrack(cluster);
            var n_sect = ClusterToRecord(cluster);

            for (int i = 0; i < RecordsPerCluster; i++)
            {
                    BIOS.SETTRK(n_trk);
                    BIOS.SETSEC(n_sect);
                    BIOS.READ();
                    accum.AddRange(BIOS.BUFFER);

                n_sect++;
                if (n_sect >= DiskParametersBlock.SPT)
                {
                    n_sect = 0;
                    n_trk++;
                }
            }

            return accum;
        }

        // read a serie of clusters from a block device
        public ByteStorage ReadCluster(List<int> clusters_list)
        {
            var accum = new ByteStorage();

            foreach (var cluster in clusters_list)
            {
                accum.AddRange(ReadCluster(cluster));
            }

            return accum;
        }


        public List<int> FindAllocatedClusters(int user, string name, string type)
        {
            var accum = new List<int>();

            var entry_number = 0;
            var entry_found = true;
            var extent_cnt = 0;

            while (entry_found)
            {
                entry_found = false;

                foreach (var de in DIRECTORY)
                {
                    if (de.EntryNumber != entry_number) continue;
                    if (user!=-1 && de.UserNumber != user) continue;
                    if (de.Filename!= name) continue;
                    if (de.Filetype!= type) continue;

                    extent_cnt++;

                    accum.AddRange(de.Allocation);
                    entry_found = true;
                    break; // stop searching current entry_number
                }

                entry_number++;
            }

            if (extent_cnt==0)
            {
                throw new EntryNotFoundException($"can't find entry {user:X2}/{name}.{type}");
            }

            return accum;
        }





        // returns list of clusters, occupied by directory
        private List<int> DirectoryClusters()
        {
            var DIR_ALLOC_LIST = new List<int>();
            var ALV = DiskParametersBlock.AL0 << 8 | DiskParametersBlock.AL1;

            for (int i = 0; i < 16; i++)
            {
                var shift_pos = 15 - i;
                var test_mask = 1 << shift_pos;
                if ((ALV & test_mask) > 0)
                {
                    DIR_ALLOC_LIST.Add(i);
                }
            }
            return DIR_ALLOC_LIST;
        }


        // returns list of DirectoryEntries from the given byte array
        private List<DirectoryEntry> ReadDirectoryEntries(ByteStorage dir_dump)
        {
            // validate
            var expected_dir_size = (DiskParametersBlock.DRM + 1) * 32;
            if (dir_dump.Count != expected_dir_size)
            {
                throw new VirtualFileSystemException($"Invalid directory dump: expected {expected_dir_size} bytes, got {dir_dump.Count} bytes");
            }

            var accum = new List<DirectoryEntry>();
            var offset = 0;
            while (offset < dir_dump.Count)
            {
                accum.Add(new DirectoryEntry(dir_dump.GetRange(offset, 32), DiskParametersBlock));
                offset += 32;
            }

            return accum;
        }


        // returns the track where the cluster begins
        private int ClusterToTrack(int cluster)
        {
            var track = cluster * RecordsPerCluster / DiskParametersBlock.SPT + DiskParametersBlock.OFF;
            return track;
        }

        // returns the sector where the cluster begins
        private int ClusterToRecord(int cluster)
        {
            var record = cluster * RecordsPerCluster % DiskParametersBlock.SPT;
            return record;
        }






    }
}
