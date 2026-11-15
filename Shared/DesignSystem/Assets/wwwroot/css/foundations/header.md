# Архитектура `<Header>` (`foundations/header.css` и `foundations/landmarks.css`)

Единый шаблон глобальной шапки ([Header.razor](../../../../Layout/Layout/Header.razor)):
- Всякий `<Header ...>` рендерит `<header ...>@ChildContent<Avatar /></header>` в секцию `HeaderOutlet` (`body > header`).
- Везде вместо связки `Иконка + Socratic` используется монохромный логотип [socratic.svg](../../img/socratic.svg) (`32×32px`, `grayscale(1)` с адаптацией яркости для Light/Dark тем), отделённый от заголовка раздела вертикальным разделителем `1px × 20px` (`::after`, `var(--md-sys-color-outline-variant)`). Иконка раздела рядом с логотипом не дублируется (навигационные иконки уже есть в `Navigation Rail` / нижней панели).
- На **Desktop (`≥ 840px`)** элементы `[drawer]` скрыты (`display: none`), навигация/поиск/действия располагаются в одну строку.
- На **Mobile (`< 840px`)** активируются кнопки `[drawer]` (`display: inline-grid`), а на `< 600px` второстепенные подписи и кнопки сжимаются в иконки.

---

## Матрица `<Header>` по страницам (24 страницы)

<table>
  <thead>
    <tr>
      <th>№</th>
      <th>Страница (Маршрут)</th>
      <th>Атрибут <code>&lt;Header&gt;</code></th>
      <th>Левый слот</th>
      <th>Центральный слот</th>
      <th>Правый слот (кроме <code>Avatar</code>)</th>
      <th>Состояние на Mobile (&lt; 840px / &lt; 600px)</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td>1</td>
      <td><code>Kiosk</code> (<code>/kiosk</code>)</td>
      <td><code>kiosk</code></td>
      <td>Лого/иконка организации + <code>Название</code> (скрыто на <code>&lt; 600px</code>)</td>
      <td><code>&lt;Search /&gt;</code> (<code>md-outlined-text-field</code>)</td>
      <td>—</td>
      <td>🟢 Компактно (на <code>&lt; 600px</code> поиск между лого и аватаром)</td>
    </tr>
    <tr>
      <td>2</td>
      <td><code>Terminal</code> (<code>/terminal</code>)</td>
      <td><code>terminal</code></td>
      <td>Логотип <code>Socratic</code> + <code>&lt;h1&gt;Терминал&lt;/h1&gt;</code></td>
      <td>—</td>
      <td>Кнопка <code>+ Товар</code> (на <code>&lt; 600px</code> только <code>+</code>)</td>
      <td>🟢 Свободно (4 доп. кнопки перенесены в <code>main[terminal]</code>)</td>
    </tr>
    <tr>
      <td>3</td>
      <td><code>ChatPage</code> (<code>/chat</code>)</td>
      <td><code>chat</code></td>
      <td>☰ <code>[drawer]</code> + <code>&lt;hgroup&gt;</code> (<code>←</code> + <code>Socratic AI</code>)</td>
      <td>—</td>
      <td>Кнопка <code>Новый диалог</code></td>
      <td>🟢 Свободно (на <code>&lt; 840px</code> <code>←</code> и кнопка скрыты, есть в ☰)</td>
    </tr>
    <tr>
      <td>4</td>
      <td><code>Detector</code> (<code>/detector</code>)</td>
      <td><code>detector</code></td>
      <td>Логотип <code>Socratic</code> + <code>AI Detector</code> (в <code>nav</code>)</td>
      <td>—</td>
      <td>—</td>
      <td>🟢 Свободно</td>
    </tr>
    <tr>
      <td>5</td>
      <td><code>Docs</code> (<code>/docs</code>)</td>
      <td><code>docs</code></td>
      <td>☰ <code>[drawer]</code> + логотип <code>Socratic</code> + <code>Документация</code></td>
      <td>—</td>
      <td>—</td>
      <td>🟢 Свободно</td>
    </tr>
    <tr>
      <td>6</td>
      <td><code>Qr</code> (<code>/qr</code>)</td>
      <td><code>qr</code></td>
      <td>Логотип <code>Socratic</code> + <code>QR-студия</code> (в <code>nav</code>)</td>
      <td>—</td>
      <td>—</td>
      <td>🟢 Свободно</td>
    </tr>
    <tr>
      <td>7</td>
      <td><code>Payment</code> (<code>/payment</code>)</td>
      <td><code>payment</code></td>
      <td>Логотип <code>Socratic</code> + <code>&lt;h1&gt;</code></td>
      <td>—</td>
      <td>До 2 кнопок (<code>Обновить</code>, <code>Демо-данные</code>) или статус загрузки</td>
      <td>🟢/🟠 Компактно (на <code>&lt; 600px</code> кнопки сжимаются в иконки)</td>
    </tr>
    <tr>
      <td>8</td>
      <td><code>OrderItemsPage</code> (<code>/orders/{id}</code>)</td>
      <td><code>orders</code></td>
      <td>Логотип <code>Socratic</code> + <code>&lt;hgroup&gt;</code> (<code>← К заказам</code> + <code>h1</code> + <code>mark</code> статуса)</td>
      <td>—</td>
      <td><code>&lt;mark&gt;</code> (итог шт.)</td>
      <td>🟠 Компактно (на <code>&lt; 600px</code> только <code>←</code>, <code>h1</code> с <code>ellipsis</code>)</td>
    </tr>
    <tr>
      <td>9</td>
      <td><code>QrDesignerPage</code> (<code>/qr-designer</code>)</td>
      <td><code>qr-designer</code></td>
      <td>Логотип <code>Socratic</code> + заголовок (в <code>nav</code>)</td>
      <td>—</td>
      <td>—</td>
      <td>🟢 Свободно</td>
    </tr>
    <tr>
      <td>10</td>
      <td><code>SeatDesignerPage</code> (<code>/seat-designer</code>)</td>
      <td><code>seat-designer</code></td>
      <td>Логотип <code>Socratic</code> + заголовок (в <code>nav</code>)</td>
      <td>—</td>
      <td>—</td>
      <td>🟢 Свободно</td>
    </tr>
    <tr>
      <td>11</td>
      <td><code>MapPage</code> (<code>/map</code>)</td>
      <td>—</td>
      <td colspan="4">Нет <code>&lt;Header&gt;</code> (полноэкранная карта)</td>
    </tr>
    <tr>
      <td>12</td>
      <td><code>Home</code> (<code>/home</code>)</td>
      <td><i>без атрибута</i></td>
      <td>Логотип <code>Socratic</code></td>
      <td><code>&lt;Search /&gt;</code> (<code>md-outlined-text-field</code>)</td>
      <td>—</td>
      <td>🟢 Свободно (логотип <code>32×32px</code> + <code>Search</code> + аватар)</td>
    </tr>
    <tr>
      <td>13</td>
      <td><code>WishList</code> (<code>/wishlist</code>)</td>
      <td><code>wishlist</code></td>
      <td>Логотип <code>Socratic</code> + <code>&lt;hgroup&gt;</code> (<code>h1</code> + <code>&lt;p&gt;</code> подзаголовок в 2 строки)</td>
      <td>—</td>
      <td>—</td>
      <td>🟢 Свободно (на <code>&lt; 600px</code> подзаголовок <code>&lt;p&gt;</code> скрыт)</td>
    </tr>
    <tr>
      <td>14</td>
      <td><code>ProductPage</code> (<code>/product/...</code>)</td>
      <td><code>product</code></td>
      <td><code>← Назад / Организация</code> + <code>h1</code> (название товара)</td>
      <td>—</td>
      <td>—</td>
      <td>🟢 На <code>&lt; 600px</code> текст ссылки «Назад» скрыт, <code>h1</code> сжимается с <code>ellipsis</code></td>
    </tr>
    <tr>
      <td>15</td>
      <td><code>Analytics</code> (<code>/analytics</code>)</td>
      <td><code>analytics</code></td>
      <td>Логотип <code>Socratic</code> + <code>&lt;h1&gt;</code></td>
      <td>—</td>
      <td>—</td>
      <td>🟢 Свободно</td>
    </tr>
    <tr>
      <td>16</td>
      <td><code>OrdersPage</code> (<code>/orders</code>)</td>
      <td><code>orders</code></td>
      <td>Логотип <code>Socratic</code> + <code>&lt;hgroup&gt;</code> (<code>h1</code> + <code>mark</code> счётчик)</td>
      <td>—</td>
      <td>Кнопка <code>Обновить</code></td>
      <td>🟢 Свободно (на <code>&lt; 600px</code> кнопка сжимается в иконку)</td>
    </tr>
    <tr>
      <td>17</td>
      <td><code>ModelViewer</code> (<code>/3d</code>)</td>
      <td>—</td>
      <td colspan="4">Нет <code>&lt;Header&gt;</code></td>
    </tr>
    <tr>
      <td>18</td>
      <td><code>Landing</code> (<code>/</code>)</td>
      <td><code>landing</code></td>
      <td>☰ <code>[drawer]</code> + логотип <code>Socratic</code></td>
      <td><code>&lt;nav&gt;</code> (4 dropdown на Desktop; на Mobile скрыто в ☰)</td>
      <td>—</td>
      <td>🟢 Свободно (на Mobile только ☰, логотип, аватар)</td>
    </tr>
    <tr>
      <td>19</td>
      <td><code>About</code> (<code>/about</code>)</td>
      <td><code>landing</code></td>
      <td>☰ <code>[drawer]</code> + логотип <code>Socratic</code></td>
      <td><code>&lt;nav&gt;</code> (скрыто на Mobile)</td>
      <td>—</td>
      <td>🟢 Свободно</td>
    </tr>
    <tr>
      <td>20</td>
      <td><code>Help</code> (<code>/help</code>)</td>
      <td><code>landing</code></td>
      <td>☰ <code>[drawer]</code> + логотип <code>Socratic</code></td>
      <td><code>&lt;nav&gt;</code> (скрыто на Mobile)</td>
      <td>—</td>
      <td>🟢 Свободно</td>
    </tr>
    <tr>
      <td>21</td>
      <td><code>Player</code> (<code>/player</code>)</td>
      <td>—</td>
      <td colspan="4">Нет <code>&lt;Header&gt;</code></td>
    </tr>
    <tr>
      <td>22</td>
      <td><code>SignIn</code> (<code>/signin</code>)</td>
      <td>—</td>
      <td colspan="4">Нет <code>&lt;Header&gt;</code> (<code>no-header</code>)</td>
    </tr>
    <tr>
      <td>23</td>
      <td><code>SignInTg</code> (<code>/signin-telegram</code>)</td>
      <td>—</td>
      <td colspan="4">Нет <code>&lt;Header&gt;</code></td>
    </tr>
    <tr>
      <td>24</td>
      <td><code>SignInGoogle</code> (<code>/signin-google</code>)</td>
      <td>—</td>
      <td colspan="4">Нет <code>&lt;Header&gt;</code></td>
    </tr>
  </tbody>
</table>

---

## Реализованные компактные правила для Mobile (`< 840px` / `< 600px`)

1. **Единый монохромный логотип (`header.css`, `landing.css`)**: на всех 13 брендовых страницах связка `<Icon Name=@AutoAwesome /> + <span>Socratic</span>` заменена на `<img src=@Socratic alt="Socratic" />` (`32×32px`), что экономит `~70–80px` ширины.
2. **Семантическое разделение слотов (`Terminal`, `Payment`, `OrdersPage`, `OrderItemsPage`)**: кнопки действий и счётчики вынесены из `<nav aria-label="Breadcrumb">` в правый слот перед `<Avatar />`.
3. **Сжатие кнопок в иконки на `< 600px` (`terminal.css`, `payment.css`, `orders.css`)**: текстовые подписи внутри `header > md-*-button > span` скрываются на узких экранах, оставляя только иконки (`+`, `Refresh`, `Science`, `←`).
4. **Разгрузка `Terminal` (`Terminal.razor`, `widgets/terminal.css`)**: 4 вторичных действия (`Организация`, `Тариф`, `Киоск`, `QR`) вынесены в горизонтально скроллируемое `<menu>` внутри `main[terminal] > section > header`.
5. **Максимум ширины для `<Search />` (`widgets/kiosk.css`)**: на `< 600px` скрывается `header[kiosk] > a > span`, оставляя логотип организации `36×36px`.
6. **Однострочность и чистота заголовков (`header.css`, `pages/wishlist.css`, `pages/orders.css`)**: в `Payment`, `Analytics`, `OrdersPage` и `WishList` убраны дубли иконок разделов рядом с логотипом (и лишние `<hgroup>` вокруг одиночных `<h1>`), добавлен вертикальный hairline-разделитель `1px × 20px`; в `WishList` на Desktop заголовок и подзаголовок идут в 2 строки, а на `< 600px` `<p>` скрывается; в `OrderItemsPage` длинный `h1` обрезается через `text-overflow: ellipsis`.
