## 8 Maintenance, Storage and Disposal

The product does not require regular maintenance. It only requires occasional cleaning. It is however advisable to check the nominal data from time to time.

### 8.1 Cleaning

How to clean the product is described in "Cleaning the product" on page 15.

Do not use any liquids for cleaning. Cleaning agents, solvents (thinners, acetone), acids and bases can damage the front panel labeling, plastic parts and display.

### 8.2 Changing Fuses

If the product does not start, it is possible that a blown fuse is the cause. The product is protected by 2 fuses of type IEC60127-T3.15H/250V (order no. 0099.6729.00). The fuses are next to the power supply socket at the rear panel.

#### Replacing the line fuses

1. Check the available supply voltage.

The mains voltage must be within the voltage range as denoted on the instrument. The label is below the power supply socket at the rear panel. There is no need to set the voltage manually.

2. If the power outlet exceeds the permissible range, contact Rohde & Schwarz customer service.

3. WARNING! The fuse is part of the main power supply. Handling the fuse while the power is on can lead to electric shock.

Before changing the fuse:

a) Set the switch on the power supply to position [0].

b) Disconnect the product from the power source.

c) Unplug the power cable.

4. To replace the line fuse.

a) Turn left the plastic cover of the fuse holder using a screwdriver to loosen the cover. The slot of the cover must be in vertical position.

b) Remove the cover from the fuse holder.

c) Pull out the fuse holder.

5. Check the condition of the fuse.

6. Replace the blown fuse. Only use a fuse of the specified type.

The fuse type and its characteristics are indicated below the fuse holder.

7. Insert the fuse holder into the mains power inlet.

8. Replace the cover and tighten it.

### 8.3 Storage

Protect the product against dust. Ensure that the environmental conditions, e.g. temperature range and climatic load, meet the values specified in the data sheet.

### 8.4 Performing Maintenance Tasks

Integrated procedures and additional capabilities make sure, that the R&S SMB works correct with high accuracy.

• Date and Time.....491
• Check Front Panel.....492
• Internal Adjustments.....494
• Selftest.....497

#### 8.4.1 Date and Time

The R&S SMB uses an internal real time clock to determine the date and time. It adjusts the time and date to the timezone of your location automatically, by providing a selection list of continents and cities.

The instrument records the time whenever you create or modify files on your instrument or you use timed licences. By default, the instrument is set to the UTC timezone, but you can select the timezone according to your location.

Moreover, the instrument supports NTP protocol for synchronizing all connected instruments and computer systems to minimize time delays in the network.

Access:

▶ Select "Setup > Environment > Date/Time" via the [SETUP] or [MENU] key.

| Date / Time | X |
| --- | --- |
| Date [DD.MM.YYYY] | 24.09.2018 |
| Time [hh:mm:ss] | 15:20:49 |
| Time Zone | UTC |
| NTP Settings | |
| NTP Address | |
| Use Time From NTP Server | On |

The "Date / Time" dialog contains the time and date settings of the operating system.

This function is password-protected. Unlock the protection level 1 to access it. To enable editing, unlock protection level 1, see Chapter 4.2.3.13, "Protection", on page 113.

##### Date

Displays the date set in the operating system in the format [dd.mm.yyyy].

Remote command:

:SYSTEM:DATE on page 454

##### Time

Displays the time set in the operating system in the format [hh.mm.ss].

The time setting corresponds to the selected Time Zone.

##### Remote command:

:SYSTEM:TIME on page 454

##### Time Zone

Selects the time zone.

You can select the time zone according to the major cities on the respective continents.

Note: By typing the first letter, you can quickly navigate through the lists to find the desired destination.

##### Remote command:

:SYSTEM:TIME:ZONE on page 455

:SYSTEM:TIME:ZONE:CATalog? on page 455

##### NTP Address

Sets the IP address or host name of the NTP server.

NTP is a network time protocol used for synchronizing all participating devices in a data network.

You can select a high-precision time server to reduce the impact of varying network delays.

##### Remote command:

:SYSTEM:NTP:HOSTname on page 451

##### Use Time from NTP Server

Activates clock synchronization of the network via the NTP protocol.

:SYSTEM:NTP:STATE on page 451

Remote command:

#### 8.4.2 Check Front Panel

With the functions provided in this dialog you can verify the functionality of the control keys of the R&S SMB.

In case of malfunctions, contact your Rohde & Schwarz Customer Support Center for technical support, see www.rohde-schwarz.com/support.

Accessing the online help in the check front panel dialog or exiting via ESC

During the test, the actual functions of all keys are disabled, including the [help] and the [esc] keys.

##### 8.4.2.1 Check Front Panel Settings

▶ To access this dialog, Press the "setup" key and select "Setup > Test > Check Front Panel".

| Preset | Roll L. | Roll r. | Roll r. |  |  |
| --- | --- | --- | --- | --- | --- |
| Help | Roll click | Roll click | Roll click | ESC |  |
|  | Up | Up |  | Diagram |  |
|  | Left | Right | Right | Mod |  |
|  | Down | Down |  | RF |  |
| Local | Freq |  |  |  |  |
| Setup | Level | 7 | 8 | 9 | GHz |
| File | Toggle | 4 | 5 | 6 | MHz |
| Info | Back | 1 | 2 | 3 | kHz |
| Winbar |  | 0 | . | +/- | Hz |

Reflecting the front panel, the "Check Front Panel" dialog contains all functions to test the operating elements of the instrument.

##### 8.4.2.2 Performing the Front Panel Tests

To perform the front panel test, you operate the keys at the front panel, and check the response of the instrument in the "Check Front Panel" dialog. To perform this test properly, it is essential that you check each key of the front panel. The test is only completed, when you have verified all keys.

During the test, the actual functions of the keys are disabled.

Proceed as follows:

1. Press the [setup] key.

2. Select "Test > Check Front Panel"

The "Check Front Panel" dialog opens.

3. Press a key on the front panel.

Check if the corresponding key in the "Check Front Panel" dialog turns green.

4. Press the same key a second time.

Check that the key in the dialog turns red.

Note: Pressing the same key again has no further effect, with the exception of the [esc] key, see Press the ESC key a third time.

5. Continue with the next key on the front panel and repeat step 3 to step 5 until all keys are tested.

Test Passed
All front panel keys were accessed correctly.

The test is completed, when each key is verified successfully, confirmed by a "Test passed" message.

Select "OK" to exit the test.

▶ Press the [esc] key a third time.

Exits the "Check Front Panel" dialog, even if you have not yet checked all the keys.

Expected responses:

• Pressing a key once (green), pressing twice (red)

• Pressing the [esc] key a third time exits the dialog.

If you detect a malfunction, for example, you press the front panel key the first time, and the color of the button in the dialog turns red (instead of green), the front panel key may be stuck. In this case, contact the Rohde & Schwarz Customer Support Center for technical support, see www.rohde-schwarz.com/support.

#### 8.4.3 Internal Adjustments

Internal adjustments are integrated adjustment procedures, which you can execute directly on the instrument.

The R&S SMB is accurate due to integrated adjustment procedures. Internal adjustments are integrated self-calibration routines, which you can execute directly on the instrument.

Self-calibration routines that require additional equipment are performed at an authorized Rohde & Schwarz service center. For description, see R&S SMB service manual.

How to: See Chapter 8.4.3.2, "Performing Internal Adjustments", on page 496.

##### When to start internal adjustments?

We recommend that you run internal adjustments in the following cases:

- Before starting any application that requires a maximum of level accuracy.

- When a long period of time has passed since the last adjustments.

- If the ambient temperature of the instrument significantly differs from the one of the last adjustments.

##### 8.4.3.1 Internal Adjustments Settings

Access:

1. Press the [setup] key.

2. Select "System > Internal Adjustments".

| Internal Adjustments | | |
| --- | --- | --- |
| Adjust All | | |
| Adjust Synthesis | | |
| Adjust Level | | |
| Ext Level Adjustment | | |
| Adjustment Data | Factory | |
| Stereo Coder | | |
| Adjust Analog Channels | | |

In this dialog, you can perform internal calibration routines.

The remote commands required to define these settings are described in Chapter 6.4, "CALibration Subsystem", on page 290.

##### Settings

| Adjust All | 495 |
| --- | --- |
| Adjust Synthesis | 495 |
| Adjust Level | 495 |
| Adjust Stereo Coder | 496 |

##### Adjust All

Performs all available internal calibration routines of the instrument.

Adjustment may take some time. Especially in instruments with frequencies above 6 GHz, it may last up to 15 minutes.

Remote command:

:CALibration:ALL[:MEASURE]? on page 290

##### Adjust Synthesis

Performs all adjustments which affect the frequency.

Remote command:

:CALibration<hw>:FREQUency[:MEASURE]? on page 291

##### Adjust Level

Performs all adjustments which affect the level. The acquired correction values improve the settling time and the signal quality.

Remote command:

:CALibration<hw>:LEVel[:MEASure]? on page 291

##### Adjust Stereo Coder

Performs all adjustments which affect the analog channels of the stereo coder. For the adjustment of the S/P DIF see Service Manual, chapter 2, "Adjustment".

Remote command:

[:SOURCE]:CALibration:STEREO:ANALOG[:MEAS]? on page 292

##### 8.4.3.2 Performing Internal Adjustments

The extent of the adjustment routines provided depends on the installed options.

##### How to execute internal adjustments

1. NOTICE! Adjustments can be invalid if performed when the instrument is not warmed-up.

Wait until the instrument has reached its operating temperature before you start the adjustment procedure.

The warm-up time is up to 30 minutes.

2. NOTICE! During level adjustments instruments without step attenuator, that means with frequency options R&S SMB-BxxxL, temporarily apply high power at the RF output. This high power can damage the DUT. Therefore, it is required that the RF connector is terminated during the adjustments.

Disconnect the DUT. Replace it by a 50 Ohm terminating resistor.

3. Press the [setup] key.

4. Select "System > Internal Adjustments > Adjust All".

Before the internal adjustment starts, a warning message prompts you to make sure that you have terminated the RF.

**Notice:** Please make sure a 50 Ohm termination with 1 watt / 30 dBm power rating is connected to the RF output of the instrument.

If the termination resistor is missing, a second warning message appears.

| Notice | ☒ |
| --- | --- |
| No 50 Ohm termination detected! | |
| OK | |

During adjustments, a progress indicator shows the status of the process. If any error occurs, the process aborts and an error message appears in the info line.

▶ Proceed the same way for further adjustments the instrument provides.

##### Continuing the adjustment process on error occurs

Per default, if any error occurs during the adjustment process, the process aborts. An error message appears in the "Info" line.

If you want to continue the adjustments also if there is an error, proceed as follows:

1. Press the [setup] key.

2. Select "Protection > Protection".

3. Unlock protection level 1, see Chapter 4.2.3.13, "Protection", on page 113.

4. In the setup menu, select "System > Internal Adjustments".

5. Select "Continue Adjustment on Error > On".

6. Proceed as described in "How to execute internal adjustments" on page 496.

#### 8.4.4 Selftest

A selftest is provided for service purposes.

Access:

1. Select "Setup > Test > Selftest"

2. Select "Start Selftest".

| Selftest | | |
| --- | --- | --- |
| Selftest | | |
| Testcase | Result | |

The following tests are only available via remote control:

:TEST<hw>:DIRECT on page 457

##### Start Selftest

Performs a selftest on all installed hardware options.

When completed, the R&S SMB displays a list of all performed test cases and the test results (passed or failed).

Note: While the self test is in progress, the actual signal level at the RF output is -50 dBm. This value is not indicated in the status bar.

Remote command:

:TEST<hw>:ALL:START on page 457

:TEST<hw>:ALL:RESULT? on page 457

### 8.5 Disposal

Rohde & Schwarz is committed to making careful, ecologically sound use of natural resources and minimizing the environmental footprint of our products. Help us by disposing of waste in a way that causes minimum environmental impact.

#### Disposing electrical and electronic equipment

A product that is labeled as follows cannot be disposed of in normal household waste after it has come to the end of its service life. Even disposal via the municipal collection points for waste electrical and electronic equipment is not permitted.

Figure 8-1: Labeling in line with EU directive WEEE

Rohde & Schwarz has developed a disposal concept for the eco-friendly disposal or recycling of waste material. As a manufacturer, Rohde & Schwarz completely fulfills its obligation to take back and dispose of electrical and electronic waste. Contact your local service representative to dispose of the product.
