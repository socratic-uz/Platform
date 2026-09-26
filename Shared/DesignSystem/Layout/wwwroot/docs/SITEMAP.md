# 🗺️ Карта сайта и матрица маршрутов (Frontend Sitemap)

Полный реестр всех маршрутов, страниц, компонентов и модулей фронтенд-платформы **Socratic**.

---

## 1. Сводная таблица маршрутов

| Маршрут (Route) | Название страницы / Модуля | Домен FSD | Целевые устройства | Файл компонента |
| :--- | :--- | :--- | :--- | :--- |
| **`/`** | Главная промо-витрина (Universal Landing Page) | `Portal/Landing` | Все (Mobile, Tablet, Desktop) | `Portal/Landing/Pages/Landing.razor` |
| **`/about`** | О платформе Socratic | `Portal/Landing` | Desktop, Mobile, Tablet | `Portal/Landing/Pages/About.razor` |
| **`/docs`** | Интерактивная документация системы | `Portal/Landing` | Desktop, Tablet | `Portal/Landing/Pages/Docs.razor` |
| **`/help`** | Центр поддержки и FAQ | `Portal/Landing` | Desktop, Mobile, Tablet | `Portal/Landing/Pages/Help.razor` |
| **`/example`** | Примеры сценариев и интеграций | `Portal/Landing` | Desktop, Tablet | `Portal/Landing/Pages/Example.razor` |
| **`/data`** | Просмотр системных данных | `Portal/Landing` | Desktop, Tablet | `Portal/Landing/Pages/Data.razor` |
| **`/player`** | Медиаплеер / Видео-презентации | `Portal/Landing` | Desktop, Mobile, Tablet | `Portal/Landing/Pages/Player.razor` |
| **`/qr`** | QR-сканер и просмотрщик кодов | `Portal/Landing` | Mobile, Tablet | `Portal/Landing/Pages/QR.razor` |
| **`/3d`** | 3D WebGL интерактивная сцена | `Portal/Landing` | Desktop, Tablet | `Portal/Landing/Pages/3d.razor` |
| **`/signin`** | Вход в систему (Пароль, SSO) | `Portal/Identity` | Все устройства | `Portal/Identity/Pages/SignIn.razor` |
| **`/signin-google-callback`** | Обработчик Google OAuth 2.0 | `Portal/Identity` | Системный | `Portal/Identity/Pages/SignInGoogleCallback.razor` |
| **`/signin-telegram-callback`** | Обработчик Telegram Login Widget | `Portal/Identity` | Системный | `Portal/Identity/Pages/SignInTelegramCallback.razor` |
| **`/home`** | Панель управления директора (Director Console) | `Portal/Organization` | Руководитель | `Portal/Organization/Pages/Home.razor` |
| **`/commerce`** | Каталог и 28 режимов Universal UX | `Retail/Commerce` | Все устройства | `Retail/Commerce/Pages/CommerceShowcase.razor` |
| **`/terminal`** | Рабочее место кассира (Cashier POS) | `Retail/POS` | Tablet POS (1024×768), Desktop | `Retail/POS/Pages/Terminal.razor` |
| **`/wishlist`** | Список избранного и отложенные товары | `Retail/POS` | Mobile, Tablet | `Retail/POS/Pages/WishList.razor` |
| **`/kiosk`** | Киоск самообслуживания / Меню гостя | `Retail/Kiosk` | Kiosk (1080×1920), Mobile | `Retail/Kiosk/Pages/Kiosk.razor` |
| **`/kiosk/{OrgId}`** | Киоск конкретной организации | `Retail/Kiosk` | Kiosk, Mobile | `Retail/Kiosk/Pages/Kiosk.razor` |
| **`/kiosk/{OrgId}/{Slug}`** | Меню филиала / категории | `Retail/Kiosk` | Kiosk, Mobile | `Retail/Kiosk/Pages/Kiosk.razor` |
| **`/store/{OrgId}`** | Онлайн-витрина филиала | `Retail/Kiosk` | Mobile, Desktop | `Retail/Kiosk/Pages/Kiosk.razor` |
| **`/store/{OrgId}/{Slug}`** | Категория витрины филиала | `Retail/Kiosk` | Mobile, Desktop | `Retail/Kiosk/Pages/Kiosk.razor` |
| **`/kiosk/{OrgId}/product/{Id}`** | Карточка блюда / товара в киоске | `Retail/Kiosk` | Kiosk, Mobile | `Retail/Kiosk/Pages/ProductPage.razor` |
| **`/store/{OrgId}/product/{Id}`** | Карточка товара в интернет-магазине | `Retail/Kiosk` | Mobile, Desktop | `Retail/Kiosk/Pages/ProductPage.razor` |
| **`/orders`** | Журнал заказов и мониторинг | `Retail/Orders` | Менеджер / Оператор | `Retail/Orders/Pages/Orders/Index.razor` |
| **`/order-items`** | Позиции заказов / Детализация чека | `Retail/Orders` | Менеджер / Оператор | `Retail/Orders/Pages/OrderItems/Index.razor` |
| **`/orders/{RouteOrderId}/items`** | Позиции конкретного заказа | `Retail/Orders` | Менеджер / Оператор | `Retail/Orders/Pages/OrderItems/Index.razor` |
| **`/payment`** | Экран оплаты (Payme, Click, Uzum, QR) | `Retail/Checkout` | Все (Kiosk, Mobile, POS) | `Retail/Checkout/Pages/Payment.razor` |
| **`/seat-designer`** | 3D/2D конструктор рассадки и залов | `Studio/SeatDesigner` | Администратор зала | `Studio/SeatDesigner/Pages/SeatDesignerPage.razor` |
| **`/qr-designer`** | Дизайнер динамических QR-кодов | `Studio/QrDesigner` | Маркетолог / Мерчант | `Studio/QrDesigner/Pages/QrDesignerPage.razor` |
| **`/map`** | Интерактивная карта филиалов и зон | `Studio/Map` | Все устройства | `Studio/Map/Pages/MapPage.razor` |
| **`/chat`** | Диалоговый ИИ-ассистент (AI Copilot) | `Intelligence/Chat` | Покупатель, Персонал | `Intelligence/Chat/Pages/ChatPage.razor` |
| **`/detector`** | Компьютерное зрение и видеоаналитика | `Intelligence/Vision` | Оператор / Охрана | `Intelligence/Vision/Pages/Detector.razor` |
| **`/components`** | Витрина дизайн-системы Material.Web | `Platform/Web.UI` | Разработчики | `Platform/Web.UI/Web.UI/Components/Pages/ComponentsShowcase.razor` |
| **`/Error`** | Страница системной ошибки | `Platform/Web.UI` | Системный | `Platform/Web.UI/Web.UI/Components/Pages/Error.razor` |

---

## 2. Разделение по типам устройств

- **Mobile (390×844 px)**: `/`, `/kiosk`, `/store`, `/wishlist`, `/payment`, `/home`, `/chat`.
- **Tablet & POS (1024×768 px)**: `/terminal`, `/commerce`, `/seat-designer`, `/home`, `/orders`.
- **Self-Service Kiosk (1080×1920 px Portrait)**: `/kiosk`, `/kiosk/{org}`, `/kiosk/{org}/product/{id}`, `/payment`.
- **Desktop & Studio (1440×900 px+)**: `/seat-designer`, `/qr-designer`, `/detector`, `/components`, `/docs`, `/home`.
