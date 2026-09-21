using Grpc.Core;
using Grpc.Net.Client.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Domain.Abstractions;
using Infrastructure.Services;
using Shared.Layout;
using Shared.Services;
using static Paying.Application.Protos.PaymentService;

namespace Shared
{
    public static class GrpcExtension
    {
        public static async Task CreateCallCredentials(AuthInterceptorContext context, Metadata metadata, IServiceProvider serviceProvider)
        {
            try
            {
                var settings = serviceProvider.GetRequiredService<ISettingsManager>();
                var token = await settings.GetAccessTokenAsync();
                var language = await settings.GetLanguageAsync();

                if (!string.IsNullOrWhiteSpace(token))
                    metadata.Add(new("Authorization", $"Bearer {token}"));
                metadata.Add("Accept-Language", language.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Dialog.Window = b => b.AddContent(1, e.ToString());
            }
        }

        public static void AddGrpcClient<TClient>(this IServiceCollection services, string? uriString) where TClient : ClientBase
        {
            uriString = string.IsNullOrWhiteSpace(uriString) ? "http://localhost:5000" : uriString;

            services.AddGrpcClient<TClient>(typeof(TClient).Name, o => { o.Address = new(uriString); })
                    .ConfigurePrimaryHttpMessageHandler(() => 
                    {
                        var handler = new HttpClientHandler();
                        if (!OperatingSystem.IsBrowser())
                        {
                            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                        }
                        return new GrpcWebHandler(handler); 
                    })
                    .ConfigureChannel(o => o.UnsafeUseInsecureChannelCallCredentials = true)
                    .AddCallCredentials((c, m, s) =>
                    CreateCallCredentials(c, m, s));
        }




        public static void AddGrpcClientWrappers(this IServiceCollection services)
        {
            // Register raw gRPC clients wrapped by IServiceClient with DTO messages
            services.AddScoped<IServiceClient<OrganizationDto>>(sp => sp.GetRequiredService<Shopping.Application.Protos.OrganizationService.OrganizationServiceClient>());
            services.AddScoped<IServiceClient<ProductDto>>(sp => sp.GetRequiredService<Shopping.Application.Protos.ProductService.ProductServiceClient>());
            services.AddScoped<IServiceClient<OrderDto>>(sp => sp.GetRequiredService<Ordering.Application.Protos.OrderService.OrderServiceClient>());
            services.AddScoped<IServiceClient<OrderItemDto>>(sp => sp.GetRequiredService<Ordering.Application.Protos.OrderItemService.OrderItemServiceClient>());
            services.AddScoped<IServiceClient<UserDto>>(sp => sp.GetRequiredService<Identifying.Application.Protos.UserService.UserServiceClient>());
            services.AddScoped<IServiceClient<ProfileDto>>(sp => sp.GetRequiredService<Identifying.Application.Protos.ProfileService.ProfileServiceClient>());
            services.AddScoped<IServiceClient<RoleDto>>(sp => sp.GetRequiredService<Identifying.Application.Protos.RoleService.RoleServiceClient>());

            // Register wrapper clients mapping domain entities to/from gRPC DTOs
            services.AddScoped<IServiceClient<Domain.Entities.Organization>, Infrastructure.Services.OrganizationServiceClient>();
            services.AddScoped<IServiceClient<Domain.Entities.Product>, Infrastructure.Services.ProductServiceClient>();
            services.AddScoped<IServiceClient<Domain.Entities.Order>, Infrastructure.Services.OrderServiceClient>();
            services.AddScoped<IServiceClient<Domain.Entities.OrderItem>, Infrastructure.Services.OrderItemServiceClient>();
            services.AddScoped<IServiceClient<Domain.Entities.User>, Infrastructure.Services.UserServiceClient>();
            services.AddScoped<IServiceClient<Domain.Entities.Role>, Infrastructure.Services.RoleServiceClient>();
        }
        public static void AddGrpcClients(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddGrpcClient<AuthServiceClient>(configuration[IDENTIFYING_SERVICE_URL]);
            services.AddGrpcClient<Identifying.Application.Protos.UserService.UserServiceClient>(configuration[IDENTIFYING_SERVICE_URL]);
            services.AddGrpcClient<Identifying.Application.Protos.RoleService.RoleServiceClient>(configuration[IDENTIFYING_SERVICE_URL]);
            services.AddGrpcClient<Identifying.Application.Protos.ProfileService.ProfileServiceClient>(configuration[IDENTIFYING_SERVICE_URL]);
            services.AddGrpcClient<Shopping.Application.Protos.OrganizationService.OrganizationServiceClient>(configuration[SHOPPING_SERVICE_URL]);
            services.AddGrpcClient<Shopping.Application.Protos.ProductService.ProductServiceClient>(configuration[SHOPPING_SERVICE_URL]);
            services.AddGrpcClient<WishServiceClient>(configuration[SHOPPING_SERVICE_URL]);
            services.AddGrpcClient<Shopping.Application.Protos.LayoutService.LayoutServiceClient>(configuration[SHOPPING_SERVICE_URL]);
            services.AddGrpcClient<Ordering.Application.Protos.OrderService.OrderServiceClient>(configuration[ORDERING_SERVICE_URL]);
            services.AddGrpcClient<Ordering.Application.Protos.OrderItemService.OrderItemServiceClient>(configuration[ORDERING_SERVICE_URL]);
            services.AddGrpcClient<PaymentServiceClient>(configuration[PAYING_SERVICE_URL]);
            services.AddGrpcClientWrappers();
            services.AddScoped<IServiceClientFactory, ServiceClientFactory>();
            services.AddScoped<Smart.Web.Services.SmartToastService>();

            services.AddHttpClient<SharedKernel.Services.UzQr.IUzQrApiClient, SharedKernel.Services.UzQr.UzQrApiClient>(client =>
            {
                client.BaseAddress = new Uri(SharedKernel.Services.UzQr.UzQrApiClient.DefaultBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(15);
            });
        }
    }
}