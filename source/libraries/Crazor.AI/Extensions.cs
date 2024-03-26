using System.Reflection;
using YamlConverter;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using Humanizer;
using System.ComponentModel;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace Crazor.AI
{
    public static class Extensions
    {

        public static async Task<T> GetYamlOrJsonAsync<T>(this HttpClient httpClient, string url, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var response = await httpClient.GetAsync(url))
            {
                var content = await response.Content.ReadAsStringAsync();
                if (response.Content.Headers.ContentType.MediaType == "text/yaml")
                    return YamlConvert.DeserializeObject<T>(content);
                else if (response.Content.Headers.ContentType.MediaType == "application/json")
                    return JsonConvert.DeserializeObject<T>(content);
                else
                {
                    if (content.StartsWith("{") && content.EndsWith("}"))
                        return YamlConvert.DeserializeObject<T>(content);
                    else
                        return JsonConvert.DeserializeObject<T>(content);
                }
            }
        }

        public static string LoadResource(this Assembly assembly, string resource)
        {
            if (assembly.GetManifestResourceNames().Any(name => name == resource))
            {
                using (var stream = assembly.GetManifestResourceStream(resource))
                {
                    using (var tr = new StreamReader(stream))
                    {
                        return tr.ReadToEnd();
                    }
                }
            }
            return null;
        }

        public static bool HasProperty(this Type type, string property)
        {
            return type.TryGetPropertyInfo(property, out var _);
        }

        public static PropertyInfo GetPropertyInfo(this Type type, string property)
        {
            return type.GetProperties().Single(p => p.Name == property);
        }

        public static bool TryGetPropertyInfo(this Type type, string property, out PropertyInfo? propertyInfo)
        {
            propertyInfo = type.GetProperties().SingleOrDefault(p => p.Name == property);
            return propertyInfo != null;
        }

        public static string GetPropertyLabel(this PropertyInfo propertyInfo)
        {
            var displayAttribute = propertyInfo.GetCustomAttribute<DisplayAttribute>();
            return displayAttribute?.GetName() ?? displayAttribute?.GetShortName() ?? propertyInfo.Name.Humanize();
        }

        public static string GetFormatedValueText(this PropertyInfo propertyInfo, object? value)
        {
            if (value == null)
                return string.Empty;

            var displayAttribute = propertyInfo.GetCustomAttribute<DisplayFormatAttribute>();
            if (displayAttribute != null && !String.IsNullOrEmpty(displayAttribute.DataFormatString))
            {
                return String.Format(displayAttribute.DataFormatString, value);
            }

            return value.ToString();
        }

        public static bool IsList(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IList<>);
        }

        public static DateTimeOffset Merge(this TimexProperty timex, DateTimeOffset? dt)
        {
            var datetimeOffset = (dt != null) ? dt.Value : DateTime.MinValue;
            return new DateTimeOffset(timex.Year ?? datetimeOffset.Year,
                timex.Month ?? datetimeOffset.Month,
                timex.DayOfMonth ?? datetimeOffset.Day,
                timex.Hour ?? datetimeOffset.Hour,
                timex.Minute ?? datetimeOffset.Minute,
                timex.Second ?? datetimeOffset.Second,
                datetimeOffset.Offset);
        }

        public static DateTime Merge(this TimexProperty timex, DateTime? dt)
        {
            var datetime = (dt != null) ? dt.Value : DateTime.MinValue;
            return new DateTime(timex.Year ?? datetime.Year,
                timex.Month ?? datetime.Month,
                timex.DayOfMonth ?? datetime.Day,
                timex.Hour ?? datetime.Hour,
                timex.Minute ?? datetime.Minute,
                timex.Second ?? datetime.Second);
        }

        public static DateOnly Merge(this TimexProperty timex, DateOnly? dt)
        {
            var date = (dt != null) ? dt.Value : DateOnly.MinValue;
            return new DateOnly(timex.Year ?? date.Year,
                timex.Month ?? date.Month,
                timex.DayOfMonth ?? date.Day);
        }

        public static TimeOnly Merge(this TimexProperty timex, TimeOnly? tm)
        {
            var time = (tm != null) ? tm.Value : TimeOnly.MinValue;
            return new TimeOnly(timex.Hour ?? time.Hour,
                timex.Minute ?? time.Minute,
                timex.Second ?? time.Second);
        }


        public static TimeSpan Merge(this TimexProperty timex, TimeSpan? tm)
        {
            var time = (tm != null) ? tm.Value : TimeSpan.MinValue;
            return new TimeSpan(timex.Hour ?? time.Hours,
                timex.Minute ?? time.Minutes,
                timex.Second ?? time.Seconds);
        }

    }
}
