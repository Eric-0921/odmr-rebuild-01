### 6.9 KBOard Subsystem

The KBOard system contains the commands to set the external keyboard.

:KBoard:LANGUAGE.....304

:KBoard:LAYout.....304

#### :KBOard:LANGUAGE <Language>

This command selects the keyboard language. The assignment of some keys depends on the selected language.

##### Parameters:

<Language> US | DE

*RST: US

Example: KBO:LANG US

selects keyboard language American English.

Usage: SCPI confirmed

Manual operation: See "Layout (USB Keyboard Settings)" on page 108

#### :KBOard:LAYout <Layout>

Selects the keyboard language. The assignment of some keys depends on the selected language.

##### Parameters:

Parameters:

<Layout>

    CHINese | DANish | DUTCh | DUTBe | ENGLish | ENGUK | FINNish | FRENch | FREBe | FRECa | GERMan | ITALian | JAPanese | KOREan | NORWegian | PORTuguese | RUSSIan | SPANish | SWEDish | ENGUS

*RST: n.a. (factory preset: ENGLish)

Example: KBO:LAY US

Activates American English keyboard layout.

Manual operation: See "Layout (USB Keyboard Settings)" on page 108

