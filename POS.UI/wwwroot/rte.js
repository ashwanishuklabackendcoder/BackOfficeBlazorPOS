window.rte = {
    exec: function (command, value) {
        document.execCommand(command, false, value || null);
    },

    getHtml: function (el) {
        return el.innerHTML;
    },

    setHtml: function (el, html) {
        el.innerHTML = html || "";
    },

    focus: function (el) {
        el.focus();
    }
};
