// Copyright (c) Adrian Sims 2020
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;

namespace CommandLineOptions
{
    public class Options
    {
        private readonly OptionSpec[] _specifiedOptions;

        private readonly IDictionary<string, OptionSpec> _longOptions;

        private readonly Dictionary<string, int> _longToInt;

        private readonly Dictionary<int, List<ParsedOption>> _parsedOptions = new Dictionary<int, List<ParsedOption>>();

        private readonly IDictionary<char, OptionSpec> _shortOptions;

        private readonly Dictionary<char, int> _shortToInt;

        private HashSet<object> _allowedGroups;
        private List<(string name, int index, ErrorCodes errorCode)> _illegalOptions = new List<(string, int, ErrorCodes errorCode)>();

        private List<(string arg, int index)> _nonOptions = new List<(string arg, int index)>();

        private PopQueue<string> _popList;
        public Options(OptionSpec[] options)
        {
            _specifiedOptions = options;

            var shortDups = options.GroupBy(x => x.ShortOption).Where(x => x.Count() > 1);
            var longDups = options.GroupBy(x => x.LongOption).Where(x => x.Count() > 1);

            if (shortDups.Any() || longDups.Any())
            {
                var smessages = shortDups.Select(x => $"option ={x} specified more than once.")
                    .Concat(longDups.Select(x => $"option --{x} specified more than once"));
                throw new ArgumentException(string.Join("\r\n", smessages), "options");
            }

            _shortToInt = options.Select((x, i) => new { x.ShortOption, Index = i }).ToDictionary(x => x.ShortOption, x => x.Index);
            _longToInt = options.Select((x, i) => new { x.LongOption, Index = i }).ToDictionary(x => x.LongOption, x => x.Index);

            _shortOptions = options.ToDictionary(x => x.ShortOption);
            _longOptions = options.ToDictionary(x => x.LongOption);
        }

        public enum ErrorCodes
        {
            /// <summary>
            /// Option not specified in the list passed to the ProgOpts constructor
            /// </summary>
            OptionNotSpecified,

            /// <summary>
            /// It's invalid to have --message="hello world" if the number of parameters specified for the option is not 1.
            /// </summary>
            EqualOptionNotSingleParam,

            /// <summary>
            /// --= is not allowed
            /// </summary>
            EqualFirstChar,

            /// <summary>
            /// we reached the end of the argument list while adding parameters for this option:
            /// </summary>
            OptionNotEnoughParams,

            /// <summary>
            /// a parameter was found immediately adjoining a single character option, but the option takes more than one parameter
            /// </summary>
            AdjoiningOptionNotSingleParam,

            /// <summary>
            /// An equals appeared in an arg which specified a valid long option but there were no more characters after the equals
            /// </summary>
            EqualOptionEmptyParameter
        }

        public object[] AllowedGroups => _allowedGroups.ToArray();
        public (string arg, int index)[] NonOptions => _nonOptions.ToArray();
        public (string arg, int index, ErrorCodes errorcode)[] IllegalOptions => _illegalOptions.ToArray();
        

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T">string or char to specify either short option name or long option name - will normally be inferred by the compiler</typeparam>
        /// <param name="optionName">either a single letter or a long option name without the leading double-dash</param>
        /// <param name="offset">where an option can occur more than once on the command line, this specifies the offset. For example, -i may occur more than one to specify several input files</param>
        /// <returns>The option parameter(s) or null if the option was not present.</returns>
        public object GetParam<T>(T optionName, int offset = 0)
        {
            int optionIndex = -1;
            bool found = false;

            if (optionName is string str)
            {
                found = _longToInt.TryGetValue(str, out optionIndex);
            }
            else if (optionName is char c)
            {
                found = _shortToInt.TryGetValue(c, out optionIndex);
            }

            if (!found)
            {
                return null;
            }

            if (!_parsedOptions.TryGetValue(optionIndex, out var optionList))
            {
                return null;
            }

            if (optionList[offset].Params is List<string> listParam)
            {
                // option takes a list of parameters
                return listParam;
            }
            else if (optionList[offset].Params is string strParam)
            {
                // option takes a single parameter
                return strParam;
            }
            return null;
        }
        public bool IsOptionPresent(char c) => _shortToInt.TryGetValue(c, out var optionIndex) && _parsedOptions.TryGetValue(optionIndex, out _);

        public bool IsOptionPresent(string s) => _longToInt.TryGetValue(s, out var optionIndex) && _parsedOptions.TryGetValue(optionIndex, out _);

        /// <summary>
        /// Get the number of occurences of a particular option specified on the command line sent to ParseOptions
        /// </summary>
        /// <typeparam name="T">should be string (double-letter option) or char (single letter option)</typeparam>
        /// <param name="optionName">a letter or a string for which to return the count</param>
        /// <returns>the number of occurrences of the option</returns>
        public int OptionCount<T>(T optionName)
        {
            int optionIndex = -1;

            if (optionName is char c)
            {
                _shortToInt.TryGetValue(c, out optionIndex);
            }
            else if (optionName is string s)
            {
                _longToInt.TryGetValue(s, out optionIndex);
            }

            if (optionIndex == -1)
            {
                return 0;
            }

            var isPresent = _parsedOptions.TryGetValue(optionIndex, out var optionList);
            if (!isPresent)
            {
                return 0;
            }
            return optionList.Count();
        }


        /// <summary>
        /// Parses a command line passed as an array and fills in internal structures. Options can then be checked via various get accessors.
        /// </summary>
        /// <param name="args">The full command line</param>
        /// <param name="offset">offset within the args array at which to start processing</param>
        /// <param name="allowedGroups">option groups to allow</param>
        public bool ParseCommandLine(string[] args, int offset = 0, object[] allowedGroups = null)
        {
            _popList = new PopQueue<string>(args, offset);
            _allowedGroups = allowedGroups == null ? new HashSet<object>() : new HashSet<object>(allowedGroups);

            while (!_popList.Empty)
            {
                var (arg, index) = _popList.PopFront();

                // check for a double-dash (long) option:
                if (arg[0] == '-' && arg[1] == '-')
                {
                    if (arg.Length == 2)
                    {
                        // here we have found an end-of=options marker (just two dashes) with nothing else:
                        break;
                    }

                    bool continueArgProcessing = ProcessDoubleDashOption(arg.Substring(2), index);
                    if (!continueArgProcessing)
                    {
                        break;
                    }
                }
                else if (arg[0] == '-')
                {
                    // this means stdin and we don't know what to do yet here
                    if (arg.Length == 1)
                    {
                        _nonOptions.Add((arg, index));
                    }
                    else
                    {
                        bool continueArgProcessing = ProcessSingleDashOptions(arg.Substring(1), index);
                        if (!continueArgProcessing)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    _nonOptions.Add((arg, index));
                }
            }
            return !_illegalOptions.Any();
        }

        public bool TryGetList(string option, out string[] list, int offset = 0)
        {
            bool result = _longToInt.TryGetValue(option, out var optionIndex);
            if (!result)
            {
                list = default;
                return false;
            }
            result = _parsedOptions.TryGetValue(optionIndex, out var optionParam);
            if (!result)
            {
                list = default;
                return false;
            }
            if (optionParam[offset].Params is List<string> strList)
            {
                list = strList.ToArray();
                return true;
            }
            list = default;
            return false;
        }

        public bool TryGetValue(string option, out string value, int offset = 0)
        {
            bool result = _longToInt.TryGetValue(option, out var optionIndex);
            if (!result)
            {
                value = default;
                return false;
            }
            result = _parsedOptions.TryGetValue(optionIndex, out var optionParam);
            if (!result)
            {
                value = default;
                return false;
            }
            if (optionParam[offset].Params is string str)
            {
                value = str;
                return true;
            }
            value = default;
            return false;
        }


        private bool ProcessDoubleDashOption(string arg, int index)
        {
            // check for a double-dash option with equals in it - the value after equals is the option parameter.
            // subsequent equals after the first ones are allowed - they become part of the option parameter:
            var equalsPosition = arg.IndexOf('=');
            if (equalsPosition == 0)
            {
                _illegalOptions.Add(("--=", index, ErrorCodes.EqualFirstChar));
            }
            else if (equalsPosition > 0)
            {
                var option = arg.Substring(0, equalsPosition);
                var found = _longOptions.TryGetValue(option, out var optionSpec);
                var optionGroupAllowed = found && (optionSpec.Group == null || _allowedGroups.Contains(optionSpec.Group));

                if (!found || !optionGroupAllowed)
                {
                    _illegalOptions.Add((option, index, ErrorCodes.OptionNotSpecified));
                    return true;
                }

                // only a single parameter is allowed after the equals:
                if (optionSpec.NumberOfParams != 1)
                {
                    _illegalOptions.Add((option, index, ErrorCodes.EqualOptionNotSingleParam));
                    return true;
                }

                var parameter = arg.Substring(equalsPosition + 1);
                if (!parameter.Any())
                {
                    _illegalOptions.Add((option, index, ErrorCodes.EqualOptionEmptyParameter));
                }

                var parsedOption = new ParsedOption { IsShortOption = false, Params = parameter, Index = index, OptionIndex = _longToInt[option] };
                DictUtils.AddEntryToList(_parsedOptions, parsedOption.OptionIndex, parsedOption);
            }
            else
            {
                // no equals in arg
                var found = _longOptions.TryGetValue(arg, out var optionSpec);

                if (!found)
                {
                    _illegalOptions.Add((arg, index, ErrorCodes.OptionNotSpecified));
                    return true;
                }

                if (optionSpec.NumberOfParams > _popList.Remaining)
                {
                    _illegalOptions.Add((arg, index, ErrorCodes.OptionNotEnoughParams));
                    // can't do anything else here as we don't have enough args to process:
                    return false;
                }

                var optionIndex = _longToInt[arg];
                var parsedOption = new ParsedOption { IsShortOption = false, Index = index, OptionIndex = optionIndex};
                if (optionSpec.NumberOfParams > 0)
                {
                    ConsumeOptionParameters(parsedOption);
                }
                DictUtils.AddEntryToList(_parsedOptions, parsedOption.OptionIndex, parsedOption);
            }
            return true;
        }

        private bool ProcessSingleDashOptions(string arg, int index)
        {
            for (int i = 0; i < arg.Length; i++)
            {
                var isLast = (i == arg.Length - 1);
                var c = arg[i];

                var found = _shortOptions.TryGetValue(c, out var optionSpec);
                var optionGroupAllowed = found && (optionSpec.Group == null || _allowedGroups.Contains(optionSpec.Group));

                if (!found || !optionGroupAllowed)
                {
                    // store illegal option, but continue processing:
                    _illegalOptions.Add(($"{c}", index, ErrorCodes.OptionNotSpecified));
                    continue;
                }

                if (isLast)
                {
                    if (optionSpec.NumberOfParams > _popList.Remaining)
                    {
                        // not enough args left as option parameters:
                        _illegalOptions.Add(($"{c}", index, ErrorCodes.OptionNotEnoughParams));

                        // can't parse anthing else here so return false:
                        return false;
                    }
                    else
                    {
                        var optionIndex = _shortToInt[c];
                        var parsedOpt = new ParsedOption { IsShortOption = false, Index = index, OptionIndex = optionIndex };
                        ConsumeOptionParameters(parsedOpt);
                        DictUtils.AddEntryToList(_parsedOptions, optionIndex, parsedOpt);
                    }
                }
                else if (optionSpec.NumberOfParams == 0)
                {
                    // it's a "boolean" option at this point (it's just a flag and has no parameters) - there may be more options
                    // in this arg as in grep -iPo pattern *.txt

                    var optionIndex = _shortToInt[c];
                    var parsedOpt = new ParsedOption { IsShortOption = true, Index = index, OptionIndex = optionIndex };
                    DictUtils.AddEntryToList(_parsedOptions, optionIndex, parsedOpt);
                }
                else if (optionSpec.NumberOfParams == 1)
                {
                    // the single parameter is the rest of the arg - store it and stop scanning this arg:
                    var optionIndex = _shortToInt[c];
                    var parsedOpt = new ParsedOption { IsShortOption = true, Index = index, Params = arg.Substring(i + 1), OptionIndex = optionIndex };
                    DictUtils.AddEntryToList(_parsedOptions, optionIndex, parsedOpt);
                    break;
                }
                else
                {
                    // you can't have an option with more than one parameter right next to the single char option:
                    _illegalOptions.Add(($"{c}", index, ErrorCodes.AdjoiningOptionNotSingleParam));

                    // can't parse anthiing else here so return:
                    return false;
                }
            }

            return true;
        }

        private void ConsumeOptionParameters(ParsedOption option)
        {
            object paramList;
            var numberOfParams = _specifiedOptions[option.OptionIndex].NumberOfParams;
            if (numberOfParams == 1)
            {
                paramList = _popList.PopFront().item;
            }
            else
            {
                var list = new List<string>();
                paramList = list;
                for (int p = 0; p < numberOfParams; p++)
                {
                    list.Add(_popList.PopFront().item);
                }
            }
            option.Params = paramList;
        }

        public class OptionSpec
        {
            public object Group { get; set; }
            public string LongOption { get; set; }
            public int MaxOccurs { get; set; } = 1;
            public int NumberOfParams { get; set; } = 0;
            public char ShortOption { get; set; } = '\0';
        }

        public class ParsedOption
        {
            public bool IsShortOption { get; set; }
            public object Params { get; set; }
            public int OptionIndex { get; set; }
            public int Index { get; set; }
        }
    }
}