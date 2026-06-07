
namespace MTGCreateYourOwnCreature.ViewModel.Helpers
{
    /// <summary>
    /// Provides extension methods for <see cref="IReadOnlyDictionary{String, String}"/> to safely parse and retrieve typed values from raw text data.
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Attempts to retrieve and parse a boolean value from the dictionary using the specified key.
        /// </summary>
        /// <param name="values">The dictionary containing the raw text data.</param>
        /// <param name="key">The key of the value to retrieve.</param>
        /// <param name="defaultValue">The fallback value to return if the key is missing or the text cannot be parsed as a boolean. Defaults to false.</param>
        /// <returns>The parsed boolean value, or the <paramref name="defaultValue"/> if the operation fails.</returns>
        public static bool GetBool(this IReadOnlyDictionary<string, string> values, string key, bool defaultValue = false)
        {
            return values.TryGetValue(key, out string? value) && bool.TryParse(value, out bool parsed) ? parsed : defaultValue;
        }

        /// <summary>
        /// Attempts to retrieve and parse an integer value from the dictionary using the specified key.
        /// </summary>
        /// <param name="values">The dictionary containing the raw text data.</param>
        /// <param name="key">The key of the value to retrieve.</param>
        /// <param name="defaultValue">The fallback value to return if the key is missing or the text cannot be parsed as an integer. Defaults to 0.</param>
        /// <returns>The parsed integer value, or the <paramref name="defaultValue"/> if the operation fails.</returns>
        public static int GetInt(this IReadOnlyDictionary<string, string> values, string key, int defaultValue = 0)
        {
            return values.TryGetValue(key, out string? value) && int.TryParse(value, out int parsed) ? parsed : defaultValue;
        }

        /// <summary>
        /// Attempts to retrieve a string value from the dictionary using the specified key.
        /// </summary>
        /// <param name="values">The dictionary containing the raw text data.</param>
        /// <param name="key">The key of the value to retrieve.</param>
        /// <param name="defaultValue">The fallback value to return if the key is missing. Defaults to an empty string.</param>
        /// <returns>The retrieved string value, or the <paramref name="defaultValue"/> if the key does not exist.</returns>
        public static string GetString(this IReadOnlyDictionary<string, string> values, string key, string defaultValue = "")
        {
            return values.TryGetValue(key, out string? value) ? value : defaultValue;
        }

        /// <summary>
        /// Attempts to retrieve a separated string value from the dictionary and splits it into an array of trimmed strings.
        /// </summary>
        /// <param name="values">The dictionary containing the raw text data.</param>
        /// <param name="key">The key of the value to retrieve.</param>
        /// <param name="separator">The character used to delimit the list items. Defaults to a comma (',').</param>
        /// <returns>An array of parsed and trimmed strings, or an empty array if the key is missing or the value is entirely whitespace.</returns>
        public static string[] GetStringArray(this IReadOnlyDictionary<string, string> values, string key, char separator = ',')
        {
            if (!values.TryGetValue(key, out string? value) || string.IsNullOrWhiteSpace(value))
            {
                return Array.Empty<string>();
            }

            return value.Split(separator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }
    }
}
