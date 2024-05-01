


using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System.Reflection;

namespace Crazor
{
    public class EmbeddedFileProvider2 : IFileProvider
    {
        private EmbeddedFileProvider fileProvider;

        public EmbeddedFileProvider2(Assembly assembly)
        {
            fileProvider = new EmbeddedFileProvider(assembly);
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            var results = fileProvider.GetDirectoryContents(subpath);
            return results;
        }

        public IFileInfo GetFileInfo(string path)
        {
            path = path
                    .TrimStart('/', '\\')
                    .Replace('\\', '.')
                    .Replace('/', '.')
                    .Replace('-', '_');
            var results = fileProvider.GetDirectoryContents("wwwroot");

            return fileProvider.GetFileInfo($"wwwroot.{path}");
        }

        public IChangeToken Watch(string filter)
        {
            return fileProvider.Watch(filter);
        }
    }
}
