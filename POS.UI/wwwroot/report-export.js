window.reportExport = {
    downloadCsv: function (filename, csvText) {
        var blob = new Blob([csvText], { type: "text/csv;charset=utf-8;" });
        var link = document.createElement("a");
        var url = URL.createObjectURL(blob);
        link.setAttribute("href", url);
        link.setAttribute("download", filename);
        link.style.visibility = "hidden";
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(url);
    },
    printHtml: function (title, html) {
        var win = window.open("", "_blank");
        if (!win) return;
        win.document.open();
        win.document.write(html);
        win.document.close();
        win.focus();
        win.print();
        win.close();
    }
};
