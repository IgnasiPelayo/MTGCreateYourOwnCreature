
namespace MTGCreateYourOwnCreature.ViewModel.Helpers
{
    public static class DictionaryExtensions
    {
        public static int GetInt(this IReadOnlyDictionary<string, string> values, string key, int defaultValue = 0)
        {
            return values.TryGetValue(key, out string? value) && int.TryParse(value, out int parsed) ? parsed : defaultValue;
        }

        public static string GetString(this IReadOnlyDictionary<string, string> values, string key, string defaultValue = "")
        {
            return values.TryGetValue(key, out string? value) ? value : defaultValue;
        }

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
