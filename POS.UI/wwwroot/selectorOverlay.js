export function measureAnchor(element) {
    if (!element) {
        return null;
    }

    const rect = element.getBoundingClientRect();
    const viewportPadding = 8;
    const preferredWidth = Math.max(rect.width, 0);
    const availableWidth = Math.max(0, window.innerWidth - (viewportPadding * 2));
    const width = Math.min(preferredWidth, availableWidth || preferredWidth);
    const leftLimit = Math.max(viewportPadding, window.innerWidth - width - viewportPadding);
    const left = Math.min(Math.max(rect.left, viewportPadding), leftLimit);
    const preferredTop = rect.bottom + 2;
    const panelHeight = 276;
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
