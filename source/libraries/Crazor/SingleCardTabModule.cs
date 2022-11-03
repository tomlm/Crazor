using Microsoft.Extensions.Configuration;

namespace Crazor
{
    /// <summary>
    /// CardTableModule which is bound to a single card via path, aka /Cards/MyCard
    /// </summary>
    /// <remarks>This is used by Tab Fetch to process registrations that are entityId path style /Cards/MyCard</remarks>
    internal class SingleCardTabModule : CardTabModule
    {
        public SingleCardTabModule(IServiceProvider services) : base(services)
        { }

        internal SingleCardTabModule(IServiceProvider services, string path) : base(services)
        {
            if (Uri.TryCreate(path, UriKind.RelativeOrAbsolute, out var uri))
            {
                this.Path = path;

                uri = uri.IsAbsoluteUri ? uri : new Uri(_configuration.GetValue<Uri>("HostUri"), uri);
                CardApp.ParseUri(uri, out var app, out var sharedId, out var view, out var subPath);
                this.Name = app;
            }
            else
            {
                throw new ArgumentException($"{path} not a uri?");
            }
        }
        
        public string Path { get; set; }

        public override Task<string[]> GetCardUrisAsync()
        {
            return Task.FromResult(new string[] { Path });
        }
    }
}
