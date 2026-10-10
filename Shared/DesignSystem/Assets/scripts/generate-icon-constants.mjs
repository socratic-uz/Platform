import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const assetsRoot = path.resolve(__dirname, '..');
const codepointsPath = path.join(assetsRoot, 'Resources', 'material-symbols.codepoints');
const outputCsPath = path.join(assetsRoot, 'Constants', 'Icons.g.cs');

// Ensure directory exists
const constantsDir = path.dirname(outputCsPath);
if (!fs.existsSync(constantsDir)) {
  fs.mkdirSync(constantsDir, { recursive: true });
}

function toPascalIdentifier(str) {
  if (!str) return '_';
  const parts = str.split(/[\s\-._]+/);
  let ident = parts
    .map(p => {
      if (!p) return '';
      return p.charAt(0).toUpperCase() + p.slice(1);
    })
    .join('');

  ident = ident.replace(/[^a-zA-Z0-9_]/g, '');
  if (!ident) ident = '_';
  if (/^[0-9]/.test(ident)) {
    ident = '_' + ident;
  }
  return ident;
}

// Category keyword rules for Material Design classification
const CATEGORY_RULES = {
  Commerce: [
    'shopping', 'cart', 'store', 'storefront', 'receipt', 'payment', 'payments', 'credit_card',
    'loyalty', 'sell', 'inventory', 'point_of_sale', 'qr_code', 'shipping', 'offer',
    'barcode', 'contactless', 'price', 'discount', 'account_balance', 'wallet', 'money',
    'currency', 'quote', 'paid', 'savings', 'order', 'orders', 'pos', 'cash', 'bank'
  ],
  Actions: [
    'save', 'cancel', 'edit', 'delete', 'check', 'close', 'search', 'add', 'remove',
    'refresh', 'done', 'visibility', 'settings', 'help', 'info', 'lock', 'favorite',
    'share', 'download', 'upload', 'print', 'filter', 'sort', 'tune', 'view', 'history',
    'schedule', 'bookmark', 'thumb', 'login', 'logout', 'fingerprint', 'face', 'touch_app',
    'verified', 'pending', 'restart', 'undo', 'redo', 'sync', 'update', 'build', 'manage'
  ],
  Navigation: [
    'arrow', 'chevron', 'expand', 'collapse', 'menu', 'fullscreen', 'more_vert',
    'more_horiz', 'first_page', 'last_page', 'unfold', 'apps', 'north', 'south',
    'east', 'west', 'navigate', 'subdirectory', 'turn', 'compass'
  ],
  Maps: [
    'location', 'directions', 'place', 'map', 'my_location', 'near_me', 'pin',
    'route', 'traffic', 'terrain', 'explore', 'restaurant', 'bar', 'cafe', 'hotel',
    'hospital', 'gas', 'park', 'flight', 'train', 'subway', 'car', 'bus', 'bike',
    'parking', 'ev_station', 'transit'
  ],
  Places: [
    'apartment', 'home', 'business', 'domain', 'event_seat', 'table_bar', 'table_restaurant',
    'meeting_room', 'room_service', 'fitness', 'pool', 'spa', 'villa', 'cottage',
    'house', 'roofing', 'deck', 'balcony', 'chalet', 'cabin', 'counter', 'lobby'
  ],
  Social: [
    'person', 'people', 'group', 'groups', 'account_circle', 'sentiment', 'notifications',
    'chat', 'forum', 'comment', 'review', 'support', 'badge', 'handshake', 'diversity',
    'co_present', 'psychology', 'social'
  ],
  Devices: [
    'desktop', 'laptop', 'tablet', 'smartphone', 'phone', 'tv', 'device', 'devices',
    'camera', 'videocam', 'mic', 'volume', 'bluetooth', 'wifi', 'battery', 'screen',
    'cast', 'headset', 'keyboard', 'mouse', 'watch', 'wearable'
  ],
  Media: [
    'play', 'pause', 'stop', 'fast_forward', 'fast_rewind', 'skip', 'replay',
    'music', 'movie', 'video', 'queue', 'equalizer', 'audio', 'album', 'podcast',
    'library_music', 'playlist', 'shuffle', 'repeat'
  ],
  Design: [
    'architecture', 'brush', 'palette', 'color', 'paint', 'layers', 'awesome',
    'mosaic', 'customize', 'grid', 'quilt', 'compact', 'lens', 'crop', 'transform',
    'draw', 'style', 'canvas', 'shape', 'gradient', 'vector'
  ],
  Hardware: [
    'scanner', 'printer', 'local_printshop', 'chip', 'memory', 'board', 'usb',
    'router', 'power', 'dock', 'sensor', 'shield', 'security', 'speed', 'terminal',
    'hardware', 'cpu', 'cable', 'hub', 'modem'
  ]
};

function categorizeIcon(rawKey) {
  for (const [cat, keywords] of Object.entries(CATEGORY_RULES)) {
    for (const kw of keywords) {
      if (rawKey === kw || rawKey.startsWith(kw + '_') || rawKey.endsWith('_' + kw) || rawKey.includes('_' + kw + '_')) {
        return cat;
      }
    }
  }
  return 'Common';
}

export function generateIconConstants() {
  console.log(`\n🎨 [Icon Pipeline] Generating strongly-typed icon constants from: ${codepointsPath}`);

  if (!fs.existsSync(codepointsPath)) {
    console.error(`❌ Codepoints file not found at: ${codepointsPath}`);
    return;
  }

  const content = fs.readFileSync(codepointsPath, 'utf8');
  const lines = content.split('\n');

  const iconMap = new Map(); // rawKey -> codepoint
  for (const line of lines) {
    const trimmed = line.trim();
    if (!trimmed) continue;
    const parts = trimmed.split(/\s+/);
    if (parts.length >= 2) {
      const rawKey = parts[0];
      const codepoint = parts[1];
      iconMap.set(rawKey, codepoint);
    }
  }

  console.log(`   Loaded ${iconMap.size} Material Symbols Outlined icons from codepoints dictionary.`);

  // Categorize
  const categories = {
    Commerce: new Map(),
    Actions: new Map(),
    Navigation: new Map(),
    Maps: new Map(),
    Places: new Map(),
    Social: new Map(),
    Devices: new Map(),
    Media: new Map(),
    Design: new Map(),
    Hardware: new Map(),
    Common: new Map()
  };

  const flatIcons = new Map();
  const sortedKeys = Array.from(iconMap.keys()).sort((a, b) => a.localeCompare(b));

  for (const rawKey of sortedKeys) {
    const codepoint = iconMap.get(rawKey);
    const ident = toPascalIdentifier(rawKey);
    const category = categorizeIcon(rawKey);

    const item = { rawKey, ident, codepoint, category };

    // Prevent collision with category name in category group
    const safeIdent = ident === category ? `${ident}Icon` : ident;
    categories[category].set(safeIdent, item);

    // Flat collection
    if (!flatIcons.has(ident)) {
      flatIcons.set(ident, item);
    }
  }

  const categoryNames = Object.keys(categories);

  // Render C# code
  let code = `// <auto-generated/>
// This file was automatically generated by generate-icon-constants.mjs.
// Strongly-typed ligature icon constants for Material Symbols Outlined font.
// 100% compile-time safety, zero runtime allocations, rich IDE tooltips.
// Supports: <Icon Name="@Icons.ShoppingCart" /> and <Icon Name="@Icons.Commerce.ShoppingCart" />.

#nullable enable
#pragma warning disable

namespace Assets.Constants
{
    /// <summary>
    /// Strongly-typed ligature string constants for Google Material Symbols Outlined font.
    /// Total available icons: ${iconMap.size}.
    /// </summary>
    public static class Icons
    {
`;

  // Render categories
  for (const catName of categoryNames) {
    const catMap = categories[catName];
    if (catMap.size === 0) continue;

    code += `        /// <summary>\n`;
    code += `        /// Category: ${catName} (${catMap.size} icons).\n`;
    code += `        /// </summary>\n`;
    code += `        public static class ${catName}\n        {\n`;

    const sortedCat = Array.from(catMap.keys()).sort((a, b) => a.localeCompare(b));
    for (const ident of sortedCat) {
      const item = catMap.get(ident);
      code += `            /// <summary>\n`;
      code += `            /// Ligature: <c>${item.rawKey}</c><br/>\n`;
      code += `            /// Codepoint: <c>\\u${item.codepoint}</c>\n`;
      code += `            /// </summary>\n`;
      code += `            public const string ${ident} = "${item.rawKey}";\n\n`;
    }

    code += `        }\n\n`;
  }

  // Render flat root constants
  code += `        #region Flat Icons (Universal Direct Access: @Icons.ShoppingCart)\n\n`;

  const sortedFlat = Array.from(flatIcons.keys()).sort((a, b) => a.localeCompare(b));
  for (const ident of sortedFlat) {
    const item = flatIcons.get(ident);
    // If identifier collides with a category name, suffix with Icon
    const safeIdent = categoryNames.includes(ident) ? `${ident}Icon` : ident;
    code += `        /// <summary>\n`;
    code += `        /// Ligature: <c>${item.rawKey}</c> | Category: <see cref="${item.category}"/><br/>\n`;
    code += `        /// Codepoint: <c>\\u${item.codepoint}</c>\n`;
    code += `        /// </summary>\n`;
    code += `        public const string ${safeIdent} = "${item.rawKey}";\n\n`;
  }

  code += `        #endregion\n\n`;
  code += `    }\n}\n`;

  fs.writeFileSync(outputCsPath, code, 'utf8');
  console.log(`✅ [Icon Pipeline] C# icon constants generated at: ${outputCsPath}`);
  console.log(`   Generated ${categoryNames.length} categories and ${sortedFlat.length} flat constants.`);
}

if (process.argv[1] && fileURLToPath(import.meta.url) === path.resolve(process.argv[1])) {
  generateIconConstants();
}
