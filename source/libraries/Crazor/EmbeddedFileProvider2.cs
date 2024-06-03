using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System.Reflection;

namespace Crazor
{
    /// <summary>
    /// This file provider wraps the normal embeddedFileProvider
    /// only it takes in a web static resource and pulls from embeded provider
    /// so ~/Cards/Dice/dice1.png => embedded resource SharedCards.Cards.Dice.dice1.png 
    /// </summary>
    public class EmbeddedFileProvider2 : IFileProvider
    {
        private EmbeddedFileProvider embeddedProvider;

        public EmbeddedFileProvider2(Assembly assembly)
        {
            embeddedProvider = new EmbeddedFileProvider(assembly);
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            var results = embeddedProvider.GetDirectoryContents(subpath);
            return results;
        }

        public IFileInfo GetFileInfo(string path)
        {
            path = path
                    .TrimStart('~')
                    .TrimStart('/', '\\')
                    .Replace('\\', '.')
                    .Replace('/', '.')
                    .Replace('-', '_');
            var results = embeddedProvider.GetDirectoryContents(path);

            var result = embeddedProvider.GetFileInfo(path);
            if (result.Exists)
                return result;
            return embeddedProvider.GetFileInfo($"wwwroot.{path}");
        }

        public IChangeToken Watch(string filter)
        {
            return embeddedProvider.Watch(filter);
        }
    }
}
