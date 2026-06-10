### 6.8 HCOPy Subsystem

The HCOPy subsystem contains the commands to generate a hardcopy of the display.

#### Example: Store a hard copy of the display

The following example lists commands to configure and execute a hard copy to an automatic named file.

```
// *****
// Hard copy settings
// *****
// HCOPY:DEVICE:LANGUAGE PNG
:HCOPY:FILE:NAME:AUTO:STATE 1
// defines the output format
// sets the instrument to automatically create output file names

// *****
// Configure hard copy options, set automatic naming rules
// An automatically generated file name consists of:
// <Prefix><YYYY><MM><DD><Number>.<Format>
// *****
// HCOPY:DEVICE:LANGUAGE BMP
// defines output format *.bmp
:HCOPY:REGION DIALog
// selects the region to be copied
:HCOPY:FILE:AUTO:DIR "usb/HCopy"
// sets destination directory of automatic named file to "/usb/HCopy"
:HCOPY:FILE:NAME:AUTO:FILE:PREFix:STATE 1
:HCOPY:FILE:NAME:AUTO:FILE:PREFix:"hardcopy"
:HCOPY:FILE:NAME:AUTO:FILE:YEAR:STATE 1
:HCOPY:FILE:NAME:AUTO:FILE:MONTH:STATE 1
// uses automatic naming prefix
// sets automatic naming prefix to "hardcopy"
// uses automatic naming date parameters year and month

// *****
// Execute and transfer the hard copy
// *****
// HCOPY:EXECute
:HCOPY:DATA
// generates a hard copy
// transfers the hard copy to the remote client
:HCOPY:FILE:AUTO:FILE?
// queries the automatic file name

// response: "hardcopy1607001.bmp"

:HCOPY:FILE:AUTO:NUMBER?

// queries the number in the automatic file name

// response: "001"

:HCOPY:FILE:AUTO?

// queries the path and file name of the automatically generated file

// response: "/usb/HCopy/hardcopy1607001.bmp"
```

| Command | Page |
|:--------|-----:|
| `:HCOPy:DATA?` | 300 |
| `:HCOPy:IMAGE:FORMAT` | 300 |
| `:HCOPy:DEVICE:LANGUAGE` | 300 |
| `:HCOPy[:EXECute]` | 301 |
| `:HCOPy:FILE[:NAME]` | 301 |
| `:HCOPy:FILE[:NAME]:AUTO?` | 301 |
| `:HCOPy:FILE[:NAME]:AUTO:DIRectory` | 301 |
| `:HCOPy:FILE[:NAME]:AUTO:DIRectory:CLEAR` | 302 |
| `:HCOPy:FILE[:NAME]:AUTO:FILE?` | 302 |
| `:HCOPy:FILE[:NAME]:AUTO[:FILE]:DAY:STATE` | 302 |
| `:HCOPy:FILE[:NAME]:AUTO[:FILE]:MONTH:STATE` | 302 |
| `:HCOPy:FILE[:NAME]:AUTO[:FILE]:YEAR:STATE` | 302 |
| `:HCOPy:FILE[:NAME]:AUTO[:FILE]:NUMBER?` | 302 |
| `:HCOPy:FILE[:NAME]:AUTO[:FILE]:PREFIX` | 303 |
| `:HCOPy:FILE[:NAME]:AUTO[:FILE]:PREFIX:STATE` | 303 |
| `:HCOPy:FILE[:NAME]:AUTO:STATE` | 303 |
| `:HCOPy:REGION` | 303 |

##### :HCOPy:DATA?

Transfers the hardcopy data directly as an NByte stream to the remote client.

##### Return values:

| Parameter | Value |
|:----------|:------|
| `<Data>` | block data |
| Example: | See Example "Store a hard copy of the display" on page 299. |
| Usage: | Query only |

##### :HCOPy:IMAGE:FORMAT <Language>

##### :HCOPy:DEVICE:LANGUAGE <Language>

Selects the graphic format for the hard copy. You can use both commands alternatively.

##### Parameters:

| Parameter | Value |
|:----------|:------|
| `<Language>` | BMP \| JPG \| XPM \| PNG |
| | *RST: PNG |
| Example: | See Example "Store a hard copy of the display" on page 299. |
| Manual operation: | See "File Options" on page 125 |

##### :HCOPy[:EXECute]

Generates a hard copy of the current display. The output destination is a file.

The data is written into the file selected/created with the HCOPy:FILE commands.

| | |
|:---|:---|
| Example: | See Example "Store a hard copy of the display" on page 299. |
| Usage: | Event |
| Manual operation: | See "Save" on page 126 |

##### :HCOPy:FILE[:NAME] <Name>

Determines the file name and path to save the hard copy, provided automatic naming is disabled.

Note: If you have enabled automatic naming, the instrument automatically generates the file name and directory.

##### Parameters:

| Parameter | Type |
|:----------|:-----|
| `<Name>` | string |
| Example: | See Example "Store a hard copy of the display" on page 299. |
| Manual operation: | See "File Info" on page 124 |

##### :HCOPy:FILE[:NAME]:AUTO?

Queries path and file name of the hard copy file, if you have enabled Automatic Naming.

##### Return values:

| Parameter | Type |
|:----------|:-----|
| `<Auto>` | string |
| Example: | See Example "Store a hard copy of the display" on page 299. |
| Usage: | Query only |
| Manual operation: | See "File Options" on page 125 |

##### :HCOPy:FILE[:NAME]:AUTO:DIRectory <Directory>

Determines the path to save the hard copy, if you have enabled Automatic Naming.

If the directory does not exist, the instrument automatically generates a new directory, using the instrument name and /var/user/ by default.

##### Parameters:

| Parameter | Type |
|:----------|:-----|
| `<Directory>` | string |
| *RST: | `/var/user/` |
| Example: | See Example "Store a hard copy of the display" on page 299. |
| Manual operation: | See "File Options" on page 125 |

##### :HCOPy:FILE[:NAME]:AUTO:DIRectory:CLEAR

Deletes all files with extensions *.bmp, *.jpg, *.png and *.xpm in the directory set for automatic naming.

| | |
|:---|:---|
| Example: | See Example "Store a hard copy of the display" on page 299. |
| Usage: | Event |
| Manual operation: | See "File Options" on page 125 |

##### :HCOPy:FILE[:NAME]:AUTO:FILE?

Queries the name of the automatically named hard copy file.

An automatically generated name consists of:

`<Prefix><YYYY><MM><DD><Number>.<Format>`

You can activate each component separately, to individually design the file name.

##### Return values:

| Parameter | Type |
|:----------|:-----|
| `<File>` | string |
| Example: | See Example "Store a hard copy of the display" on page 299. |
| Usage: | Query only |
| Manual operation: | See "File Info" on page 124 |

##### :HCOPy:FILE[:NAME]:AUTO[:FILE]:DAY:STATE <State>

##### :HCOPy:FILE[:NAME]:AUTO[:FILE]:MONTH:STATE <State>

##### :HCOPy:FILE[:NAME]:AUTO[:FILE]:YEAR:STATE <State>

Uses the date parameters (day, month or year) for the automatic naming. You can activate each parameter separately.

##### Parameters:

| Parameter | Value |
|:----------|:------|
| `<State>` | 0 \| 1 \| OFF \| ON |
| *RST: | 1 |
| Example: | See Example "Store a hard copy of the display" on page 299. |
| Manual operation: | See "File Options" on page 125 |

##### :HCOPy:FILE[:NAME]:AUTO[:FILE]:NUMBER?

Queries the number that is used as part of the file name for the next hard copy in automatic mode.

At the beginning, the count starts at 0. The R&S SMB searches the specified output directory for the highest number in the stored files. It increases this number by one to achieve a unique name for the new file.

The resulting auto number is appended to the resulting file name with at least three digits.

##### Return values:

| Parameter | Type |
|:----------|:-----|
| `<Number>` | integer |
| Range: | 0 to 999999 |
| *RST: | 0 |
| Example: | See Example "Store a hard copy of the display" on page 299. |
| Usage: | Query only |
| Manual operation: | See "File Options" on page 125 |

##### :HCOPy:FILE[:NAME]:AUTO[:FILE]:PREFix <Prefix>

Defines the prefix part in the automatic file name. The usage of the prefix is activated with command `:HCOPy:FILE[:NAME]:AUTO[:FILE]:PREFIX:STATE`.

##### :HCOPy:FILE[:NAME]:AUTO[:FILE]:PREFIX:STATE <State>

Uses the prefix for the generation of the automatic filename, provided Automatic Naming is activated.

##### Parameters:

| Parameter | Value |
|:----------|:------|
| `<State>` | 0 \| 1 \| OFF \| ON |
| *RST: | 1 |
| Example: | See Example "Store a hard copy of the display" on page 299. |
| Manual operation: | See "File Options" on page 125 |

##### :HCOPy:FILE[:NAME]:AUTO:STATE <State>

Activates automatic naming of the hard copy files.

##### Parameters:

| Parameter | Value |
|:----------|:------|
| `<State>` | 0 \| 1 \| OFF \| ON |
| *RST: | 1 |
| Example: | See Example "Store a hard copy of the display" on page 299. |
| Manual operation: | See "Automatic Naming" on page 124 |

##### :HCOPy:REGION <Region>

Selects the area to be copied.

You can create a snapshot of the screen or an active dialog.

##### Parameters:

| Parameter | Value |
|:----------|:------|
| `<Region>` | ALL \| DIALog |
| | *RST: ALL |
| Example: | See Example "Store a hard copy of the display" on page 299. |
| Manual operation: | See "File Options" on page 125 |
