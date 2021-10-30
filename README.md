# KdiExplorer

A tool for accessing data stored on CP/M v.2.2 disk images

## Usage

`exkdi imagefile [command [subcommand]] [command-related arguments]`

### Mandatory arguments

`imagefile`  path to a CP/M disk image file (currently only **KDI** supported)

### Commands and their subcommands

`DIR`                   display list of files from a CP/M directory

`TYPE file`             display contents of a CP/M file

`EXPORT FILE file`      save a CP/M file to the host computer

~~EXPORT TEXT file  save a text file to the host computer in UTF-8~~

~~EXPORT SYSTEM       save the system tracks (if present)~~

~~PRINT STAT                     provides general statistical information about file storage and device assignment~~

~~PRINT DPB                      print Disk Parameter Block values~~

~~PRINT MAP                      print disk map~~

`PRINT CLUSTER cluster`          print contents of a cluster

`PRINT SECTOR track sector`      print contents of a 128-byte CP/M sector

### Where to look for more information

`exkdi [command [subcommand]] --help`
