window.printElement = (elementId) => {
    const element = document.getElementById(elementId);
    if (!element) return;

    const printWindow = window.open('', '', 'width=800,height=600');
    const doc = printWindow.document;

    doc.write('<html><head><title>Print</title>');

    // Copy all stylesheets and inline styles
    const styles = Array.from(document.querySelectorAll('link[rel="stylesheet"], style'));
    styles.forEach(style => {
        doc.write(style.outerHTML);
    });

    doc.write('</head><body>');
    doc.write(element.outerHTML);
    doc.write('</body></html>');
    doc.close();
    printWindow.focus();
    printWindow.print();
    printWindow.close();
};
