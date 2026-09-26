using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.AI;
using Smart.Web.Components;
using Smart.Web.Services;
using Socratic.QrDesigner;
using Domain.Entities;
using SharedKernel.ValueObjects;
using Material.Web.Components;
using QuickGrid;

namespace Shared.Pages;

public partial class ComponentsShowcase : ComponentBase
{
    [Inject] private SmartToastService toastService { get; set; } = default!;

    private string selectedCategory = "all";
    private string searchQuery = "";
    private bool isMobileSidebarOpen = false;
    private HashSet<string> openCodeSnippets = new();

    private string lastAction = "Нажмите на любой компонент для теста";
    private bool isIconSelected = true;
    private string selectedTimePeriod = "Week";
    private bool demoCheckbox = true;
    private bool demoIndeterminateCheckbox = true;
    private bool demoSwitch = true;
    private string selectedDelivery = "courier";
    private decimal sliderValue = 65m;
    private decimal rangeStart = 150m;
    private decimal rangeEnd = 750m;
    private string pickedColor = "#0ea5e9";
    private string demoInputText = "alex_developer";
    private string demoAmountText = "250000";
    private string selectedPaymentMethod = "UzCard";
    private int activeTabIndex = 0;
    private int activeNavTab = 0;
    private string selectedChip = "pizza";
    private bool isDemoDialogOpen = false;

    private void HandleSliderChanged(decimal v) => sliderValue = v;
    private void HandleRangeStartChanged(decimal v) => rangeStart = v;
    private void HandleRangeEndChanged(decimal v) => rangeEnd = v;

    // Smart.Web State
    private string filterBarText = "";
    private WorkSchedule demoSchedule = new WorkSchedule();
    private List<FilterChip> demoFilterChips = new()
    {
        new FilterChip { Label = "В наличии", IsSelected = true },
        new FilterChip { Label = "Популярное", IsSelected = false }
    };

    private void HandleFilterChipToggled(FilterChip chip)
    {
        toastService.ShowInfo("Переключен фильтр: " + chip.Label);
    }

    // QuickGrid State
    private PaginationState quickGridPagination = new PaginationState { ItemsPerPage = 4 };
    private List<DemoGridRow> demoGridItems = new()
    {
        new DemoGridRow(1, "Пицца Маргарита", "Пицца", 65000, true),
        new DemoGridRow(2, "Бургер Классик", "Бургеры", 45000, true),
        new DemoGridRow(3, "Кофе Капучино", "Напитки", 22000, true),
        new DemoGridRow(4, "Чизкейк Нью-Йорк", "Десерты", 38000, false),
        new DemoGridRow(5, "Стейк Рибай", "Горячее", 145000, true),
        new DemoGridRow(6, "Лимонад Цитрус", "Напитки", 18000, true),
        new DemoGridRow(7, "Цезарь с курицей", "Салаты", 48000, true),
    };

    // QR State
    private string qrInputData = "https://socratic.uz/components";
    private QrStylingOptions qrStylingOptions = new QrStylingOptions
    {
        PrimaryColor = "#0ea5e9",
        CornerColor = "#6366f1",
        BackgroundColor = "#ffffff"
    };

    // Receipt State
    private List<DemoReceiptRow> demoReceiptRows = new()
    {
        new DemoReceiptRow("Пицца Пепперони 35см", 85000, 1),
        new DemoReceiptRow("Капучино XL", 25000, 2),
        new DemoReceiptRow("Соус Чесночный", 5000, 2)
    };

    // Chat Messages
    private ChatMessage demoUserChatMessage = new ChatMessage(ChatRole.User, "Здравствуйте! Подскажите, как забронировать столик на вечер?");
    private ChatMessage demoAssistantChatMessage = new ChatMessage(ChatRole.Assistant, "Добрый день! С радостью помогу. Выберите желаемый зал и время через наш интерактивный модуль рассадки.");

    // Markdown Content
    private string demoMarkdownContent = @"### Платформа Socratic UI

**Socratic UI** предоставляет набор современных компонентов для создания веб-приложений и киосков:
- ⚡ **Высокая скорость работы**: интеграция с Native AOT и Blazor WebAssembly.
- 🎨 **Material Design 3**: динамическая адаптация под светлую и темную темы.
- 📱 **Адаптивность**: поддержка мобильных устройств, планшетов и сенсорных терминалов.

| Библиотека | Назначение | Статус |
| :--- | :--- | :--- |
| `Material.Web` | Базовые элементы UI | ✅ Стабильно |
| `Smart.Web` | Комплексные виджеты | ✅ Стабильно |
| `QrDesigner` | Векторные QR-коды | ✅ Стабильно |";

    // Chat Suggestions
    private string[] demoChatSuggestions = new[]
    {
        "Показать меню на сегодня",
        "Какие акции действуют?",
        "Рассчитать время доставки"
    };

    // Domain Modules State
    private bool isAuctionModalOpen = false;
    private bool isDonationModalOpen = false;
    private bool isSubscriptionModalOpen = false;
    private bool isBookingModalOpen = false;

    private Product demoProduct = new Product
    {
        Id = Guid.NewGuid(),
        Name = "Премиум комбо Socratic",
        Price = 120000,
        Description = "Демонстрационный товар для проверки работы модулей"
    };

    private ProductUxMode demoUxMode = ProductUxMode.CatalogItem;

    private List<Select.SelectOption> demoPaymentOptions = new()
    {
        new Select.SelectOption("UzCard", "UzCard Онлайн", "Мгновенное списание", "credit_card"),
        new Select.SelectOption("Humo", "Humo Pay", "Бесконтактная оплата", "contactless"),
        new Select.SelectOption("Cash", "Наличные при получении", "Курьеру в руки", "payments")
    };

    private int TotalComponentsCount => 38;

    private void ToggleMobileSidebar() => isMobileSidebarOpen = !isMobileSidebarOpen;

    private void SelectCategory(string category)
    {
        selectedCategory = category;
        isMobileSidebarOpen = false;
    }

    private void HandleSearchInput(ChangeEventArgs e)
    {
        searchQuery = e.Value?.ToString() ?? "";
    }

    private bool MatchesCategory(string category)
    {
        return selectedCategory == "all" || selectedCategory == category;
    }

    private bool MatchesSearch(string keywords)
    {
        if (string.IsNullOrWhiteSpace(searchQuery)) return true;
        return keywords.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
               searchQuery.Contains(keywords, StringComparison.OrdinalIgnoreCase);
    }

    private void ToggleCode(string codeKey)
    {
        if (openCodeSnippets.Contains(codeKey))
            openCodeSnippets.Remove(codeKey);
        else
            openCodeSnippets.Add(codeKey);
    }

    private bool IsCodeOpen(string codeKey) => openCodeSnippets.Contains(codeKey);

    private void IncrementClick(string btnName)
    {
        lastAction = $"Клик по: {btnName} в {DateTime.Now:HH:mm:ss}";
        toastService.ShowInfo(lastAction);
    }

    public record DemoGridRow(int Id, string Name, string Category, decimal Price, bool InStock);
    public record DemoReceiptRow(string Name, decimal Price, int Quantity);
}
