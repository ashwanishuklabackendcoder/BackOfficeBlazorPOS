window.paymentShortcuts = (function () {
    let handler = null;

    function isEditableTarget(target) {
        if (!target) return false;
        const tag = (target.tagName || "").toUpperCase();
        return tag === "INPUT" ||
            tag === "TEXTAREA" ||
            tag === "SELECT" ||
            target.isContentEditable === true;
    }

    function mapKey(event) {
        const key = event.key;

        if (/^[0-9]$/.test(key)) return key;
        if (key === "." || key === "Decimal") return ".";
        if (key === "Backspace") return "BACK";
        if (key === "Delete" || key === "Escape") return "CLR";
        if (key === "Enter") return "APPLY";

        return null;
    }

    return {
        attach: function (dotNetRef) {
            this.detach();

            handler = function (event) {
                if (isEditableTarget(event.target)) return;

                const mapped = mapKey(event);
                if (!mapped) return;

                event.preventDefault();
                dotNetRef.invokeMethodAsync("HandleGlobalPaymentKey", mapped);
            };

            window.addEventListener("keydown", handler, true);
        },

        detach: function () {
            if (!handler) return;
            window.removeEventListener("keydown", handler, true);
            handler = null;
        }
    };
})();
