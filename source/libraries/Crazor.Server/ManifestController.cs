// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Crazor.Teams;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.IO;
using System.IO.Compression;

namespace Crazor.Server.Controllers
{
    // This ASP Controller is created to handle a request. Dependency Injection will provide the Adapter and IBot
    // implementation at runtime. Multiple different IBot implementations running at different endpoints can be
    // achieved by specifying a more specific type for the bot constructor argument.
    [Route("/teams")]
    [ApiController]
    public class ManifestController : ControllerBase
    {
        private FileContentResult _zip;

        public ManifestController(IConfiguration configuration, IWebHostEnvironment environment, Manifest manifest)
        {
            var botIcon = configuration.GetValue<string>("BotIcon") ?? "/images/boticon.png";
            var botIconPath = Path.Combine(environment.WebRootPath, botIcon.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar));

            var outline = configuration.GetValue<string>("OutlineIcon") ?? "/images/outline.png";
            var outlinePath = Path.Combine(environment.WebRootPath, outline.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar));

            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    var entry = archive.CreateEntry("manifest.json");
                    using (var stream = entry.Open())
                    {
                        using (var streamWriter = new StreamWriter(stream))
                        {
                            streamWriter.Write(JsonConvert.SerializeObject(manifest, Formatting.Indented));
                        }
                    }
                    

                    entry = archive.CreateEntry(Path.GetFileName(botIconPath).ToLower());
                    using (var inputStream = System.IO.File.OpenRead(botIconPath))
                    {
                        using (var outputStream = entry.Open())
                        {
                            inputStream.CopyTo(outputStream);
                        }
                    }

                    entry = archive.CreateEntry(Path.GetFileName(outlinePath).ToLower());
                    using (var inputStream = System.IO.File.OpenRead(outlinePath))
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

        [HttpGet]
        public FileContentResult GetAsync()
        {
            return _zip;
        }
    }
}

