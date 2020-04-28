using System;
using System.IO;

namespace ProgOpts2
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                var options = new ProgOpts.OptionSpec[]
                {
                    new ProgOpts.OptionSpec { ShortOption = 'a', LongOption = "all", NumberOfParams=1 },
                    new ProgOpts.OptionSpec { ShortOption = 'b', LongOption = "ball", NumberOfParams=0 },
                    new ProgOpts.OptionSpec { ShortOption = 'p', LongOption = "print", NumberOfParams=0 },
                    new ProgOpts.OptionSpec { ShortOption = 'o', LongOption = "order", NumberOfParams=0 }
                };
                var args1 = new[] { "-bp", "--all hello said the man" };
                var optionParser = new ProgOpts(options);
                optionParser.ParseCommandLine(args1);

                var r2 = optionParser.IsOptionPresent('c');
                var r2a = optionParser.IsOptionPresent('p');
                var r2b = optionParser.IsOptionPresent('o');
                var r2c = optionParser.IsOptionPresent('b');
                var r3 = optionParser.GetParam("a");
                var r4 = optionParser.GetParam('a');
                var r5 = optionParser.GetParam("all");

                Console.WriteLine();
            }
            catch (Exception ex)
            {
                var fullname = System.Reflection.Assembly.GetEntryAssembly().Location;
                var progname = Path.GetFileNameWithoutExtension(fullname);
                Console.Error.WriteLine(progname + ": Error: " + ex.Message);
                Console.WriteLine();
            }
        }
    }
}