(function () {
    const state = {
        listenerAttached: false,
        registrations: new Map(),
        activeId: null,
        seq: 0
    };

    function buildId() {
        state.seq += 1;
        return "kbd-" + state.seq;
    }

    function attachListener() {
        if (state.listenerAttached) {
            return;
        }

        document.addEventListener("keydown", onKeyDown, true);
        state.listenerAttached = true;
    }

    function detachListenerIfIdle() {
        if (state.registrations.size > 0 || !state.listenerAttached) {
            return;
        }

        document.removeEventListener("keydown", onKeyDown, true);
        state.listenerAttached = false;
    }

    function getActiveRegistration() {
        if (state.activeId && state.registrations.has(state.activeId)) {
            return state.registrations.get(state.activeId);
        }

        const last = Array.from(state.registrations.keys()).at(-1);
        if (!last) {
            return null;
        }

        state.activeId = last;
        return state.registrations.get(last);
    }

    function isElementVisible(element) {
        if (!element) {
            return false;
        }

        const style = window.getComputedStyle(element);
        return style.display !== "none" && style.visibility !== "hidden" && element.offsetParent !== null;
    }

    function isFocusable(element) {
        if (!element || element.disabled) {
            return false;
        }

        const tabIndex = element.getAttribute("tabindex");
        if (tabIndex !== null && Number(tabIndex) < 0) {
            return false;
        }

        return isElementVisible(element);
    }

    function getScopeElement(active, scopeSelector) {
        if (active && scopeSelector) {
            const match = active.closest(scopeSelector);
            if (match) {
                return match;
            }
        }

        if (scopeSelector) {
            return document.querySelector(scopeSelector) || document;
        }

        return active?.closest("[data-shortcuts-scope]") || document;
    }

    function getFocusableElements(scope) {
        const query = [
            "input:not([type='hidden'])",
            "select",
            "textarea",
            "button",
            "[tabindex]"
        ].join(",");

        return Array
            .from(scope.querySelectorAll(query))
            .filter(isFocusable);
    }

    function moveFocusToNext(scopeSelector) {
        const active = document.activeElement;
        const scope = getScopeElement(active, scopeSelector);
        const fields = getFocusableElements(scope);
        const index = fields.indexOf(active);

        if (index < 0 || index >= fields.length - 1) {
            return false;
        }

        fields[index + 1].focus();
        return true;
    }

    function shouldHandleEnter(e) {
        if (e.key !== "Enter" || e.shiftKey || e.altKey || e.ctrlKey || e.metaKey) {
            return false;
        }

        const target = e.target;
        if (!target) {
            return false;
        }

        if (target.closest("[data-enter-skip='true']")) {
            return false;
        }

        if (target.tagName === "TEXTAREA") {
            return false;
        }

        return true;
    }

    function invokeSafely(dotnetRef, method) {
        if (!dotnetRef || typeof dotnetRef.invokeMethodAsync !== "function") {
            return;
        }

        dotnetRef.invokeMethodAsync(method).catch(() => { });
    }

    function onKeyDown(e) {
        const reg = getActiveRegistration();
        if (!reg) {
            return;
        }

        if (reg.enableEsc && e.key === "Escape") {
            e.preventDefault();
            invokeSafely(reg.dotnetRef, "OnEsc");
            return;
        }

        if (reg.enableF1 && e.key === "F1") {
            e.preventDefault();
            invokeSafely(reg.dotnetRef, "OnF1");
            return;
        }

        if (reg.enableF5 && e.key === "F5") {
            e.preventDefault();
            invokeSafely(reg.dotnetRef, "OnF5");
            return;
        }

        if (reg.enableEnter && shouldHandleEnter(e)) {
            const moved = moveFocusToNext(reg.scopeSelector);
            if (moved) {
                e.preventDefault();
            }
        }
    }

    function register(dotnetRef, options) {
        const id = buildId();
        const config = options || {};

        state.registrations.set(id, {
            id: id,
            dotnetRef: dotnetRef,
            scopeSelector: config.scopeSelector || null,
            enableEsc: config.enableEsc !== false,
            enableEnter: config.enableEnter !== false,
            enableF1: config.enableF1 === true,
            enableF5: config.enableF5 === true
        });

        state.activeId = id;
        attachListener();

        return id;
    }

    function unregister(id) {
        if (!id) {
            return;
        }

        state.registrations.delete(id);

        if (state.activeId === id) {
            state.activeId = Array.from(state.registrations.keys()).at(-1) || null;
        }

        detachListenerIfIdle();
    }

    function setActive(id) {
        if (id && state.registrations.has(id)) {
            state.activeId = id;
        }
    }

    window.blazorShortcuts = {
        register: register,
        unregister: unregister,
        setActive: setActive
    };

    // Backward-compatible wrappers used by existing pages.
    window.enableProductShortcuts = (dotnetHelper) => {
        if (window.__legacyShortcutId) {
            unregister(window.__legacyShortcutId);
        }

        window.__legacyShortcutId = register(dotnetHelper, {
            enableEsc: true,
            enableEnter: true,
            enableF1: true,
            enableF5: true
        });
    };

    window.enableCustomerShortcuts = window.enableProductShortcuts;
    window.enableEnterNavigation = () => { };
    window.focusNextInput = () => moveFocusToNext(null);
})();
