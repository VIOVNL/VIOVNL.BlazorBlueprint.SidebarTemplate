window.cssTheme = {
    setTheme(path) {
        let link = document.getElementById('css-theme');
        if (!link) {
            link = document.createElement('link');
            link.id = 'css-theme';
            link.rel = 'stylesheet';
            document.head.appendChild(link);
        }
        link.href = path;
    },

    clearFonts() {
        document.querySelectorAll('link[data-gf]').forEach(l => l.remove());
    },

    async loadFontsFromTheme(path) {
        const text = await fetch(path).then(r => r.text());

        const generics = new Set([
            'ui-sans-serif', 'ui-serif', 'ui-monospace', 'system-ui',
            '-apple-system', 'BlinkMacSystemFont', 'sans-serif', 'serif',
            'monospace', 'cursive', 'fantasy', 'math'
        ]);

        const extract = (variable) => {
            const m = text.match(new RegExp(`${variable}\\s*:\\s*([^;]+);`));
            if (!m) return null;
            const first = m[1].trim().split(',')[0].trim().replace(/['"]/g, '');
            return generics.has(first) ? null : first;
        };

        const sans  = extract('--font-sans');
        const serif = extract('--font-serif');
        const mono  = extract('--font-mono');

        const families = [];
        if (sans)  families.push(`${sans.replace(/ /g,  '+')}:wght@300;400;500;600;700`);
        if (serif) families.push(`${serif.replace(/ /g, '+')}:ital,wght@0,300;0,400;0,600;1,400`);
        if (mono)  families.push(`${mono.replace(/ /g,  '+')}:wght@400;500`);

        if (families.length === 0) return;

        const link = document.createElement('link');
        link.rel = 'stylesheet';
        link.href = `https://fonts.googleapis.com/css2?${families.map(f => `family=${f}`).join('&')}&display=swap`;
        link.dataset.gf = '1';
        document.head.appendChild(link);
    },

    toggleDark(isDark) {
        document.documentElement.classList.toggle('dark', isDark);
    },

    isDark() {
        return document.documentElement.classList.contains('dark');
    },

    getStored(key) {
        return localStorage.getItem(key);
    },

    setStored(key, value) {
        localStorage.setItem(key, value);
    }
};
