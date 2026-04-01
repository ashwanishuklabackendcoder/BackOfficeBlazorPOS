window.registerClickOutside = (element, dotNetRef) => {
    const handler = (e) => {
        if (!element || !e || !e.target) {
            return;
        }

        if (!element.contains(e.target)) {
            dotNetRef.invokeMethodAsync("OnClickOutside");
        }
    };

    document.addEventListener("mousedown", handler);

    return {
        dispose: () => document.removeEventListener("mousedown", handler)
    };
};

window.enableProductShortcuts = function (dotNetRef) {
    document.addEventListener("keydown", function (e) {
        if (e.key === "Escape") {
            e.preventDefault();
            dotNetRef.invokeMethodAsync("OnEsc");
        }
    });
};

window.selectorOverlay = {
    measureAnchor: (element) => {
        if (!element) {
            return null;
        }

        const rect = element.getBoundingClientRect();
        const viewportPadding = 8;
        const preferredWidth = Math.min(Math.max(rect.width * 1.5, 300), 560);
        const availableWidth = Math.max(0, window.innerWidth - (viewportPadding * 2));
        const width = Math.min(preferredWidth, availableWidth || preferredWidth);
        const leftLimit = Math.max(viewportPadding, window.innerWidth - width - viewportPadding);
        let left = Math.min(Math.max(rect.left, viewportPadding), leftLimit);
        const preferredTop = rect.bottom + 4;
        const panelHeight = 260;
        let top = preferredTop;

        if (top + panelHeight > window.innerHeight - viewportPadding && rect.top - panelHeight - 4 >= viewportPadding) {
            top = rect.top - panelHeight - 4;
        }

        return {
            top,
            left,
            width,
            maxWidth: availableWidth
        };
    }
};
