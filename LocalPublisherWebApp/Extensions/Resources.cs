using System.IO;
using System.Threading.Tasks;

namespace LocalPublisherWebApp
{
    public static class Resources
    {
        public static Task<string> GetEmbeddedResource(this string name)
        {
            var resourceName = $"{typeof(Resources).FullName}.{name}";
            using (var resourceStream = typeof(Resources).Assembly.GetManifestResourceStream(resourceName))
            {
                if (resourceStream == null)
                    return null;

                using (var reader = new StreamReader(resourceStream))
                    return reader.ReadToEndAsync();
            }
        }
    }
}
