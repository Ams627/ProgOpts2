using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgOpts2
{
    class ProgOpts
    {
        public class Option
        {
            public char ShortOption { get; set; }
            public string LoginOption { get; set; }
            public int NumberOfParams { get; set; } = 0;
            public object Group { get; set; }
        }

        Option[] _definedOptions;
        List<(string name, int index)> _illegalOptions = new List<(string, int)>();
        HashSet<string> _allowedLongOptions;
        HashSet<char> _allowedShortOptions;

        public ProgOpts()
        {

        }

        public void DefineOptions(Option[] options)
        {
            _definedOptions = options;

            var shortDups = options.GroupBy(x => x.ShortOption).Select(x => x.Count() > 1);
            var longDups = options.GroupBy(x => x.LoginOption).Select(x => x.Count() > 1);

            _allowedLongOptions = options.Select(x => x.LoginOption).ToHashSet();
            _allowedShortOptions = options.Select(x => x.ShortOption).ToHashSet();
        }

        public void ParseOptions(string[] args)
        {
            foreach (var arg in args)
            {
                if (arg[0] == '-' && arg[1] == '-')
                {
                    if (arg.Length == 2)
                    {
                        break;
                    }
                }
            }
        }

    }
    class Program
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
                Console.Error.WriteLine(progname + ": Error: " + ex.Message);
            }

        }
    }
}
