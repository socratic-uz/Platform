using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using Domain.Abstractions;
using Domain.Entities;
using Ordering.Application.Protos;
using Shopping.Application.Protos;
using Shared.Extensions;
using SharedKernel.ValueObjects;

namespace Shared.Pages.ProductPages
{
    public abstract class ProductsComponentBase : DataComponentBase
    {
        [Inject] public AuthenticationStateProvider Auth { get; set; } = default!;
        [Inject] public NavigationManager Nav { get; set; } = default!;
        [Inject] public IStringLocalizer L { get; set; } = default!;
        [Inject] public IJSRuntime JS { get; set; } = default!;

        public string? KioskAlertMessage { get; protected set; }
        public string? KioskAlertTitle { get; protected set; }
        public bool IsKioskAlertDialogOpen { get; protected set; }

        public void ShowKioskAlert(string message, string? title = null)
        {
            KioskAlertMessage = message;
            KioskAlertTitle = title ?? (L?["Notice"] ?? "Внимание");
            IsKioskAlertDialogOpen = true;
            StateHasChanged();
        }

        public void CloseKioskAlert()
        {
            IsKioskAlertDialogOpen = false;
            KioskAlertMessage = null;
            KioskAlertTitle = null;
            StateHasChanged();
        }

        [SupplyParameterFromQuery] public string OrganizationId { get; set; } = default!;
        [SupplyParameterFromQuery] public string? PlaceId { get; set; }
        [SupplyParameterFromQuery] public string? SearchText { get; set; }
        [SupplyParameterFromQuery] public int? UxMode { get; set; }

        [Parameter] public string? OrgId { get; set; }
        [Parameter] public string? Slug { get; set; }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            if (!string.IsNullOrWhiteSpace(OrgId))
            {
                OrganizationId = OrgId;
            }
            selectedUxModeFilter = (UxMode.HasValue && UxMode.Value > 0) ? UxMode.Value : null;
        }

        public static string EscapeJson(string? s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", " ");
        }

        public string? activeTag;

        public void SelectTag(string? tag)
        {
            if (activeTag == tag)
            {
                activeTag = null;
            }
            else
            {
                activeTag = tag;
            }
            StateHasChanged();
        }

        public void ClearAllFilters()
        {
            selectedUxModeFilter = null;
            activeTag = null;
            SearchText = null;
            StateHasChanged();
        }

        public IEnumerable<string> GetUniqueTags()
        {
            return Products
                .Where(p => p.Tags != null && !p.Tags.Contains("place"))
                .SelectMany(p => p.Tags)
                .Distinct()
                .OrderBy(t => t);
        }

        public IEnumerable<ProductUxMode> GetAvailableUxModes()
        {
            var modeValues = Products
                .Where(p => p.Tags == null || !p.Tags.Contains("place"))
                .Select(p => p.ProductUxModeValue)
                .Distinct();

            foreach (var val in modeValues)
            {
                if (ProductUxMode.TryFromValue(val, out var mode))
                {
                    yield return mode;
                }
            }
        }

        public int? selectedUxModeFilter;

        public IEnumerable<Product> FilteredProducts
        {
            get
            {
                var query = Products.Where(p => p.Tags == null || !p.Tags.Contains("place"));
                if (selectedUxModeFilter.HasValue && selectedUxModeFilter.Value > 0)
                {
                    query = query.Where(p => p.ProductUxModeValue == selectedUxModeFilter.Value);
                }
                if (!string.IsNullOrEmpty(activeTag))
                {
                    query = query.Where(p => p.Tags != null && p.Tags.Contains(activeTag));
                }
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var term = SearchText.Trim();
                    query = query.Where(p =>
                        (p.Name?.Contains(term, StringComparison.OrdinalIgnoreCase) == true) ||
                        (p.Description?.Contains(term, StringComparison.OrdinalIgnoreCase) == true) ||
                        (p.Tags != null && p.Tags.Any(t => t.Contains(term, StringComparison.OrdinalIgnoreCase))));
                }
                return query;
            }
        }

        public List<Product> Products { get; set; } = new();
        public List<Order> Orders { get; set; } = new();
        public List<OrderItem> OrderItems { get; set; } = new();
        public Order? Order { get; set; }
        public Organization? organization { get; set; }

        public Product? selectedProductForEdit;
        public bool isEditProductOpen = false;

        public Product? selectedProductForDetails;
        public bool isDetailsProductOpen = false;

        protected void TriggerHapticFeedback()
        {
            try
            {
                _ = JS.InvokeVoidAsync("navigator.vibrate", 30);
            }
            catch { }
        }

        public void OpenCreateProductWindow()
        {
            selectedProductForEdit = new Product { OrganizationId = Guid.TryParse(OrganizationId, out var orgG) ? orgG : null, Tags = new List<string>() };
            isEditProductOpen = true;
        }

        public void OpenEditProductWindow(Product product)
        {
            selectedProductForEdit = product;
            isEditProductOpen = true;
        }

        public void OpenDetailsProductWindow(Product product)
        {
            selectedProductForDetails = product;
            isDetailsProductOpen = true;
        }

        public async Task UpdateProduct()
        {
            if (selectedProductForEdit != null)
            {
                var productService = ClientFactory.GetClient<Product>();
                if (selectedProductForEdit.Id == null || selectedProductForEdit.Id == Guid.Empty)
                {
                    selectedProductForEdit.Id = Guid.NewGuid();
                    await productService.Create(selectedProductForEdit);
                }
                else
                {
                    await productService.Update(selectedProductForEdit);
                }
                await ReloadAsync<Product>();
                Products = Data<Product>().ToList();
            }
            isEditProductOpen = false;
            selectedProductForEdit = null;
        }

        public async Task DeleteProduct(Product product)
        {
            if (product != null)
            {
                var productService = ClientFactory.GetClient<Product>();
                await productService.Delete(product);
                await ReloadAsync<Product>();
                Products = Data<Product>().ToList();
            }
        }

        public static string FormatAddress(SharedKernel.Protos.Address? address)
        {
            if (address == null)
                return "г. Ташкент, Узбекистан";

            var parts = new[] { address.City, address.District, address.Street, address.House, address.Apartment }
                .Where(s => !string.IsNullOrWhiteSpace(s));

            var result = string.Join(", ", parts);
            return string.IsNullOrWhiteSpace(result) ? "г. Ташкент, Узбекистан" : result;
        }

        protected bool isCreatingOrder = false;
        protected bool isEditOrganizationOpen = false;
        protected bool isSubscriptionModalOpen = false;

        protected void OpenEditOrganization() => isEditOrganizationOpen = true;
        protected void OpenSubscriptionModal() => isSubscriptionModalOpen = true;
        protected void CloseSubscriptionModal() => isSubscriptionModalOpen = false;

        protected async Task UpdateOrganization()
        {
            if (organization != null)
            {
                var organizationService = ClientFactory.GetClient<Organization>();
                organization = await organizationService.Update(organization);
            }
            isEditOrganizationOpen = false;
        }

        protected async Task LoadCommonDataAsync()
        {
            var productService = ClientFactory.GetClient<Product>();

            // Если передан PlaceId, но нет OrganizationId, получаем его из места
            if (!string.IsNullOrWhiteSpace(PlaceId) && string.IsNullOrWhiteSpace(OrganizationId))
            {
                var productStream = productService.Read(new() { Id = Guid.TryParse(PlaceId, out var placeG) ? placeG : null }).ResponseStream;
                if (await productStream.MoveNext(new()))
                {
                    OrganizationId = productStream.Current.OrganizationId?.ToString() ?? "";
                }
            }

            if (string.IsNullOrWhiteSpace(OrganizationId) && !string.IsNullOrWhiteSpace(OrgId))
            {
                OrganizationId = OrgId;
            }

            if (string.IsNullOrWhiteSpace(OrganizationId))
            {
                var defaultOrgId = SharedKernel.Extensions.ObjectIdExtension.DemoId.ToString();
                OrganizationId = defaultOrgId;
            }

            Guid.TryParse(OrganizationId, out var orgG);

            Register<Product>(new() { OrganizationId = orgG });
            Register<Organization>(new() { Id = orgG });
            Register<Order>();

            await base.OnInitializedAsync();

            Products = Data<Product>().ToList();
            if (Products.Count == 0)
            {
                if (orgG == SharedKernel.Extensions.ObjectIdExtension.SocraticId)
                {
                    Products = GetSocraticPlatformProducts(orgG);
                }
                else
                {
                    Products = GetDemoProducts(orgG);
                }
            }
            foreach (var p in Products)
            {
                if (p.ProductUxModeValue <= 0 || p.ProductUxModeValue == 1)
                {
                    var dto = (Shopping.Application.Protos.ProductDto)p;
                    var inferred = Shopping.Application.Protos.ProductDto.InferUxModeValue(dto);
                    if (inferred > 0)
                    {
                        p.ProductUxModeValue = inferred;
                    }
                }
            }

            activeTag = null;

            if (await Auth.IsAuthenticatedAsync())
            {
                try
                {
                    Orders = Data<Order>().Where(o => o.OrganizationId == orgG).ToList();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ProductsComponentBase] Error filtering orders for organization {orgG}: {ex.Message}");
                    Orders.Clear();
                }

                Order = Orders
                    .Where(i => i.Status < OrderStatus.Paid)
                    .OrderByDescending(x => x.CreatedAt)
                    .FirstOrDefault();

                if (Order != null)
                {
                    var orderProductService = ClientFactory.GetClient<OrderItem>();
                    var orderProductStream = orderProductService.Read(new() { OrderId = Order.Id }).ResponseStream;
                    OrderItems.Clear();
                    while (await orderProductStream.MoveNext(new()))
                        OrderItems.Add(orderProductStream.Current);
                }
            }

            organization = Data<Organization>().FirstOrDefault(o => o.Id == orgG);
            if (organization == null)
            {
                if (orgG == SharedKernel.Extensions.ObjectIdExtension.SocraticId)
                {
                    organization = new Organization
                    {
                        Id = orgG,
                        Name = "Socratic",
                        Description = "Omnichannel AI & Retail Platform — тарифы облачной подписки, умное кассовое оборудование, партнерская сеть и подключение организаций",
                        Images = new List<Domain.Entities.File> { new Domain.Entities.File { Url = "https://socratic.uz/img/icon-512.png" } }
                    };
                }
                else
                {
                    organization = new Organization
                    {
                        Id = orgG,
                        Name = "Demo",
                        Description = "Интерактивный мультивертикальный шоурум Socratic — 28 универсальных UX-режимов для любого бизнеса",
                        Images = new List<Domain.Entities.File> { new Domain.Entities.File { Url = "https://images.unsplash.com/photo-1517248135467-4c7edcad34c4?w=100" } }
                    };
                }
            }
        }

        protected List<Product> GetDemoProducts(Guid orgG)
        {
            return new List<Product>
            {
                // 1. CatalogItem
                new Product
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = orgG,
                    Name = "iPhone 16 Pro Max",
                    Price = 15500000,
                    Description = "Флагманский смартфон с титановым корпусом. Выберите цвет и память.",
                    Tags = new List<string> { "Электроника", "Каталог" },
                    ProductUxModeValue = ProductUxMode.CatalogItem.Value,
                    Images = new List<Domain.Entities.File> { new Domain.Entities.File { Url = "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?w=400" } }
                },
                // 2. Configurator
                new Product
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = orgG,
                    Name = "Пицца Неаполитана (Мастер сборки)",
                    Price = 85000,
                    Description = "Соберите идеальную пиццу: выберите тип теста, сырные бортики и любимые топпинги.",
                    Tags = new List<string> { "Еда", "Конфигуратор" },
                    ProductUxModeValue = ProductUxMode.Configurator.Value,
                    Images = new List<Domain.Entities.File> { new Domain.Entities.File { Url = "https://images.unsplash.com/photo-1513104890138-7c749659a591?w=400" } }
                },
                // 3. BookableResource
                new Product
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = orgG,
                    Name = "Консультация главного врача",
                    Price = 250000,
                    Description = "Запись на удобную дату и время в клинике Socratic Med.",
                    Tags = new List<string> { "Медицина", "Бронирование" },
                    ProductUxModeValue = ProductUxMode.BookableResource.Value,
                    Images = new List<Domain.Entities.File> { new Domain.Entities.File { Url = "https://images.unsplash.com/photo-1622253692010-333f2da6031d?w=400" } }
                },
                // 4. TicketBooking
                new Product
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = orgG,
                    Name = "VIP билет на Симфонический Концерт",
                    Price = 120000,
                    Description = "Выбор интерактивного места в зале + именной QR-ваучер.",
                    Tags = new List<string> { "Мероприятия", "Билеты" },
                    ProductUxModeValue = ProductUxMode.TicketBooking.Value,
                    Images = new List<Domain.Entities.File> { new Domain.Entities.File { Url = "https://images.unsplash.com/photo-1514525253161-7a46d19cd819?w=400" } }
                },
                // 5. HotelAccommodation
                new Product
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = orgG,
                    Name = "Люкс номер в Socratic Grand Hotel",
                    Price = 850000,
                    Description = "Проживание с балконом и видом на город. Выбор дат заезда.",
                    Tags = new List<string> { "Отели", "Проживание" },
                    ProductUxModeValue = ProductUxMode.HotelAccommodation.Value,
                    Images = new List<Domain.Entities.File> { new Domain.Entities.File { Url = "https://images.unsplash.com/photo-1566073771259-6a8506099945?w=400" } }
                },
                // 6. VehicleRental
                new Product
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = orgG,
                    Name = "Аренда Tesla Model 3 Performance",
                    Price = 450000,
                    Description = "Посуточная аренда с автопилотом и полной страховкой КАСКО.",
                    Tags = new List<string> { "Аренда", "Транспорт" },
                    ProductUxModeValue = ProductUxMode.VehicleRental.Value,
                    Images = new List<Domain.Entities.File> { new Domain.Entities.File { Url = "https://images.unsplash.com/photo-1536700503339-1e4b06520771?w=400" } }
                },
                // 7. DeliveryScheduled
                new Product
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = orgG,
                    Name = "Плановая доставка свежих продуктов",
                    Price = 145000,
                    Description = "Курьерская доставка по адресу в точный 2-часовой интервал времени.",
                    Tags = new List<string> { "Доставка", "Логистика" },
                    ProductUxModeValue = ProductUxMode.DeliveryScheduled.Value,
                    Images = new List<Domain.Entities.File> { new Domain.Entities.File { Url = "https://images.unsplash.com/photo-1542838132-92c53300491e?w=400" } }
                },
                // 8. LocationBased
                new Product
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = orgG,
                    Name = "Экспресс-такси и доставка (Точка А ➔ Б)",
                    Price = 35000,
                    Description = "Выбор точек отправления и получения на интерактивной карте.",
                    Tags = new List<string> { "Такси", "Гео" },
                    ProductUxModeValue = ProductUxMode.LocationBased.Value,
                    Images = new List<Domain.Entities.File> { new Domain.Entities.File { Url = "https://images.unsplash.com/photo-1526367790999-0150786686a2?w=400" } }
                },
                // 9. Subscription
                new Product
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = orgG,
                    Name = "Подписка на Cloud PRO Workspace",
                    Price = 120000,
                    Description = "Регулярный корпоративный тариф с автоматическим списанием и отменой в 1 клик.",
                    Tags = new List<string> { "Подписки", "IT" },
                    ProductUxModeValue = ProductUxMode.Subscription.Value,
                    Images = new List<Domain.Entities.File> { new Domain.Entities.File { Url = "https://images.unsplash.com/photo-1460925895917-afdab827c52f?w=400" } }
                },
                // 10. DigitalGoods
                new Product
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = orgG,
                    Name = "Лицензионный ключ Windows 11 Pro",
                    Price = 220000,
                    Description = "Мгновенная цифровая выдача ключа активации и ссылки на скачивание.",
                    Tags = new List<string> { "Цифровые", "Софт" },
                    ProductUxModeValue = ProductUxMode.DigitalGoods.Value,
                    Images = new List<Domain.Entities.File> { new Domain.Entities.File { Url = "https://images.unsplash.com/photo-1550751827-4bd374c3f58b?w=400" } }
                },
                // 11. OnlineService
                new Product
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = orgG,
                    Name = "Доступ к API нейросети Socratic AI",
                    Price = 150000,
                    Description = "Мгновенная активация аккаунта разработчика и выдача API-токена.",
                    Tags = new List<string> { "Онлайн-сервис", "AI" },
                    ProductUxModeValue = ProductUxMode.OnlineService.Value,
                    Images = new List<Domain.Entities.File> { new Domain.Entities.File { Url = "https://images.unsplash.com/photo-1677442136019-21780ecad995?w=400" } }
                },
                // 12. Donation
                new Product
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = orgG,
                    Name = "Благотворительный взнос в Фонд Socratic",
                    Price = 50000,
                    Description = "Добровольная поддержка экологических и социальных инициатив.",
                    Tags = new List<string> { "Донат", "Благотворительность" },
                    ProductUxModeValue = ProductUxMode.Donation.Value,
                    Images = new List<Domain.Entities.File> { new Domain.Entities.File { Url = "https://images.unsplash.com/photo-1532629345422-7515f3d16bb0?w=400" } }
                },
                // 13. GiftCertificate
                new Product
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = orgG,
                    Name = "Подарочный сертификат Socratic Store",
                    Price = 300000,
                    Description = "Электронный подарочный сертификат с поздравительным текстом и промокодом.",
                    Tags = new List<string> { "Подарки", "Сертификат" },
                    ProductUxModeValue = ProductUxMode.GiftCertificate.Value,
                    Images = new List<Domain.Entities.File> { new Domain.Entities.File { Url = "https://images.unsplash.com/photo-1549465220-1a8b9238cd48?w=400" } }
                },
                // 14. PriceQuoteRequest
                new Product
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = orgG,
                    Name = "Запрос расчёта сметы: Дизайн Интерьера",
                    Price = 0,
                    Description = "Отправка ТЗ и параметров помещения для составления коммерческого предложения.",
                    Tags = new List<string> { "RFQ", "Смета" },
                    ProductUxModeValue = ProductUxMode.PriceQuoteRequest.Value,
                    Images = new List<Domain.Entities.File> { new Domain.Entities.File { Url = "https://images.unsplash.com/photo-1618221195710-dd6b41faaea6?w=400" } }
                },
                // 15. BiddingAuction
                new Product
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = orgG,
                    Name = "Аукционный Лот: Картина 19-го Века",
                    Price = 350000,
                    Description = "Эксклюзивные торги с живой динамической ставкой и таймером.",
                    Tags = new List<string> { "Аукцион", "Торги" },
                    ProductUxModeValue = ProductUxMode.BiddingAuction.Value,
                    Images = new List<Domain.Entities.File> { new Domain.Entities.File { Url = "https://images.unsplash.com/photo-1579783902614-a3fb3927b675?w=400" } }
                },
                // 16. ListingClassified
                new Product
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = orgG,
                    Name = "MacBook Pro 16 M3 Max (P2P Объявление)",
                    Price = 28000000,
                    Description = "Безопасная P2P сделка с эскроу-гарантом платформы до проверки устройства.",
                    Tags = new List<string> { "P2P", "Объявления" },
                    ProductUxModeValue = ProductUxMode.ListingClassified.Value,
                    Images = new List<Domain.Entities.File> { new Domain.Entities.File { Url = "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?w=400" } }
                },
                // 17. ServiceMarketplace
                new Product
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = orgG,
                    Name = "Услуги мастера: Монтаж электрики и умного дома",
                    Price = 200000,
                    Description = "Биржа проверенных мастеров с безопасной оплатой после приемки работ.",
                    Tags = new List<string> { "Услуги", "Биржа" },
                    ProductUxModeValue = ProductUxMode.ServiceMarketplace.Value,
                    Images = new List<Domain.Entities.File> { new Domain.Entities.File { Url = "https://images.unsplash.com/photo-1621905251189-08b45d6a269e?w=400" } }
                },
                // 18. ProductBundle
                new Product
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = orgG,
                    Name = "Турпакет 'Выходные в Самарканде' (3-в-1 Комбо)",
                    Price = 1200000,
                    Description = "Выгодный комбо-набор: Скоростной поезд Afrosiyob + Отель 4* + Экскурсия.",
                    Tags = new List<string> { "Туризм", "Наборы" },
                    ProductUxModeValue = ProductUxMode.ProductBundle.Value,
                    Images = new List<Domain.Entities.File> { new Domain.Entities.File { Url = "https://images.unsplash.com/photo-1569949381669-ecf31ae8e613?w=400" } }
                },
                // 19. PriceCalculator
                new Product
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = orgG,
                    Name = "Калькулятор генеральной уборки помещений",
                    Price = 15000,
                    Description = "Интерактивный расчёт стоимости клининга по площади в кв. метрах и доп. опциям.",
                    Tags = new List<string> { "Клининг", "Калькулятор" },
                    ProductUxModeValue = ProductUxMode.PriceCalculator.Value,
                    Images = new List<Domain.Entities.File> { new Domain.Entities.File { Url = "https://images.unsplash.com/photo-1581578731548-c64695cc6952?w=400" } }
                },
                // 20. ApplicationForm
                new Product
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = orgG,
                    Name = "Анкета на оформление резидентской карты",
                    Price = 0,
                    Description = "Официальная онлайн-подача заявки с верификацией паспортных данных.",
                    Tags = new List<string> { "Документы", "Заявки" },
                    ProductUxModeValue = ProductUxMode.ApplicationForm.Value,
                    Images = new List<Domain.Entities.File> { new Domain.Entities.File { Url = "https://images.unsplash.com/photo-1450133064473-71024230f91b?w=400" } }
                },
                // 21. RaffleLottery
                new Product
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = orgG,
                    Name = "Золотой билет в праздничный розыгрыш",
                    Price = 25000,
                    Description = "Суперприз — 100 000 000 сум! Выберите количество билетов и получите счастливые номера.",
                    Tags = new List<string> { "Лотерея", "Розыгрыш" },
                    ProductUxModeValue = ProductUxMode.RaffleLottery.Value,
                    Images = new List<Domain.Entities.File> { new Domain.Entities.File { Url = "https://images.unsplash.com/photo-1518609878373-06d740f60d8b?w=400" } }
                },
                // 22. PreOrder
                new Product
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = orgG,
                    Name = "Предзаказ: Игровая консоль PlayStation 5 Pro",
                    Price = 9200000,
                    Description = "Забронируйте новинку из первой официальной партии с гарантией неизменности цены.",
                    Tags = new List<string> { "Игры", "Предзаказ" },
                    ProductUxModeValue = ProductUxMode.PreOrder.Value,
                    Images = new List<Domain.Entities.File> { new Domain.Entities.File { Url = "https://images.unsplash.com/photo-1606813907291-d86efa9b94db?w=400" } }
                },
                // 23. GroupBuy
                new Product
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = orgG,
                    Name = "Совместная закупка: Зерновой кофе Arabica 10кг",
                    Price = 180000,
                    Description = "Групповой оптовый заказ со скидкой -25% при сборе 10 участников.",
                    Tags = new List<string> { "Совместные закупки", "Опт" },
                    ProductUxModeValue = ProductUxMode.GroupBuy.Value,
                    Images = new List<Domain.Entities.File> { new Domain.Entities.File { Url = "https://images.unsplash.com/photo-1559056199-641a0ac8b55e?w=400" } }
                },
                // 24. Membership
                new Product
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = orgG,
                    Name = "Годовой VIP абонемент в фитнес-клуб",
                    Price = 4500000,
                    Description = "Неограниченное посещение тренажерного зала, бассейна, SPA и групповых программ.",
                    Tags = new List<string> { "Спорт", "Абонемент" },
                    ProductUxModeValue = ProductUxMode.Membership.Value,
                    Images = new List<Domain.Entities.File> { new Domain.Entities.File { Url = "https://images.unsplash.com/photo-1534438327276-14e5300c3a48?w=400" } }
                },
                // 25. Crowdfunding
                new Product
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = orgG,
                    Name = "Краудфандинг: Эко-стартап 'Зеленый Ташкент'",
                    Price = 100000,
                    Description = "Сбор средств на установку станций раздельного сбора отходов с вознаграждениями спонсорам.",
                    Tags = new List<string> { "Краудфандинг", "Экология" },
                    ProductUxModeValue = ProductUxMode.Crowdfunding.Value,
                    Images = new List<Domain.Entities.File> { new Domain.Entities.File { Url = "https://images.unsplash.com/photo-1542601906990-b4d3fb778b09?w=400" } }
                },
                // 26. ExchangeTrading
                new Product
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = orgG,
                    Name = "P2P Обмен: Игровая приставка на электросамокат",
                    Price = 3500000,
                    Description = "Платформа безопасного обмена вещами и техникой с гарантией платформы.",
                    Tags = new List<string> { "Бартер", "Обмен" },
                    ProductUxModeValue = ProductUxMode.ExchangeTrading.Value,
                    Images = new List<Domain.Entities.File> { new Domain.Entities.File { Url = "https://images.unsplash.com/photo-1558981806-ec527fa84c39?w=400" } }
                },
                // 27. ResourceReservation
                new Product
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = orgG,
                    Name = "Резерв переговорной комнаты 'Sky Lounge'",
                    Price = 180000,
                    Description = "Бронирование премиум-конференц зала на 12 персон с проектором и аудиосистемой.",
                    Tags = new List<string> { "Коворкинг", "Ресурсы" },
                    ProductUxModeValue = ProductUxMode.ResourceReservation.Value,
                    Images = new List<Domain.Entities.File> { new Domain.Entities.File { Url = "https://images.unsplash.com/photo-1497366216548-37526070297c?w=400" } }
                },
                // 28. HourlyService
                new Product
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = orgG,
                    Name = "Почасовая консультация архитектора решений",
                    Price = 400000,
                    Description = "Индивидуальная консультация по архитектуре программных систем и облачной инфраструктуре.",
                    Tags = new List<string> { "Консалтинг", "Почасовая" },
                    ProductUxModeValue = ProductUxMode.HourlyService.Value,
                    Images = new List<Domain.Entities.File> { new Domain.Entities.File { Url = "https://images.unsplash.com/photo-1552664730-d307ca884978?w=400" } }
                }
            };
        }

        protected List<Product> GetSocraticPlatformProducts(Guid orgG)
        {
            return new List<Product>
            {
                // 1. Subscription (9) — Тариф «Старт / Solo POS»
                new Product
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = orgG,
                    Name = "Тариф «Старт / Solo POS» (Облачная касса)",
                    Price = 290000,
                    Description = "Облачная касса: 1-2 рабочих места (Web/Android), фискализация чеков, складской учет, Telegram-бот и онлайн-витрина.",
                    Tags = new List<string> { "Тарифы", "Подписка", "B2B" },
                    ProductUxModeValue = ProductUxMode.Subscription.Value,
                    Images = new List<Domain.Entities.File> { new Domain.Entities.File { Url = "https://images.unsplash.com/photo-1556742049-0a67c5574f73?w=500" } }
                },
                // 2. Subscription (9) — Тариф «Бизнес Про»
                new Product
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = orgG,
                    Name = "Тариф «Бизнес Про» (Кассы, Киоски и AI)",
                    Price = 690000,
                    Description = "Комплексный тариф для кафе и ритейла: терминалы самообслуживания, Face ID биометрия, экран кухни (KDS) и аналитика.",
                    Tags = new List<string> { "Тарифы", "Подписка", "Популярный" },
                    ProductUxModeValue = ProductUxMode.Subscription.Value,
                    Images = new List<Domain.Entities.File> { new Domain.Entities.File { Url = "https://images.unsplash.com/photo-1551836022-d5d88e9218df?w=500" } }
                },
                // 3. Subscription (9) — Тариф «Сеть / Франшиза»
                new Product
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = orgG,
                    Name = "Тариф «Сеть / Франшиза» (Enterprise)",
                    Price = 1490000,
                    Description = "Мультисеть, неограниченно филиалов, 3D Digital Twin залов, выделенный ScyllaDB кластер, интеграция с 1С/ERP и SLA 99.99%.",
                    Tags = new List<string> { "Тарифы", "Подписка", "Enterprise" },
                    ProductUxModeValue = ProductUxMode.Subscription.Value,
                    Images = new List<Domain.Entities.File> { new Domain.Entities.File { Url = "https://images.unsplash.com/photo-1486406146926-c627a92ad1ab?w=500" } }
                },
                // 4. Membership (24) — Партнерская программа 35%
                new Product
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = orgG,
                    Name = "Партнерская программа «Socratic Partner» (35% от продаж)",
                    Price = 0,
                    Description = "Станьте партнером Socratic: получайте 35% рекуррентного дохода ежемесячно от подписок каждого привлеченного вами заведения.",
                    Tags = new List<string> { "Партнерам", "35% Продажи", "Реферальная программа" },
                    ProductUxModeValue = ProductUxMode.Membership.Value,
                    Images = new List<Domain.Entities.File> { new Domain.Entities.File { Url = "https://images.unsplash.com/photo-1521791136064-7986c2920216?w=500" } }
                },
                // 5. ProductBundle (18) — Оборудование
                new Product
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = orgG,
                    Name = "Комплект «Socratic Hardware: POS-терминал + Киоск»",
                    Price = 4500000,
                    Description = "Готовое аппаратное решение: сенсорный моноблок, фискальный термопринтер 80мм, 2D QR-сканер и модуль киоска с эквайрингом.",
                    Tags = new List<string> { "Оборудование", "Бандл", "Кассы" },
                    ProductUxModeValue = ProductUxMode.ProductBundle.Value,
                    Images = new List<Domain.Entities.File> { new Domain.Entities.File { Url = "https://images.unsplash.com/photo-1556740758-90de374c12ad?w=500" } }
                },
                // 6. ApplicationForm (20) — Регистрация заведения
                new Product
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = orgG,
                    Name = "Регистрация заведения (Онлайн подключение)",
                    Price = 0,
                    Description = "Быстрая онлайн-регистрация заведения в экосистеме Socratic: укажите название, выберите один из 28 готовых UX-режимов и запустите бизнес.",
                    Tags = new List<string> { "Онбординг", "Регистрация", "Бизнес" },
                    ProductUxModeValue = ProductUxMode.ApplicationForm.Value,
                    Images = new List<Domain.Entities.File> { new Domain.Entities.File { Url = "https://images.unsplash.com/photo-1450133064473-71024230f91b?w=500" } }
                }
            };
        }

        protected async Task CreateOrderItem((Product? p, OrderItem? op) tupl)
        {
            if (tupl.p == null)
            {
                ShowKioskAlert("Product is required");
                return;
            }

            if (await Auth.IsAuthenticatedAsync() == false)
            {
                Nav.NavigateTo("/signin?ReturnUrl=" + Nav.ToAbsoluteUri(Nav.Uri).PathAndQuery);
                return;
            }

            if (isCreatingOrder)
            {
                ShowKioskAlert("Please wait, processing previous request");
                return;
            }

            if (Order == null || Order.OrganizationId?.ToString() != OrganizationId)
            {
                isCreatingOrder = true;
                try
                {
                    var placeId = PlaceId ?? Products.FirstOrDefault(p => p.Tags.Contains("place"))?.Id?.ToString();
                    if (string.IsNullOrWhiteSpace(placeId))
                    {
                        ShowKioskAlert("No place available for this organization");
                        return;
                    }

                    var orderService = ClientFactory.GetClient<Order>();
                    Order = await orderService.Create(new()
                    {
                        OrganizationId = Guid.TryParse(OrganizationId, out var orgG) ? orgG : null,
                        PlaceId = Guid.TryParse(placeId, out var placeG) ? placeG : null,
                    });
                }
                catch (Exception ex)
                {
                    ShowKioskAlert($"Error creating order: {ex.Message}", "Ошибка");
                    Order = null;
                    return;
                }
                finally
                {
                    isCreatingOrder = false;
                }
            }

            try
            {
                var orderProductService = ClientFactory.GetClient<OrderItem>();
                tupl.op = await orderProductService.Create(new()
                {
                    Name = tupl.p?.Name,
                    ProductId = tupl.p?.Id,
                    OrderId = Order?.Id,
                    Quantity = 1,
                });

                if (!Orders.Contains(Order))
                    Orders.Add(Order);

                OrderItems.Add(tupl.op);
                StateHasChanged();
            }
            catch (Exception ex)
            {
                ShowKioskAlert($"Error adding item: {ex.Message}", "Ошибка");
            }
        }

        protected async Task Add(OrderItem? op)
        {
            if (op == null)
                return;

            var oldQuantity = op.Quantity;
            try
            {
                var orderProductService = ClientFactory.GetClient<OrderItem>();
                ++op.Quantity;
                await orderProductService.Update(op);
                StateHasChanged();
            }
            catch (Exception ex)
            {
                ShowKioskAlert($"Error updating quantity: {ex.Message}", "Ошибка");
                op.Quantity = oldQuantity;
                StateHasChanged();
            }
        }

        protected async Task Remove(OrderItem? op)
        {
            if (op == null)
                return;

            var oldQuantity = op.Quantity;
            try
            {
                var orderProductService = ClientFactory.GetClient<OrderItem>();
                if (op.Quantity > 1)
                {
                    --op.Quantity;
                    await orderProductService.Update(op);
                }
                else
                {
                    await orderProductService.Delete(op);
                    OrderItems.Remove(op);
                }
                StateHasChanged();
            }
            catch (Exception ex)
            {
                ShowKioskAlert($"Error removing item: {ex.Message}", "Ошибка");

                if (oldQuantity == 1 && !OrderItems.Contains(op))
                {
                    OrderItems.Add(op);
                    op.Quantity = oldQuantity;
                }
                else
                {
                    op.Quantity = oldQuantity;
                }
                StateHasChanged();
            }
        }

        public IEnumerable<Order> ActiveOrders => Orders
            .Where(o => o.Status < OrderStatus.Paid && o.Id != null && o.Id != Guid.Empty)
            .GroupBy(o => o.Id)
            .Select(g => g.First())
            .OrderBy(o => o.CreatedAt);

        public async Task SwitchOrder(Order order)
        {
            Order = order;
            OrderItems.Clear();
            if (Order != null)
            {
                try
                {
                    var orderProductService = ClientFactory.GetClient<OrderItem>();
                    var orderProductStream = orderProductService.Read(new() { OrderId = Order.Id }).ResponseStream;
                    while (await orderProductStream.MoveNext(new()))
                    {
                        OrderItems.Add(orderProductStream.Current);
                    }
                }
                catch (Exception ex)
                {
                    ShowKioskAlert($"Error loading order items: {ex.Message}", "Ошибка");
                }
            }
            StateHasChanged();
        }

        public async Task CreateNewOrder()
        {
            if (isCreatingOrder)
                return;

            isCreatingOrder = true;
            try
            {
                var placeId = PlaceId ?? Products.FirstOrDefault(p => p.Tags.Contains("place"))?.Id?.ToString();
                if (string.IsNullOrWhiteSpace(placeId))
                {
                    ShowKioskAlert("No place available for this organization");
                    return;
                }

                var orderService = ClientFactory.GetClient<Order>();
                var newOrder = await orderService.Create(new()
                {
                    OrganizationId = Guid.TryParse(OrganizationId, out var orgG) ? orgG : null,
                    PlaceId = Guid.TryParse(placeId, out var placeG) ? placeG : null,
                });

                if (!Orders.Any(o => o.Id == newOrder.Id))
                {
                    Orders.Add(newOrder);
                }

                await SwitchOrder(newOrder);
            }
            catch (Exception ex)
            {
                ShowKioskAlert($"Error creating new order: {ex.Message}", "Ошибка");
            }
            finally
            {
                isCreatingOrder = false;
            }
        }

        public async Task DeleteOrder(Order order)
        {
            if (order == null)
                return;

            try
            {
                var orderService = ClientFactory.GetClient<Order>();
                await orderService.Delete(order);
                Orders.RemoveAll(o => o.Id == order.Id);

                if (Order?.Id == order.Id)
                {
                    var nextOrder = ActiveOrders.FirstOrDefault();
                    if (nextOrder != null)
                    {
                        await SwitchOrder(nextOrder);
                    }
                    else
                    {
                        Order = null;
                        OrderItems.Clear();
                    }
                }
                StateHasChanged();
            }
            catch (Exception ex)
            {
                ShowKioskAlert($"Error deleting order: {ex.Message}", "Ошибка");
            }
        }
    }
}
