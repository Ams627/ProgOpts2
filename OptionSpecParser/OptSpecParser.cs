using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;


internal class OptSpecParser
{
    private static StringReader _reader;
    private readonly Regex groupRegex = new Regex(@".group (?'GroupName'\w+)(\s*)?(#.+)?");
    private readonly Regex optionRegex = new Regex(@"
                                    (
                                        (?'SingleOption'-\w)|(?'DoubleOption'--[\w-]+)) # first option (single or double-dashed)
                                    (   
                                        ,? #optional comma between options
                                        \s*((?'SingleOption'-\w)|(?'DoubleOption'--[\w-]+)))* # zero or more further options
                                    (.*(?'Comment'\#.+))? # any remaining characters (will be printed) followed by an optional 
                                                          # comment at the end of the line
                                  ",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

    private readonly Regex commentRegex = new Regex(@"\#\s*(?'Pair'\w+=\w+)\s*(,\s*(?'Pair'\w+=\w+))*",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

    private readonly Dictionary<string, OptionsWithHelp> _options = new();

    private OptSpecParser()
    {
    }

    public bool IsHelpRequest => false;
    internal OptSpecParser Create(string helpText, string[] args)
    {
        using (_reader = new StringReader(helpText))
        {
            string line;
            while (true)
            {
                line = _reader.ReadLine()?.Trim();
                if (line is null) break;
                if (line[0] == '=')
                {
                    ProcessBody(line);
                }
                else if (line.StartsWith(".group"))
                {
                    ProcessGroup(line);
                }
            }
            var instance = new OptSpecParser();
            return instance;
        }
    }

    private string ProcessGroup(string line)
    {
        var groupMatch = groupRegex.Match(line);
        if (!groupMatch.Success)
        {
            throw new ParseException("The line starting with .group has a syntax error.");
        }

        var optionList = new List<OptionSpec>();

        while (true)
        {
            line = _reader.ReadLine()?.Trim();
            if (line is null) throw new ParseException("End of group not detected.");
            if (line[0] == '#')
            {
                continue;
            }
            if (line[0] == '=')
            {
                return line;
            }
            if (line.StartsWith(".group"))
            {
                return line;
            }
            var optionMatch = optionRegex.Match(line);
            if (optionMatch.Success)
            {
                var singleOptions = optionMatch.Groups["SingleOption"].Captures.Cast<Capture>().Select(c => c.Value).ToList();
                var doubleOptions = optionMatch.Groups["DoubleOption"].Captures.Cast<Capture>().Select(c => c.Value).ToList();
                var comment = optionMatch.Groups["Comment"].Value;
                GetOptionProperties(comment, out int maxOccurs, out int parameters, out Type type);


            }

        }
    }

    private void GetOptionProperties(string comment, out int maxOccurs, out int parameters, out Type type)
    {
        maxOccurs = 1; parameters = 0; type = typeof(string);
        if (string.IsNullOrEmpty(comment)) return;
        var commentMatch = commentRegex.Match(comment);
        if (!commentMatch.Success) return;
        var pairs = commentMatch.Groups["Pair"].Captures.Cast<Capture>()
            .Select(c => c.Value)
            .Select(x => x.Split('=')).Select(y => new { Name = y[0], Value = y[1] })
            .ToLookup(x => x.Name, x => x.Value);

        if (pairs[Type])



    }

    private void ProcessBody(string line)
    {
        throw new NotImplementedException();
    }
}
