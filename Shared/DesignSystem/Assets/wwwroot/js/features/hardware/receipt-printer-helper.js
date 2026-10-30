/**
 * Offscreen iframe printing helper for Blazor Receipt Printer.
 */

export function printElementById(elementId, styleElementId) {
    const el = document.getElementById(elementId);
    const styleEl = document.getElementById(styleElementId);
    if (!el) {
        console.warn(`[Receipt Printer] Element not found: ${elementId}`);
        return;
    }

    let iframe = document.getElementById('receipt-print-iframe');
    if (!iframe) {
        iframe = document.createElement('iframe');
        iframe.id = 'receipt-print-iframe';
        iframe.style.position = 'fixed';
        iframe.style.width = '0';
        iframe.style.height = '0';
        iframe.style.border = 'none';
        iframe.style.left = '-9999px';
        document.body.appendChild(iframe);
    }
    
    const doc = iframe.contentWindow.document;
    const styleHtml = styleEl ? styleEl.outerHTML : '';
    
    doc.open();
    doc.write(
        '<!DOCTYPE html><html><head><meta charset="utf-8"><title>Print</title>' + 
        styleHtml + 
        '</head><body style="margin:0;padding:0;">' + 
        el.outerHTML + 
        '</body></html>'
    );
    doc.close();
    
    // Allow styles and content to render inside the iframe before printing
    setTimeout(function() {
        try {
            iframe.contentWindow.focus();
            iframe.contentWindow.print();
        } catch (e) {
            console.error('[Receipt Printer] Print error:', e);
        }
    }, 250);
}

export function printHtmlDocument(title, htmlContent) {
    let iframe = document.getElementById('receipt-print-iframe');
    if (!iframe) {
        iframe = document.createElement('iframe');
        iframe.id = 'receipt-print-iframe';
        iframe.style.position = 'fixed';
        iframe.style.width = '0';
        iframe.style.height = '0';
        iframe.style.border = 'none';
        iframe.style.left = '-9999px';
        document.body.appendChild(iframe);
    }
    
    const doc = iframe.contentWindow.document;
    
    doc.open();
    doc.write(
        '<!DOCTYPE html><html><head><meta charset="utf-8"><title>' + title + '</title></head><body style="margin:0;padding:0;">' + 
        htmlContent + 
        '</body></html>'
    );
    doc.close();
    
    setTimeout(function() {
        try {
            iframe.contentWindow.focus();
            iframe.contentWindow.print();
        } catch (e) {
            console.error('[Receipt Printer] Print HTML error:', e);
        }
    }, 250);
}
