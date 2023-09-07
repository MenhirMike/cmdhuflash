# cmdhuflash
Command Line Utility for the [Low-Cost Flash HuCard from gamingenterprisesinc.com](http://www.gamingenterprisesinc.com/Flash_HuCard/). Tested with a V2.0 card, based on the Flash HuCard Protocol Specification V1.0.

![V2.0 Flash HuCard](https://github.com/MenhirMike/cmdhuflash/assets/22442377/679613e4-6750-4cd9-9376-41208dc5a49c)

# Usage

* `cmdhuflash -l`: List all COM Ports
* `cmdhuflash -f somefile.pce -p COM3`: Flash somefile.pce to COM3 for use in a North American Turbografx16
* `cmdhuflash -f somefile.pce -p COM3 -j`: Flash somefile.pce to COM3 for use in a Japanese PC Engine (This reverses the data bits when writing to the flash card, because the flash card is keyed for the US TG16)

# FAQ

## What does the -j switch actually do?

The HuCard slots in the Japanese PC Engine and North American TurboGrafx-16 are pretty much exactly the same, but have their data pins reversed. The D0 pin in the PC Engine is D7 in the TG16, D1 is D6, ..., D7 is D0. So a HuCard/TurboChip made for one region won't work in the console of the other region.

There are very simple adapters that just pass through all pins except for the data pins which are wired in reverse. There is no need for any electrical translation or regional identification.

The Flash HuCard's pin layout is meant for the North American TurboGrafx-16 - D0 on the Flash HuCard goes to D0 on the TG16, but to D7 on the PC Engine. If you want to use it in a Japanese PC Engine, you can pass -j which tells the cmdhuflash tool to reverse the bits when writing.

So if the ROM file on your hard drive has e.g., a 0xD4 byte (Bits: 11010100), the tool writes a 0x2B instead (Bits: 10110010). This then causes the PC Engine to see 0xD4 when reading from the reversed data pins on the Flash HuCard.

You need the -j for EVERY ROM that's written to the card, regardless if it's a ROM for the TG16 or PC Engine. That's because the ROM File will always have the bits in the correct order, but the Flash HuCard will always have its pins in the wrong order when used on a Japanese System. On the other hand, you should NEVER use the -j switch when using the card in a North American TurboGrafx-16.

## The tool complains about a ROM with a header?

The tool is intended for pure binary data, e.g., the output of your compiler toolchain. The expectation is that those output roms are a multiple of 256 bytes in size, to align to the size of an EPROM that would be on the HuCard. If you use a ROM that's intended for an emulator, there might be an additional header for that emulator. You will need to remove this header with an applicable tool, e.g., the original Flash_HuCard_8M tool.

## The operation times out when flashing multiple times in a row

This is a limitation of the Flash HuCard itself. It's intended to be connected, written to once, and then disconnected. That's because the Microcontroller on the card will delete the entire Flash ROM on the first write command and then just happily keep writing, but if trying to write another ROM to the card later, the Microcontroller seems to get confused. Just disconnect and reconnect the USB cable and it should be good to go again.
