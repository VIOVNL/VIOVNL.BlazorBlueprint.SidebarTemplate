using Microsoft.JSInterop;

namespace SidebarTemplate.Services;

public record ThemeDefinition(string Name, string FileName);

public class ThemeService(IJSRuntime js)
{
    public static readonly List<ThemeDefinition> Themes =
    [
        new("Default",         "default.css"),
        new("Claude",          "claude.css"),
        new("Modern Minimal",  "modernminimal.css"),
        new("Light Green",     "lightgreen.css"),
        new("WhatsApp",        "whatsapp.css"),
    ];

    public ThemeDefinition CurrentTheme { get; private set; } = Themes[0];
    public bool IsDark { get; private set; }
    public bool LoadFonts { get; private set; } = true;

    public event Action? OnChanged;

    public async Task InitializeAsync()
    {
        // Sync C# state from localStorage (visual already applied by inline <script> in <head>)
        var storedTheme = await js.InvokeAsync<string?>("cssTheme.getStored", "theme");
        if (storedTheme is not null)
            CurrentTheme = Themes.FirstOrDefault(t => t.FileName == storedTheme) ?? Themes[0];

        var storedDark = await js.InvokeAsync<string?>("cssTheme.getStored", "dark");
        IsDark = storedDark == "true";

        var storedFonts = await js.InvokeAsync<string?>("cssTheme.getStored", "loadFonts");
        LoadFonts = storedFonts != "false";

        // Load fonts (can't be done in the inline script)
        if (LoadFonts)
        {
            var path = $"css/themes/{CurrentTheme.FileName}";
            await js.InvokeVoidAsync("cssTheme.loadFontsFromTheme", path);
        }

        OnChanged?.Invoke();
    }

    public async Task SetThemeAsync(ThemeDefinition theme)
    {
        CurrentTheme = theme;
        await js.InvokeVoidAsync("cssTheme.setStored", "theme", theme.FileName);
        await ApplyThemeAsync();
        OnChanged?.Invoke();
    }

    public async Task ToggleDarkAsync()
    {
        IsDark = !IsDark;
        await js.InvokeVoidAsync("cssTheme.setStored", "dark", IsDark.ToString().ToLower());
        await js.InvokeVoidAsync("cssTheme.toggleDark", IsDark);
        OnChanged?.Invoke();
    }

    public async Task SetLoadFontsAsync(bool value)
    {
        LoadFonts = value;
        await js.InvokeVoidAsync("cssTheme.setStored", "loadFonts", value.ToString().ToLower());
        await ApplyThemeAsync();
        OnChanged?.Invoke();
    }

    private async Task ApplyThemeAsync()
    {
        var path = $"css/themes/{CurrentTheme.FileName}";
        await js.InvokeVoidAsync("cssTheme.setTheme", path);
        await js.InvokeVoidAsync("cssTheme.clearFonts");
        if (LoadFonts)
            await js.InvokeVoidAsync("cssTheme.loadFontsFromTheme", path);
    }
}
