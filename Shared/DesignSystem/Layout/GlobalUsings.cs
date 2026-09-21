global using static SharedKernel.ValueObjects.EnvironmentVariables;
global using static Shared.Helpers.FileHelper;


global using static Microsoft.AspNetCore.Components.Web.RenderMode;
global using static Identifying.Application.Protos.AuthService;
global using static Identifying.Application.Protos.UserService;
global using static Identifying.Application.Protos.RoleService;
global using static Identifying.Application.Protos.ProfileService;
global using static Shopping.Application.Protos.OrganizationService;
global using static Shopping.Application.Protos.ProductService;
global using static Shopping.Application.Protos.WishService;
global using static Ordering.Application.Protos.OrderService;
global using static Ordering.Application.Protos.OrderItemService;


global using static System.Net.Mime.MediaTypeNames;


// Aliases для разрешения конфликтов имен между proto и SharedKernel.ValueObjects

global using SharedLanguage = SharedKernel.ValueObjects.Language;
global using SharedTheme = SharedKernel.ValueObjects.Theme;
global using SharedOrderStatus = SharedKernel.ValueObjects.OrderStatus;
global using SharedPaymentType = SharedKernel.ValueObjects.PaymentType;
global using SharedProductType = SharedKernel.ValueObjects.ProductType;

global using Identifying.Application.Protos;
global using Shopping.Application.Protos;
global using Ordering.Application.Protos;
global using Domain.Entities;
global using SharedKernel.Abstractions;