namespace KorvetDiskImage.Interfaces
{
    public interface IFileSystem
    {

        // Get DPB
        public IDiskParameters DiskParametersBlock { get; }


        public List<DirectoryEntry> DIRECTORY { get; }

        // TODO
        //public DirectoryEntry ReadDirectoryEntry(int entry_id);

        public ByteStorage ReadCluster(List<int> clusters);
        public ByteStorage ReadCluster(int cluster);

        public List<int> FindAllocatedClusters(int user, string name, string type);


        // Get list of the free clusters
        //public List<int> FindFreeClusters { get; }

        //public IFileSystem WriteCluster(int cluster);
        //public IFileSystem WriteCluster(List<int> clusters);

        //public IFileSystem DeleteDirectoryEntry(int entry_id);
        //public IFileSystem NewDirectoryEntry(DirectoryEntry entry);

        //public IFileSystem UpdateDirectoryEntry(int entry_id, DirectoryEntry entry);

    }
}
