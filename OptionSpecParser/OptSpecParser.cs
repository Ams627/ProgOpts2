using System;
using System.IO;
using System.Text.RegularExpressions;

internal class OptSpecParser
{
    private class ParseException : Exception
    {
        public ParseException(string message) : base(message)
        {
        }
    }
    private static StringReader _reader;

    private readonly Regex groupRegex = new Regex(@".group (?'GroupName'\w+)(\s*)?(#.+)?");
    internal OptSpecParser Create(string helptext)
    {
        using (_reader = new StringReader(helptext))
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
            return null;
        }
    }

    public CommandInfo GetCommandInfo(string[] args)
    {

    }

    private void ProcessGroup(string line)
    {
        var groupMatch = groupRegex.Match(line);
        if (!groupMatch.Success)
        {
            throw new ParseException("The line starting with .group has a syntax error.");
        }
    }

    private void ProcessBody(string line)
    {
        throw new NotImplementedException();
    }
}
