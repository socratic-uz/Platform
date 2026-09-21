# MenuDivider Component

## Îáçîð

`MenuDivider` - ýòî ïðîñòîé Blazor êîìïîíåíò-îáåðòêà äëÿ Material Design 3 `<md-divider>`, êîòîðûé ñîçäàåò âèçóàëüíûé ðàçäåëèòåëü ìåæäó ýëåìåíòàìè ìåíþ.

## Èñïîëüçîâàíèå

### Áàçîâûé ïðèìåð
```razor
<Menu Anchor="menu-anchor" @bind-Open="isOpen">
    <MenuItem Headline="Ñîçäàòü" StartIcon="add" OnClick="HandleCreate" />
    <MenuItem Headline="Îòêðûòü" StartIcon="folder_open" OnClick="HandleOpen" />
    
    <MenuDivider />
    
    <MenuItem Headline="Íàñòðîéêè" StartIcon="settings" OnClick="OpenSettings" />
    <MenuItem Headline="Î ïðîãðàììå" StartIcon="info" OnClick="ShowAbout" />
    
    <MenuDivider />
    
    <MenuItem Headline="Âûõîä" StartIcon="logout" OnClick="HandleLogout" />
</Menu>
```

### Â êîíòåêñòå ãðóïïèðîâêè äåéñòâèé
```razor
<Menu Anchor="context-menu" @bind-Open="contextMenuOpen">
    <!-- Îñíîâíûå äåéñòâèÿ -->
    <MenuItem Headline="Êîïèðîâàòü" StartIcon="content_copy" OnClick="Copy" />
    <MenuItem Headline="Âñòàâèòü" StartIcon="content_paste" OnClick="Paste" />
    <MenuItem Headline="Âûðåçàòü" StartIcon="content_cut" OnClick="Cut" />
    
    <MenuDivider />
    
    <!-- Äåéñòâèÿ ñ ôàéëàìè -->
    <MenuItem Headline="Ïåðåèìåíîâàòü" StartIcon="edit" OnClick="Rename" />
    <MenuItem Headline="Äóáëèðîâàòü" StartIcon="file_copy" OnClick="Duplicate" />
    
    <MenuDivider />
    
    <!-- Îïàñíûå äåéñòâèÿ -->
    <MenuItem Headline="Óäàëèòü" StartIcon="delete" OnClick="Delete" />
</Menu>
```

### Ñ ïîäìåíþ
```razor
<Menu Anchor="file-menu" @bind-Open="fileMenuOpen" HasOverflow="true">
    <MenuItem Headline="Íîâûé ôàéë" StartIcon="note_add" OnClick="NewFile" />
    <MenuItem Headline="Îòêðûòü" StartIcon="folder_open" OnClick="OpenFile" />
    
    <MenuDivider />
    
    <SubMenu ItemHeadline="Íåäàâíèå ôàéëû" ItemStartIcon="history" ItemEndIcon="arrow_right">
        <MenuItem Headline="document.docx" OnClick="OpenRecent1" />
        <MenuItem Headline="spreadsheet.xlsx" OnClick="OpenRecent2" />
        <MenuDivider />
        <MenuItem Headline="Î÷èñòèòü èñòîðèþ" StartIcon="clear_all" OnClick="ClearHistory" />
    </SubMenu>
    
    <MenuDivider />
    
    <MenuItem Headline="Ñîõðàíèòü" StartIcon="save" OnClick="SaveFile" />
    <MenuItem Headline="Ñîõðàíèòü êàê..." StartIcon="save_as" OnClick="SaveAs" />
</Menu>
```

## Ïàðàìåòðû

`MenuDivider` íå èìååò ñïåöèôè÷íûõ ïàðàìåòðîâ, êðîìå:

| Ïàðàìåòð | Òèï | Îïèñàíèå |
|----------|-----|----------|
| `AdditionalAttributes` | `Dictionary<string, object>` | Äîïîëíèòåëüíûå HTML àòðèáóòû |

## Accessibility

- Àâòîìàòè÷åñêè óñòàíàâëèâàåò `role="separator"`
- Óñòàíàâëèâàåò `tabindex="-1"` äëÿ èñêëþ÷åíèÿ èç tab navigation
- Ñîîòâåòñòâóåò ñòàíäàðòàì ARIA äëÿ ðàçäåëèòåëåé â ìåíþ

## Ñòèëèçàöèÿ

`MenuDivider` èñïîëüçóåò ñòàíäàðòíûå ñòèëè Material Design 3 äëÿ ðàçäåëèòåëåé:
- Òîíêàÿ ëèíèÿ ñ öâåòîì outline
- Ïðàâèëüíûå îòñòóïû â êîíòåêñòå ìåíþ
- Àâòîìàòè÷åñêàÿ àäàïòàöèÿ ê òåìå

### Ïðèìåð êàñòîìèçàöèè
```razor
<!-- Ñ äîïîëíèòåëüíûìè CSS êëàññàìè -->
<MenuDivider class="custom-divider" />

<!-- Ñ inline ñòèëÿìè -->
<MenuDivider style="margin: 8px 0;" />
```

## Ëó÷øèå ïðàêòèêè

### Ãðóïïèðîâêà äåéñòâèé
Èñïîëüçóéòå ðàçäåëèòåëè äëÿ ëîãè÷åñêîé ãðóïïèðîâêè ñâÿçàííûõ äåéñòâèé:

```razor
<Menu>
    <!-- Ãðóïïà: Îñíîâíûå îïåðàöèè -->
    <MenuItem Headline="Ñîçäàòü" />
    <MenuItem Headline="Îòêðûòü" />
    <MenuItem Headline="Ñîõðàíèòü" />
    
    <MenuDivider />
    
    <!-- Ãðóïïà: Ðåäàêòèðîâàíèå -->
    <MenuItem Headline="Îòìåíèòü" />
    <MenuItem Headline="Ïîâòîðèòü" />
    
    <MenuDivider />
    
    <!-- Ãðóïïà: Íàñòðîéêè -->
    <MenuItem Headline="Íàñòðîéêè" />
    <MenuItem Headline="Î ïðîãðàììå" />
</Menu>
```

### Èçáåãàéòå ÷ðåçìåðíîãî èñïîëüçîâàíèÿ
Íå èñïîëüçóéòå ñëèøêîì ìíîãî ðàçäåëèòåëåé - ýòî ìîæåò ñäåëàòü ìåíþ âèçóàëüíî ïåðåãðóæåííûì:

```razor
<!-- ? Ïëîõî: ñëèøêîì ìíîãî ðàçäåëèòåëåé -->
<Menu>
    <MenuItem Headline="Äåéñòâèå 1" />
    <MenuDivider />
    <MenuItem Headline="Äåéñòâèå 2" />
    <MenuDivider />
    <MenuItem Headline="Äåéñòâèå 3" />
</Menu>

<!-- ? Õîðîøî: ðàçäåëèòåëè òîëüêî äëÿ ãðóïïèðîâêè -->
<Menu>
    <MenuItem Headline="Äåéñòâèå 1" />
    <MenuItem Headline="Äåéñòâèå 2" />
    <MenuDivider />
    <MenuItem Headline="Äðóãàÿ ãðóïïà" />
</Menu>
```

### Â íà÷àëå è êîíöå ìåíþ
Îáû÷íî ðàçäåëèòåëè íå ñòàâÿòñÿ â ñàìîì íà÷àëå èëè êîíöå ìåíþ:

```razor
<!-- ? Ïëîõî -->
<Menu>
    <MenuDivider />
    <MenuItem Headline="Äåéñòâèå" />
    <MenuDivider />
</Menu>

<!-- ? Õîðîøî -->
<Menu>
    <MenuItem Headline="Äåéñòâèå 1" />
    <MenuDivider />
    <MenuItem Headline="Äåéñòâèå 2" />
</Menu>
```