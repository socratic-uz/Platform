using System;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;

namespace Socratic.Platform.Composition.Services
{
    /// <summary>
    /// Universal IHostEnvironment implementation for client platforms (Blazor WebAssembly, MAUI Hybrid)
    /// where ASP.NET Core server environment is not naturally registered in DI.
    /// </summary>
    public sealed class SocraticHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "Socratic";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory ?? string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();

        private sealed class NullFileProvider : IFileProvider
        {
            public IDirectoryContents GetDirectoryContents(string subpath) => NotFoundDirectoryContents.Singleton;
            public IFileInfo GetFileInfo(string subpath) => new NotFoundFileInfo(subpath);
            public IChangeToken Watch(string filter) => NullChangeToken.Singleton;
        }
    }
}
