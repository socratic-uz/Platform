# Архитектура модальных диалогов (`foundations/dialogs.css`)

Единая обёртка — [MaterialDialog.razor](../../../../Material.Web/Components/MaterialDialog.razor) (`<md-dialog>`).

> Боковые шторки и встроенные панели `[docked]` внутри `<Aside>` описаны отдельно в [layouts/aside.md](../layouts/aside.md). Здесь зафиксирован контракт и инвентарь **центрованных, alert и fullscreen модальных диалогов**.

---

## 1. Контракт стилей и слотов (`foundations/dialogs.css`)

- **Контейнер `md-dialog`**:
  - Базовая ширина: `min(320px, 100vw - 24px)` ... `min(560px, 100vw - 24px)`, высота: `max-height: calc(100dvh - 24px)`.
  - Скругление: `--md-dialog-container-shape: var(--md-sys-shape-corner-extra-large, 28px)`.
  - Варианты размеров: `[medium]` (`560px`), `[large]` (`780px`), `[extra-large]` / `[wide]` (`1020px`), `[fullscreen]` (`100vw × 100dvh`, `border-radius: 0`).
- **Слот заголовка (`header[slot="headline"]` / `[slot="headline"]`)**:
  - Сетка `grid-template-columns: minmax(0, 1fr) auto;` (заголовок + кнопка закрытия `✕`).
  - Внутри `hgroup`: `grid-template-columns: auto minmax(0, 1fr); min-width: 0;`, а текст заголовка (`span`, `h2`–`h4`) обрезается через `overflow: hidden; text-overflow: ellipsis; white-space: nowrap;`, чтобы длинное название никогда не наезжало на `✕`.
- **Слот контента (`section[slot="content"]` / `form[slot="content"]` / `[slot="content"]`)**:
  - Вертикальный `grid` с `gap: var(--md-sys-spacing-4)`, `min-width: 0;`, `max-height: min(68dvh, 600px);`, `overflow-y: auto;`, `overscroll-behavior: contain;`.
  - Вложенные `fieldset` внутри контента сбрасывают внешнюю рамку (`border: none; padding: 0;`), а `label` стилизованы как карточки выбора.
- **Слот действий (`footer[slot="actions"]` / `[slot="actions"]`)**:
  - Горизонтальный ряд кнопок справа (`grid-auto-flow: column; justify-content: end;`).

---

## 2. Матрица модальных диалогов

### A. Платформенные, Checkout, POS и Studio (20 модалок)

| № | Компонент / Страница | Атрибуты `<MaterialDialog>` / `<md-dialog>` | Слоты (`H` / `C` / `A`) | Назначение и примечания |
|---:|---|---|:-:|:---|
| 1 | `MaterialDialog` (`Material.Web`) | Базовый `<md-dialog>` | `H` / `C` / `A` | Базовый шаблон (`Headline`, `ChildContent`, `Actions`) |
| 2 | `SmartForm` (`Smart.Web`) | `smart-form`, `id=@Id` | `H` / `C` / `A` | Автогенерируемая CRUD-форма по модели `TItem` |
| 3 | `SmartSchedulePicker` (`Smart.Web`) | — | `H` / `C` / `A` | Выбор расписания / интервалов |
| 4 | `ColorPicker` (`Material.Web`) | `color-picker-popover` | `H` / `C` / `A` | Палитра выбора цвета |
| 5 | `ReconnectModal` (`Web.UI`) | `id="components-reconnect-modal"` | `—` / `C` / `—` | Системный диалог переподключения SignalR |
| 6 | `Avatar` (`Layout`) | `faceid-enrollment`, `Type="alert"` | `H` / `C` / `A` | Регистрация биометрии Face ID |
| 7 | `OrganizationSubscriptionCard` (`Layout`) | — | `H` / `C` / `A` | Смена тарифа организации |
| 8 | `ProductPromotionModal` (`Layout`) | — | `H` / `C` / `A` | Настройка продвижения товара |
| 9 | `Analytics` (`Portal/Landing`) | `config` | `H` / `C` / `A` | Конфигурация дашборда аналитики |
| 10 | `Landing` (`Portal/Landing`) | — | `H` / `C` / `A` | Быстрый просмотр коммерческого режима |
| 11 | `Payment` (`Retail/Checkout`) | — (график платежей) | `H` / `C` / `A` | График платежей рассрочки (внутри таблицы) |
| 12 | `Payment` (`Retail/Checkout`) | — (FaceID KYC) | `H` / `C` / `A` | Биометрическая KYC-верификация плательщика |
| 13 | `Kiosk` (`Retail/PointOfSale`) | — (Checkout, Receipt, Alert, Subscription) | `H` / `C` / `A` | 4 модальных сценария киоска самообслуживания |
| 14 | `Terminal` (`Retail/PointOfSale`) | — (Subscription) | `H` / `C` / `A` | Модалка подписки / тарифа в терминале |
| 15 | `PosPaymentDialog` (`Retail/PointOfSale`) | `payment` | `H` / `C` / `A` | Приём оплаты в POS |
| 16 | `PosReceiptDialog` (`Retail/PointOfSale`) | — | `H` / `C` / `A` | Просмотр и печать чека |
| 17 | `MapPopup` (`Studio/Map`) | `aria-modal="true"` | `—` / `C` / `—` | Попап деталей объекта на карте (без стандартных слотов `H`/`A`) |
| 18 | `SeatBookingModal` (`Retail/Commerce`) | `seat-booking`, `aria-modal="true"` | `—` / `C` / `—` | Бронирование мест на схеме (без стандартных слотов `H`/`A`) |
| 19 | `BookingMetadataDialog` (`Retail/Commerce`) | `booking` | `H` / `C` / `A` | Детали метаданных бронирования |
| 20 | `ProductVariantSelector` (`Retail/Commerce`) | `variant-selector` | `H` / `C` / `A` | Выбор вариации товара |

*(Остальные вхождения `<MaterialDialog>` — боковые шторки и `[docked]` панели в `ChatPage`, `Detector`, `Avatar[profile]`, `LandingHeader`, `Docs`, `Qr`, `Payment[docked]`, `OrderDrawer`, `TransientCartDrawer`, `OrderItemsPage`, `Terminal[invoice]`, `ControlsPanel`, `QrDesignerPage`, `SeatDesignerToolbar` — описаны в [aside.md](../layouts/aside.md).)*

---

### B. Модалки коммерческих режимов (`Retail/Commerce/Widgets/Modes/`, 16 шт.)

| № | Компонент | Атрибут | Слоты (`H` / `C` / `A`) | Примечания |
|---:|---|---|:-:|:---|
| 1 | `AuctionBiddingFeature` | `auction` | `H` / `C` / `A` | Ставки аукциона |
| 2 | `BookingCalendarFeature` | `booking` | `H` / `C` / `—` | Календарь бронирования (нет стандартного `slot="actions"`) |
| 3 | `ClassifiedListingFeature` | `classified` | `H` / `C` / `A` | Объявление / контакты |
| 4 | `ConfiguratorFeature` | `configurator` | `H` / `C` / `A` | Конфигуратор товара |
| 5 | `DeliveryScheduledFeature` | `delivery` | `H` / `C` / `A` | Выбор слота доставки |
| 6 | `DigitalGoodsFeature` | `digital` | `H` / `C` / `A` | Цифровой товар / лицензия |
| 7 | `DonationWidgetFeature` | `donation` | `H` / `C` / `A` | Пожертвование / выбор суммы |
| 8 | `GiftCertificateFeature` | `gift` | `H` / `C` / `A` | Подарочный сертификат |
| 9 | `GroupBuyFeature` | `groupbuy` | `H` / `C` / `A` | Групповая покупка |
| 10 | `HotelBookingFeature` | `hotel` | `H` / `C` / `—` | Бронирование номера (нет стандартного `slot="actions"`) |
| 11 | `LocationRouteFeature` | `route` | `H` / `C` / `—` | Маршрут / поездка (нет стандартного `slot="actions"`) |
| 12 | `PriceQuoteFormFeature` | `quote` | `H` / `C` / `A` | Запрос расчёта сметы |
| 13 | `ProductBundleFeature` | `bundle` | `H` / `C` / `A` | Комплект / набор товаров |
| 14 | `SeatPickerFeature` | `lottery` | `H` / `C` / `A` | Выбор билета / места |
| 15 | `SubscriptionPlanFeature` | `subscription` | `H` / `C` / `A` | Подписка / тарифный план |
| 16 | `VehicleRentalFeature` | `rental` | `H` / `C` / `A` | Аренда транспорта |

---

## 3. Типовые зоны внимания при ревью диалогов

1. **Длинные заголовки и кнопка закрытия `✕`**: не задавайте `grid-auto-columns: max-content` внутри `hgroup` шапки — `dialogs.css` гарантирует обрезку через `minmax(0, 1fr)` и `ellipsis`.
2. **Дубли заголовков**: избегайте повторения заголовка из `slot="headline"` вторым крупным блоком с иконкой в начале `slot="content"`, чтобы не съедать вертикальное пространство на мобильных экранах.
3. **Нестандартные модалки без `slot="headline"` / `slot="actions"`**: `SeatBookingModal`, `MapPopup`, `BookingCalendarFeature`, `HotelBookingFeature`, `LocationRouteFeature` не используют стандартный `slot="actions"` (либо `slot="headline"`), из-за чего кнопки подтверждения прокручиваются вместе с контентом вместо фиксации в футере диалога.
4. **Локализация подзаголовков**: не подставляйте сырые enum-ключи (например, `(Specialist)`) в видимые заголовки и подзаголовки.
