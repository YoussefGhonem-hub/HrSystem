namespace HrSystem.Shared.Common;

public static class LanguageDefaults
{
    public const string English = "en";
    public const string Arabic = "ar";

    private static readonly HashSet<string> _supported = new(StringComparer.OrdinalIgnoreCase)
    {
        English,
        Arabic
    };

    public static IReadOnlyCollection<string> SupportedLanguages => _supported;

    public static bool IsSupported(string? language)
    {
        if (string.IsNullOrWhiteSpace(language))
        {
            return false;
        }

        return _supported.Contains(Normalize(language));
    }

    public static string NormalizeOrDefault(string? language, string fallback = English)
    {
        var normalizedFallback = Normalize(fallback);
        if (!_supported.Contains(normalizedFallback))
        {
            normalizedFallback = English;
        }

        if (string.IsNullOrWhiteSpace(language))
        {
            return normalizedFallback;
        }

        var normalized = Normalize(language);
        return _supported.Contains(normalized) ? normalized : normalizedFallback;
    }

    private static string Normalize(string language)
    {
        var value = language.Trim();
        var separatorIndex = value.IndexOfAny(['-', '_']);

        if (separatorIndex > 0)
        {
            value = value[..separatorIndex];
        }

        return value.ToLowerInvariant();
    }
}