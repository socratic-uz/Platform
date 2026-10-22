import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const assetsRoot = path.resolve(__dirname, '..');
const resourcesDir = path.join(assetsRoot, 'Resources');
const constantsDir = path.join(assetsRoot, 'Constants');
if (!fs.existsSync(constantsDir)) {
  fs.mkdirSync(constantsDir, { recursive: true });
}
const outputCsPath = path.join(constantsDir, 'Loc.g.cs');

// Known domain and framework types that would create ambiguity with 'using static'
const COLLIDING_DOMAIN_TYPES = new Set([
  'System',
  'String',
  'Object',
  'Type',
  'Action',
  'Task',
  'Environment',
  'Math',
  'Convert',
  'Console',
  'App',
  'Application',
  'Component',
  'Material',
  'Select',
  'Search',
  'Language',
  'Permission',
  'Theme',
  'Accent',
  'Name',
  'Address',
  'Point',
  'Line',
  'Polygon',
  'Route',
  'Zone',
  'OrderStatus',
  'ProductType',
  'Card',
  'Badge',
  'Chip',
  'Button',
  'Dialog',
  'Table',
  'Progress',
  'Avatar',
  'Divider',
  'Tab',
  'Tabs',
  'Nav',
  'Header',
  'Footer',
  'Sidebar',
  'Modal',
  'Icon',
  'Container',
  'Root',
  'Grid',
  'Menu',
  'PageTitle',
  'Order',
  'Orders',
  'Product',
  'Products',
  'PaymentType',
  'PaymentGatewayType',
  'Payment',
  'Payments',
  'New',
  'Default',
  'Class',
  'Event',
  'Delegate',
  'Interface',
  'Void',
  'Int',
  'Double',
  'Float',
  'Bool',
  'Long',
  'Short',
  'Byte',
  'Decimal',
  'Char',
  'Support',
  'Reset',
  'Clear',
  'Subtitle',
  'Canceled',
  'Collected',
  'Collecting',
  'Completed',
  'Created',
  'Made',
  'Making',
  'Ready',
  'Shipping',
  'Trial',
  'TotalLabel',
  'Total',
  'Active',
  'InStock',
  'Open',
  'PreviewLabel',
  'ProTier',
  'ResendCountdown',
  'ShippingFee',
  'SupportLabel',
  'TotalAmount',
  'Unit',
  'PayButton',
  'VideoCaption',
  'Number',
  'GroupTitle',
  'PreviewText',
  'QrHint',
  'Organization',
  'Organizations',
  'Kiosk',
  'Kiosks',
  'Terminal',
  'Terminals',
  'Receipt',
  'Receipts',
  'Invoice',
  'Invoices',
  'Store',
  'Stores',
  'User',
  'Users',
  'Account',
  'Accounts',
  'Role',
  'Roles',
  'Tenant',
  'Tenants',
  'Customer',
  'Customers',
  'Device',
  'Devices',
  'Print',
  'Item',
  'Items'
]);

function parseResx(filePath) {
  const map = new Map();
  if (!fs.existsSync(filePath)) {
    return map;
  }
  const content = fs.readFileSync(filePath, 'utf8');
  const dataRegex = /<data\s+name="([^"]+)"[^>]*>([\s\S]*?)<\/data>/g;
  let match;
  while ((match = dataRegex.exec(content)) !== null) {
    const key = match[1];
    const body = match[2];
    const valMatch = /<value>([\s\S]*?)<\/value>/.exec(body);
    const value = valMatch ? valMatch[1].trim() : '';
    map.set(key, value);
  }
  return map;
}

function normalizeIdentifier(str) {
  if (!str) return '_';
  let res = str.replace(/[^a-zA-Z0-9_]/g, '');
  if (!res) res = '_';
  if (/^[0-9]/.test(res)) {
    res = '_' + res;
  }
  return res;
}

function toPascalIdentifier(key) {
  if (!key) return '_';
  const parts = key.split(/[\s\-./\\]+/);
  let ident = parts
    .map(p => {
      if (!p) return '';
      return p.charAt(0).toUpperCase() + p.slice(1);
    })
    .join('');

  return normalizeIdentifier(ident);
}

function sanitizeXmlDoc(text) {
  if (!text) return '';
  return text
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/\r?\n/g, ' ');
}

const DOMAIN_DEFINITIONS = {
  Auth: ['Auth_', 'SignIn_', 'OAuth_', 'FaceId_'],
  Cart: ['Cart_'],
  Checkout: ['Checkout_', 'Payment_'],
  Kiosk: ['Kiosk_', 'AdSlot_'],
  Booking: ['Booking_'],
  UxMode: ['UxMode_', 'UX_'],
  Store: ['Store_'],
  Config: ['Config_'],
  Quote: ['Quote_'],
  Geo: ['Geo_'],
  GroupBuy: ['GroupBuy_'],
  Schedule: ['Schedule_'],
  Receipt: ['Receipt_', 'Invoice_'],
  Lottery: ['Lottery_'],
  Bundle: ['Bundle_'],
  Donation: ['Donation_'],
  Chat: ['Chat_'],
  Settings: ['Settings_'],
  Orders: ['Order_', 'Orders_', 'OrderItems_', 'Kds_', 'OrderStatus_'],
  Terminal: ['Terminal_'],
  QrDesigner: ['QrDesigner_'],
  Subscription: ['Subscription_'],
  Auction: ['Auction_'],
  Rental: ['Rental_'],
  Hotel: ['Hotel_'],
  Classified: ['Classified_'],
  Configurator: ['Configurator_'],
  Delivery: ['Delivery_'],
  Digital: ['Digital_'],
  Gift: ['Gift_'],
  Seats: ['Seats_'],
  Landing: ['Landing_', 'About_', 'Help_', 'Docs_', 'QR_', 'Analytics_', 'Landing', 'About', 'Help', 'Docs', 'Qr', 'Analytics']
};

export function generateLocalizationKeys() {
  console.log(`\n🌐 [Localization Pipeline] Scanning .resx dictionaries in: ${resourcesDir}`);

  const ruPath = path.join(resourcesDir, 'ResourceRu.resx');
  const uzPath = path.join(resourcesDir, 'ResourceUz.resx');
  const enPath = path.join(resourcesDir, 'ResourceEn.resx');

  const ruMap = parseResx(ruPath);
  const uzMap = parseResx(uzPath);
  const enMap = parseResx(enPath);

  const allKeys = new Set([...ruMap.keys(), ...uzMap.keys(), ...enMap.keys()]);
  const sortedKeys = Array.from(allKeys).sort((a, b) => a.localeCompare(b));

  console.log(`   Found ${allKeys.size} distinct localization keys (RU: ${ruMap.size}, UZ: ${uzMap.size}, EN: ${enMap.size}).`);

  const keyItems = new Map();
  for (const rawKey of sortedKeys) {
    keyItems.set(rawKey, {
      rawKey,
      ru: ruMap.get(rawKey) || '',
      uz: uzMap.get(rawKey) || '',
      en: enMap.get(rawKey) || ''
    });
  }

  // Domain categorization
  const domainGroups = {
    Common: new Map(),
    Actions: new Map()
  };

  for (const d of Object.keys(DOMAIN_DEFINITIONS)) {
    domainGroups[d] = new Map();
  }

  const ACTION_KEYWORDS = new Set([
    'Save', 'Cancel', 'Edit', 'Delete', 'Create', 'Close', 'Back', 'Search',
    'Reset', 'Refresh', 'Submit', 'Select', 'Apply', 'Loading', 'Next', 'Previous',
    'Log out', 'Done', 'Pay', 'Retry', 'Add', 'Remove', 'Confirm', 'Ok', 'Yes', 'No'
  ]);

  const codepointsPath = path.join(assetsRoot, 'Resources', 'material-symbols.codepoints');
  const iconNames = new Set();
  if (fs.existsSync(codepointsPath)) {
    const lines = fs.readFileSync(codepointsPath, 'utf8').split('\n');
    for (const l of lines) {
      const name = l.trim().split(/\s+/)[0];
      if (name) iconNames.add(toPascalIdentifier(name));
    }
  }

  const snapshotPath = path.join(__dirname, 'component-classes-snapshot.json');
  const cssClassNames = new Set();
  if (fs.existsSync(snapshotPath)) {
    try {
      const snap = JSON.parse(fs.readFileSync(snapshotPath, 'utf8'));
      for (const group of Object.values(snap)) {
        for (const k of Object.keys(group)) {
          cssClassNames.add(k);
        }
      }
    } catch {}
  }

  const classesCsPath = path.join(assetsRoot, 'Constants', 'Classes.g.cs');
  if (fs.existsSync(classesCsPath)) {
    try {
      const content = fs.readFileSync(classesCsPath, 'utf8');
      const matches = content.matchAll(/public\s+const\s+string\s+([A-Za-z0-9_]+)\s*=/g);
      for (const m of matches) {
        cssClassNames.add(m[1]);
      }
    } catch {}
  }

  function isColliding(ident) {
    return COLLIDING_DOMAIN_TYPES.has(ident) || iconNames.has(ident) || cssClassNames.has(ident);
  }

  for (const rawKey of sortedKeys) {
    const item = keyItems.get(rawKey);
    let assignedDomain = 'Common';
    let matchedPrefix = null;

    for (const [dom, prefixes] of Object.entries(DOMAIN_DEFINITIONS)) {
      for (const pref of prefixes) {
        if (rawKey.startsWith(pref)) {
          assignedDomain = dom;
          matchedPrefix = pref;
          break;
        }
      }
      if (matchedPrefix) break;
    }

    // Check if it is a pure action
    if (ACTION_KEYWORDS.has(rawKey)) {
      assignedDomain = 'Actions';
      const actionIdent = toPascalIdentifier(rawKey);
      if (isColliding(actionIdent)) {
        domainGroups.Actions.set(`${actionIdent}Action`, item);
        domainGroups.Actions.set(`${actionIdent}Text`, item);
      } else {
        domainGroups.Actions.set(actionIdent, item);
        domainGroups.Actions.set(`${actionIdent}Action`, item);
        domainGroups.Actions.set(`${actionIdent}Text`, item);
      }
    } else if (assignedDomain === 'Common') {
      const primaryIdent = toPascalIdentifier(rawKey);

      // Protect against collisions with SharedKernel domain types and Icons when 'using static' is applied
      if (isColliding(primaryIdent)) {
        domainGroups.Common.set(`${primaryIdent}Text`, item);
        domainGroups.Common.set(`${primaryIdent}Label`, item);
      } else {
        domainGroups.Common.set(primaryIdent, item);
      }

      if (primaryIdent.includes('_')) {
        const altIdent = primaryIdent.split('_').map(p => p.charAt(0).toUpperCase() + p.slice(1)).join('');
        if (altIdent && !isColliding(altIdent) && !domainGroups.Common.has(altIdent)) {
          domainGroups.Common.set(altIdent, item);
        }
      }
    } else {
      const shouldStrip = Boolean(matchedPrefix && matchedPrefix.endsWith('_'));
      const strippedRaw = shouldStrip ? rawKey.substring(matchedPrefix.length) : rawKey;
      const strippedIdent = toPascalIdentifier(strippedRaw);
      const fullIdent = toPascalIdentifier(rawKey);

      let safeStripped = isColliding(strippedIdent) || strippedIdent === assignedDomain 
        ? `${strippedIdent}Text` 
        : strippedIdent;
      if (domainGroups.Common.has(safeStripped) || domainGroups.Actions.has(safeStripped) || cssClassNames.has(safeStripped)) {
        safeStripped = `${assignedDomain}${safeStripped}`;
      }
      domainGroups[assignedDomain].set(safeStripped, item);

      if (strippedIdent.includes('_')) {
        const altStripped = normalizeIdentifier(strippedIdent.split('_').map(p => p.charAt(0).toUpperCase() + p.slice(1)).join(''));
        if (altStripped && !isColliding(altStripped) && !domainGroups[assignedDomain].has(altStripped) && altStripped !== assignedDomain && !domainGroups.Common.has(altStripped) && !cssClassNames.has(altStripped)) {
          domainGroups[assignedDomain].set(altStripped, item);
        }
      }

      if (fullIdent !== safeStripped && fullIdent !== assignedDomain) {
        let safeFull = isColliding(fullIdent) ? `${fullIdent}Text` : fullIdent;
        if (domainGroups.Common.has(safeFull) || domainGroups.Actions.has(safeFull) || cssClassNames.has(safeFull)) {
          safeFull = `${assignedDomain}${safeFull}`;
        }
        domainGroups[assignedDomain].set(safeFull, item);
      }

      if (fullIdent.includes('_')) {
        const altIdent = normalizeIdentifier(fullIdent.split('_').map(p => p.charAt(0).toUpperCase() + p.slice(1)).join(''));
        if (altIdent && !isColliding(altIdent) && !domainGroups[assignedDomain].has(altIdent) && altIdent !== assignedDomain && !domainGroups.Common.has(altIdent) && !cssClassNames.has(altIdent)) {
          domainGroups[assignedDomain].set(altIdent, item);
        }
      }

      if (/^[a-zA-Z_][a-zA-Z0-9_]*$/.test(rawKey) && !isColliding(rawKey) && !domainGroups[assignedDomain].has(rawKey) && rawKey !== assignedDomain && !domainGroups.Common.has(rawKey) && !cssClassNames.has(rawKey)) {
        domainGroups[assignedDomain].set(rawKey, item);
      }
    }
  }

  // Root flat constants
  const rootFlatConstants = new Map();
  const domainNames = new Set([...Object.keys(domainGroups), 'Actions']);

  for (const rawKey of sortedKeys) {
    const item = keyItems.get(rawKey);
    const primaryIdent = toPascalIdentifier(rawKey);

    let altIdent = null;
    if (primaryIdent.includes('_')) {
      const parts = primaryIdent.split('_');
      altIdent = normalizeIdentifier(parts.map(p => p.charAt(0).toUpperCase() + p.slice(1)).join(''));
      if (altIdent === primaryIdent) altIdent = null;
    }

    const safePrimary = domainNames.has(primaryIdent) || isColliding(primaryIdent) ? `${primaryIdent}Text` : primaryIdent;
    if (!rootFlatConstants.has(safePrimary)) {
      rootFlatConstants.set(safePrimary, item);
    }

    if (altIdent) {
      const safeAlt = domainNames.has(altIdent) || isColliding(altIdent) ? `${altIdent}Text` : altIdent;
      if (!rootFlatConstants.has(safeAlt)) {
        rootFlatConstants.set(safeAlt, item);
      }
    }
  }

  function renderConstantsBlock(entriesMap, indent = '        ') {
    let out = '';
    const sorted = Array.from(entriesMap.keys()).sort((a, b) => a.localeCompare(b));
    for (const rawIdent of sorted) {
      const ident = normalizeIdentifier(rawIdent);
      const item = entriesMap.get(rawIdent);
      const escapedRawKey = item.rawKey.replace(/\\/g, '\\\\').replace(/"/g, '\\"');
      const ruDoc = sanitizeXmlDoc(item.ru);
      const uzDoc = sanitizeXmlDoc(item.uz);
      const enDoc = sanitizeXmlDoc(item.en);

      out += `${indent}/// <summary>\n`;
      out += `${indent}/// Key: <c>${item.rawKey}</c><br/>\n`;
      if (ruDoc) out += `${indent}/// &#x1F1F7;&#x1F1FA; RU: &quot;${ruDoc}&quot;<br/>\n`;
      if (uzDoc) out += `${indent}/// &#x1F1FA;&#x1F1FF; UZ: &quot;${uzDoc}&quot;<br/>\n`;
      if (enDoc) out += `${indent}/// &#x1F1EC;&#x1F1E7; EN: &quot;${enDoc}&quot;\n`;
      out += `${indent}/// </summary>\n`;
      out += `${indent}public const string ${ident} = "${escapedRawKey}";\n\n`;
    }
    return out;
  }

  let code = `// <auto-generated/>
// This file was automatically generated by generate-localization-keys.mjs.
// Strongly-typed localization key constants for Socratic IStringLocalizer.
// 100% compile-time safety, zero magic strings, rich IDE tooltips.
// Supports: @L[Save] via @using static Assets.Constants.Loc.Common or @using static Assets.Constants.Loc.Actions.

#nullable enable
#pragma warning disable

namespace Assets.Constants
{
    /// <summary>
    /// Strongly-typed string keys for <see cref="Microsoft.Extensions.Localization.IStringLocalizer"/>.
    /// Total unique keys: ${allKeys.size}.
    /// </summary>
    public static class Loc
    {
`;

  const sortedDomainNames = Object.keys(domainGroups).sort((a, b) => {
    if (a === 'Actions') return -2;
    if (a === 'Common') return -1;
    if (b === 'Actions') return 2;
    if (b === 'Common') return 1;
    return a.localeCompare(b);
  });

  for (const dom of sortedDomainNames) {
    const map = domainGroups[dom];
    if (map.size === 0) continue;
    code += `        /// <summary>\n`;
    code += `        /// Domain group: ${dom} (${map.size} keys). Use with <c>@using static Assets.Constants.Loc.${dom};</c>.\n`;
    code += `        /// </summary>\n`;
    code += `        public static class ${dom}\n        {\n`;
    code += renderConstantsBlock(map, '            ');
    code += `        }\n\n`;
  }

  code += `        #region Flat Constants (Backward Compatibility & Global Access)\n\n`;
  code += renderConstantsBlock(rootFlatConstants, '        ');
  code += `        #endregion\n`;

  code += `    }\n}\n`;

  let locNamespaceBody = '';
  for (const dom of sortedDomainNames) {
    const map = domainGroups[dom];
    if (map.size === 0) continue;
    locNamespaceBody += `    /// <summary>\n`;
    locNamespaceBody += `    /// Domain group: ${dom} (${map.size} keys). Use with <c>@using static Loc.${dom};</c>.\n`;
    locNamespaceBody += `    /// </summary>\n`;
    locNamespaceBody += `    public static class ${dom}\n    {\n`;
    locNamespaceBody += renderConstantsBlock(map, '        ');
    locNamespaceBody += `    }\n\n`;
  }

  code += `\nnamespace Loc\n{\n${locNamespaceBody}}\n`;

  fs.writeFileSync(outputCsPath, code, 'utf8');
  console.log(`✅ [Localization Pipeline] C# localization keys generated at: ${outputCsPath}`);
  console.log(`   Generated ${sortedDomainNames.length} domain groups and ${rootFlatConstants.size} flat constants.`);
  console.log(`   Common: ${domainGroups.Common.size} keys, Actions: ${domainGroups.Actions.size} keys.`);
}

if (process.argv[1] && fileURLToPath(import.meta.url) === path.resolve(process.argv[1])) {
  generateLocalizationKeys();
}
