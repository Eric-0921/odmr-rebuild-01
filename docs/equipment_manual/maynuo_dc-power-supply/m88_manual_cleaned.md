# MAYNUO ELECTRONIC M88 SERIES POWER SUPPLY USER MANUAL
# warning! our equipment is m8812!
<!-- img: https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//aa4edb48-ce58-418b-b66a-6566adc941d2/markdown_0/imgs/img_in_image_box_298_525_880_893.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-05T06%3A49%3A42Z%2F-1%2F%2F178ae623b9ba467904f40db9dc3b7cc577724150be97cf80881987550fb9d27d -->


## Overview

Programmable DC power supply

Models M88XX Series

(Including M8811/M8812/M8813/M8851/M8852/M8853/

M8871/M8872/M8873/M8874)

## Table of Contents

Chapter 1 Introduction ..... 1    
Chapter2 Technical Specifications ..... 2    
2.1 Main Technical Specification ..... 2    
2.2 Supplemental Characteristics ..... 6    
2.3 M88XX Power Supplies Dimension ..... 6    
Chapter 3 Quick Start ..... 9    
3.1 Front-panel and Rear-panel ..... 9    
3.2 Preliminary Checkout ..... 10    
3.3 If the Power Supply Does Not Turn On ..... 10    
3.4 How to Adjust the Carrying Handle ..... 11    
3.5 How to Rackmount the Instrument ..... 12    
Chapter 4 Panel Operation ..... 13    
4.1 Key Layout ..... 13    
4.2 Front-panel Operation Overview ..... 14    
4.3 Constant Voltage Operation ..... 14    
4.4 Constant Current Operation ..... 15    
4.5 Saving and Recalling Operation ..... 15    
4.6 Menu Operation ..... 15    
4.6.1 Menu Description ..... 15    
4.6.2 Menu Function ..... 18    
4.7 Output On-Off Operation ..... 23    
4.8 Remote Measurement Function ..... 23    
4.9 Milliohmmeter Function ..... 24    
4.10 Voltmeter Function ..... 25    
Chapter 5 Remote Operation Mode ..... 26    
5.1 M-131 Communication Cable ..... 26    
5.2 Communication between Power Supply and PC ..... 26    
Chapter 6 SCPI Communication Protocol ..... 28    
6.1 Communication command introduction ..... 28    
6.2 Commonly used relevant command description ..... 28    
6.2.1 Basic command (IEEE-488.2 Common Command Set) ..... 28    
6.2.2 Command specified by M8800 software ..... 29    
6.2.3 Measurement Command ..... 31

6.2.4 Setting Command.....31    
6.2.5 List Operation (LIST) Related Command.....34    
Quick Reference.....39

### Chapter 1 Introduction

M88XX Series power supplies are high performance single-output programmable DC power supplies with communication interface, possessing the character of fast rise time speed (The rise speed of M8811 power supply can be less than 10mS and that of M8851 can be less than 20mS). The combination of bench-top and system features in these power supplies provides versatile solutions for your design and test requirements. The M88XX Series can not only be programmed through the keyboard on the panel, but also be functioned as voltmeter and milliohmmeter, which will bring great convenience to the users. As a regeneration product of ordinary programmable power supplies, M88XX Series power supplies are more cost-effective.

#### M88XX Series power supplies' features

• Low Periodic And Random Deviation (PARD) and noise

• High resolution and accuracy (0.1mV/0.01mA)

• Installing a high-accuracy 5 1/2 voltmeter and milliohmmeter

• Supporting high-accuracy and dynamic programming output

• High – luminance VFD screen and two lines& four ways display

• Smart fan will be automatically initiated according to the temperature.

• Supporting remote voltage compensation and multidata storage

• Supporting external trigger input and output.

• Power-on-self-test, software calibration and standard designed instrument stand

• Supporting TTL level Serial communication interface

### Chapter2 Technical Specifications

### 2.1 Main Technical Specification

> M881X Series power supplies technical specification table


| Model |  | M8811 | M8812 | M8813 |
| --- | --- | --- | --- | --- |
| Input Rating | Voltage | 0-30V | 0-75V | 0-150V |
| Current | 0-5A | 0-2A | 0-1A |  |
| Load Regulation | Voltage | <0.01%+0.5mV | <0.01%+0.5mV | <0.01%+0.5mV |
| Current | <0.01%+0.1mA | <0.01%+0.1mA | <0.01%+0.1mA |  |
| Setting Value Resolution | Voltage | 0.5mV | 1mV | 2mV |
| Current | 0.1mA | 0.05mA | 0.01mA |  |
| Readback Value Resolution | Voltage | 0.1mV | 0.1mV | 1mV |
| Current | 0.01mA | 0.01mA | 0.01mA |  |
| Setting Value Accuracy | Voltage | 0.01%+2mV | 0.01%+5mV | 0.01%+15mV |
| Current | 0.05%+1mA | 0.05%+0.5mA | 0.05%+0.1mA |  |
| Readback Value Accuracy | Voltage | 0.02%+5mV | 0.02%+12mV | 0.02%+25mV |
| Current | 0.1%+5mA | 0.05%+2mA | 0.05%+1mA |  |
| Periodic And Random Deviation | Voltage | 3mvp-p | 5mvp-p | 10mvp-p |
| Current | 2mA rms | 1mA rms | 0.5mA rms |  |
| Voltmeter Accuracy | 0-12V Accuracy: 0.02%+2mV; 0-50V Accuracy: 0.02%+5mV |  |  |  |
| Milliohmmeter Accuracy | 10W. 0-1000mΩ Accuracy: 0.2%+3 mΩ; 1000-10000 mΩ\nAccuracy: 0.2%+6 mΩ |  |  |  |
| Working Condition | 0-40°C; 0-90% RH |  |  |  |
| Power Required | AC 110V/220V ±10%; 50/60 HZ |  |  |  |
| Weight | 9Kg |  |  |  |
| Dimension | 108X214X365mm |  |  |  |

> M885X Series power supplies technical specification table


| Model | M8851 | M8852 | M8853 |
| --- | --- | --- | --- |
| Input Rating | 0-6V | 0-30V | 0-75V |
| 0-60A | 0-20A | 0-8A |  |
| Load Regulation | <0.01%+1mV | <0.01%+1mV | <0.01%+1mV |
| <0.01%+0.1mA | <0.01%+0.1mA | <0.01%+0.1mA |  |
| Setting Value Resolution | 0.1mV | 0.5mV | 1mV |
| 1mA | 0.5mA | 0.2mA |  |
| Readback Value Resolution | 0.1mV | 0.1mV | 0.1mV |
| 0.1mA | 0.1mA | 0.1mA |  |
| Setting Value Accuracy | 0.01%+1mV | 0.01%+5mV | 0.01%+10mV |
| 0.05%+6mA | 0.05%+2mA | 0.05%+1mA |  |
| Readback Value Accuracy | 0.02%+2mV | 0.02%+5mV | 0.02%+12mV |
| 0.05%+30mA | 0.05%+15mA | 0.05%+8mA |  |
| Periodic And Random Deviation | 3mvp-p | 5mvp-p | 7mvp-p |
| 15mA rms | 7mA rms | 4mA rms |  |
| Voltmeter Accuracy | 0-12V\nAccuracy: 0.02%+2mV;\n0-50V\nAccuracy: 0.02%+5mV |  |  |
| Milliohmmeter Accuracy | 10W. 0-1000mΩ\nAccuracy: 0.2%+3 mΩ;\n1000-10000 mΩ\nAccuracy: 0.2%+6 mΩ |  |  |
| Working Condition | 0-40°C;\n0-90% RH |  |  |
| Power Required | AC 120V/230V ±10%; 50/60 HZ |  |  |
| Weight | 28Kg |  |  |
| Dimension | 428X103.5X453.5mm |  |  |

> M887X Series power supplies technical specification table


| Model | M8871 |
| --- | --- |
| Input Rating | 0-15V, 0-60A |
| Load Regulation | <0.01%+1mV, <0.01%+0.1mA |
| Setting Value Resolution | 0.1mV, 1mA |
| Readback Value Resolution | 0.1mV, 0.05%+6mA |
| Readback Value Accuracy | 0.03%+5mV, 0.05%+45mA |
| Ripple | 4mvp-p, 10mA rms |
| Voltmeter Accuracy | 0-12V Accuracy: 0.02%+2mV; 0-50V Accuracy: 0.02%+5mV |
| Milliohmmeter Accuracy | 10W 0-1000mAuracy:0.2%+3mΩ;1000-10000mΩ\nAccuracy:0.2%+6 mΩ |
| Working Condition | 0~40°C; 0~90% RH |
| Power Required | AC 100V/120V/220V ±10%; 50/60 HZ |
| Weight | 38Kg |
| Dimension | 583mm (W)*180mm(H)*445mm(D) |


| Model | M8872 |
| --- | --- |
| Input Rating | 0-30V, 0-35A |
| Load Regulation | <0.01%+1mV, <0.01%+0.1mA |
| Setting Value Resolution | 0.5mV, 0.5mA |
| Readback Value Resolution | 0.1mV, 0.05%+2mA |
| Readback Value Accuracy | 0.03%+5mV, 0.05%+25mA |
| Ripple | 5mvp-p, 5mA rms |
| Voltmeter Accuracy | 0-12V Accuracy: 0.02%+2mV; 0-50V Accuracy: 0.02%+5mV |
| Milliohmmeter Accuracy | 10W 0-1000 mΩ\nAccuracy:0.2%+3 mΩ; 1000-10000mΩ\nAccuracy:0.2%+6 mΩ |
| Working Condition | 0~40°C; 0~90% RH |
| Power Required | AC 100V/120V/220V ±10%; 50/60 HZ |
| Weight | 38Kg |
| Dimension | 583mm (W)  *180mm(H)  *445mm(D) |

> Nanjing Maynuo Electronics Co., Ltd


| Model | M8873 |
| --- | --- |
| Input Rating | 0-75V, 0-15A |
| Load Regulation | <0.01%+1mV, <0.01%+0.1mA |
| Setting Value Resolution | 2mV, 0.2mA |
| Readback Value Resolution | 0.1mV, 0.05%+1mA |
| Readback Value Accuracy | 0.03%+15mV, 0.05%+15mA |
| Ripple | 6mvp-p, 3mA rms |
| Voltmeter Accuracy | 0-12V Accuracy: 0.02%+2mV; 0-50V Accuracy: 0.02%+5mV |
| Milliohmmeter Accuracy | 10W 0-1000 mΩ\nAccuracy:0.2%+3 mΩ;1000-10000mΩ\nAccuracy:0.2%+6 mΩ |
| Working Condition | 0~40°C; 0~90% RH |
| Power Required | AC 100V/120V/220V ±10%; 50/60 HZ |
| Weight | 38Kg |
| Dimension | 583mm (W)  *180mm(H)  *445mm(D) |


| Model | M8874 |
| --- | --- |
| Input Rating | 0-100V, 0-11A |
| Load Regulation | <0.01%+1mV, <0.01%+0.1mA |
| Setting Value Resolution | 2mV, 0.2mA |
| Readback Value Resolution | 1mV, 0.05%+1mA |
| Readback Value Accuracy | 0.03%+25mV, 0.05%+12mA |
| Ripple | 8mvp-p, 2.5mA rms |
| Voltmeter Accuracy | 0-12V Accuracy: 0.02%+2mV; 0-50V Accuracy: 0.02%+5mV |
| Milliohmmeter Accuracy | 10W 0-1000 mΩ\nAccuracy:0.2%+3 mΩ;1000-10000mΩ\nAccuracy:0.2%+6 mΩ |
| Working Condition | 0~40°C; 0~90% RH |
| Power Required | AC 100V/120V/220V ±10%; 50/60 HZ |
| Weight | 38Kg |
| Dimension | 583mm (W)  *180mm(H)  *445mm(D) |

## 2.2 Supplemental Characteristics

Recommended Calibration Interval: Once/Year

AC Input Ratings (selectable via switch on the rear panel)

Option 01: 220VAC ± 10%, 47 to 63 Hz

Option.02: 110 VAC ± 10%, 47 to 63 Hz

Cooling: forced cooling

Operating Temperature: 0 to 40 °C

Storage Temperature: -20 to 70 °C

Environmental Conditions: Designed for indoor use with pollution degree 2 environment. Designed to operate at maximum relative humidity of 95%.

## 2.3 M88XX Power Supplies Dimension

The dimension of M881X Series is 256mmW x 108mm H x 365mm L. Please refer to the following figure:


<!-- img: https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//06410cf1-364c-4026-924f-6b9b28130e50/markdown_1/imgs/img_in_image_box_285_759_513_861.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-05T06%3A50%3A00Z%2F-1%2F%2F327cb4b25b7c695317295365cb361a6df1f9c37110ca88aeff776195f2ea3377 -->


<!-- img: https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//06410cf1-364c-4026-924f-6b9b28130e50/markdown_1/imgs/img_in_image_box_559_759_992_860.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-05T06%3A50%3A00Z%2F-1%2F%2F558787b05b52005e967b085cb6bd7cd8e12528326f69f9885fdae808cec9315d -->


<!-- img: https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//06410cf1-364c-4026-924f-6b9b28130e50/markdown_1/imgs/img_in_image_box_225_931_525_1139.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-05T06%3A50%3A01Z%2F-1%2F%2F19f0365ddc3db2799fd17a8d0880494a3e7594a117432ace161dba8ed40116cd -->


<!-- img: https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//06410cf1-364c-4026-924f-6b9b28130e50/markdown_1/imgs/img_in_image_box_550_903_1007_1208.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-05T06%3A50%3A01Z%2F-1%2F%2F73da4cf5fb06dcce2d5f9183b39bbcbf068149945705ce320d55cb404effb186 -->


> Figure 2.1: M881X Power Supplies Dimension Figure


The dimension of M885X Series is 428mmW×103.5mmH×453.5mmL, Please refer to the following figure:


<!-- img: https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//06410cf1-364c-4026-924f-6b9b28130e50/markdown_2/imgs/img_in_image_box_257_252_915_343.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-05T06%3A50%3A02Z%2F-1%2F%2F2afaf9b5281628a486666037e3ed4a91f7cc237d7aa379ca464ea137ef1541f8 -->


<!-- img: https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//06410cf1-364c-4026-924f-6b9b28130e50/markdown_2/imgs/img_in_image_box_256_371_585_638.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-05T06%3A50%3A03Z%2F-1%2F%2F8af754c2d70d5ccbec23934b17b849dbdf33e50f546a4bf52a006b8e3a9bd24a -->


<!-- img: https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//06410cf1-364c-4026-924f-6b9b28130e50/markdown_2/imgs/img_in_image_box_631_449_971_603.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-05T06%3A50%3A03Z%2F-1%2F%2F2e4d594a31047b4866a7e576b4b1946af55d171ecd354857b1d269bb6115a13b -->


> Figure 2.2: M885X Power Supplies Dimension Figure


The dimension of M887X Series is 482mmW×184.5mmH×583.5mmD. Please refer to the following figure:


<!-- img: https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//06410cf1-364c-4026-924f-6b9b28130e50/markdown_3/imgs/img_in_image_box_225_354_1066_758.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-05T06%3A50%3A04Z%2F-1%2F%2Fb1042cd8307e9feef9e11004c4beb7cff513427d25210384872541a41cc6b24a -->


> Figure 2.3: M887X Power Supplies Dimension Figure


### Chapter 3 Quick Start

This chapter mainly focuses on the brief introduction of the surface appearance and basic functions of M88XX Series power supplies so that both experienced and inexperienced users can be acquainted with the new products quickly. Meanwhile, the chapter also clarifies some preliminary checkout that should be made prior to operation to make sure the normal running of the products.

### 3.1 Front-panel and Rear-panel

The front panel layout of M881X Series power supply is as follows:


<!-- img: https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//06410cf1-364c-4026-924f-6b9b28130e50/markdown_4/imgs/img_in_image_box_261_464_1003_746.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-05T06%3A50%3A05Z%2F-1%2F%2F987763c4870b341a3ba557e64776d867bef5f67aa0f77d551f03cd4da83d132b -->


> Picture 3.1: The Front Panel of M881X Series Power Supplies


The upper half is black VFD display screen and knob

The bottom half, left side to right side, is Numeric keys 0-9, ESC key, Function keys, Up-Down keys, Enter key, Input terminal and Output terminal.

The rear-panel layout of M881X is as follows:


<!-- img: https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//06410cf1-364c-4026-924f-6b9b28130e50/markdown_4/imgs/img_in_image_box_312_1063_1057_1381.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-05T06%3A50%3A06Z%2F-1%2F%2F6c9b42c35d8336adf6d9a78d79346bbde4f5d074e80116a030bcb6da7c900bf4 -->


> Picture 3.2: The Rear Panel of M881X Series Power Supplies


① Cooling Window

② Multifunction Interface Connector

③ 9 Pin COM Interface

④ Power Switch Key (110V / 220V)

⑤ Power Socket

## 3.2 Preliminary Checkout

The following steps help you verify that the power supply is ready for use

# 1. Check the list of supplied items

Verify that you have received the following items with your power supply. If anything is missing, contact your nearest Sales Office.
- [ ] One power cord for your location
- [ ] The user's manual
- [ ] One CD(only when you have bought communication accessories)
- [ ] One communication cable (only when you have bought communication accessories)
- [ ] One testing line (only M8811, M8812, M8813 have the testing line)

# 2. Connect the power cord and turn on the power supply.

When you turn on the power supply, the VFD display screen will light up briefly and the power supply performs its power-on self-test. Please check if there is any stroke loss on VFD display.

Warning: The power supply is shipped from the factory with a power-line cord that has a plug appropriate for your location. Your power supply is equipped with a 3-wire grounding type power cord. The power supply is grounded only when the power-line cord is plugged into an appropriate receptacle. Do not operate your power supply without adequate cabinet ground connection.


<!-- img: https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//abed4778-7e14-4756-95d2-9cdcc119fb65/markdown_0/imgs/img_in_image_box_141_950_199_1007.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-05T06%3A49%3A33Z%2F-1%2F%2Fc19ec07dda150d2d3c1087afb7adc8c62057ebfb4fee79f0df1420dd0a3f8efe -->


### 3.3 If the Power Supply Does Not Turn On

Use the following steps to help solve problems you might encounter when turning on the instrument.

# 1. Verify that there is AC power to the power supply.

First, verify that the power cord is firmly plugged into the power receptacle on the rear panel of the power supply. You should also make sure that the power source you plugged the power supply into is energized. Then, verify that the power supply is turned on.

# 2. Verify the power-line voltage setting.

The line voltage is set to the proper value for your country (110VAC or 220VAC) when the power supply is shipped from the factory. Change the voltage setting if it's not correct.

# 3. Verify that the correct power-line fuse is installed.

> If the fuse was damaged, please see the table below to replace the fuse for your power supply.


| Model | Fuse Description (110VAC) | Fuse Description (220VAC) |
| --- | --- | --- |
| M8811 | T5A 250V | T3.15A 250V |
| M8812 | T5A 250V | T3.15A 250V |
| M8813 | T5A 250V | T3.15A 250V |
| M8851 | T10A 250V | T6.3A 250V |
| M8852 | T10A 250V | T6.3A 250V |
| M8853 | T10A 250V | T6.3A 250V |
| M8871 | T15A 250V | T10A 250V |
| M8872 | T15A 250V | T10A 250V |
| M8873 | T15A 250V | T10A 250V |
| M8874 | T15A 250V | T10A 250V |

# 4. How to replace the power-line fuse

Open the plastic cover which locates at below the power input socket in the rear panel of the power supplies by screwdriver, then you will see the fuse. Please replace the damaged fuse with the matched fuse.

### 3.4 How to Adjust the Carrying Handle

To adjust the position, grasp the handle by the sides and pull outward. Then, rotate the handle to the desired position. There are following three positions for you to choose:


<!-- img: https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//abed4778-7e14-4756-95d2-9cdcc119fb65/markdown_1/imgs/img_in_image_box_328_1026_898_1355.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-05T06%3A49%3A34Z%2F-1%2F%2F26baa3fbff8c56c8624efdeffbf65834b86aa0442ca67aae5353da125a9668b5 -->


> Picture 3.3 M88XX Series Power Supplies Viewing Positions


## 3.5 How to Rackmount the Instrument

The power supply can be mounted in a standard 19-inch rack cabinet and be easily applied to your testing system. If you want to rackmount the M88XX Series, please buy MR01 rack kit.

Note: Remove the carrying handle, the rubber coating, and the foot on the rear panel before rackmounting the instrument.


<!-- img: https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//abed4778-7e14-4756-95d2-9cdcc119fb65/markdown_2/imgs/img_in_image_box_275_475_994_676.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-05T06%3A49%3A36Z%2F-1%2F%2Fa9d86b08cefcc2ec36d6dfbed8ecc0f0d29887def7766d1e6d0c2f3e48a46bcf -->


> Picture 3.4: To Rackmount a Single Instrument


<!-- img: https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//abed4778-7e14-4756-95d2-9cdcc119fb65/markdown_2/imgs/img_in_image_box_287_771_995_964.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-05T06%3A49%3A36Z%2F-1%2F%2Fb3d7b0047a57928b8c376e89d951252dce3faec9f837211d36a525508ade7584 -->


> Picture 3.5 To Rackmount Two Instruments Side by Side


### Chapter 4 Panel Operation

This chapter mainly introduces the front-panel operation in detail from the following parts:

Voltage Set Operation

Current Set Operation

Saving and Recalling Operation

Menu Operation

Output ON/OFF Operation

Remote Measurement Function

Milliohmmeter Function

Voltmeter Function

### 4.1 Key Layout

<!-- img: https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//abed4778-7e14-4756-95d2-9cdcc119fb65/markdown_3/imgs/img_in_image_box_167_554_1033_913.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-05T06%3A49%3A37Z%2F-1%2F%2Fd242fdb3ccfd5e42deefcc1899e503a4a0f321b505363e15a89ac3643b1a2956 -->


> Picture 4.1 Operation Panel Picture


> Multifunction Keys Directions


| 0～9 | Numeric keys |
| --- | --- |
| Menu | Menu operation key |
| List | List output operation |
| Trigger | Trigger key |
| V/mΩ | Voltmeter and milliohmmeter switch button |
| 0.1W | Choosing 0.1W power output operation when the instrument is used as milliohmmeter |
| 1W | Choosing 1W power output when the instrument is used as milliohmmeter |
| 10W | Choosing 10W power output when the instrument is used as milliohmmeter |
| Local | Local operation |
| Esc | Esc key (can be exited from any working condition) |


> Function Keys Direction


| V-set | Setting the voltage value |
| --- | --- |
| I-set | Setting the maximum current value |
| Save | Saving the current settings to a particular storage locations |
| Recall | Recalling the saved settings |
| Menu | Menu operation: setting the parameter |
| On/Off | Control the output state |
| Shift | Used together with multifunction key to perform diversity functions and applications(for example: shift+Menu can perform menu function) |
| ▲ | Up-key: choosing menu item in menu operation or to increase the output voltage |
| ▼ | Down-key: choosing menu item in menu operation or to reduce the output voltage |
| Enter | Confirm key |

## 4.2 Front-panel Operation Overview

The following section describes an overview of the front-panel keys before operating your power supply.

The power supply is shipped from the factory configured in the front-panel operation mode. At power-on, the power supply is automatically set to operate in the front-panel operation mode. When in this mode, the front panel keys can be used.

When the power supply is in remote operation mode, you cannot use the front-panel. A change between front-panel and remote operation modes will not result in any change in the output parameters. You can change the front-panel and remote operation modes by PC.

The output of the power supply can be enabled or disabled from the front panel by pressing the key On/Off.

The VFD display shows the current operating status of the power supply with annunciators. When it is powered on, the VFD will display two lines of data. The first line shows the actual voltage value, actual current value and power supply's status while the second line shows the voltage value that can be measured by voltmeter and output setting value of the M88XX Series power supplies.

## 4.3 Constant Voltage Operation

The constant voltage range is from 0V to the maximum voltage value of each model. It is very easy for you to set the constant voltage output.

Solution One: Press the ▲ and ▼ keys to change the value when the M88XX Series power

suppliers are powered on.

Solution Two: Step1. Power on the M88XX series instrument.

Step2. Press V-Set key.

Step3. Press the numeric keys 0 to 9 to enter the voltage value you wanted.

Step4. Press Enter to confirm the value

Close knob lock function when you in config menu, also can use the following two operations:

1) Use upper revolving encoder directly to adjust voltage;

2) Press V-set button then use upper revolving encoder to adjust voltage

## 4.4 Constant Current Operation

The constant current output range is from 0A to the maximum current value of each type. It is very easy for you to set the constant current output.

Step1. Power on the M88XX series instrument

Step2. Press (I-Set) key

Step3. Use the numeric keys  0 to  9 to enter the voltage value you wanted.

Step4. Press Enter key to confirm the value

IF you close knob lock function in config menu, can use the following operation:

Press V-set button then use upper revolving encoder to adjust voltage

### 4.5 Saving and Recalling Operation

You can store up to 50 different output states in storage register locations so that you can recall the saved settings quickly. This kind of store operation can be performed by the keys  **Save** and  **Recall** located in the front panel. When the fast recalling is activated, you can press the numberic keys  **9** to invoke the corresponding data.

Each output state includes 1. Constant voltage value, 2. Constant current value, 3. Maximum voltage setting value, 4. Maximum output voltage value.

Step1. After you setting an output state (CV value, CC value and Maximum voltage), press the Save key

Step2.Use the numeric keys  0 to  9 or  ▲ and  ▼ keys to select the memory location (the range is 1 to 50) which you want to store in.

Step3. Press  **Enter** to confirm the memory location.

Step4. Press  Recall key.

Step5. Use numeric keys  0 to  0 or  ▲ and  ▼ keys to select the states which you want to recall.

Step6. Press Enter key to confirm. Then the saved settings will come out

## 4.6 Menu Operation

#### 4.6.1 Menu Description

Press the key (MENU) to access to the menu function and at the moment theVFD display screen

shows the menu items. You can select the menu items by pressing the ▲ and ▼ keys or by rotating the knob, and then press the key  **ENTER** to enter in the menu item you wanted. Or you can press the key  **Esc** to get back to the higher level menu.


<!-- img: https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//51bad6f5-161f-431d-9380-a3b87861f38d/markdown_1/imgs/img_in_image_box_1100_155_1133_190.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-05T06%3A50%3A12Z%2F-1%2F%2F84b69173f031a92a538bff7f3a01ea1cec716b7f9f67b873925cf319079b1d52 -->


The first level menu includes:

Config

System set

List Set

Auto test

Output Timer

When accessing to Config item, the following menu items will be displayed on the VFD display screen by pressing the keys ▲ and ▼.

##### Init: All set for factory value

Output Recall:


| On | Setting the output to OFF state when the power supply is powered on. |
| --- | --- |
| Off(Default) | Setting the same state as last time you turned off the power supply. |

##### Key Sound Set:

| On (Default) | The buzzer will sound when any key was pressed. |
| --- | --- |
| Off | the buzzer will not sound when any key was pressed |

##### Knob Lock Set:

| On | The knob is locked and rotating the knob will not cause any change of the setting value. |
| --- | --- |
| Off (Default) | The knob is unlocked and rotating the knob will cause change of the setting value. |

##### Remote Meas.:

| On | Activating the remote meas. mode (Rear-panel meas. mode) |
| --- | --- |
| Off (Default) | Activating the front-panel meas. mode |

##### Current Unit Set:

| mA | Current unit mA |
| --- | --- |
| A(Default) | Current unit A (Default) |

##### Match Power:

| 50HZ | Match 50HZ (50HZ Default) |
| --- | --- |
| Off(Default) | Match 50HZ |

##### Baudrate Set:

| 4800 | Setting the baudrate as 4800bps |
| --- | --- |
| 9600(Default) | Setting the baudrate as 9600bps |
| 19200 | Setting the baudrate as 19200bps |
| 38400 | Setting the baudrate as 38400bps |

##### Comm. Parity:

| None(Default) | Setting no parity bit |
| --- | --- |
| Even | Setting comm. parity as even parity bit |
| Odd | Setting comm. parity as odd parity bit |


| Port Mode: | Trigger(Def) | Trigger mode |
| --- | --- | --- |
| RI/DFI |  |  |
| Digital I/O | Digit port |  |
| Key Lock Set: | Password= | Entering password and then pressing the key Enter to unlock the password |

Exit:

When accessing to System Set item, the following menu items will be displayed on the VFD display screen by pressing the keys ▲ and ▼.


| Max Volt. Set: | Max= | Setting the maximum output voltage |
| --- | --- | --- |


| Step Volt Set: | Step= | Inputting the voltage step value and pressing up-down keys to change it |
| --- | --- | --- |

Exit

When accessing to the List Set item, the following menu items will be displayed on the VFD display screen by pressing the keys ▲ and ▼.

Load List File: Read list

Edit List File:


| Continuous | Continuous mode: once:, repeat |
| --- | --- |
| Step | Single-step mode, once: repeat: |

Edit File Format:

Exit:


| 8X25 Steps | Setting list file as 8X25step |
| --- | --- |
| 4X50 Steps | Setting list file as 4X50step |
| 2X100 Steps | Setting list file as 2X100 step |
| 1X200 Steps | Setting list file as 1X200 step |

When accessing to the Auto Test item, the following menu items will be displayed on the VFD display screen by pressing the keys ▲ and ▼.

Load Atest File:

Edit Atest File:

Exit:

When accessing to the Menu Timing item, you can see the item Close Delay, which means to set the output timing.

Note: When accessing to the menu item, you can press  Esc to exit the menu operation.

No matter which function the instrument is performing, you can exit the function operation status by pressing the key  Esc

### 4.6.2 Menu Function

#### Initiating the Output State (>Output Setup)

This instruction can initiate the output state when the power supply is powered on. If you select the item On, the power supply will initiate the output to OFF state when the power supply is powered on. If you select the item Off, the output will remain the same state as last time you turned off the power supply

#### Setting the Key Sound (Key Sound Set)

This instruction can switch on/off the buzzing sound when you press any key, if you select the item On, the buzzer will sound when any key was pressed. If you select Off, the buzzer will not sound when the keys were pressed. Default setting is the item On; the buzzer will sound when you press any key.

#### Fast Recalling Function (>Fast Recall)

This instruction can help you recall the saved data very fast when it is set as On condition. The detailed operations are as follows: first you need to save several often used voltage value and current value, then use the Recall key to recall those data.

Press the keys and to access to the menu function. When VFD display screen shows the item  **Config**, press the key  **ENTER** to confirm.

Press the keys▲ and ▼to select the item Fast Recall, and then confirm by pressing the key ENTER

Press the keys ▲ and ▼ to select the item On.


<!-- img: https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//51bad6f5-161f-431d-9380-a3b87861f38d/markdown_3/imgs/img_in_image_box_294_981_343_1019.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-05T06%3A50%3A14Z%2F-1%2F%2F1bb835c55ef50ab049387f0088b5ede9916e3c59fe3f1cc79029008737c5f34b -->


Press the key twice to exit the menu operation.

Press the keys (V-set) and (I-set) to set your favored voltage value and current value. After you setting an output state (CV value, CC value and Maximum voltage), press the key

Save numeric keys 0 to 9 or ▲ and ▼ keys to select the memory location (the range is 1 to 50) which you want to store in.

. Press  **Enter** key to confirm the memory location.

. Press  **Recall** key.

Use numeric keys   0  to   9  or  ▲ and  ▼ keys to select the states which you want to recall.

. Press  **Enter** key to confirm. Then the saved settings will come out

#### Overcurrent Protection (>OCP Set)

M8811, M8812, M8813, M8851, M8852, M8853 are not available of this function for while.

##### Setting the Baudrate (>Baudrate Set)

This instruction can change the communication baud rate of the power supply; the baud rate range is 4800bps, 9600bps, 19200bps, 38400bps. Before the communication, you must make sure that there is same baud rate between the power supply and the host computer. Default baud rate is 9600bps.

##### Setting Address (>Sour. Address)

This instruction can set the communication address of each power supply. The address range is from 0 to 30. Before the communication, you must make sure that there is same address between the power supply and the host computer. Default address is 0.

Operation steps:

Press the keys  **Shift** and  **MENU** to access to the menu function. When the VFD display screen shows the item  **Config**., press the key  **ENTER** to confirm.

Press the keys ▲ and ▼to get the item Sour. Address, then press the key ☺ENTER to confirm.

When the VFD display screen shows   Address=**, choose the number keys from  0 to  9 to set the address, followed by pressing the key  ENTER to confirm.

Press the key

twice to exit the menu operation.


<!-- img: https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//51bad6f5-161f-431d-9380-a3b87861f38d/markdown_4/imgs/img_in_image_box_303_712_354_757.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-05T06%3A50%3A15Z%2F-1%2F%2F7c340a812439d7c21a2959d33091ad3bf33d9cd9fda091f24a84740286f85693 -->


##### Port Mode (>Port Mode):

M8811, M8812, M8813, M8851, M8852, M8853 are not available of this function for while.

#### Trigger Operation (Shift+Trigger)

Just as described above, the trigger operation can also be applied to the list of output operation.

There are three trigger modes for the power supply. Before performing the trigger function, users need to select one trigger mode.

Trigger by keyboard (pressing the keys  Shift) and  Trigger): The trigger operation will be initiated once when pressing the keys  Shift and  Trigger under the condition that the trigger by keyboard is effective.

##### Remote Control Function (>Output Contr.)

There are three modes under the remote control function: LATCHED, FOLLOW, OFF.

LATCHED Mode: The output of power supply will be turned off when testing out the change in voltage level from high to low from TRQ port.

FOLLOW Mode: M88XX series power supplies are not available of this function for while.

OFF Mode: The output status of M88XX series power supplies are not affected by RI input level.

Fault Indication Function (>DFI Status)

DFI results from following five sources: Quest, OPER, StEV, Requ., Off.

Setting Password for Function Keys (>Key Lock Set)

This instruction can set a password (1 through 4 digits) to lock the function keys operation. After setting the password, all the function keys on the front panel will be locked except the key  On/Off. You must enter the correct password to unlock them, and then you can continue to do the function key operation. If you don't want to lock the function keys, please don't press any number key when you enter the Key Lock Set item, just press  Enter key to unlock it.

Note: When shipped from factory, there is no password and function keys. The start bit of your desired password shouldn't be 0.

#### List Operation (>List Set)

You can set every single-step value and duration time by new list operation so as to get different output list. The parameters of the list operation include the name of the list file which are input, input single-step numbers (200steps at most), single-step duration time (1mS at least) and the setting value of every single-step.

When list operation is under  **continuous** mode, the power supply will begin the list operation until the list operation is finished or until receiving the next trigger signal.

Before editing the list, if you want to change the file storage format, please operate as the following steps:

1) Press the keys Shift and Menu to access to the menu function.

2) When VFD display screen shows the item  **Config**, press the key  ▼to select the item  **List**  Set, press the key  **Enter** to confirm.

3) When VFD display screen shows the item Load ListFile, Press the key ▼ twice, select the item Edit

File Format, press the key


<!-- img: https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//236168b5-e0f3-4be6-ba2f-d43a5d46d509/markdown_0/imgs/img_in_image_box_386_1118_453_1153.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-05T06%3A49%3A34Z%2F-1%2F%2F5ba6d3f4a1f7227f0ad11e66416b32e25e4368d4ae7cca72cf050914869ec9a4 -->


to confirm.

4) When VFD display screen shows  8×25 Steps, Press the key  ▼ twice, select a list mode you want from  1×200 Steps,  2×100 Steps,  4×50 Steps.

2*100 Steps means to set a two100 steps of list file.

1*200 Steps means to set a one 200 steps of list lie..

8*25 Steps means to set an eight 25 steps of list file.

4*50 Steps means to set a four 50 steps of list file.

5) Press ▼ to select the item Edit List File, press the key Enter to confirm.

6) When VFD display screen show the item Edit List File 1, press the key ▲ and ▼ to select sequential

number you want to edit, press the key Enter to confirm.

7) When VFD display screen show the item List x Steps= xxx, press numeric keys to set the total list steps(x). Press the key Enter to confirm.

When VFD display screen shows the item List File x Loop Mode, press the key ▲ and ▼ to select operation mode:

Loop Mode means to looping execute list file

Continuous means to execute all steps in the list file once a trigger

Step Mode means one trigger one step

9) When VFD display screen show the item Step 1 Time= xxxxx mS, press numeric keys to set time for step 1, press the key Enter to confirm.

10) When VFD display screen show the item Step 1 Volt = xxxxx V, press numeric keys to set voltage, press the key Enter to confirm.

11) When VFD display screen show the item Step 1 Curr=xxxx A, press numeric keys to set current, press the key Enter to confirm.

12) Repeat step 9, 10, 11 to complete all steps in list file

13) Repeat step 5 to 15 to edit other list files if needed

14) Press ▼ to select Load List File, press the key Enter to confirm.

15) Electronic load is in the LIST MODE. VFD display screen shows OFF in the right when output is on. VFD display screen shows List1 in the right when output is off. Press key On/Off to execute. Press key Shift+2 or Esc to exit. Press key Shift+2 direct to execute the last list file.

16) Under loop mode, electronic load looping executes automatically. Please find the following table at Continuous Mode and Step Mode.


<!-- img: https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//236168b5-e0f3-4be6-ba2f-d43a5d46d509/markdown_1/imgs/img_in_image_box_355_1036_909_1183.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-05T06%3A49%3A35Z%2F-1%2F%2F33bcc372842f94893be4d60595b2c678484500de60897dfe1aa5f4fe07fb8015 -->


> Picture 4.2 Repeat Trigger with 2 Times Cyclical Pattern


When the list operation is under the single step mode, the power supply will not be changed to next step until receiving a trigger signal. Please refer to the following picture


<!-- img: https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//236168b5-e0f3-4be6-ba2f-d43a5d46d509/markdown_2/imgs/img_in_image_box_366_104_904_268.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-05T06%3A49%3A37Z%2F-1%2F%2F5c9811d1e6e3a5f71c5ba42a5a3ad5895c01abd4796d1243c6e9db0716dc11e0 -->


> Picture 4.3 Single-Step Trigger


##### Automatic Test (>Atest Set)

You can check if the equipment is qualified by editing the automatic test files, such as the value set, comparison parameters, delay time, inputting the step No.(50 steps at most). 4 data of automatic test files can be edited.

How to edit automatic test files

17) When the VFD display shows the item Menu Config, press the key ▼to select the item Atest Set, followed by pressing the key Enter to confirm.

18) When the VFD display shows the item Load Atest File, press the key ▼ twice and choose the item

Edit List File, followed by pressing the key

19) When the VFD display shows the item Edit Atest File 1, press the keys ▲ and ▼ and select the sequential number that need to be edited, followed by pressing the key Enter to confirm.

20) When the VFD display shows Atestx Steps= xxx, press the numeric keys to set the steps of list files, followed by pressing the key Enter to confirm.

21) When the VFD display shows Step 1 Test Curr, press the keys ▲ and ▼ to select one of the following testing types:

a) Curr means to test the current.

b) Volt means to test the output voltage.

c) DVM means to test voltmeter input.

22) When the VFD display shows Step 1 Time=xxxxx S, press the numeric keys to set the test delay

time, followed by pressing the key Enter to confirm. Please note that the delay time range is 0.2S-25.5S. If delay time is set as 25.5S, the power supply will be automatically in the pause mode and go on testing only when another trigger is input.

23) When the VFD display shows Step 1 Volt= xxxxx V, press the numeric keys to set the voltage,

followed by pressing the key Enter to confirm.

24) When the VFD display shows Step 1 Curr= xxxxx A, press the numeric keys to set the current, followed by pressing the key Enter to confirm.

25) When the VFD display shows Step 1 Max=xxxx X, press the numeric keys to set the maximum value for the qualified test, followed by pressing the key Enter to confirm.

26) When the VFD display shows Step 1 Min= xxxxx X, press the numeric keys to set the minimum value for the qualified test, followed by pressing the key Enter to confirm.

27) Repeat from step 21 to step 26 to set all the other steps.

28) Repeat from step 18 to step 27 to edit other automatic test files according to your need.

29) Press the key ▼ to select the item Load Atest File, followed by pressing the key Enter to confirm.

30) Then the electronic load enter into the automatic test mode, showing AUTO1 at the right upper corner of the VFD display and OFF at the right lower corner of the VFD display. That means to wait for a trigger.

31) There are three trigger modes for automatic test

d) Press the key On/Off to trigger.

e) Press the key Shift+3 to trigger.

f) Become the high voltage level to the low voltage level by  Trig\_in at the rear panel and last more than 5mS.

32) When the power supply is having the automatic test, the word Wait will be showed at the right lower corner of the VFD display. When in pause mode, the word Stay will be showed at the right lower corner of the VFD display. When one test is finished, the test result Pass or Fail will be showed at the right lower corner of the VFD display.

33) When one test is finished, users can press the up-down keys to make one single step worked manually. At the moment, step n will be showed on the VFD display, indicating the step which is working; then Han is showed, indicating that the load is in hand-operated single step test state, followed by the test results being showed at the right lower corner of the VFD display.

### 4.7 Output On-Off Operation

When in front-panel operation, you can press the key On/Off to control the output on-off state. When in remote control, you can send SCPI order (Output: ON | OFF) to control the output state. The current setting values will not be changed because of the output on-off operation.

## 4.8 Remote Measurement Function

When the load consumes high current, the power supply will produce voltage drop in the connecting wire between power supply and load terminals. In order to guarantee the measurement

accuracy, remote measurement terminals is installed at the rear-panel of the power supply. Users can measure the output terminals voltage of the instrument under test by these terminals.

Before performing the remote measurement function, you need to set the power supply as the remote measurement mode.

Please refer to the picture 4.4 for the trigger terminals and measurement terminals.


<!-- img: https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//236168b5-e0f3-4be6-ba2f-d43a5d46d509/markdown_4/imgs/img_in_image_box_482_289_789_424.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-05T06%3A49%3A40Z%2F-1%2F%2Fc8d5fa0a0de44922cd2376109f5e9c02773770f0030f22b8c8b0c49d62e97360 -->


> -S +S TRQ TRI GND


> Picture4.4 Remote Measurement Terminals


-S and +S are remote measurement terminals; TRQ and TRI are trigger terminals, the last two terminals are ground terminals.

The output of power supply will be turned off when testing out the change in voltage level from high to low from TRQ port which is under the latched mode of the remote control function. As a multifunction extended port, TRI port is designed for future expanded.

## 4.9 Milliohmmeter Function

The power supply provides the four-line electrical resistance measurement, just as showed in following picture 4.5, which can measure accurately the low resistance and the maximum measurement resistance is  10Ω. In order to avoid the damage of the resistance under test, please make sure the resistance under test is within the measurement range.

Three measurement ranges can be optional: 0.1W, 1W, 10W.

##### Operation Method

1) Press the keys and V/mΩ (VFD display screen shows---, --mΩ, Range:0.1W) to measure the resistance.

2) Press the keys  Shift and 0.1W or 1W or 10W to set different measurement range of Milliohmmeter.


<!-- img: https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//6ae05176-0738-47ec-a2e3-2b29e1e3e3e0/markdown_0/imgs/img_in_image_box_437_109_829_453.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-05T06%3A50%3A44Z%2F-1%2F%2Fc3f741c86537ecbc7ac7d60bf7530cf2f5fa7111ae4d91f29f5cc1ded9d6e78d -->


> Picture 4.5 Milliohmmeter Measuring Electrical Resistance


## 4.10 Voltmeter Function

The voltage of the instrument under test can be measured if the wires are connected together just as the picture 4.6.


<!-- img: https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//6ae05176-0738-47ec-a2e3-2b29e1e3e3e0/markdown_0/imgs/img_in_image_box_358_688_738_1047.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-05T06%3A50%3A44Z%2F-1%2F%2F6ae935aeec3c03c85baed79f185434c607d516b87e46640a64d78e93e4d89faa -->


> Picture 4.6 Measuring the Voltage of the Instrument under Test


> **Note**: The remote control chapters (Chapter 5 and Chapter 6) have been extracted into a standalone reference document:
> [`m8812_remote_control_reference.md`](m8812_remote_control_reference.md)
> — this file contains the cleaned SCPI command set, communication parameters, LIST mode details, and M8812-specific specifications.

### Chapter 5 Remote Operation Mode

The DB9 interface connector on the rear panel of the power supply can be transferred to RS232 interface through the voltage level shift cable (M-131), the following information will tell you how to use the computer to control the output of the power supply. Before carrying out the remote operation mode, please use the voltage level shift cable (M-131) provided by our company, for M-131 can not only transform TTL voltage level into RS232 signal, but also connect the DB9 interface connector with computer's serial interface.

## 5.1 M-131 Communication Cable

The DB9 interface connector on the rear panel of power supply is TTL voltage level; you can use the communication cable (M-131) to connect the DB9 interface connector of the power supply and the RS-232 interface connector of computer for the communication. Please refer to the following picture for M-131.


<!-- img: https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//6ae05176-0738-47ec-a2e3-2b29e1e3e3e0/markdown_1/imgs/img_in_image_box_261_655_964_1003.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-05T06%3A50%3A45Z%2F-1%2F%2Fa881708bb4bdcdf0edc3cd6ec482f7d2a072d5d653d2c2e629c8b412fe247475 -->


> Picture 5.1 M-131


Note: It will not work if you connect the DB9 interface connector of the power supply to the RS232 interface connector of computer directly by a standard RS232 cable. Please use IT-E131 to connect them.

### 5.2 Communication between Power Supply and PC

The DB9 interface connector on the rear panel of the power supply can be transferred to RS232 interface through the voltage level shift cable (M-131). The following instructions can help you understand how to control the output of power supply by PC.

# 1. Communication Setting

Before using the remote operation mode, please make sure that the baudrate and communication address in power supply are the same as that in the computer software; otherwise, the communication will fail. You can change the baud rate and communication address from the front panel or from computer.

(1) Baud rate: 9600(4800, 9600, 19200, 38400, which are selectable from the menu on the front-panel.)

(2) Data bit: 8

(3) Stop bit: 1

(4) Parity: (none, even, odd)

# 2. DB9 Serial Interface

<!-- img: https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//6ae05176-0738-47ec-a2e3-2b29e1e3e3e0/markdown_2/imgs/img_in_image_box_488_479_781_643.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-05T06%3A50%3A46Z%2F-1%2F%2F5b24ff50880d16c79a35d935ba4541d65af0733db3a50830df49eaeac371b237 -->


> DB9 Serial Interface


The output of DB9 interface on the rear-panel of the power supply is TTL voltage level, so the voltage level shift cable (M-131) must be applied before connecting the DB9 interface with the serial interface on PC.


<!-- img: https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//6ae05176-0738-47ec-a2e3-2b29e1e3e3e0/markdown_2/imgs/img_in_chart_box_276_916_978_1310.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-05T06%3A50%3A47Z%2F-1%2F%2Fd6391c47eace5b3db838ea775abcca6faa54cb835d87dfa2fdf9d6038d25538a -->


Note: It will not work if you connect the DB9 interface connector of the power supply to the RS232 interface connector of computer directly by a standard RS232 cable. Please use IT-E131 to connect them.

### Chapter 6 SCPI Communication Protocol

## 6.1 Communication command introduction

M88 series programmable DC power supply supports SCPI communication protocol.

SCPI communication protocol is a standard commands for programmable instruments, which defines a standard set of communication commands. Command for M88 series power supply can be divided into two categories: basic commands (IEEE-488.2 Common Command Set) and command specified by M8800 software. SCPI is case-insensitive, namely commands are not case sensitive and each command has an EOI end: Instruction end <LF> (that ASCII code character 'n', that is, line breaks decimal ASCII code 10, hex 0x0A).

Note: All of the following commands to send back all require add <LF>. The following statement appears in [:] represents a semi-colon (:), the character can be written in commands, or you can not write.

### 6.2 Commonly used relevant command description

#### 6.2.1 Basic command (IEEE-488.2 Common Command Set)

Basic command includes all IEEE-488.2 includes all general functions in IEEE-488.2. These functions are usually applied to support the IEEE488.2 standard measuring instruments. The group command begin at an asterisk (*)and has no hierarchy.

##### Command * IDN?

Command * IDN is used to read the relative information of power supply, including manufacture, product model number, serial number, and version number.

Return command: return parameter contains four fields separated by comma(.)

##### Example:

If you sending command * IDN?, the corresponding expressed as a hexadecimal is 0x2A 0x49 0x44 0x4E 0x3F 0x0A. The return command is MAYNUO, M8812, 881201096006118000, V1.0

It means:

MAYNUO

Manufacturer

M8812

product model number

881201096006118000

product serial number

V1.0

software version number

#### 6.2.2 Command specified by M8800 software

This command is ordered arrangements of tree constructs. Each command contains a number of strings (mnemonic). Layers separated by a colon (:). At the top of the command in the command tree known as the "root shell" or simply "root." Access to the next command, you must specify a path.

Ordered tree is structured as follows:


<!-- img: https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//6ae05176-0738-47ec-a2e3-2b29e1e3e3e0/markdown_4/imgs/img_in_image_box_307_353_833_531.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-05T06%3A50%3A49Z%2F-1%2F%2F8fd4b2534bc513ba7ea5e6f4854e05dbbcb70401beb7b8dd62dbf366183538a2 -->


In the above table: AA represents the root path; BB, CC, DD represents the root path of the direct sub-path. EE, FF, GG said that the path is a sub-path under the BB. HH, JJ said that the path is a sub-path under DD.

Command description:

1) [:] AA: CC means the current root path for the AA, visit the AA under the CC.

2) [:] AA: BB: FF means the current root path for the AA, visit the AA path to the root path of the sub-BB under the FF.

3) [:] AA: DD: HH means the current root path for the AA, visit the AA path to the root path of the sub-DD under the HH.

Command specified by M8800 software also can send more than one order in a message and use a semicolon (;) between each order.

##### Example:

[:]AA:BB:EE;FF;GG means access to BB under the EE, FF, GG.

[:]AA:BB:FF;:AA:DD:HH means access to BB under the FF and DD under the HH.

SCPI language mnemonic or keywords has a long format and short format. Short format is

actually short for long format. It uses the following rules:

Short format mnemonic is the first four characters of long format mnemonic. If the length of the long-form mnemonic characters less than or equal to 4, then the length of the mnemonic the same.

If the length is greater than the length of 4 mnemonic, and the fourth character is a vowel, short mnemonic will discard the fourth vowel characters and become the third characters.

Example:

short-type mnemonic of ERRor is ERR, rather than the ERRO

Some command needs parameters. Setting command, for example. There is a space (ASCII code is 32) between the command and the first parameter.

Example:

setup voltage value command [:] VOLT 12.000

There is a space (ASCII code is 32) between 12.00 and VOLT

##### System command

Commands described in this section are under the root path [:] SYSTEM commands. The following commands are used short-type mnemonic format.

Command [:] SYSTEM: ERROR?

Command [:] SYSTEM: ERRor is used to read the error code and error message.

Return command: Error Code, error message.

Example:

If you send command [:] SYST: ERR?, the return command ought to be"0, 'No Error", or "50, 'Error Para Count", or "70, 'Invalid Command'" etc..

##### Command [:] SYSTEM: REM

Command [:] SYSTEM: REM is used to set remote control mode, namely the PC control mode.

Return command: None

After sending command successfully, the power supply has been in the remote control mode when “REM” appears in the right corner front panel. Otherwise the previous operation is not valid. And please return the front panel control model by pressing Shift + the number 7 key, or sending the command [:] SYST: LOC.

Command [:] SYSTEM: LOC

Command [:] SYSTEM: LOC is used to set the front panel control mode..

Return command: None

Example:

Sending command [:] SYST: LOC in the remote control (PC) mode,

After sending successfully, the power has been in the front panel control mode when

“REM”disappears from the right corner of front panel.

Command [:] SYSTEM: SENSE <bool>

Command [:] SYSTEM: SENSE <bool> is used to check the power is under remote mode.

Return command: None

Parameters: bool-type parameters, (0-OFF, 1-ON)

Example:

Sending command [:] SYST: SENS 1, If sending successfully, the power supply is in the remote mode.

#### 6.2.3 Measurement Command

Commands described in this section are under the root path [:] MEASure commands. The following commands are used short-type mnemonic format.

Command [:] MEASure: VOLTage?

Command [:] MEASure: VOLTage ? is used to read output voltage value.

Return command: Numeric

Return parameters Unit: V

Example:

Sending command [:] MEAS: VOLT?, if return command is 12.560, it means the current output voltage is 12.560V.

Command [:] MEASure: CURRent?

Command [:] MEASure: CURRent ? is used to read output current value.

Return command: Numeric

Return parameters Unit: A

Example:

Sending command [:] MEAS: CURR?, if return command is 1.245, it means the present output current is 1.245A.

Command [:] MEASure: DVM?

Command [:] MEASure: DVM? is used to read input voltage of the voltmeter in the power supply

Return command: Numerical

Return parameters Unit: V

Example:

If sending command [:]MEAS:DVM?, if return command is12.560, it means the current voltmeter input voltage is 12.560V.

#### 6.2.4 Setting Command

Command [:] OUTPUT <bool>

1.Command [:] OUTPUT <bool> is used to set the output ON/OFF status.

Return command: None

Parameters: bool-type parameters, (0-OFF, 1-ON)

##### Example:

If sending command [:] OUTP 1, the power supply output is ON if you set successfully.

2. Related command: to query output status of power supply.

Return command parameter: <bool>.

##### Command [:] MODE <mode>

1. Command [:] MODE <mode> is used to select the power operating mode. That is command set mode or sequential list mode, or milliohmmeter mode.

Parameter: mode has three following modes.

FIXed command set mode

LIST sequential list mode

DRM milliohmmeter mode

Return command: None

##### Example:

Sending command [:] MODE LIST, the power supply is in sequential list mode.

2. Related command: to query working mode of power supply. [:] MODE?

Return command parameter: <mode>

##### Command [:] VOLTage <Value>

1. This command is used to set the setup voltage value of power supply.

Parameters : numeric |MAX|MIN

Unit: V

Return Command : None

Example :

1) Sending command [: ]VOLT 30<LF>

means the current setup voltage value of the power supply is 30V.

2) Sending command [:]VOLT MAX

Providing the maximum output voltage is 76.000V, it means the current maximum voltage value is the maximum setup voltage, 76.000V.

Providing the return common is 76.0000, it means the maximum setup voltage is 76.0000V.

3) Sending command [:]VOLT MIN

Providing the minimum output current is 0.000V, it means the current minimum voltage value is the minimum setup voltage, 0.000V.

2. Related Command : the command is used to query the setup voltage value , the maximu setup

voltage value and the minimum setup voltage value. [:]VOLT? MAX/MIN

Return command parameters: numeric

Example:

1) If you want to query the setup voltage value, please send command [:]VOLT?,

If the return command is : 10.0000 , it means the setup voltage of power supply is 10.0000V.

2) If you want to query the maximum setup voltage value, please send command [:]VOLT? MAX,

If the return command is76.0000 , it means the maximum setup voltage value of power supply is

76.0000V.

3) If you want to query the minimum setup voltage value, please send command [:]VOLT? MIN, If the return command is :0.0000 , it means the minimum setup voltage value of the power supply is 0.0000V.

##### Command [:] CURRent<Value>

1. This command is used to set the setup current value.

Parameters : numeric |MAX|MIN

Unit: A

Return command : none

Example :

1) Sending command [:]CURR 3, it means to set the current setup current value as 3A.

2) Sending command [:]CURR MAX

Providing the maximum setup current value of the power supply is 2.0000A, it means to set the

current setup current value as the maximum current value 2.0000A.

3) Command sent [:]CURR MIN

Providing the current minimum setup current is 0.0000A, it means to set the current setup current as the minimum current 0.0000 A.

2. Related Command : the command used to query the setup current value, the maximum setup

current value and the minimum setup current value [:]CURR? MAX|MIN

Return command parameters: numerical value

Example:

1) If you want to query the setup current value, please send command [:]CURR?,

If the return command is1.0000 , it means the setup current of power supply is 10.0000A.

2) If you want to query the maximum setup current value, please send command [:]CURR? MAX.

If the return command is 2.0000, it means the maximum setup current value of power supply is 2.0000A

3) If you want to query the minimum setup current value, please send command [:]CURR? MIN, If the return command is : 0.0000 , it means the minimum setup current value of the power supply is 0.0000A.

##### Command [:] VOLTage:PROTECTION<Value>

1. This command is used to set the upper limit voltage of the power supply, namely the maximum output voltage.

Parameters : numeric |MAX|MIN

Unit: V

Return command: none

##### Example:

1) If sending command [:]VOLT:PROT 30, it means to set 30.000V as the upper limit voltage of the power supply.

2) If sending command [:]VOLT:PROT MAX, it means to set the maximum output voltage value as the upper limit voltage of the power supply.

3) If sending command [:]VOLT:PROT MIN, it means to set the minimum output voltage value as the upper limit voltage of the power supply.

2. Related command : to query the upper limit voltage

##### [:]VOLTage:PROTection? [MAX]

Return command parameters: numerical value

Example:

1 ) If sending command [:]VOLT:PROT?, If the return command is 20.000 , it means the upper limit voltage of the power supply is 20.000V.

2) If sending command [:]VOLT:PROT? MAX, If the return command is 76.000, it means the maximum setup voltage value of the power supply is 76.000V.

#### 6.2.5 List Operation (LIST) Related Command

Command [:] LIST:AREA<num>

1. This command is used to set the list location division mode.

Parameter: 1|2|4|8

1. Set 1 file of 200 list steps in the list operation;

2. Set 2 files of 100 list steps in the list operation;

4. Set 4 files of 50 list steps in the list operation;

8. Set 8 files of 25 list steps in the list operation;

##### Example :

If sending command sent [:]LIST:AREA 8, it means to set 8 files of 25 list steps.

2. Related command : query the location division mode of the list operation. [:]LIST:AREA?

Return parameters : <num>

Command [:] LIST:RCL <num>

This command is used to recall the saved files from a certain location so as to make them executed in the list.

Parameter: 1~8

Return command : none

Example:

If sendin command [:]LIST:RC /, 2, it means to recall the list file from the storage register location

2 and make the file executed.

#### Command [:LIST:COUNt <count>

1. This command is used to set the step number of list operation file.

Parameter: count range: 1 ~ 200

Return command: none

Example :

If sending command [:]LIST:COUN 20, it means the step number of the list operation file is set as 20.

2. Related command: to query the step number of the current list files

[:]LIST : COUNTt?

Return parameter : <count>

Command [:] LIST:MODE <mode>

1. This command is used to set the working mode of the list operation files.

Parameters: mode has 3 types:

CONTINUOUS: continuous mode

STEP: 1 step mode (once mode)

LOOP: loop mode

Return command: none

Example:

if sending command [:]LIST:MODE CONT, it means the working mode of list files is set as continuous mode.

2. Related command : to query the working mode of the current list files

#### [:]LIST:MODE?

Return parameters: <mode>

Command [:] LIST:VOLTage <count>,<value>

1. This command is used to set the setup voltage value of the appointed step in the list operation file.

Parameter : count is appointed step number; value is the setup voltage.

Unit: V

Return command: none

Example :

If sending command [:]LIST:VOLT 1,5, it means to set the setup voltage value of the first step of the list file as 5.000V.

2. Related command : to query the setup voltage value of the appointed step in the list file.

[:]LIST:VOLTage? <count>

Return parameters : <value>

Example :

If sending command [:]LIST:VOLT? 1, it means to query the setup voltage value of the first step in the current list file.

Command [:] LIST:CURRent<count>,<value>

1. This command is used to setup currentvalue of the appointed step in the list operation file.

Parameters: count is appointed step number; value is the setup current.

Unit : A

Return command : none

Example :

If sending command [:]LIST:CURR 1,2, it means to set the setup current value of the first step in the list file as 2.000A.

2. Related command : to query the setup current value of the appointed step in the list file

##### [:]LIST:CURRent? <count>

Return parameter : <value>

Example:

If sending command [:]LIST:CURR? 1, it means to query the setup current value of the first step in the current list file.

Command [:] LIST:WIDTH<count>,<time>

1. This command is used to set the delay time of the appointed step in the operation files.

Parameters: count is the appointed step number; time is the setup delay time.

Unit : mS

Return command : none

Example :

If sending command [:]LIST:WIDT 1,2000 ,it means to set the delay time of the first step of the list file as 2000mS.

2. Related command : to query the delay time of the appointed step in the current list files.

##### [:]LIST:WIDTH? <count>

Return parameters : <time>

#### Command [:] TRIGger

When the trigger source is set as communication command trigger mode(Comm.), this command will give out a trigger signal. Command [:] TRIGger has the same function like *TRIG.

This function can also be realized by pressing the keys Shift+3 located in the front panel.

Return command : None

Example:

Command sent [:]TRIG

##### Command [:] TRIGger:SOURce <mode>

This command is used to set the trigger mode of the power supply.

Parameter : mode has 3 types:

IMMediate: keyboard trigger mode.

EXTERNAL: external signal(TTL voltage level) mode

BUS: communication command trigger mode.

Return command : none

##### Example:

If sending command [:]TRIG:SOUR IMM, it means the trigger mode of power supply is set as keyboard trigger mode.

### Quick Reference

### Safety

Please do not install any spare or repair the instrument without permission. In order to make sure the normal work of the instrument, please have it mended in the maintenance department designated by our company.

Pease review the following safety precautions before operating our equipment.

### Safety Symbols

Please keep in mind the following items which may result in injuries on your body.

Connect it to safety earth ground using the wire recommended in the user manual.


<!-- img: https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//f84126ea-6392-422d-a9e3-ec0c36eac7c8/markdown_4/imgs/img_in_image_box_144_640_197_689.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-05T06%3A49%3A56Z%2F-1%2F%2F6cccdbed05b3155a62fcbb992be8d9b03e773b2f1fe5672f388e28e3f1650e83 -->


High voltage danger (Non-professionals are forbidden to open the instrument)


<!-- img: https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//f84126ea-6392-422d-a9e3-ec0c36eac7c8/markdown_4/imgs/img_in_image_box_142_722_198_778.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-05T06%3A49%3A56Z%2F-1%2F%2F6f210996be0d559e3a4cd500a6d9a3be9dd7dd5f00647e9019e71dc19189f153 -->


The symbol on an instrument indicates that the user should refer to the operating Instructions located in the manual.

### Certification and Warranty

M881X Series power supplies meet its published specifications at time of shipment from the factory.

### Warranty

This instrument product is warranted against defects in material and workmanship for a period of one year from date of delivery.

### Maintenance Service

This product must be returned to maintenance department designated by our company for repairing. Customer shall prepay shipping charges (and shall pay all duty and taxes) for products returned to the supplier for warranty service. Except for products returned to customer from another country, supplier shall pay for return of products to customer.

### Limitation of Warranty

The foregoing warranty shall not apply to

1. Defects resulting from improper or inadequate maintenance by the Customer.

2. Customer-supplied software or interfacing.

3. Unauthorized modification or misuse.

4. Operation outside of the environmental specifications for the product, or improper site preparation and maintenance.

5. Defects resulting from the circuit installed by clients themselves

### Attention

No inform will be given for any changes in the content of the user's guide.
