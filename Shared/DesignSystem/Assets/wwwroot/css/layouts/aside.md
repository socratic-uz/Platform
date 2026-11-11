# Архитектура `<Aside>` (`layouts/aside.css`)

Единый термин для `<md-dialog>` внутри `<aside>` — **Панель**:
- **Встроенная панель (`Встр.`)** — `md-dialog[docked]` в потоке сетки `body` (всегда открыта на десктопе, сдвигает `<main>`).
- **Всплывающая панель (`Вспл.`)** — `md-dialog` поверх экрана (`position: fixed`), открывается кнопкой-триггером (`Панель ← Кнопка (расположение)`).
- **Взаимное исключение (Mutual Exclusion)**: в один момент времени может быть открыта только **одна** модальная всплывающая панель (открытие новой автоматически закрывает предыдущую).

---

## Эргономика триггеров и поведение шторок на мобильных устройствах (< 840px)

- ☰ **Левый угол `header`** ➔ **`Вспл. лев.`** (Левая шторка: Главное меню навигации, AI чаты, Документация):
  - Занимает **всю высоту** экрана (`top: 0; bottom: 0; height: 100dvh`), перекрывая нижний бар.
  - Полноэкранный затемняющий скрим (`z-index: 1000`, `inset: 0`) закрывает экран и нижний бар; клик по скриму закрывает шторку.
- ⚡ **Правый нижний угол `main` (FAB)** ➔ **`Вспл. прав.`** (Правая шторка: Корзина, Чек, Параметры, Инструменты):
  - Занимает **всю высоту** экрана (`top: 0; bottom: 0; height: 100dvh`), перекрывая нижний бар.
  - Полноэкранный затемняющий скрим (`z-index: 1000`, `inset: 0`) закрывает экран и нижний бар; клик по скриму закрывает шторку.
- 👤 **Правый угол `header` (Аватар)** ➔ **`Вспл. ниж.`** (Нижняя шторка: Профиль пользователя):
  - Открывается от самого низа экрана (`bottom: 0`, поверх нижнего бара) со скруглением верхних углов (`28px 28px 0 0`).
  - Полноэкранный затемняющий скрим (`z-index: 1000`, `inset: 0`) закрывает фон; клик по скриму закрывает шторку.

---

## Архитектура скрима (Затемнение фона `#sheet-scrim`)

1. **Где скрим ЕСТЬ (Modal Scrim)**:
   - **Mobile (`< 840px`)**: все всплывающие шторки (`Вспл. лев.`, `Вспл. прав.`, `Вспл. ниж.` Профиль). Скрим полноэкранный (`inset: 0`, `100dvh`), перекрывает нижний бар.
   - **Desktop (`≥ 840px`)**: все всплывающие шторки поверх экрана (`Вспл. прав.` — Корзина в Киоске; `Вспл. лев.`/`Вспл. прав.` — Профиль).
2. **Где скрима НЕТ**:
   - **Встроенные панели (`[docked]`) на Desktop**: AI-чат (`ChatPage`), Документация (`Docs`), Чек (`Terminal`), Детектор (`Detector`), QR (`Qr`), Кабинет (`Payment`), Сводка (`OrderItems`), Опции QR (`QrDesigner`), Тулбар (`SeatDesigner`). Они находятся в потоке сетки `body` и не перекрывают контент.
   - **Карта (`MapPage`)**: поисковая панель `[map-controls]` не имеет скрима, сохраняя интерактивность карты для зума и панорамирования.

---

## Матрица панелей и их триггеров по страницам

<table>
  <thead>
    <tr>
      <th rowspan="2">№</th>
      <th rowspan="2">Страница (Маршрут)</th>
      <th rowspan="2">Рельс /<br>Ниж. бар</th>
      <th colspan="5">Desktop (≥ 840px)</th>
      <th colspan="3">Mobile (&lt; 840px)</th>
    </tr>
    <tr>
      <th>Встр. лев.</th>
      <th>Вспл. лев.</th>
      <th>Вспл. прав.</th>
      <th>Вспл. ниж.</th>
      <th>Встр. прав.</th>
      <th>Вспл. лев.</th>
      <th>Вспл. прав.</th>
      <th>Вспл. ниж.</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td>1</td>
      <td><code>Kiosk</code> (<code>/kiosk</code>)</td>
      <td>✅</td>
      <td>—</td>
      <td><b>Профиль</b> ← Аватар (низ рельса)</td>
      <td><b>Корзина</b> ← FAB (прав. низ <code>main</code>)</td>
      <td>⚪</td>
      <td>—</td>
      <td>⚪</td>
      <td><b>Корзина</b> ← FAB (прав. низ <code>main</code>)</td>
      <td><b>Профиль</b> ← Аватар (прав. угол <code>header</code>)</td>
    </tr>
    <tr>
      <td>2</td>
      <td><code>Terminal</code> (<code>/terminal</code>)</td>
      <td>✅</td>
      <td>—</td>
      <td><b>Профиль</b> ← Аватар (низ рельса)</td>
      <td>⚪</td>
      <td>⚪</td>
      <td><b>Чек</b> (всегда)</td>
      <td>⚪</td>
      <td><b>Чек</b> ← FAB (прав. низ <code>main</code>)</td>
      <td><b>Профиль</b> ← Аватар (прав. угол <code>header</code>)</td>
    </tr>
    <tr>
      <td>3</td>
      <td><code>ChatPage</code> (<code>/chat</code>)</td>
      <td>✅</td>
      <td><b>AI</b> (всегда)</td>
      <td><b>Профиль</b> ← Аватар (низ рельса)</td>
      <td>⚪</td>
      <td>⚪</td>
      <td>—</td>
      <td><b>AI</b> ← ☰ (лев. угол <code>header</code>)</td>
      <td>⚪</td>
      <td><b>Профиль</b> ← Аватар (прав. угол <code>header</code>)</td>
    </tr>
    <tr>
      <td>4</td>
      <td><code>Detector</code> (<code>/detector</code>)</td>
      <td>✅</td>
      <td><b>Стрим</b> (всегда)</td>
      <td><b>Профиль</b> ← Аватар (низ рельса)</td>
      <td>⚪</td>
      <td>⚪</td>
      <td>—</td>
      <td>⚪</td>
      <td><b>Стрим</b> ← FAB (прав. низ <code>main</code>)</td>
      <td><b>Профиль</b> ← Аватар (прав. угол <code>header</code>)</td>
    </tr>
    <tr>
      <td>5</td>
      <td><code>Docs</code> (<code>/docs</code>)</td>
      <td>✅</td>
      <td><b>Доки</b> (всегда)</td>
      <td><b>Профиль</b> ← Аватар (низ рельса)</td>
      <td>⚪</td>
      <td>⚪</td>
      <td>—</td>
      <td><b>Доки</b> ← ☰ (лев. угол <code>header</code>)</td>
      <td>⚪</td>
      <td><b>Профиль</b> ← Аватар (прав. угол <code>header</code>)</td>
    </tr>
    <tr>
      <td>6</td>
      <td><code>Qr</code> (<code>/qr</code>)</td>
      <td>✅</td>
      <td><b>QR</b> (всегда)</td>
      <td><b>Профиль</b> ← Аватар (низ рельса)</td>
      <td>⚪</td>
      <td>⚪</td>
      <td>—</td>
      <td>⚪</td>
      <td><b>QR</b> ← FAB (прав. низ <code>main</code>)</td>
      <td><b>Профиль</b> ← Аватар (прав. угол <code>header</code>)</td>
    </tr>
    <tr>
      <td>7</td>
      <td><code>Payment</code> (<code>/payment</code>)</td>
      <td>✅</td>
      <td><b>Кабинет</b> (всегда)</td>
      <td><b>Профиль</b> ← Аватар (низ рельса)</td>
      <td>⚪</td>
      <td>⚪</td>
      <td>—</td>
      <td>⚪</td>
      <td><b>Кабинет</b> ← FAB (прав. низ <code>main</code>)</td>
      <td><b>Профиль</b> ← Аватар (прав. угол <code>header</code>)</td>
    </tr>
    <tr>
      <td>8</td>
      <td><code>OrderItems</code> (<code>/order-items</code>)</td>
      <td>✅</td>
      <td><b>Сводка</b> (всегда)</td>
      <td><b>Профиль</b> ← Аватар (низ рельса)</td>
      <td>⚪</td>
      <td>⚪</td>
      <td>—</td>
      <td>⚪</td>
      <td><b>Сводка</b> ← FAB (прав. низ <code>main</code>)</td>
      <td><b>Профиль</b> ← Аватар (прав. угол <code>header</code>)</td>
    </tr>
    <tr>
      <td>9</td>
      <td><code>QrDesigner</code> (<code>/qr-designer</code>)</td>
      <td>✅</td>
      <td><b>Опции QR</b> (всегда)</td>
      <td><b>Профиль</b> ← Аватар (низ рельса)</td>
      <td>⚪</td>
      <td>⚪</td>
      <td>—</td>
      <td>⚪</td>
      <td><b>Опции QR</b> ← FAB (прав. низ <code>main</code>)</td>
      <td><b>Профиль</b> ← Аватар (прав. угол <code>header</code>)</td>
    </tr>
    <tr>
      <td>10</td>
      <td><code>SeatDesigner</code> (<code>/seat-designer</code>)</td>
      <td>✅</td>
      <td><b>Тулбар</b> (всегда)</td>
      <td><b>Профиль</b> ← Аватар (низ рельса)</td>
      <td>⚪</td>
      <td>⚪</td>
      <td>—</td>
      <td>⚪</td>
      <td><b>Тулбар</b> ← FAB (прав. низ <code>main</code>)</td>
      <td><b>Профиль</b> ← Аватар (прав. угол <code>header</code>)</td>
    </tr>
    <tr>
      <td>11</td>
      <td><code>MapPage</code> (<code>/map</code>)</td>
      <td>✅</td>
      <td>—</td>
      <td><b>Поиск</b> (сразу)<br><b>Профиль</b> ← Аватар (низ рельса)</td>
      <td>⚪</td>
      <td>⚪</td>
      <td>—</td>
      <td>⚪</td>
      <td>⚪</td>
      <td><b>Поиск</b> ← Ручка / поле (над баром)<br><b>Профиль</b> ← Аватар (прав. угол <code>header</code>)</td>
    </tr>
    <tr>
      <td>12</td>
      <td><code>Home</code> (<code>/home</code>)</td>
      <td>✅</td>
      <td>—</td>
      <td><b>Профиль</b> ← Аватар (низ рельса)</td>
      <td>⚪</td>
      <td>⚪</td>
      <td>—</td>
      <td>⚪</td>
      <td>⚪</td>
      <td><b>Профиль</b> ← Аватар (прав. угол <code>header</code>)</td>
    </tr>
    <tr>
      <td>13</td>
      <td><code>WishList</code> (<code>/wishlist</code>)</td>
      <td>✅</td>
      <td>—</td>
      <td><b>Профиль</b> ← Аватар (низ рельса)</td>
      <td>⚪</td>
      <td>⚪</td>
      <td>—</td>
      <td>⚪</td>
      <td>⚪</td>
      <td><b>Профиль</b> ← Аватар (прав. угол <code>header</code>)</td>
    </tr>
    <tr>
      <td>14</td>
      <td><code>ProductPage</code> (<code>/product</code>)</td>
      <td>✅</td>
      <td>—</td>
      <td><b>Профиль</b> ← Аватар (низ рельса)</td>
      <td>⚪</td>
      <td>⚪</td>
      <td>—</td>
      <td>⚪</td>
      <td>⚪</td>
      <td><b>Профиль</b> ← Аватар (прав. угол <code>header</code>)</td>
    </tr>
    <tr>
      <td>15</td>
      <td><code>OrdersPage</code> (<code>/orders</code>)</td>
      <td>✅</td>
      <td>—</td>
      <td><b>Профиль</b> ← Аватар (низ рельса)</td>
      <td>⚪</td>
      <td>⚪</td>
      <td>—</td>
      <td>⚪</td>
      <td>⚪</td>
      <td><b>Профиль</b> ← Аватар (прав. угол <code>header</code>)</td>
    </tr>
    <tr>
      <td>16</td>
      <td><code>Analytics</code> (<code>/analytics</code>)</td>
      <td>✅</td>
      <td>—</td>
      <td><b>Профиль</b> ← Аватар (низ рельса)</td>
      <td>⚪</td>
      <td>⚪</td>
      <td>—</td>
      <td>⚪</td>
      <td>⚪</td>
      <td><b>Профиль</b> ← Аватар (прав. угол <code>header</code>)</td>
    </tr>
    <tr>
      <td>17</td>
      <td><code>ModelViewer</code> (<code>/3d</code>)</td>
      <td>✅</td>
      <td>—</td>
      <td><b>Профиль</b> ← Аватар (низ рельса)</td>
      <td>⚪</td>
      <td>⚪</td>
      <td>—</td>
      <td>⚪</td>
      <td>⚪</td>
      <td><b>Профиль</b> ← Аватар (прав. угол <code>header</code>)</td>
    </tr>
    <tr>
      <td>18</td>
      <td><code>Landing</code> (<code>/</code>)</td>
      <td>—</td>
      <td>—</td>
      <td>⚪</td>
      <td><b>Профиль</b> ← Аватар (прав. угол <code>header</code>)</td>
      <td>⚪</td>
      <td>—</td>
      <td><b>Меню</b> ← ☰ (лев. угол <code>header</code>)</td>
      <td>⚪</td>
      <td><b>Профиль</b> ← Аватар (прав. угол <code>header</code>)</td>
    </tr>
    <tr>
      <td>19</td>
      <td><code>About</code> (<code>/about</code>)</td>
      <td>—</td>
      <td>—</td>
      <td>⚪</td>
      <td><b>Профиль</b> ← Аватар (прав. угол <code>header</code>)</td>
      <td>⚪</td>
      <td>—</td>
      <td><b>Меню</b> ← ☰ (лев. угол <code>header</code>)</td>
      <td>⚪</td>
      <td><b>Профиль</b> ← Аватар (прав. угол <code>header</code>)</td>
    </tr>
    <tr>
      <td>20</td>
      <td><code>Help</code> (<code>/help</code>)</td>
      <td>—</td>
      <td>—</td>
      <td>⚪</td>
      <td><b>Профиль</b> ← Аватар (прав. угол <code>header</code>)</td>
      <td>⚪</td>
      <td>—</td>
      <td><b>Меню</b> ← ☰ (лев. угол <code>header</code>)</td>
      <td>⚪</td>
      <td><b>Профиль</b> ← Аватар (прав. угол <code>header</code>)</td>
    </tr>
    <tr>
      <td>21</td>
      <td><code>Player</code> (<code>/player</code>)</td>
      <td>—</td>
      <td>—</td>
      <td>—</td>
      <td>—</td>
      <td>—</td>
      <td>—</td>
      <td>—</td>
      <td>—</td>
      <td>—</td>
    </tr>
    <tr>
      <td>22</td>
      <td><code>SignIn</code> (<code>/signin</code>)</td>
      <td>—</td>
      <td>—</td>
      <td>—</td>
      <td>—</td>
      <td>—</td>
      <td>—</td>
      <td>—</td>
      <td>—</td>
      <td>—</td>
    </tr>
    <tr>
      <td>23</td>
      <td><code>SignInTg</code> (<code>/signin-telegram</code>)</td>
      <td>—</td>
      <td>—</td>
      <td>—</td>
      <td>—</td>
      <td>—</td>
      <td>—</td>
      <td>—</td>
      <td>—</td>
      <td>—</td>
    </tr>
    <tr>
      <td>24</td>
      <td><code>SignInGoogle</code> (<code>/signin-google</code>)</td>
      <td>—</td>
      <td>—</td>
      <td>—</td>
      <td>—</td>
      <td>—</td>
      <td>—</td>
      <td>—</td>
      <td>—</td>
      <td>—</td>
    </tr>
  </tbody>
</table>

> **Сокращения**: `Встр.` — встроенная панель (`[docked]`) | `Вспл.` — всплывающая панель (`position: fixed`) | `✅` — активно | `⚪` — доступно по атрибуту | `—` — нет
