window.registerClickOutside = (element, dotNetRef) => {
    const handler = (e) => {
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
