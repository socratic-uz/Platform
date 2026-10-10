# Socratic Design System — Исходные стили компонентов (Source Styles)

В этой директории разработчики пишут **человекочитаемый компонентный CSS**.

## Принцип работы
Вам больше не нужно вручную открывать 10 разных файлов (`display.css`, `margin.css`, `sizing.css`, `border.css`), чтобы описать один компонент.

Вы пишете компонент классическим способом:
```css
/* src/styles/components/my-card.css */
.my-card {
    display: grid;
    width: 100%;
    padding: 1.5rem;
    border-radius: var(--md-sys-shape-expressive-card, 24px);
    background-color: var(--md-sys-color-surface-container);
    border: 1px solid var(--md-sys-color-outline-variant);
}

@media (max-width: 599.98px) {
    .my-card {
        padding: 1rem;
        border-radius: 16px;
    }
}
```

## Автоматическая компиляция и консолидация
1. **Однократная сборка**:
   ```bash
   npm run consolidate
   ```
   AST-сплиттер разберет все компоненты на свойства, сгруппирует селекторы с одинаковыми значениями под `:where(...)` и разложит их по каноническим модулям в `wwwroot/css/`.

2. **Режим реального времени (Watch)**:
   ```bash
   npm run consolidate:watch
   ```
   При любом сохранении CSS-файла в этой папке сплиттер мгновенно обновит W3C-модули и пересоберет бандл `styles.bundle.min.css`.

3. **Аудит чистоты стилей**:
   ```bash
   npm run consolidate:audit
   ```
   Проверяет текущие W3C-модули на предмет дубликатов свойств.
