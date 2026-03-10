window.posEnterNext = function () {

    const active = document.activeElement;

    if (active && active.tagName === "BUTTON") {
        active.click();
        return;
    }

    const focusable = Array.from(
        document.querySelectorAll(
            'input, select, textarea, button, [tabindex]'
        )
    ).filter(el => !el.disabled && el.offsetParent !== null);

    const index = focusable.indexOf(active);

    if (index > -1 && index < focusable.length - 1) {
        focusable[index + 1].focus();
    }
};

window.posFocusFirst = function () {
    document.querySelector('[tabindex="1"]')?.focus();
};
