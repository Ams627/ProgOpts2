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
        public enum ErrorCodes
        {
            OptionNotSpecified,
            EqualOptionNotSingleParam,
        }
        public class Option
        {
            public char ShortOption { get; set; }
            public string LongOption { get; set; }
            public int NumberOfParams { get; set; } = 0;
            public int MaxOccurs { get; set; } = 1;
            public object Group { get; set; }
        }

        public class ParsedOption
        {
            public int Position { get; set; }
            public bool IsShortOption { get; set; }
            public object Params { get; set; }
        }

        Option[] _definedOptions;
        List<(string name, int index, ErrorCodes errorCode)> _illegalOptions = new List<(string, int, ErrorCodes errorCode)>();
        HashSet<string> _allowedLongOptions;
        HashSet<char> _allowedShortOptions;
        readonly ILookup<char, Option> _shortOptions;
        readonly ILookup<string, Option> _longOptions;

        readonly Dictionary<char, List<ParsedOption>> parseOptions = new Dictionary<char, List<ParsedOption>>();

        public ProgOpts(Option[] options)
        {
            _definedOptions = options;

            _shortOptions = options.ToLookup(x => x.ShortOption);
            _longOptions = options.ToLookup(x => x.LongOption);

            var shortDups = options.GroupBy(x => x.ShortOption).Select(x => x.Count() > 1);
            var longDups = options.GroupBy(x => x.LongOption).Select(x => x.Count() > 1);

            if (shortDups.Any() || longDups.Any())
            {
                var smessages = shortDups.Select(x => $"option ={x} specified more than once.")
                    .Concat(longDups.Select(x => $"option --{x} specified more than once"));
                throw new Exception(string.Join("\r\n", smessages));
            }

            _allowedLongOptions = options.Select(x => x.LongOption).ToHashSet();
            _allowedShortOptions = options.Select(x => x.ShortOption).ToHashSet();
        }

        public void ParseOptions(string[] args)
        {
            int index = -1;
            foreach (var arg in args)
            {
                index++;

                // check for a double-dash (long) option:
                if (arg[0] == '-' && arg[1] == '-')
                {
                    if (arg.Length == 2)
                    {
                        // here we have found an end-of=options marker (just two dashes) with nothing else:
                        break;
                    }

                    // check for a double-dash option with equals in it - the value after equals is the option parameter.
                    // subsequent equals after the first ones are allowed - they become part of the option parameter:
                    var equalsPosition = arg.IndexOf('=');
                    if (equalsPosition >= 0)
                    {
                        var option = arg.Substring(0, equalsPosition);
                        if (!_longOptions[option].Any())
                        {
                            _illegalOptions.Add((option, index, ));
                            continue;
                        }

                        // only a single parameter is allowed after the equals:
                        if (_longOptions[option].First().NumberOfParams != 1)
                        {
                            _illegalOptions.Add((option, index, ErrorCodes.EqualOptionNotSingleParam));
                        }
                    }
                }
                else if (arg[0] == '-')
                {
                    // this means stdin and we don't know what to do yet here
                    if (arg.Length == 1)
                    {
                        throw new NotImplementedException("not implemented");
                    }
                    else
                    {
                        int i = 1;

                        char c;
                        while (char.IsLetterOrDigit(c = arg[i++]))
                        {
                            if (_allowedShortOptions.Contains(c))
                            {

                            }
                        }
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
