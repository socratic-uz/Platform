# MenuItem Component

## Îáçîð

`MenuItem` - ýòî Blazor êîìïîíåíò-îáåðòêà äëÿ Material Design 3 `<md-menu-item>`, êîòîðûé ïðåäñòàâëÿåò îòäåëüíûé ýëåìåíò â ìåíþ.

## Èñïîëüçîâàíèå

### Áàçîâûé ïðèìåð
```razor
<MenuItem Headline="Ñîçäàòü" StartIcon="add" OnClick="HandleCreate" />
```

### Ñ äîïîëíèòåëüíûì òåêñòîì
```razor
<MenuItem Headline="Ñîõðàíèòü ôàéë" 
          SupportingText="Ctrl+S" 
          StartIcon="save" 
          OnClick="HandleSave" />
```

### Êàê ññûëêà
```razor
<MenuItem Headline="Îòêðûòü â íîâîì îêíå" 
          Href="/external-page" 
          Target="_blank" 
          EndIcon="open_in_new" />
```

### Îòêëþ÷åííûé ýëåìåíò
```razor
<MenuItem Headline="Íåäîñòóïíî" 
          SupportingText="Òðåáóåòñÿ àâòîðèçàöèÿ" 
          StartIcon="lock" 
          Disabled="true" />
```

### Âûáðàííûé ýëåìåíò
```razor
<MenuItem Headline="Òåìíàÿ òåìà" 
          StartIcon="dark_mode" 
          Selected="@(currentTheme == ThemeType.Dark)" 
          OnClick="@(() => SetTheme(ThemeType.Dark))" />
```

## Ïàðàìåòðû

| Ïàðàìåòð | Òèï | Ïî óìîë÷àíèþ | Îïèñàíèå |
|----------|-----|--------------|----------|
| `Disabled` | `bool` | `false` | Îòêëþ÷àåò ýëåìåíò ìåíþ |
| `Type` | `string` | `"menuitem"` | Ðîëü ýëåìåíòà (menuitem, option, etc.) |
| `Href` | `string` | `""` | URL äëÿ íàâèãàöèè |
| `Target` | `string` | `""` | Öåëåâîå îêíî äëÿ ññûëêè |
| `KeepOpen` | `bool` | `false` | Îñòàâëÿòü ìåíþ îòêðûòûì ïîñëå êëèêà |
| `Selected` | `bool` | `false` | Îòîáðàæàòü ýëåìåíò êàê âûáðàííûé |
| `Headline` | `string?` | `null` | Îñíîâíîé òåêñò ýëåìåíòà |
| `SupportingText` | `string?` | `null` | Äîïîëíèòåëüíûé òåêñò |
| `StartIcon` | `string?` | `null` | Èêîíêà â íà÷àëå ýëåìåíòà |
| `EndIcon` | `string?` | `null` | Èêîíêà â êîíöå ýëåìåíòà |
| `ChildContent` | `RenderFragment?` | `null` | Ïîëüçîâàòåëüñêèé êîíòåíò |
| `OnClick` | `EventCallback<MouseEventArgs>` | - | Îáðàáîò÷èê êëèêà |

## Ñîáûòèÿ

### OnClick
Âûçûâàåòñÿ ïðè êëèêå íà ýëåìåíò ìåíþ (åñëè ýëåìåíò íå îòêëþ÷åí).

```razor
<MenuItem Headline="Óäàëèòü" 
          StartIcon="delete" 
          OnClick="@(async (e) => await ConfirmDelete())" />
```

## Accessibility

- Àâòîìàòè÷åñêè óñòàíàâëèâàåò `role="menuitem"` (ìîæíî èçìåíèòü ÷åðåç ïàðàìåòð `Type`)
- Ïîääåðæèâàåò keyboard navigation
- Ïðàâèëüíî îáðàáàòûâàåò ñîñòîÿíèå `disabled`
- Ïîääåðæèâàåò `selected` ñîñòîÿíèå ñ ñîîòâåòñòâóþùèìè ARIA àòðèáóòàìè

## Èíòåãðàöèÿ ñ Menu

`MenuItem` ïðåäíàçíà÷åí äëÿ èñïîëüçîâàíèÿ âíóòðè êîìïîíåíòà `Menu`:

```razor
<Menu Anchor="menu-anchor" @bind-Open="isOpen">
    <MenuItem Headline="Ñîçäàòü" StartIcon="add" OnClick="HandleCreate" />
    <MenuItem Headline="Îòêðûòü" StartIcon="folder_open" OnClick="HandleOpen" />
    <MenuDivider />
    <MenuItem Headline="Âûõîä" StartIcon="logout" OnClick="HandleLogout" />
</Menu>
```