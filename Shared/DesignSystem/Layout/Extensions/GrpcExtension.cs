using System;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Core;

namespace Shared
{
    public static class GrpcExtension
    {
        [Obsolete("Use Infrastructure.Core.GrpcExtension.CreateCallCredentials instead.")]
        public static Task CreateCallCredentials(AuthInterceptorContext context, Metadata metadata, IServiceProvider serviceProvider)
            => Infrastructure.Core.GrpcExtension.CreateCallCredentials(context, metadata, serviceProvider);

        [Obsolete("Use Infrastructure.Core.GrpcExtension.AddGrpcClient instead.")]
        public static void AddGrpcClient<TClient>(this IServiceCollection services, string? uriString) where TClient : ClientBase
            => Infrastructure.Core.GrpcExtension.AddGrpcClient<TClient>(services, uriString);

        [Obsolete("Use Infrastructure.Core.GrpcExtension.AddGrpcClientWrappers instead.")]
        public static void AddGrpcClientWrappers(this IServiceCollection services)
            => Infrastructure.Core.GrpcExtension.AddGrpcClientWrappers(services);

        public static void AddGrpcClients(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddInfrastructureGrpcClients(configuration);
            services.AddScoped<Smart.Web.Services.SmartToastService>();
        }
    }
}