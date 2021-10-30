using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;

using exkdi;





RootCommand rootCommand = new RootCommand("KDI Explorer, a tool for accessing data stored on CP/M disk images"){
                // This is a positional argument; no command line flag is required.
            new Argument<FileInfo>("imagefile", "path to a CP/M disk image file")
};

var dirCommand = new Command("DIR", "display list of files from a CP/M directory")
            {
                //new Option<DirOutputOptions>(
                //    "-f",
                //    description: "output format: list of names, wide detailed view, or a very detailed CSV",
                //    getDefaultValue: ()=>DirOutputOptions.list
                //    ),
                new Option<bool>("-d", "include deleted files"),
                new Option<bool>("-l", "display a list of names"),
                new Option<bool>("--text-score", "calculate text-likeness score (experimental)"),
                new Option<bool>("--hash", "calculate MD5 hash (slow)"),
                new Option<bool>("--csv", "as CSV for further processing"),
            };

var typeCommand = new Command("TYPE", "display contents of a CP/M file") {
                new Argument<string>("file", "example: 00/README.TXT"),
                new Option<TypeOutputOptions>("-f", description: "a hex dump, readable text, or raw unprocessed bytes",  getDefaultValue: () => TypeOutputOptions.text )
            };




var exportCommand = new Command("EXPORT", "export to host FILE, TEXT, or SYSTEM tracks");

var exportFileCommand = new Command("FILE", "save a CP/M file to the host computer") {
                new Argument<string>("file", "example: 00/README.TXT"),
                new Option<FileInfo>("--to", "host path to save data"),
                new Option<bool>("--eof", "cut the source data stream on the EOF (ASCII 26) character (primarily for the text files)"),
};

var exportTextCommand = new Command("TEXT", "save a text file to the host computer in UTF-8") {
                new Argument<string>("file", "example: 00/README.TXT"),
                new Option<FileInfo>("--to", "host path to save data")
};

var exportSystemCommand = new Command("SYSTEM", "save the system tracks (if present)") {
                new Option<FileInfo>("--to", "host path to save data")
};





var printCommand = new Command("PRINT", "display STAT, DPB, MAP, CLUSTER, or SECTOR");

var printSectorCommand = new Command("SECTOR", "print contents of a 128-byte CP/M sector")
            {
                new Argument<int>("track", "track number, 0-based"),
                new Argument<int>("sector", "sector number, 0-based"),
                new Option<ClusterOutputOptions>("-f", description: "a hex dump, an MD5 hash, or raw unprocessed bytes", getDefaultValue: () => ClusterOutputOptions.hex)
            };

var printClusterCommand = new Command("CLUSTER", "print contents of a cluster") {
                new Argument<int>("cluster", "cluster number, 0-based"),
                new Option<ClusterOutputOptions>("-f", description: "a hex dump, an MD5 hash, or raw unprocessed bytes", getDefaultValue: () => ClusterOutputOptions.hex)

};

var printDpbCommand = new Command("DPB", "print Disk Parameter Block values")
{
                new Option<bool>("--csv", "as CSV for further processing")
};

var printStatCommand = new Command("STAT", "provides general statistical information about file storage and device assignment")
{
                new Option<bool>("--csv", "as CSV for further processing")
};

var printMapCommand = new Command("MAP", "print disk map") 
{
                new Option<bool>("--csv", "as CSV for further processing")
};




var dsu = new DoSomethingUseful();


// register root subcommand handlers
//rootCommand.Handler = CommandHandler.Create(() => exportCommand.InvokeAsync(" -h"));
rootCommand.Handler = CommandHandler.Create(dsu.Root);
dirCommand.Handler = CommandHandler.Create<FileInfo, DirOutputOptions, bool, bool, bool>(dsu.Dir);
typeCommand.Handler = CommandHandler.Create<FileInfo, string, TypeOutputOptions>(dsu.Type);

// register EXPORT subcommand handlers
//exportCommand.Handler = CommandHandler.Create(() => exportCommand.InvokeAsync(" EXPORT -h"));
exportCommand.Handler = CommandHandler.Create(dsu.Root);
exportFileCommand.Handler = CommandHandler.Create<FileInfo, string, FileInfo, bool>(dsu.ExportFile);
exportTextCommand.Handler = CommandHandler.Create<FileInfo, string, FileInfo>(dsu.ExportText);
exportSystemCommand.Handler = CommandHandler.Create<FileInfo, FileInfo>(dsu.ExportSystem);

// register PRINT subcommand handlers
//printCommand.Handler = CommandHandler.Create(() => exportCommand.InvokeAsync(" PRINT -h"));
printCommand.Handler = CommandHandler.Create(dsu.Root);
printSectorCommand.Handler = CommandHandler.Create<FileInfo, int, int, ClusterOutputOptions>(dsu.PrintSector);
printClusterCommand.Handler = CommandHandler.Create<FileInfo, int, ClusterOutputOptions>(dsu.PrintCluster);
printStatCommand.Handler = CommandHandler.Create<FileInfo, bool>(dsu.PrintStat);
printDpbCommand.Handler = CommandHandler.Create<FileInfo, bool>(dsu.PrintDpb);
printMapCommand.Handler = CommandHandler.Create<FileInfo, bool>(dsu.PrintMap);

// set up root subcommands
rootCommand.AddCommand(dirCommand);
rootCommand.AddCommand(typeCommand);

// EXPORT and its subcommands
rootCommand.AddCommand(exportCommand);
exportCommand.AddCommand(exportSystemCommand);
exportCommand.AddCommand(exportFileCommand);
exportCommand.AddCommand(exportTextCommand);

// PRINT and its subcommands
rootCommand.AddCommand(printCommand);
printCommand.AddCommand(printStatCommand);
printCommand.AddCommand(printDpbCommand);
printCommand.AddCommand(printMapCommand);
printCommand.AddCommand(printClusterCommand);
printCommand.AddCommand(printSectorCommand);



// set up common functionality like --help, --version, and dotnet-suggest support
var commandLine = new CommandLineBuilder(rootCommand)
    .UseDefaults() // automatically configures dotnet-suggest
    .Build();

// invokes our handler callback and actually runs our application
await commandLine.InvokeAsync(args);





