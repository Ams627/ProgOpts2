using System;
using System.IO;

namespace TreeOpt
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {

            }
            catch (Exception ex)
            {
                var fullname = System.Reflection.Assembly.GetEntryAssembly().Location;
                var progname = Path.GetFileNameWithoutExtension(fullname);
                Console.Error.WriteLine($"{progname} Error: {ex.Message}");
            }

        }
    }
}
