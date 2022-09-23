using System;

namespace LocalPublisherWebApp.Extensions
{
    public class AppSettingWriter
    {
        public static void SetAppSettingValue(string key, string value, string? appSettingsJsonFilePath = null)
        {
            var jsonPath = appSettingsJsonFilePath ?? System.IO.Path.Combine(System.AppContext.BaseDirectory, "appsettings.json");

            var json = System.IO.File.ReadAllText(jsonPath);
            dynamic? jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(json);
            if (jsonObj == null) throw new NullReferenceException(nameof(json));
            jsonObj[key] = value;

            string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);

            System.IO.File.WriteAllText(jsonPath, output);
        }
    }
}
