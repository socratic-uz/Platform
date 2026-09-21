# SubMenu Component

## Îáçîð

`SubMenu` - ýòî Blazor êîìïîíåíò-îáåðòêà äëÿ Material Design 3 `<md-sub-menu>`, êîòîðûé ñîçäàåò âëîæåííûå ïîäìåíþ ñ ñîáñòâåííûìè ýëåìåíòàìè.

## Èñïîëüçîâàíèå

### Áàçîâûé ïðèìåð
```razor
<SubMenu ItemHeadline="Èìïîðò äàííûõ" ItemEndIcon="arrow_right">
    <MenuItem Headline="Èç CSV" StartIcon="description" OnClick="ImportCsv" />
    <MenuItem Headline="Èç Excel" StartIcon="table_chart" OnClick="ImportExcel" />
    <MenuItem Headline="Èç JSON" StartIcon="code" OnClick="ImportJson" />
</SubMenu>
```

### Ñ íàñòðîéêîé ïîçèöèîíèðîâàíèÿ
```razor
<SubMenu ItemHeadline="Ýêñïîðò" 
         ItemStartIcon="file_download"
         ItemEndIcon="arrow_right"
         AnchorCorner="start-start"
         MenuCorner="start-end">
    <MenuItem Headline="PDF" StartIcon="picture_as_pdf" OnClick="ExportPdf" />
    <MenuItem Headline="Excel" StartIcon="table_chart" OnClick="ExportExcel" />
    <MenuItem Headline="Word" StartIcon="description" OnClick="ExportWord" />
</SubMenu>
```

### Ìíîãîóðîâíåâîå âëîæåíèå
```razor
<SubMenu ItemHeadline="Íàñòðîéêè" ItemStartIcon="settings">
    <MenuItem Headline="Îáùèå" StartIcon="tune" OnClick="GeneralSettings" />
    <MenuDivider />
    
    <SubMenu ItemHeadline="Òåìà" ItemStartIcon="palette" ItemEndIcon="arrow_right">
        <MenuItem Headline="Ñâåòëàÿ" StartIcon="light_mode" OnClick="SetLightTheme" />
        <MenuItem Headline="Òåìíàÿ" StartIcon="dark_mode" OnClick="SetDarkTheme" />
        <MenuItem Headline="Ñèñòåìà" StartIcon="computer" OnClick="SetSystemTheme" />
    </SubMenu>
    
    <SubMenu ItemHeadline="ßçûê" ItemStartIcon="language" ItemEndIcon="arrow_right">
        <MenuItem Headline="Ðóññêèé" OnClick="SetRussian" />
        <MenuItem Headline="English" OnClick="SetEnglish" />
        <MenuItem Headline="O'zbek" OnClick="SetUzbek" />
    </SubMenu>
</SubMenu>
```

### Ñ çàäåðæêàìè íàâåäåíèÿ
```razor
<SubMenu ItemHeadline="Áûñòðûå äåéñòâèÿ" 
         ItemStartIcon="flash_on"
         HoverOpenDelay="200"
         HoverCloseDelay="600">
    <MenuItem Headline="Ñîõðàíèòü" StartIcon="save" OnClick="QuickSave" />
    <MenuItem Headline="Îòïðàâèòü" StartIcon="send" OnClick="QuickSend" />
</SubMenu>
```

## Ïàðàìåòðû

### Ïîçèöèîíèðîâàíèå
| Ïàðàìåòð | Òèï | Ïî óìîë÷àíèþ | Îïèñàíèå |
|----------|-----|--------------|----------|
| `AnchorCorner` | `string` | `"start-end"` | Óãîë ïðèâÿçêè äëÿ ïîäìåíþ |
| `MenuCorner` | `string` | `"start-start"` | Óãîë ìåíþ äëÿ âûðàâíèâàíèÿ |

### Ïîâåäåíèå ïðè íàâåäåíèè
| Ïàðàìåòð | Òèï | Ïî óìîë÷àíèþ | Îïèñàíèå |
|----------|-----|--------------|----------|
| `HoverOpenDelay` | `int` | `400` | Çàäåðæêà îòêðûòèÿ ïðè íàâåäåíèè (ìñ) |
| `HoverCloseDelay` | `int` | `400` | Çàäåðæêà çàêðûòèÿ ïðè óõîäå êóðñîðà (ìñ) |

### Ñâîéñòâà ýëåìåíòà ìåíþ
| Ïàðàìåòð | Òèï | Ïî óìîë÷àíèþ | Îïèñàíèå |
|----------|-----|--------------|----------|
| `ItemDisabled` | `bool` | `false` | Îòêëþ÷èòü ýëåìåíò ïîäìåíþ |
| `ItemSelected` | `bool` | `false` | Ïîêàçàòü ýëåìåíò êàê âûáðàííûé |
| `ItemHeadline` | `string?` | `null` | Òåêñò ýëåìåíòà ïîäìåíþ |
| `ItemSupportingText` | `string?` | `null` | Äîïîëíèòåëüíûé òåêñò |
| `ItemStartIcon` | `string?` | `null` | Èêîíêà â íà÷àëå |
| `ItemEndIcon` | `string?` | `null` | Èêîíêà â êîíöå (îáû÷íî ñòðåëêà) |
| `ItemContent` | `RenderFragment?` | `null` | Ïîëüçîâàòåëüñêèé êîíòåíò ýëåìåíòà |

### Êîíòåíò è ñîáûòèÿ
| Ïàðàìåòð | Òèï | Îïèñàíèå |
|----------|-----|----------|
| `ChildContent` | `RenderFragment?` | Ñîäåðæèìîå ïîäìåíþ |
| `OnItemClick` | `EventCallback<MouseEventArgs>` | Îáðàáîò÷èê êëèêà ïî ýëåìåíòó |
| `MenuAttributes` | `Dictionary<string, object>` | Äîïîëíèòåëüíûå àòðèáóòû äëÿ ìåíþ |

## Ñîáûòèÿ

### OnItemClick
Âûçûâàåòñÿ ïðè êëèêå íà îñíîâíîé ýëåìåíò ïîäìåíþ:

```razor
<SubMenu ItemHeadline="Äîïîëíèòåëüíî" 
         OnItemClick="@((e) => LogMenuItemClick("Äîïîëíèòåëüíî"))">
    <!-- Ñîäåðæèìîå ïîäìåíþ -->
</SubMenu>
```

## Èíòåãðàöèÿ ñ Menu

`SubMenu` äîëæåí èñïîëüçîâàòüñÿ âíóòðè `Menu` ñ óñòàíîâëåííûì àòðèáóòîì `HasOverflow`:

```razor
<Menu Anchor="menu-anchor" @bind-Open="isOpen" HasOverflow="true">
    <MenuItem Headline="Ñîçäàòü ôàéë" StartIcon="note_add" OnClick="CreateFile" />
    <MenuItem Headline="Îòêðûòü ïàïêó" StartIcon="folder_open" OnClick="OpenFolder" />
    <MenuDivider />
    
    <SubMenu ItemHeadline="Íåäàâíèå ôàéëû" ItemStartIcon="history" ItemEndIcon="arrow_right">
        <MenuItem Headline="document.docx" StartIcon="description" OnClick="OpenRecent1" />
        <MenuItem Headline="spreadsheet.xlsx" StartIcon="table_chart" OnClick="OpenRecent2" />
        <MenuItem Headline="presentation.pptx" StartIcon="slideshow" OnClick="OpenRecent3" />
    </SubMenu>
    
    <MenuDivider />
    <MenuItem Headline="Íàñòðîéêè" StartIcon="settings" OnClick="OpenSettings" />
</Menu>
```

## Accessibility

- Àâòîìàòè÷åñêè óïðàâëÿåò ARIA àòðèáóòàìè äëÿ âëîæåííûõ ìåíþ
- Ïîääåðæèâàåò keyboard navigation
- Ïðàâèëüíî îáðàáàòûâàåò ôîêóñ ïðè îòêðûòèè/çàêðûòèè
- Ãåíåðèðóåò ñîáûòèÿ äëÿ óïðàâëåíèÿ parent menu

## Ïîçèöèîíèðîâàíèå

### Óãëû ïðèâÿçêè (AnchorCorner)
- `"start-start"` - âåðõíèé ëåâûé ê âåðõíåìó ëåâîìó
- `"start-end"` - âåðõíèé ëåâûé ê âåðõíåìó ïðàâîìó  
- `"end-start"` - âåðõíèé ïðàâûé ê âåðõíåìó ëåâîìó
- `"end-end"` - âåðõíèé ïðàâûé ê âåðõíåìó ïðàâîìó

### Óãëû ìåíþ (MenuCorner)
- `"start-start"` - âûðàâíèâàíèå ïî âåðõíåìó ëåâîìó óãëó
- `"start-end"` - âûðàâíèâàíèå ïî âåðõíåìó ïðàâîìó óãëó
- `"end-start"` - âûðàâíèâàíèå ïî íèæíåìó ëåâîìó óãëó
- `"end-end"` - âûðàâíèâàíèå ïî íèæíåìó ïðàâîìó óãëó