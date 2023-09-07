# cmdhuflash
Command Line Utility for the [Low-Cost Flash HuCard from gamingenterprisesinc.com](http://www.gamingenterprisesinc.com/Flash_HuCard/). Tested with a V2.0 card, based on the Flash HuCard Protocol Specification V1.0.

![V2.0 Flash HuCard](https://github.com/MenhirMike/cmdhuflash/assets/22442377/679613e4-6750-4cd9-9376-41208dc5a49c)

# Usage

* `cmdhuflash -l`: List all COM Ports
* `cmdhuflash -i somefile.pce -o COM3`: Flash somefile.pce to COM3 for use in a North American Turbografx16
* `cmdhuflash -i somefile.pce -o COM3 -j`: Flash somefile.pce to COM3 for use in a Japanese PC Engine (This reverses the data bits when writing to the flash card, because the flash card is keyed for the US TG16)

# FAQ

## What does the -j switch actually do?

The HuCard slots in the Japanese PC Engine and North American TurboGrafx-16 are pretty much exactly the same, but have their data pins reversed. The D0 pin in the PC Engine is D7 in the TG16, D1 is D6, ..., D7 is D0. So a HuCard/TurboChip made for one region won't work in the console of the other region.

There are very simple adapters that just pass through all pins except for the data pins, which are simply reversed.

The Flash HuCard's pin layout is meant for the North American TurboGrafx-16. If you want to use it in a Japanese PC Engine, you can pass -j which tells the cmdhuflash tool to reverse the bits when writing. So if the ROM file on your hard drive has e.g., a 0xD4 byte (Bits: 11010100), the tool writes a 0x2B instead (Bits: 10110010). This then causes the PC Engine to see 0xD4 when reading from the reversed data pins on the Flash HuCard.

You need the -j for EVERY ROM that's written to the card, regardless if it's a ROM for the TG16 or PC Engine. That's because the ROM File will always have the bits in the correct order, but the Flash HuCard will always have its pins in the wrong order when used on a Japanese System. On the other hand, you should NEVER use the -j switch when using the card in a North American Turbografx-16.
