using System.CommandLine;
using System.IO.Ports;

namespace cmdhuflash
{
    internal class Program
    {
        static int Main(string[] args)
        {
            var rootCommand = new RootCommand("Flash HuCard flasher utility")
            {
                CreateListSerialPortsCommand(),
                CreateFlashCommand()
            };

            return rootCommand.Invoke(args);
        }

        private static Command CreateListSerialPortsCommand()
        {
            var listSerialPorts = new Command("-l", "List All Serial Ports");
            listSerialPorts.SetHandler(() =>
            {
                Console.WriteLine($"Serial Ports: {string.Join(",", SerialPort.GetPortNames().OrderBy(p => p))}");
            });
            return listSerialPorts;
        }

        private static Command CreateFlashCommand()
        {
            var flashCommand = new Command("-f", "Flash selected ROM to the card");
            var fileArgument = new Argument<FileInfo>("The .pce filename to flash") { Arity = ArgumentArity.ExactlyOne };
            flashCommand.AddArgument(fileArgument);

            var serialPortOption = new Option<string>("-p", "Serial Port Name, e.g., COM3");
            serialPortOption.IsRequired = true;
            flashCommand.Add(serialPortOption);

            var forJapanOption = new Option<bool>("-j", "Flash for use in a Japanese PC Engine");
            forJapanOption.SetDefaultValue(false);
            forJapanOption.IsRequired = false;

            flashCommand.Add(forJapanOption);

            flashCommand.SetHandler(HandleFlashCommand, fileArgument, serialPortOption, forJapanOption);

            return flashCommand;
        }

        private static void HandleFlashCommand(FileInfo file, string serialPortName, bool forJapan)
        {
            var targetConsole = forJapan ? "Japanese PC Engine" : "NA TurboGrafx-16";
            Console.WriteLine($"Flashing {file.Name} to Flash HuCard connected to {serialPortName}, for use in a {targetConsole}...");

            bool success = false;
            try
            {
                success = Flasher.FlashRomToCard(file.FullName, serialPortName, forJapan, Console.WriteLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message} [{ex.GetType().Name}]");
            }
            finally
            {
                if (success)
                {
                    Console.WriteLine("SUCCESS! You can now remove the Flash HuCard.");
                }
                else
                {
                    Console.WriteLine("The flash process has failed.");
                }
            }
        }
    }
}