# cmdhuflash
Command Line Utility for the [Low-Cost Flash HuCard from gamingenterprisesinc.com](http://www.gamingenterprisesinc.com/Flash_HuCard/). Tested with a V2.0 card, based on the Flash HuCard Protocol Specification V1.0.

![V2.0 Flash HuCard](https://github.com/MenhirMike/cmdhuflash/assets/22442377/7f9f2605-2a48-44a8-9e25-3903e66a3c9b)

# Usage

* `cmdhuflash -l`: List all COM Ports
* `cmdhuflash -i somefile.pce -o COM3`: Flash somefile.pce to COM3 for use in a North American Turbografx16
* `cmdhuflash -i somefile.pce -o COM3 -j`: Flash somefile.pce to COM3 for use in a Japanese PC Engine (This reverses the data bits when writing to the flash card, because the flash card is keyed for the US TG16)
