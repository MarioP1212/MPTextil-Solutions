using MudBlazor;

namespace MPTextil.Web.Presentacion;

public static class Tema
{
    public static readonly MudTheme MPTextil = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#1F3A68",
            Secondary = "#B8862E",
            Tertiary = "#2E7D6B",
            AppbarBackground = "#1F3A68",
            Background = "#F5F4F0",
            DrawerBackground = "#FFFFFF"
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "8px"
        }
    };
}
