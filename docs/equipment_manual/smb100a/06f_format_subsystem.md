### 6.7 FORMat Subsystem

The FORMat subsystem contains the commands which determine the format of the data that the R&S SMB returns to the controller. This affects all query commands which return a list of numerical data or block data. Reference is made to this in the descriptions of the commands.

:FORMAT:BORDER.....297

:FORMAT[:DATA].....298

:FORMAT:SREGister.....298

##### :FORMAT:BORDER <Border>

Determines the sequence of bytes within a binary block. This only affects blocks which use the IEEE754 format internally.

##### Parameters:

<Border>

NORMAL | SWAPped

NORMal

The instrument expects (with setting commands) and sends (with queries) the least significant byte of each IEEE754 floating-point number first and the most significant byte last.

SWAPped

The instrument expects (with setting commands) and sends (with queries) the most significant byte of each IEEE754 floating-point number first and the least significant byte last.

*RST: NORMAL

le:

FORM: BORD SWAP

The data is transferred with the most significant bit first.

##### :FORMAT[:DATA] <Data>

Determines the data format which the R&S SMB uses to return data. When data is transferred from the control computer to the instrument, the instrument detects the data format automatically. In this case, the value set here is irrelevant.

##### Parameters:

<Data>

    ASCII | PACKed

    ASCII

    Numerical data is transferred as plain text separated by commas.

    PACKed

    Numerical data is transferred as binary block data. The format within the binary data depends on the command. The various binary data formats are explained in the description of the parameter types.

Example: FORM ASC

The data is transferred as ASCII data.

##### :FORMAT:SREGISTER <Format>

Determines the numerical format which is returned when the status registers are queried.

##### Parameters:

<Format>

    ASCII | BINary | HEXadecimal | OCTal

    ASCII

    The register content is returned as a decimal number.

    BINARY

    The register content is returned as a binary number. #B is placed in front of the number.

    HEXadecimal

    The register content is returned as a hexadecimal number. #H is placed in front of the number.

    OCTal

    The register content is returned as an octal number. #Q is placed in front of the number.

    *RST: ASCII

