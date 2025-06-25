using MudBlazor;

namespace WiseHR
{
    public static class CustomMudTheme
    {
        
     public static MudTheme MyTheme = new MudTheme()
     {
         PaletteLight = new PaletteLight()
         {
             Primary = "#3a4f66",       // Deep Navy
             Secondary = "#f5f7fa",     // Light Gray
             Tertiary = "#4d9de0",      // Soft Blue
             Background = "#ffffff",    // White
             TextPrimary = "#2c3e50",   // Dark Slate
             TextSecondary = "#64748b", // Slate Gray
             Success = "#10b981",       // Green
             Warning = "#f59e0b",       // Amber
             Error = "#ef4444"          // Red
         }
     };
  }     
}
