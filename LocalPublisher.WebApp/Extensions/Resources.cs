using System.IO;
using System.Threading.Tasks;

namespace LocalPublisherWebApp
{
    public static class Resources
    {
        public async static Task<string?> GetEmbeddedResource(this string name)
        {
            var resourceName = $"{typeof(Resources).FullName}.{name}";
            using (var resourceStream = typeof(Resources).Assembly.GetManifestResourceStream(resourceName))
            {
                if (resourceStream == null)
                    return await Task.FromResult<string?>(null);

                using (var reader = new StreamReader(resourceStream))
                    return await reader.ReadToEndAsync();
            }
        }
    }
}
