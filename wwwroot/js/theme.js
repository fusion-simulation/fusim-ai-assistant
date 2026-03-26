(function () {
    const storageKey = 'fusim-theme';
    const defaultTheme = 'dark';
    const root = document.documentElement;

    function normalize(theme) {
        return theme === 'light' ? 'light' : defaultTheme;
    }

    function apply(theme) {
        const normalizedTheme = normalize(theme);
        root.setAttribute('data-theme', normalizedTheme);
        return normalizedTheme;
    }

    function getStoredTheme() {
        try {
            return window.localStorage.getItem(storageKey);
        } catch {
            return root.getAttribute('data-theme');
        }
    }

    function getTheme() {
        return normalize(getStoredTheme());
    }

    function setTheme(theme) {
        const normalizedTheme = apply(theme);

        try {
            window.localStorage.setItem(storageKey, normalizedTheme);
        } catch {
        }

        return normalizedTheme;
    }

    window.themeManager = {
        getTheme,
        setTheme,
        storageKey
    };

    apply(getStoredTheme());
})();
