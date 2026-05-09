using HrSystem.Shared.Common;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Resources;

namespace HrSystem.API.Common.Localization;

public interface IApiMessageLocalizer
{
    string? Localize(string? message);
    string[]? LocalizeMany(string[]? messages);
    void LocalizeObject(object? value);
}

public sealed class ApiMessageLocalizer : IApiMessageLocalizer
{
    private static readonly ResourceManager ResourceManager =
        new("HrSystem.API.Resources.ApiMessages", typeof(ApiMessageLocalizer).Assembly);

    private static readonly Lazy<Dictionary<string, string>> ArabicToEnglishMap = new(BuildArabicToEnglishMap);

    public string? Localize(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return message;
        }

        var language = LanguageDefaults.NormalizeOrDefault(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
        var text = message.Trim();

        return language == LanguageDefaults.Arabic ? TranslateToArabic(text) : TranslateToEnglish(text);
    }

    public string[]? LocalizeMany(string[]? messages)
    {
        if (messages is null || messages.Length == 0)
        {
            return messages;
        }

        return messages.Select(m => Localize(m) ?? m).ToArray();
    }

    public void LocalizeObject(object? value)
    {
        if (value is null)
        {
            return;
        }

        switch (value)
        {
            case GenericResponse response:
                response.Message = Localize(response.Message);
                response.Errors = LocalizeMany(response.Errors);
                return;
            case ProblemDetails details:
                details.Title = Localize(details.Title);
                details.Detail = Localize(details.Detail);
                if (details is ValidationProblemDetails validation)
                {
                    foreach (var key in validation.Errors.Keys.ToList())
                    {
                        validation.Errors[key] = LocalizeMany(validation.Errors[key]) ?? validation.Errors[key];
                    }
                }
                return;
        }

        // GenericResponse<T> and similar response wrappers.
        var type = value.GetType();
        var messageProp = type.GetProperty("Message");
        if (messageProp?.CanRead == true && messageProp.CanWrite && messageProp.PropertyType == typeof(string))
        {
            var currentMessage = messageProp.GetValue(value) as string;
            messageProp.SetValue(value, Localize(currentMessage));
        }

        var errorsProp = type.GetProperty("Errors");
        if (errorsProp?.CanRead == true && errorsProp.CanWrite && errorsProp.PropertyType == typeof(string[]))
        {
            var currentErrors = errorsProp.GetValue(value) as string[];
            errorsProp.SetValue(value, LocalizeMany(currentErrors));
        }
    }

    private static string TranslateToArabic(string text)
    {
        var translated = ResourceManager.GetString(text, CultureInfo.GetCultureInfo(LanguageDefaults.Arabic));
        if (!string.IsNullOrWhiteSpace(translated))
        {
            return translated;
        }

        if (text.StartsWith("Branches updated:", StringComparison.OrdinalIgnoreCase))
        {
            return text.Replace("Branches updated:", "تم تحديث الفروع:", StringComparison.OrdinalIgnoreCase);
        }

        return text;
    }

    private static string TranslateToEnglish(string text)
    {
        var translated = ResourceManager.GetString(text, CultureInfo.GetCultureInfo(LanguageDefaults.English));
        if (!string.IsNullOrWhiteSpace(translated))
        {
            return translated;
        }

        if (ArabicToEnglishMap.Value.TryGetValue(text, out translated))
        {
            return translated;
        }

        if (text.StartsWith("تم تحديث الفروع:", StringComparison.OrdinalIgnoreCase))
        {
            return text.Replace("تم تحديث الفروع:", "Branches updated:", StringComparison.OrdinalIgnoreCase);
        }

        return text;
    }

    private static Dictionary<string, string> BuildArabicToEnglishMap()
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var arabicCulture = CultureInfo.GetCultureInfo(LanguageDefaults.Arabic);
        var set = ResourceManager.GetResourceSet(arabicCulture, createIfNotExists: true, tryParents: true);

        if (set is null)
        {
            return map;
        }

        foreach (System.Collections.DictionaryEntry entry in set)
        {
            var key = entry.Key as string;
            var value = entry.Value as string;

            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            if (!map.ContainsKey(value))
            {
                map[value] = key;
            }
        }

        return map;
    }
}