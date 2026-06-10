### 6.10 MMEMory subsystem

The MMEMory subsystem (Mass Memory) contains the commands for managing files and directories as well as for loading and storing complete instrument settings in files.

The files are stored on the internal flash memory of the instrument or on external USB memory devices.

The /var/user/ directory can be used to save user-defined data; any subdirectory structure can be created on /var/user/. Some default subdirectories are predefined, but can be changed at any time.

The default directory is determined using the command MMEMORY:CDIR.

Use the command :SYSTEM:MMEMORY:PATH:USER? to query the path of the directory for user-defined data.

The /opt directory is a protected and therefore a not accessible system directory. The files on this directory contain data that must not be changed. Therefore, this directory should not be accessed, since reconstruction of the system partition will lead to data loss.

#### 6.10.1 File naming conventions

To enable files in different file systems to be used, the following file naming conventions should be observed.

The file name can be of any length and is case-sensitive, meaning it is distinguished between uppercase and lowercase letters.

The file and the optional file extension are separated by a dot. All letters and numbers are permitted (numbers are, however, not permitted at the beginning of the file name). If possible, special characters should not be used. The use of the slashes "\" and "/" should be avoided since they are used in file paths. A number of names are reserved for the operating system, e.g. CLOCK$, CON, AUX, COM1 ... COM4, LPT1 ... LPT3, NUL and PRN.

In the R&S SMB all files in which lists and settings are stored are given a characteristic extension. The extension is separated from the actual file name by a dot (see "Extensions for User Files" on page 89 for an overview of the file types).

The two characters "*" and "?" function as "wildcards", meaning they are used for selecting several files. The "?" character represents exactly one character, while the "*" character represents all characters up to the end of the file name. "*.*" therefore stands for all files in a directory.

When used in conjunction with the commands, the parameter <file_name> is specified as a string parameter with quotation marks. It can contain either the complete path including the drive, only the path and the file name, or only the file name. The file name must include the file extension. The same applies for the parameters <directory_name> and <path>.

Depending on how much information is provided, either the values specified in the parameter or the values specified with the command MMEM:CDIR (default directory) are used for the path and the drive settings in the commands.

Before the instrument settings can be stored in a file, they have to be stored in an intermediate memory using common command *SAV <number>. The specified number is subsequently used in the :MMEMory:STORE:STATE on page 313 command. Also, subsequently to loading a file with instrument settings with command :MMEMory:LOAD:STATE on page 312, these settings have to be activated with the common command *RCL <number>.

#### 6.10.2 Extensions for user files

The following table lists all available file extensions for user files. The currently available files on the instrument depend on the installed options.

**Table 6-1: List of the automatically assigned file extensions in the instrument**

| Function | List type | Contents | File suffix |
|----------|-----------|----------|-------------|
| Instrument State | Settings | Instrument settings | *.savrcltxt |
| "User Correction" | List | User-defined level correction values | *.uco |
| "User Correction" | List | Export Data | *.txt or *.csv |
| "List Mode" | List | User-defined frequency/level value pairs | *.lsw |
| "List Mode" | List | Export Data | *.txt or *.csv |
| "Pulse Train List" | | User-defined offline/ontime/repetition values | *.pulstrn |

#### 6.10.3 Examples

In these examples, the current instrument setting is stored in the file test.savrcltxt in the directory /var/user/..

##### Storing and Loading Current Settings

1. Store the current setting in an intermediate memory with the number 4. This setting can be called using command *RCL and the associated number of the memory, for example *RCL 4.

2. To store the settings in a file in a specific directory, specify the complete path.

MMEM:STOR:STAT 4,"/var/user/test.savrcltxt"

3. To store the settings in a file in the default drive, set the default drive and specify only the file name.

MMEM:CDIR '/var/user/' *SAV 4

MMEM:STOR:STAT 4, "test.savrcltxt"

4. Load the file test.savrcltxt in the user directory.

MMEM:LOAD:STAT 4,'/var/user/test.savrcltxt'

5. Activate the instrument setting of the file test.savrcltxt.

*RCL 4

##### Working with Files and Directories

Read out all files in the specified directory.
MMEM:CAT? '/usb/user'
Response: 127145265, 175325184, "test, DIR, 0", "temp, DIR, 0",
"readme.txt, ASC, 1324", "state.savrcltxt, STAT, 5327",
"waveform.wv, BIN, 2342"
the directory /usb/user contains the subdirectories test and temp as well as
the files readme.txt, state.savrcltxt and waveform.wv which have different file types.
Tip: To query only the subdirectories of the current or specified directory, perform:
MMEM:DCAT? '/usb/user'
Response: 'test', 'temp'
To query only the number of subdirectories in the current or specified directory, perform:
MMEM:DCAT:LENG? '/usb/user'

Response: 2

2. To query the number of files in the current or specified directory, perform:

MMEM:CAT:LENG? '/usb/user'

Response: 3

3. Create a new subdirectory for mass memory storage in the specified directory. MMEM:MDIR '/usb/new'

4. Copy the file state to a new file.

MMEM:COPY '/var/user/state.savrcltxt', '/usb/new'

5. Rename the file state.

MMEM:MOVE 'state.savrcltxt', 'state_new.savrcltxt'

6. Remove the test directory.

MMEM:RDIR '/usb/test'

#### 6.10.4 Remote control commands

:MMEMory:CATalog? ..... 308
:MMEMory:CATalog:LENGTH? ..... 308
:MMEMory:CDIRectory ..... 309
:MMEMory:COPY ..... 309
:MMEMory:DATA ..... 310
:MMEMory:DCATalog? ..... 310
:MMEMory:DCATalog:LENGTH? ..... 311
:MMEMory:DELETE ..... 311
:MEMory:HFRee? ..... 311
:MMEMory:LOAD:STATE ..... 312
:MMEMory:MDIRectory ..... 312
:MMEMory:MOVE ..... 312
:MMEMory:MSIS ..... 313
:MMEMory:RDIRectory ..... 313
:MMEMory:STORE:STATE ..... 313

##### :MMEMory:CATalog? <path>

Returns the content of a particular directory.

Query parameters:
<path> string
String parameter to specify the directory.
If you leave out the path, the command returns the contents of the directory selected with :MMEMory:CDIRectory.
The path may be relative or absolute.
Return values:
<UsedDiskSpace> Byte size of all files in the directory.
<FreeDiskSpace> Remaining disk space in bytes.
<FileInfo> <NameFileN>,<SuffixFileN>,<SizeFileN>
List of files, separated by commas
<NameFileN>
Name of the file.
<SuffixFileN>
Type of the file. Possible suffixes are: ASCII, BINary, DIRectory
<SizeFileN>
Size of the file in bytes.
Example: See "Working with Files and Directories" on page 307.
Usage: Query only
Manual operation: See "Directory, File List and File Name" on page 129

#### :MMEMory:CATalog:LENGTH? <Path>

Returns the number of files in the current or in the specified directory.

##### Query parameters:

<Path> string

String parameter to specify the directory. If the directory is omitted, the command queries the content of the current directory, queried with :MMEMory:CDIRectory command.

##### Return values:

<FileCount> integer

Number of files.

Example: See "Working with Files and Directories" on page 307.

Usage: Query only

#### :MMEMory:CDIRectory <Directory>

Changes the default directory for mass memory storage. The directory is used for all subsequent MMEM commands if no path is specified with them.

##### Parameters:

<Directory>

    <directory_name>

        String containing the path to another directory. The path can be relative or absolute.

        To change to a higher directory, use two dots '..' .

    </directory_name>

</Directory>

Example: See "Working with Files and Directories" on page 307.

Usage: SCPI confirmed

Manual operation: See "Directory, File List and File Name" on page 129

#### :MMEMory:COPY <SourceFile>[,<DestinationFile>]

Copies an existing file to a new file. Instead of just a file, this command can also be used to copy a complete directory together with all its files.

##### Setting parameters:

<SourceFile> string

String containing the path and file name of the source file.

<DestinationFile> string

String containing the path and name of the target file. The path can be relative or absolute.

If <DestinationFile> is not specified, the <SourceFile> is copied to the current directory, queried with the :MMEMory:CDIRectory command.

Note: Existing files with the same name in the destination directory are overwritten without an error message.

Example: See "Working with Files and Directories" on page 307.

Usage: Setting only

SCPI confirmed

Manual operation: See "Copy" on page 133

#### :MMEMory:DATA <Filename>, <BinaryBlock>

:MMEMory:DATA? <Filename>

The setting command writes the block data <BinaryBlock> to the file identified by <Filename>.

Set the GPIB-bus terminator to EOI to ensure correct data transfer.

The query command transfers the specified file from the instrument to the GPIB-bus and then on to the controller. It is important to ensure that the intermediate memory on the controller is large enough to take the file. The setting for the GPIB-bus terminator is irrelevant.

Tip: Use this command to read/transfer stored instrument settings or waveforms directly from/to the instrument.

##### Parameters:

<BinaryBlock>  #<number><length_entry><data>

  #: Hash sign; always comes first in the binary block

  <number>: the first digit indicates how many digits the subsequent length entry has

  <length_entry>: indicates the number of subsequent bytes

  <data>: binary block data for the specified length.

  For files with a size with more than nine digits (gigabytes), the instrument allows the syntax #(<Length>), where <Length> is the file size in decimal format.

##### Parameters for setting and query:

<Filename> string

String parameter to specify the name of the file.

Example: :MMEMory:DATA '/var/user/test.txt', #15hallo

Writes the block data to the file test.txt.

The digit 1 indicates a length entry of one digit; the digit 5 indicates a length of the binary data (hallo) in bytes.

:MMEMory:DATA? '/var/user/test.txt'

Sends the data of the file test.txt from the instrument to the controller in the form of a binary block.

Response: #15hallo

Usage: SCPI confirmed

#### :MMEMory:DCATalog? <path>

Returns the subdirectories of a particular directory.

##### Query parameters:

<path>

String parameter to specify the directory. If the directory is omitted, the command queries the content of the current directory, queried with :MMEMory:CDIRectory command.

##### Return values:

<Catalog>

    <file_entry>

        Names of the subdirectories separated by colons. The first two strings are related to the parent directory.

Example: See "Working with Files and Directories" on page 307.

Usage: Query only

#### :MMEMory:DCATalog:LENGTH? [<Path>]

Returns the number of subdirectories in the current or specified directory.

##### Query parameters:

<Path> String parameter to specify the directory. If the directory is omitted, the command queries the contents of the current directory, to be queried with :MMEMory:CDIRectory command.

##### Return values:

<DirectoryCount> integer

Number of parent and subdirectories.

Example: See "Working with Files and Directories" on page 307.

Usage: Query only

##### :MMEMory:DELete <Filename>

Removes a file from the specified directory.

##### Setting parameters:

<Filename> string

String parameter to specify the name and directory of the file to be removed.

Example: See "Working with Files and Directories" on page 307.

Usage: Event SCPI confirmed

Manual operation: See "Cut" on page 133

##### :MEMORY:HFRee?

Returns the used and available memory in Kb.

##### Return values:

<TotalPhysMemKb> integer

Total physical memory.

<ApplicMemKb> integer

Application memory.

| `<HeapUsedKb>` | integer |
| | Used heap memory. |
| `<HeapAvailableKb>` | integer |
| | Available heap memory. |
| Usage: | Query only |

#### :MMEMory:LOAD:STATe <SavRclStateNumb>, <file_name>

Loads the specified file stored under the specified name in an internal memory.

After the file has been loaded, the instrument setting must be activated using an *RCL command.

##### Setting parameters:

<SavRclStateNumb> Determines the specific <number> to be used with the *RCL command, e.g. *RCL 4.

<file_name> String parameter to specify the file name with extension *.savrcltxt.

Example: See "Storing and Loading Current Settings" on page 306.

Usage: Setting only

Manual operation: See "Recall" on page 131

#### :MMEMory:MDIRectory <Directory>

Creates a subdirectory for mass memory storage in the specified directory. If no directory is specified, a subdirectory is created in the default directory. This command can also be used to create a directory tree.

##### Setting parameters:

| `<Directory>` | string |
| | String parameter to specify the new directory. |
| Example: | See "Working with Files and Directories" on page 307. |
| Usage: | Event |
| Manual operation: | See "Create New Directory" on page 133 |

##### :MMEMory:MOVE <SourceFile>, <DestinationFile>

Moves an existing file to a new location or, if no path is specified, renames an existing file.

##### Setting parameters:

<SourceFile> string String parameter to specify the name of the file to be moved.

| `<DestinationFile>` | string |
| | String parameters to specify the name of the new file. |
| Example: | See "Working with Files and Directories" on page 307. |
| Usage: | Event SCPI confirmed |
| Manual operation: | See "Rename" on page 133 |

##### :MMEMory:MSIS <Msis>

Defines the drive or network resource (in the case of networks) for instruments with windows operating system, using msis (MSIS = Mass Storage Identification String).

Note: Instruments with Linux operating system ignore this command, since Linux does not use drive letter assignment.

Usage: SCPI confirmed

#### :MMEMory:RDIRectory <Directory>

Removes an existing directory from the mass memory storage system. If no directory is specified, the subdirectory with the specified name is deleted in the default directory.

##### Setting parameters:

| `<Directory>` | string |
| | String parameter to specify the directory to be deleted. |
| Example: | See "Working with Files and Directories" on page 307. |
| Usage: | Event |

##### :MMEMory:STORE:STATE <savrcl_state_nr>, <file_name>

Stores the current instrument setting in the specified file.

The instrument setting must first be stored in an internal memory with the same number using the common command *SAV.

##### Setting parameters:

| `<savrcl_state_nr>` | Corresponds to the specific `<number>` defined with the *SAV command, e.g. *SAV 4. |
| `<file_name>` | String parameter to specify the file name with extension *.savrcltxt. |
| Example: | See "Storing and Loading Current Settings" on page 306. |
| Usage: | Event |
| Manual operation: | See "Save" on page 129 |
