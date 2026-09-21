
import { Hct, MaterialDynamicColors, DynamicScheme, themeFromSourceColor,/**/  hexFromArgb, argbFromHex } from '/_content/Material.Web/material-color-utilities.bundle.js';

export function getCssVariables(source, customColors = [], isDark, contrastLevel = 0.0) {
    // �������� ���� � ������� ARGB
    const sourceColorArgb = argbFromHex(source); // ��������, ����������
    // ������� HCT-������ �� ��������� �����
    const sourceColorHct = Hct.fromInt(sourceColorArgb);
    const theme = themeFromSourceColor(sourceColorArgb, customColors);

    const scheme = new DynamicScheme({
        sourceColorArgb,
        sourceColorHct,
        primaryPalette: theme.palettes.primary,
        secondaryPalette: theme.palettes.secondary,
        tertiaryPalette: theme.palettes.tertiary,
        neutralPalette: theme.palettes.neutral,
        neutralVariantPalette: theme.palettes.neutralVariant,
        isDark: isDark, // ������� ����
        contrastLevel: contrastLevel // ����������� ��������
    });

    // -------------------------------------------------
    // ���������� ���� �����
    const toneSteps = [0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 95, 99, 100];

    // ������� ��� ��������� CSS-���������� ��� �������
    const generateTonalVariables = (paletteName, palette) => {
        return toneSteps.map(tone => {
            const color = hexFromArgb(palette.tone(tone));
            return `--md-sys-color-${paletteName}${tone}: ${color};`;
        }).join('\n        ');
    };
    function blendColors(argb1, argb2, opacity) {
        const r1 = (argb1 >> 16) & 0xFF;
        const g1 = (argb1 >> 8) & 0xFF;
        const b1 = argb1 & 0xFF;

        const r2 = (argb2 >> 16) & 0xFF;
        const g2 = (argb2 >> 8) & 0xFF;
        const b2 = argb2 & 0xFF;

        const r = Math.round(r1 * (1 - opacity) + r2 * opacity);
        const g = Math.round(g1 * (1 - opacity) + g2 * opacity);
        const b = Math.round(b1 * (1 - opacity) + b2 * opacity);

        return (0xFF << 24) | (r << 16) | (g << 8) | b;
    }

    // ������ ������������ ��� Surface 1�5 (��� ������� ����)
    const surfaceOpacities = {
        1: 0.05, // Surface 1: 5%
        2: 0.08, // Surface 2: 8%
        3: 0.11, // Surface 3: 11%
        4: 0.12, // Surface 4: 12%
        5: 0.14  // Surface 5: 14%
    };

    // �������� ����� surface � surfaceTint
    const surfaceArgb = scheme.getArgb(MaterialDynamicColors.surface);
    const surfaceTintArgb = scheme.getArgb(MaterialDynamicColors.surfaceTint);

    // ���������� ����� ��� Surface 1�5
    const surfaceColors = {};
    for (let level = 1; level <= 5; level++) {
        surfaceColors[level] = hexFromArgb(blendColors(surfaceArgb, surfaceTintArgb, surfaceOpacities[level]));
    }

    // ���������� ���� ��� ������� ������ (�� ������ Material Design 3)
    const elevationShadows = {
        0: 'none',
        1: '0px 1px 2px 0px rgba(0, 0, 0, 0.3), 0px 1px 3px 1px rgba(0, 0, 0, 0.15)',
        2: '0px 1px 2px 0px rgba(0, 0, 0, 0.3), 0px 2px 6px 2px rgba(0, 0, 0, 0.15)',
        3: '0px 1px 3px 0px rgba(0, 0, 0, 0.3), 0px 4px 8px 3px rgba(0, 0, 0, 0.15)',
        4: '0px 2px 3px 0px rgba(0, 0, 0, 0.3), 0px 6px 10px 4px rgba(0, 0, 0, 0.15)',
        5: '0px 4px 4px 0px rgba(0, 0, 0, 0.3), 0px 8px 12px 6px rgba(0, 0, 0, 0.15)'
    };
    // -------------------------------------------------

    var cssBody = `:root, [data-theme], body {

        /* ------------------------------------------------- */

        /*   */
        ${generateTonalVariables('primary', theme.palettes.primary)}
        ${generateTonalVariables('secondary', theme.palettes.secondary)}
        ${generateTonalVariables('tertiary', theme.palettes.tertiary)}
        ${generateTonalVariables('neutral', theme.palettes.neutral)}
        ${generateTonalVariables('neutral-variant', theme.palettes.neutralVariant)}
        ${generateTonalVariables('error', theme.palettes.error)}

        /* ����������� 1�5 (Surface 1�5) */
        --md-sys-color-surface-1: ${surfaceColors[1]};
        --md-sys-color-surface-2: ${surfaceColors[2]};
        --md-sys-color-surface-3: ${surfaceColors[3]};
        --md-sys-color-surface-4: ${surfaceColors[4]};
        --md-sys-color-surface-5: ${surfaceColors[5]};

        /* ���� ��� ������� ������ */
        --md-sys-elevation-0: ${elevationShadows[0]};
        --md-sys-elevation-1: ${elevationShadows[1]};
        --md-sys-elevation-2: ${elevationShadows[2]};
        --md-sys-elevation-3: ${elevationShadows[3]};
        --md-sys-elevation-4: ${elevationShadows[4]};
        --md-sys-elevation-5: ${elevationShadows[5]};

        /* ------------------------------------------------- */


        /* �������� ����� ������ */
        --md-sys-color-primary-palette-key-color: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.primaryPaletteKeyColor))};
        --md-sys-color-secondary-palette-key-color: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.secondaryPaletteKeyColor))};
        --md-sys-color-tertiary-palette-key-color: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.tertiaryPaletteKeyColor))};
        --md-sys-color-neutral-palette-key-color: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.neutralPaletteKeyColor))};
        --md-sys-color-neutral-variant-palette-key-color: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.neutralVariantPaletteKeyColor))};

        --md-sys-color-background: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.background))};
        --md-sys-color-on-background: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.onBackground))};

        /* ����� ����������� (�������� ������������ elevation) */
        --md-sys-color-surface: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.surface))};
        --md-sys-color-surface-dim: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.surfaceDim))};
        --md-sys-color-surface-bright: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.surfaceBright))};
        --md-sys-color-surface-container-lowest: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.surfaceContainerLowest))};
        --md-sys-color-surface-container-low: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.surfaceContainerLow))};
        --md-sys-color-surface-container: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.surfaceContainer))};
        --md-sys-color-surface-container-high: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.surfaceContainerHigh))};
        --md-sys-color-surface-container-highest: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.surfaceContainerHighest))};

        --md-sys-color-on-surface: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.onSurface))};
        --md-sys-color-surface-variant: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.surfaceVariant))};
        --md-sys-color-on-surface-variant: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.onSurfaceVariant))};
        --md-sys-color-inverse-surface: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.inverseSurface))};
        --md-sys-color-inverse-on-surface: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.inverseOnSurface))};
        --md-sys-color-outline: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.outline))};
        --md-sys-color-outline-variant: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.outlineVariant))};
        --md-sys-color-shadow: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.shadow))};
        --md-sys-color-scrim: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.scrim))};
        --md-sys-color-surface-tint: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.surfaceTint))};
        --md-sys-color-primary: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.primary))};
        --md-sys-color-on-primary: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.onPrimary))};
        --md-sys-color-primary-container: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.primaryContainer))};
        --md-sys-color-on-primary-container: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.onPrimaryContainer))};
        --md-sys-color-inverse-primary: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.inversePrimary))};
        --md-sys-color-secondary: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.secondary))};
        --md-sys-color-on-secondary: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.onSecondary))};
        --md-sys-color-secondary-container: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.secondaryContainer))};
        --md-sys-color-on-secondary-container: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.onSecondaryContainer))};
        --md-sys-color-tertiary: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.tertiary))};
        --md-sys-color-on-tertiary: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.onTertiary))};
        --md-sys-color-tertiary-container: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.tertiaryContainer))};
        --md-sys-color-on-tertiary-container: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.onTertiaryContainer))};
        --md-sys-color-error: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.error))};
        --md-sys-color-on-error: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.onError))};
        --md-sys-color-error-container: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.errorContainer))};
        --md-sys-color-on-error-container: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.onErrorContainer))};
        --md-sys-color-primary-fixed: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.primaryFixed))};
        --md-sys-color-primary-fixed-dim: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.primaryFixedDim))};
        --md-sys-color-on-primary-fixed: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.onPrimaryFixed))};
        --md-sys-color-on-primary-fixed-variant: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.onPrimaryFixedVariant))};
        --md-sys-color-secondary-fixed: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.secondaryFixed))};
        --md-sys-color-secondary-fixed-dim: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.secondaryFixedDim))};
        --md-sys-color-on-secondary-fixed: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.onSecondaryFixed))};
        --md-sys-color-on-secondary-fixed-variant: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.onSecondaryFixedVariant))};
        --md-sys-color-tertiary-fixed: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.tertiaryFixed))};
        --md-sys-color-tertiary-fixed-dim: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.tertiaryFixedDim))};
        --md-sys-color-on-tertiary-fixed: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.onTertiaryFixed))};
        --md-sys-color-on-tertiary-fixed-variant: ${hexFromArgb(scheme.getArgb(MaterialDynamicColors.onTertiaryFixedVariant))};
    }`;

    return cssBody;
}

export function isSystemDark() {
    return window.matchMedia('(prefers-color-scheme: dark)').matches;
}

export function applyTheme(source, isDark) {
    try {
        const themeName = isDark ? 'dark' : 'light';
        document.documentElement.setAttribute('data-theme', themeName);
        document.documentElement.style.colorScheme = themeName;

        const css = getCssVariables(source, [{ name: "custom-1", value: source, blend: true }], isDark);
        let el = document.getElementById('socratic-dynamic-accent');
        if (!el) {
            el = document.createElement('style');
            el.id = 'socratic-dynamic-accent';
            document.head.appendChild(el);
        }
        el.textContent = css;
        return css;
    } catch (e) {
        console.warn('Failed to apply dynamic theme accent:', e);
        return '';
    }
}
