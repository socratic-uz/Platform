using System;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;

namespace Shared.Services
{
    [Obsolete("Use Infrastructure.Gateways.Vision.RemoteDetectorService instead.")]
    public class RemoteDetectorService : Infrastructure.Gateways.Vision.RemoteDetectorService
    {
        public RemoteDetectorService(NavigationManager navManager, IConfiguration? configuration = null)
            : base(navManager, configuration)
        {
        }

        public RemoteDetectorService(IConfiguration configuration)
            : base(configuration)
        {
        }

        public RemoteDetectorService(string hubUrl)
            : base(hubUrl)
        {
        }

        public RemoteDetectorService()
            : base()
        {
        }
    }
}
