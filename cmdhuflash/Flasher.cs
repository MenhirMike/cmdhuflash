using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cmdhuflash
{
    public static class Flasher
    {
        public readonly static int FlashSizeBytes = 1_048_576;

        public static bool FlashRomToCard(string romFileName, string portName, bool swapBits, Action<string> logMethod)
            => FlashRomToCard(LoadRomFile(romFileName), portName, swapBits, logMethod);

        public static bool FlashRomToCard(byte[] bytes, string portName, bool swapBits, Action<string> logMethod)
        {
            var totalBytes = bytes.Length;
            var packetCount = totalBytes / 256;
            var currentPacket = 0;


            var port = new SerialPort(portName, 256000, Parity.None, 8, StopBits.One);
            port.ReadBufferSize = 1024;
            port.WriteBufferSize = 512;
            port.WriteTimeout = 30_000;
            port.ReadTimeout = 30_000;

            try
            {
                port.Open();
                if (port.IsOpen)
                {
                    logMethod($"Opened Port {port.PortName}, writing {packetCount} packets...");

                    bool receiveError = false;
                    for (int pIx = 0; pIx < packetCount; pIx++)
                    {
                        var packet = CreateDataPacket(bytes, pIx * 256, swapBits);
                    }
                }
            }
            finally
            {
                if (port.IsOpen)
                {
                    try
                    {
                        port.Close();
                        logMethod($"Closed Port {port.PortName}");
                    }
                    catch { /* Nothing much do do here. */ }
                }
            }
            return true;
        }

        internal static byte[] CreateDataPacket(byte[] bytes, int offset, bool reverseBits)
        {
            var buffer = new byte[261];
            for (var ix = 0; ix < buffer.Length; ix++)
            {
                buffer[ix] = 0xFF;
            }

            (byte msb, byte middle, byte lsb) = AddressToBytes(offset);

            buffer[0] = 0x5A; // Command Byte
            buffer[1] = msb; // Address MSB
            buffer[2] = middle; // Address Middle
            buffer[3] = lsb; // Address LSB

            for (int i = 0; i < 256; i++)
            {
                var b = bytes[offset + i];
                if (reverseBits)
                {
                    b = SwappedBits[b];
                }
                buffer[4 + i] = b;
            }

            buffer[260] = CalculateChecksum(buffer, 1, 259);

            return buffer;
        }

        internal static byte CalculateChecksum(byte[] buffer, int offset, int count)
        {
            if (buffer == null) { throw new ArgumentNullException(nameof(buffer)); }
            if (offset < 0) { throw new ArgumentOutOfRangeException($"{nameof(offset)} must be non-negative, was {offset}"); }
            if (count <= 0) { throw new ArgumentOutOfRangeException($"{nameof(count)} must be positive, was {count}"); }
            if (offset + count > buffer.Length)
            {
                throw new ArgumentException($"Buffer Size: {buffer.Length}, Offset: {offset}, Count: {count}, Offset + Count: {offset + count} - Buffer too small!");
            }

            // Overflow is expected and intended
            unchecked
            {
                byte checksum = 0x00;
                for (int ix = offset; ix < (offset + count); ix++)
                {
                    checksum += buffer[ix];
                }
                return checksum;
            }
        }

        internal static byte[] LoadRomFile(string filename)
        {
            var bytes = File.ReadAllBytes(filename);
            if (bytes.Length > FlashSizeBytes)
            {
                throw new ArgumentException($"ROM is too large, can be at most {FlashSizeBytes} bytes, was {bytes.Length}.");
            }

            // TODO: Check for header to auto-remove it? Allow padding smaller roms to 256 byte boundary?
            if (bytes.Length % 256 != 0)
            {
                throw new ArgumentException($"ROM seems to contain a header, file size must be a multiple of 256, but was {bytes.Length} % 256 = {bytes.Length % 256}.");
            }

            return bytes;
        }

        internal static (byte msb, byte middle, byte lsb) AddressToBytes(int address)
        {
            if (address < 0 || address > FlashSizeBytes) { throw new ArgumentOutOfRangeException($"{nameof(address)} must be be between 0 and {FlashSizeBytes}, was {address}"); }

            byte msb = 0x00;
            byte middle = 0x00;
            byte lsb = 0x00;

            // This is pretty inefficient, but it saves me the headache of dealing with endianness and all that fun stuff.
            // It's fast enough for my purposes.
            while (address > 0)
            {
                lsb++;
                if (lsb == 0x00)
                {
                    middle++;
                    if (middle == 0x00)
                    {
                        msb++;
                    }
                }
                address--;
            }

            return (msb, middle, lsb);
        }

        /// <summary>
        /// The Flash HuCard is keyed for the North American Turbografx-16.
        /// 
        /// The TG16 swaps the data bits compared to the Japanese PC Engine (D0 => D7, D1 => D6, ... D7 => D0)
        /// ROM Files will have the bits in the same order either way, but when writing a ROM (regardless which region)
        /// to the flash card, the data bits needs to be flipped in order to make it work on a Japanese PC Engine.
        /// </summary>
        internal static byte SwapDataBits(byte input) => SwappedBits[input];

        // Using a Lookup table because bit twiddling gives me a headache
        private static byte[] SwappedBits =
        {
            0x00, 0x80, 0x40, 0xC0, 0x20, 0xA0, 0x60, 0xE0, 0x10, 0x90, 0x50, 0xD0, 0x30, 0xB0, 0x70, 0xF0,
            0x08, 0x88, 0x48, 0xC8, 0x28, 0xA8, 0x68, 0xE8, 0x18, 0x98, 0x58, 0xD8, 0x38, 0xB8, 0x78, 0xF8,
            0x04, 0x84, 0x44, 0xC4, 0x24, 0xA4, 0x64, 0xE4, 0x14, 0x94, 0x54, 0xD4, 0x34, 0xB4, 0x74, 0xF4,
            0x0C, 0x8C, 0x4C, 0xCC, 0x2C, 0xAC, 0x6C, 0xEC, 0x1C, 0x9C, 0x5C, 0xDC, 0x3C, 0xBC, 0x7C, 0xFC,
            0x02, 0x82, 0x42, 0xC2, 0x22, 0xA2, 0x62, 0xE2, 0x12, 0x92, 0x52, 0xD2, 0x32, 0xB2, 0x72, 0xF2,
            0x0A, 0x8A, 0x4A, 0xCA, 0x2A, 0xAA, 0x6A, 0xEA, 0x1A, 0x9A, 0x5A, 0xDA, 0x3A, 0xBA, 0x7A, 0xFA,
            0x06, 0x86, 0x46, 0xC6, 0x26, 0xA6, 0x66, 0xE6, 0x16, 0x96, 0x56, 0xD6, 0x36, 0xB6, 0x76, 0xF6,
            0x0E, 0x8E, 0x4E, 0xCE, 0x2E, 0xAE, 0x6E, 0xEE, 0x1E, 0x9E, 0x5E, 0xDE, 0x3E, 0xBE, 0x7E, 0xFE,
            0x01, 0x81, 0x41, 0xC1, 0x21, 0xA1, 0x61, 0xE1, 0x11, 0x91, 0x51, 0xD1, 0x31, 0xB1, 0x71, 0xF1,
            0x09, 0x89, 0x49, 0xC9, 0x29, 0xA9, 0x69, 0xE9, 0x19, 0x99, 0x59, 0xD9, 0x39, 0xB9, 0x79, 0xF9,
            0x05, 0x85, 0x45, 0xC5, 0x25, 0xA5, 0x65, 0xE5, 0x15, 0x95, 0x55, 0xD5, 0x35, 0xB5, 0x75, 0xF5,
            0x0D, 0x8D, 0x4D, 0xCD, 0x2D, 0xAD, 0x6D, 0xED, 0x1D, 0x9D, 0x5D, 0xDD, 0x3D, 0xBD, 0x7D, 0xFD,
            0x03, 0x83, 0x43, 0xC3, 0x23, 0xA3, 0x63, 0xE3, 0x13, 0x93, 0x53, 0xD3, 0x33, 0xB3, 0x73, 0xF3,
            0x0B, 0x8B, 0x4B, 0xCB, 0x2B, 0xAB, 0x6B, 0xEB, 0x1B, 0x9B, 0x5B, 0xDB, 0x3B, 0xBB, 0x7B, 0xFB,
            0x07, 0x87, 0x47, 0xC7, 0x27, 0xA7, 0x67, 0xE7, 0x17, 0x97, 0x57, 0xD7, 0x37, 0xB7, 0x77, 0xF7,
            0x0F, 0x8F, 0x4F, 0xCF, 0x2F, 0xAF, 0x6F, 0xEF, 0x1F, 0x9F, 0x5F, 0xDF, 0x3F, 0xBF, 0x7F, 0xFF
        };
    }
}
