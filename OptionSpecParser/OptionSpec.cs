public record OptionSpec(
    string LongOption,
    int MaxOccurs = 1,
    int NumberOfParams = 0,
    char ShortOption = '\0',
    object Group = null);
