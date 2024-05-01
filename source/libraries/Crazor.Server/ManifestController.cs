


using Crazor.Teams;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace Crazor.Server.Controllers
{
    // This ASP Controller is created to handle a request. Dependency Injection will provide the Adapter and IBot
    // implementation at runtime. Multiple different IBot implementations running at different endpoints can be
    // achieved by specifying a more specific type for the bot constructor argument.
    [Route("/teams.zip")]
    [ApiController]
    [AllowAnonymous]
    public class ManifestController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private FileContentResult _zip;

        public ManifestController(IConfiguration configuration, IWebHostEnvironment environment, CrazorServerOptions crazorServerOptions)
        {
            this._environment = environment;
            var botIcon = configuration.GetValue<string>("BotIcon") ?? "/images/boticon.png";
            var outlineIcon = configuration.GetValue<string>("OutlineIcon") ?? "/images/outline.png";

            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    var entry = archive.CreateEntry("manifest.json");
                    using (var stream = entry.Open())
                    {
                        using (var streamWriter = new StreamWriter(stream))
                        {
                            streamWriter.Write(JsonConvert.SerializeObject(crazorServerOptions.Manifest, Formatting.Indented));
                        }
                    }


                    entry = archive.CreateEntry(Path.GetFileName(botIcon).ToLower());
                    using (var inputStream = GetIconStream(botIcon))
                    {
                        using (var outputStream = entry.Open())
                        {
                            inputStream.CopyTo(outputStream);
                        }
                    }

                    entry = archive.CreateEntry(Path.GetFileName(outlineIcon).ToLower());
                    using (var inputStream = GetIconStream(outlineIcon))
                    {
                        using (var outputStream = entry.Open())
                        {
                            inputStream.CopyTo(outputStream);
                        }
                    }
                }


                memoryStream.Seek(0, SeekOrigin.Begin);
                _zip = new FileContentResult(memoryStream.GetBuffer(), "application/zip");
            }

        }

        private Stream? GetIconStream(string icon)
        {
            var iconPath = Path.Combine(_environment.WebRootPath, icon.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(iconPath))
                return System.IO.File.OpenRead(iconPath);

            // look for crazor defaults.
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(asm=> !asm.FullName.StartsWith("Crazor.")))
            {
                var resourceName = $"{assembly.GetName().Name}.wwwroot{icon.Replace(Path.DirectorySeparatorChar, '.').Replace(Path.AltDirectorySeparatorChar, '.')}";
                if (assembly.GetManifestResourceNames().Contains(resourceName))
                {
                    var stream = assembly.GetManifestResourceStream(resourceName);
                    if (stream != null)
                        return stream;
                }
            }
            return null;
        }

        [HttpGet]
        public FileContentResult GetAsync()
        {
            return _zip;
        }
    }
}

