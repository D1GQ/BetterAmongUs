using BetterAmongUs.Data.Config;
using BetterAmongUs.Generated;
using BetterAmongUs.Utilities;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BetterAmongUs.Modules;

/// <summary>
/// Provides translation services for BetterAmongUs, supporting multiple languages and fallback mechanisms.
/// </summary>
internal static class Translator
{
    internal static Dictionary<string, int> TranslateIdLookup = [];
    internal static Dictionary<string, Dictionary<int, string>> TranslateMaps = [];
    private const string ResourcePath = "BetterAmongUs.Resources.Lang";

    /// <summary>
    /// Initializes the translator by loading all language files from embedded resources.
    /// </summary>
    internal static void Initialize()
    {
        Logger_.Log("Loading language files...", "Translator");
        LoadLanguages();
        Logger_.Log("Language files loaded successfully", "Translator");
    }

    /// <summary>
    /// Loads all language JSON files from the assembly's embedded resources.
    /// </summary>
    private static void LoadLanguages()
    {
        try
        {
            var assembly = ModInfo.Assembly;
            var jsonFileNames = GetJsonResourceNames(assembly);

            TranslateMaps = [];

            if (jsonFileNames.Length == 0)
            {
                Logger_.Error("JSON translation files do not exist.", "Translator");
                return;
            }

            foreach (var jsonFileName in jsonFileNames)
            {
                LoadLanguageFile(assembly, jsonFileName);
            }

            TranslateIdLookup = TranslateIdLookup.OrderBy(kvp => kvp.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
        catch (Exception ex)
        {
            Logger_.Error($"Error loading languages: {ex}", "Translator");
        }
    }

    /// <summary>
    /// Gets the names of all JSON translation files in the assembly resources.
    /// </summary>
    /// <param name="assembly">The assembly to search for resources.</param>
    /// <returns>Array of JSON resource file names.</returns>
    private static string[] GetJsonResourceNames(System.Reflection.Assembly assembly)
    {
        return assembly.GetManifestResourceNames()
            .Where(resourceName => resourceName.StartsWith(ResourcePath) && resourceName.EndsWith(".json"))
            .ToArray();
    }

    /// <summary>
    /// Loads a single language file from embedded resources.
    /// </summary>
    /// <param name="assembly">The assembly containing the resource.</param>
    /// <param name="resourceName">The name of the resource file.</param>
    private static void LoadLanguageFile(System.Reflection.Assembly assembly, string resourceName)
    {
        try
        {
            using var resourceStream = assembly.GetManifestResourceStream(resourceName);
            if (resourceStream == null)
                return;

            using var reader = new StreamReader(resourceStream);
            var jsonContent = reader.ReadToEnd();
            var jsonDictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);

            if (jsonDictionary == null)
            {
                Logger_.Error($"Failed to deserialize JSON from {resourceName}", "Translator");
                return;
            }

            if (jsonDictionary.TryGetValue("LanguageID", out var languageIdStr) &&
                int.TryParse(languageIdStr, out var languageId))
            {
                jsonDictionary.Remove("LanguageID");
                var name = resourceName[(ResourcePath.Length + 1)..^5]; // remove path from name
                TranslateIdLookup[name] = languageId;
                MergeTranslations(TranslateMaps, languageId, jsonDictionary);
            }
            else
            {
                Logger_.Error($"Invalid JSON format in {resourceName}: Missing or invalid 'LanguageID' field.", "Translator");
            }
        }
        catch (Exception ex)
        {
            Logger_.Error($"Error loading language file {resourceName}: {ex}", "Translator");
        }
    }

    /// <summary>
    /// Merges translations from a language file into the main translation maps.
    /// </summary>
    /// <param name="translationMaps">The main translation dictionary to merge into.</param>
    /// <param name="languageId">The language ID for the translations.</param>
    /// <param name="translations">The translations to merge.</param>
    private static void MergeTranslations(
        Dictionary<string, Dictionary<int, string>> translationMaps,
        int languageId,
        Dictionary<string, string> translations)
    {
        foreach (var (key, value) in translations)
        {
            if (!translationMaps.ContainsKey(key))
            {
                translationMaps[key] = [];
            }

            // Replace escape sequences with actual characters
            var processedValue = value.Replace("\\n", "\n").Replace("\\r", "\r");
            translationMaps[key][languageId] = processedValue;
        }
    }

    /// <summary>
    /// Gets the language ID by its name.
    /// </summary>
    /// <param name="name">The name of the language.</param>
    /// <returns>The language ID, or -1 if not found.</returns>
    internal static int GetLanguageIdByName(string name)
    {
        if (TranslateIdLookup.TryGetValue(name, out var id))
        {
            return id;
        }

        return -1;
    }

    /// <summary>
    /// Gets a translated string for a specific language.
    /// </summary>
    /// <param name="translationString">The translation key.</param>
    /// <param name="languageId">The language to use.</param>
    /// <param name="showInvalid">Whether to show invalid key indicators.</param>
    /// <returns>The translated string.</returns>
    internal static string GetString(TranslationStrings.TranslationString translationString, SupportedLangs languageId, bool showInvalid = true)
    {
        var fallbackText = showInvalid ? $"<INVALID:{translationString.Key}>" : translationString.Key;

        try
        {
            // Try to get from custom translations
            if (TranslateMaps.TryGetValue(translationString.Key, out var languageMap))
            {
                var result = GetTranslationFromMap(translationString, languageId, languageMap, showInvalid);
                if (result != null) return result;
            }

            // Fallback to vanilla string names
            return GetVanillaStringFallback(translationString, fallbackText);
        }
        catch (Exception ex)
        {
            Logger_.Error($"Error retrieving string [{translationString}]: {ex}", "Translator");
            return fallbackText;
        }
    }

    /// <summary>
    /// Retrieves a localized string corresponding to the specified key, with optional formatting and retrieval options.
    /// </summary>
    /// <param name="translationString">The key that identifies the string resource to retrieve.</param>
    /// <param name="formatting">An array of objects to format the retrieved string with, or null to return the string without formatting.</param>
    /// <param name="useConsoleLanguage">true to force retrieval in English for console output; otherwise, false.</param>
    /// <param name="showInvalid">true to return a placeholder for invalid or missing keys; otherwise, false to return the key itself.</param>
    /// <param name="vanilla">true to retrieve the string using the default (vanilla) translation set; otherwise, false to use the current or
    /// specified language.</param>
    /// <returns>The localized string corresponding to the specified key, formatted if formatting is provided. Returns a
    /// placeholder or the key itself if the key is invalid, depending on the showInvalid parameter.</returns>
    internal static string GetString(TranslationStrings.TranslationString translationString, string[]? formatting = null, bool useConsoleLanguage = false, bool showInvalid = true, bool vanilla = false)
    {
        if (vanilla)
        {
            string nameToFind = translationString.Key;
            if (Enum.TryParse(nameToFind, out StringNames text))
            {
                return TranslationController.Instance.GetString(text);
            }
            else
            {
                return showInvalid ? $"<INVALID:{nameToFind}> (vanillaStr)" : nameToFind;
            }
        }
        var langId = TranslationController.InstanceExists ? TranslationController.Instance.currentLanguage.languageID : SupportedLangs.English;
        if (useConsoleLanguage) langId = SupportedLangs.English;
        if (BAUConfigs.ForceOwnLanguage.Value) langId = GetUserSystemLanguage();
        string str = GetString(translationString, langId, showInvalid);
        if (formatting != null)
            str = string.Format(str, formatting);
        return str ?? string.Empty;
    }

    /// <summary>
    /// Retrieves the localized string values corresponding to the specified keys.
    /// </summary>
    /// <param name="keys">A collection of string keys for which to retrieve localized values. Cannot be null.</param>
    /// <param name="console">true to format the returned strings for console output; otherwise, false. The default is false.</param>
    /// <param name="showInvalid">true to include a placeholder or indicator for invalid or missing keys; otherwise, false. The default is true.</param>
    /// <param name="vanilla">true to retrieve the original, unmodified string values; otherwise, false. The default is false.</param>
    /// <returns>An array of strings containing the localized values for each key in the input collection. The order of the
    /// returned array matches the order of the input keys.</returns>
    internal static string[] GetStrings(IEnumerable<TranslationStrings.TranslationString> keys, bool console = false, bool showInvalid = true, bool vanilla = false)
    {
        var results = new List<string>();
        foreach (var trans in keys)
        {
            string result = GetString(trans, useConsoleLanguage: console, showInvalid: showInvalid, vanilla: vanilla);
            results.Add(result);
        }
        return [.. results];
    }

    /// <summary>
    /// Gets a translation from the language map with Chinese character detection.
    /// </summary>
    /// <param name="key">The translation key to look up.</param>
    /// <param name="languageId">The target language ID.</param>
    /// <param name="languageMap">The language map containing translations.</param>
    /// <param name="showInvalid">Whether to show invalid key indicators.</param>
    /// <returns>The translated string, or null if not found.</returns>
    private static string GetTranslationFromMap(TranslationStrings.TranslationString key, SupportedLangs languageId, Dictionary<int, string> languageMap, bool showInvalid)
    {
        if (languageMap.TryGetValue((int)languageId, out var translation) &&
            !string.IsNullOrEmpty(translation))
        {
            // Check for Chinese characters in non-Chinese languages
            if (!IsChineseLanguage(languageId) && ContainsChineseCharacters(translation))
            {
                var chineseTranslation = GetString(key, SupportedLangs.SChinese, showInvalid);
                if (translation == chineseTranslation)
                {
                    return GetEnglishFallback(key);
                }
            }
            return translation;
        }

        // Fallback to English if translation not found
        return languageId == SupportedLangs.English ? $"*{key}" : GetString(key, SupportedLangs.English, showInvalid);
    }

    /// <summary>
    /// Fallback method to get vanilla string names.
    /// </summary>
    /// <param name="translationString">The translation key to look up.</param>
    /// <param name="fallbackText">The fallback text to return if not found.</param>
    /// <returns>The translated string or fallback text.</returns>
    private static string GetVanillaStringFallback(TranslationStrings.TranslationString translationString, string fallbackText)
    {
        var matchingStringNames = EnumUtils.GetAllValues<StringNames>()
            .Where(x => x.ToString() == translationString.Key)
            .ToArray();

        return matchingStringNames.Length > 0 ? GetString(matchingStringNames[0]) : fallbackText;
    }

    /// <summary>
    /// Gets a vanilla Among Us string by StringNames enum.
    /// </summary>
    /// <param name="stringName">The StringNames enum value.</param>
    /// <returns>The translated string.</returns>
    internal static string GetString(StringNames stringName) =>
        TranslationController.Instance.GetString(stringName, new Il2CppReferenceArray<Il2CppSystem.Object>(0));

    /// <summary>
    /// Gets the target language ID based on settings and system configuration.
    /// </summary>
    /// <param name="useConsoleLanguage">Whether to force English for console.</param>
    /// <returns>The target language ID.</returns>
    internal static SupportedLangs GetTargetLanguageId(bool useConsoleLanguage = false)
    {
        if (useConsoleLanguage) return SupportedLangs.English;
        if (BAUConfigs.ForceOwnLanguage.Value) return GetUserSystemLanguage();

        return TranslationController.InstanceExists ?
            TranslationController.Instance.currentLanguage.languageID :
            SupportedLangs.English;
    }

    /// <summary>
    /// Gets the user's system language as a SupportedLangs enum.
    /// </summary>
    /// <returns>The system language ID.</returns>
    internal static SupportedLangs GetUserSystemLanguage()
    {
        try
        {
            var cultureName = CultureInfo.CurrentUICulture.Name;

            return cultureName switch
            {
                string name when name.StartsWith("zh_CHT") => SupportedLangs.TChinese,
                string name when name.StartsWith("zh") => SupportedLangs.SChinese,
                string name when name.StartsWith("ru") => SupportedLangs.Russian,
                string name when name.StartsWith("en") => SupportedLangs.English,
                _ => TranslationController.Instance.currentLanguage.languageID
            };
        }
        catch
        {
            return SupportedLangs.English;
        }
    }

    /// <summary>
    /// Applies string replacements to a translated text.
    /// </summary>
    /// <param name="text">The text to apply replacements to.</param>
    /// <param name="replacements">Dictionary of replacement pairs.</param>
    /// <returns>The text with replacements applied.</returns>
    private static string ApplyReplacements(string text, Dictionary<string, string> replacements)
    {
        if (replacements == null) return text;

        foreach (var replacement in replacements)
        {
            text = text.Replace(replacement.Key, replacement.Value);
        }
        return text;
    }

    /// <summary>
    /// Gets an English fallback translation.
    /// </summary>
    /// <param name="key">The translation key to look up.</param>
    /// <returns>The English translation of the key.</returns>
    private static string GetEnglishFallback(TranslationStrings.TranslationString key) =>
        GetString(key, SupportedLangs.English);

    /// <summary>
    /// Checks if a language ID represents a Chinese language.
    /// </summary>
    /// <param name="languageId">The language ID to check.</param>
    /// <returns>true if the language is Chinese; otherwise, false.</returns>
    private static bool IsChineseLanguage(SupportedLangs languageId) =>
        languageId is SupportedLangs.SChinese or SupportedLangs.TChinese;

    /// <summary>
    /// Checks if a string contains Chinese characters.
    /// </summary>
    /// <param name="text">The text to check.</param>
    /// <returns>true if Chinese characters are found; otherwise, false.</returns>
    private static bool ContainsChineseCharacters(string text) =>
        Regex.IsMatch(text, @"[\u4e00-\u9fa5]");
}