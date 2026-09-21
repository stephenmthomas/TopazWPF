using System;
using System.Windows;
using System.Windows.Media;
using TopazWPF.ColorUtils;

namespace TopazWPF.Themes
{
    /// <summary>
    /// Manages runtime theme switching and dynamic color generation.
    /// 
    /// The theme dictionary is always the FIRST entry (index 0) in
    /// Application.Resources.MergedDictionaries. Every style file
    /// references theme tokens via DynamicResource, so swapping
    /// the dictionary at index 0 updates the entire UI instantly.
    /// 
    /// Usage:
    ///   // Switch themes
    ///   ThemeManager.ApplyTheme("Themes/ThemeDark.xaml");
    ///   ThemeManager.ApplyTheme("Themes/ThemeLight.xaml");
    ///   
    ///   // Change Primary color (auto-generates Hover, Pressed, Light, Subtle variants)
    ///   ThemeManager.SetPrimaryColor("#2A7AE2");
    ///   
    ///   // Change Secondary color
    ///   ThemeManager.SetSecondaryColor("#8B5CF6");
    ///   
    ///   // Change Tertiary color
    ///   ThemeManager.SetTertiaryColor("#E28A2A");
    ///   
    ///   // Generate Secondary/Tertiary from Primary via hue rotation
    ///   ThemeManager.GenerateSecondaryFromPrimary();  // +120° hue rotation
    ///   ThemeManager.GenerateTertiaryFromPrimary();   // +240° hue rotation
    /// </summary>
    public static class ThemeManager
    {
        private const int ThemeDictionaryIndex = 0;

        /// <summary>
        /// Current theme file path (relative to the application).
        /// </summary>
        public static string CurrentTheme { get; private set; } = "Themes/ThemeDark.xaml";

        /// <summary>
        /// Replace the entire theme dictionary at runtime.
        /// </summary>
        /// <param name="relativeUri">
        /// Path to the theme XAML file, e.g. "Themes/ThemeDark.xaml"
        /// </param>
        public static void ApplyTheme(string relativeUri)
        {
            var appDictionaries = Application.Current.Resources.MergedDictionaries;

            if (appDictionaries.Count == 0)
                throw new InvalidOperationException("No resource dictionaries found.");

            // Generic.xaml is at index 0
            var genericDictionary = appDictionaries[0];

            // Build the new theme dictionary
            var newTheme = new ResourceDictionary
            {
                Source = new Uri(relativeUri, UriKind.Relative)
            };

            // Swap: remove old theme, insert new at the same index (0) inside Generic.xaml
            if (genericDictionary.MergedDictionaries.Count > 0)
                genericDictionary.MergedDictionaries.RemoveAt(0);

            genericDictionary.MergedDictionaries.Insert(0, newTheme);
            CurrentTheme = relativeUri;
        }

        #region Primary Color

        /// <summary>
        /// Set the Primary color and automatically generate all variants
        /// (Hover, Pressed, Light, Subtle10/20/25).
        /// </summary>
        public static void SetPrimaryColor(string hexColor)
        {
            var baseColor = ThemeColor.ColorFromHex(hexColor);
            SetPrimaryColor(baseColor);
        }

        /// <summary>
        /// Set the Primary color and automatically generate all variants.
        /// </summary>
        public static void SetPrimaryColor(Color baseColor)
        {
            var palette = ThemeColor.GeneratePalette(baseColor);
            ApplyPalette("Primary", palette);
        }

        #endregion

        #region Secondary Color

        /// <summary>
        /// Set the Secondary color and automatically generate all variants
        /// (Hover, Pressed, Light, Subtle10/20/25).
        /// </summary>
        public static void SetSecondaryColor(string hexColor)
        {
            var baseColor = ThemeColor.ColorFromHex(hexColor);
            SetSecondaryColor(baseColor);
        }

        /// <summary>
        /// Set the Secondary color and automatically generate all variants.
        /// </summary>
        public static void SetSecondaryColor(Color baseColor)
        {
            var palette = ThemeColor.GeneratePalette(baseColor);
            ApplyPalette("Secondary", palette);
        }

        /// <summary>
        /// Generate Secondary color from Primary using +120° hue rotation.
        /// </summary>
        public static void GenerateSecondaryFromPrimary()
        {
            var dict = GetThemeDictionary();

            if (!dict.Contains("Theme.Primary.Base"))
                throw new InvalidOperationException("Theme.Primary.Base not found in theme dictionary.");

            var primaryBase = (Color)dict["Theme.Primary.Base"];
            var secondaryBase = ThemeColor.GenerateSecondary(primaryBase);

            SetSecondaryColor(secondaryBase);
        }

        #endregion

        #region Tertiary Color

        /// <summary>
        /// Set the Tertiary color and automatically generate all variants
        /// (Hover, Pressed, Light, Subtle10/20/25).
        /// </summary>
        public static void SetTertiaryColor(string hexColor)
        {
            var baseColor = ThemeColor.ColorFromHex(hexColor);
            SetTertiaryColor(baseColor);
        }

        /// <summary>
        /// Set the Tertiary color and automatically generate all variants.
        /// </summary>
        public static void SetTertiaryColor(Color baseColor)
        {
            var palette = ThemeColor.GeneratePalette(baseColor);
            ApplyPalette("Tertiary", palette);
        }

        /// <summary>
        /// Generate Tertiary color from Primary using +240° hue rotation.
        /// </summary>
        public static void GenerateTertiaryFromPrimary()
        {
            var dict = GetThemeDictionary();

            if (!dict.Contains("Theme.Primary.Base"))
                throw new InvalidOperationException("Theme.Primary.Base not found in theme dictionary.");

            var primaryBase = (Color)dict["Theme.Primary.Base"];
            var tertiaryBase = ThemeColor.GenerateTertiary(primaryBase);

            SetTertiaryColor(tertiaryBase);
        }

        #endregion

        #region Semantic Colors (Success, Warning, Error)

        /// <summary>
        /// Set the Success color and automatically generate all variants
        /// (Hover, Pressed, Light, Subtle10/20/25).
        /// </summary>
        public static void SetSuccessColor(string hexColor)
        {
            var baseColor = ThemeColor.ColorFromHex(hexColor);
            SetSuccessColor(baseColor);
        }

        /// <summary>
        /// Set the Success color and automatically generate all variants.
        /// </summary>
        public static void SetSuccessColor(Color baseColor)
        {
            var palette = ThemeColor.GeneratePalette(baseColor);
            ApplyPalette("Success", palette);
        }

        /// <summary>
        /// Set the Warning color and automatically generate all variants
        /// (Hover, Pressed, Light, Subtle10/20/25).
        /// </summary>
        public static void SetWarningColor(string hexColor)
        {
            var baseColor = ThemeColor.ColorFromHex(hexColor);
            SetWarningColor(baseColor);
        }

        /// <summary>
        /// Set the Warning color and automatically generate all variants.
        /// </summary>
        public static void SetWarningColor(Color baseColor)
        {
            var palette = ThemeColor.GeneratePalette(baseColor);
            ApplyPalette("Warning", palette);
        }

        /// <summary>
        /// Set the Error color and automatically generate all variants
        /// (Hover, Pressed, Light, Subtle10/20/25).
        /// </summary>
        public static void SetErrorColor(string hexColor)
        {
            var baseColor = ThemeColor.ColorFromHex(hexColor);
            SetErrorColor(baseColor);
        }

        /// <summary>
        /// Set the Error color and automatically generate all variants.
        /// </summary>
        public static void SetErrorColor(Color baseColor)
        {
            var palette = ThemeColor.GeneratePalette(baseColor);
            ApplyPalette("Error", palette);
        }

        #endregion

        #region Palette Application

        /// <summary>
        /// Apply a complete color palette to a color family 
        /// (Primary, Secondary, Tertiary, Success, Warning, or Error).
        /// </summary>
        private static void ApplyPalette(string colorFamily, ColorPalette palette)
        {
            var dict = GetThemeDictionary();

            // Update Color resources
            dict[$"Theme.{colorFamily}.Base"] = palette.Base;
            dict[$"Theme.{colorFamily}.Hover"] = palette.Hover;
            dict[$"Theme.{colorFamily}.Pressed"] = palette.Pressed;
            dict[$"Theme.{colorFamily}.Lighter"] = palette.Lighter;
            dict[$"Theme.{colorFamily}.Light"] = palette.Light;
            dict[$"Theme.{colorFamily}.Dark"] = palette.Dark;
            dict[$"Theme.{colorFamily}.Darker"] = palette.Darker;
            dict[$"Theme.{colorFamily}.Subtle25"] = palette.Subtle25;
            dict[$"Theme.{colorFamily}.Subtle50"] = palette.Subtle50;
            dict[$"Theme.{colorFamily}.Subtle75"] = palette.Subtle75;

            // Update Brush resources
            dict[$"Theme.Brush.{colorFamily}.Base"] = new SolidColorBrush(palette.Base);
            dict[$"Theme.Brush.{colorFamily}.Hover"] = new SolidColorBrush(palette.Hover);
            dict[$"Theme.Brush.{colorFamily}.Pressed"] = new SolidColorBrush(palette.Pressed);
            dict[$"Theme.Brush.{colorFamily}.Lighter"] = new SolidColorBrush(palette.Lighter);
            dict[$"Theme.Brush.{colorFamily}.Light"] = new SolidColorBrush(palette.Light);
            dict[$"Theme.Brush.{colorFamily}.Dark"] = new SolidColorBrush(palette.Dark);
            dict[$"Theme.Brush.{colorFamily}.Darker"] = new SolidColorBrush(palette.Darker);
            dict[$"Theme.Brush.{colorFamily}.Subtle25"] = new SolidColorBrush(palette.Subtle25);
            dict[$"Theme.Brush.{colorFamily}.Subtle50"] = new SolidColorBrush(palette.Subtle50);
            dict[$"Theme.Brush.{colorFamily}.Subtle75"] = new SolidColorBrush(palette.Subtle75);

            RefreshThemeProperties();
        }
        

        private static void RefreshThemeProperties()
        {
            foreach (Window window in Application.Current.Windows)
            {
                RefreshElement(window);
            }
        }

        private static void RefreshElement(DependencyObject obj)
        {
            Theme.Refresh(obj);

            int count = VisualTreeHelper.GetChildrenCount(obj);

            for (int i = 0; i < count; i++)
            {
                RefreshElement(VisualTreeHelper.GetChild(obj, i));
            }
        }

        #endregion

        #region Legacy Methods

        /// <summary>
        /// Override a single Color token in the current theme.
        /// Useful for one-off customization.
        /// </summary>
        public static void SetColor(string key, string hex)
        {
            var color = (Color)ColorConverter.ConvertFromString(hex);

            var dict = GetThemeDictionary();
            dict[key] = color;

            // Also update the matching brush if it exists
            string brushKey = key.Replace("Theme.", "Theme.Brush.");
            if (dict.Contains(brushKey))
            {
                dict[brushKey] = new SolidColorBrush(color);
            }
        }

        /// <summary>
        /// Override a single CornerRadius token.
        /// </summary>
        public static void SetCornerRadius(string key, CornerRadius radius)
        {
            var dict = GetThemeDictionary();
            dict[key] = radius;
        }

        /// <summary>
        /// Override a single Thickness token (border thickness).
        /// </summary>
        public static void SetThickness(string key, Thickness thickness)
        {
            var dict = GetThemeDictionary();
            dict[key] = thickness;
        }

        /// <summary>
        /// Override a single Double token (font size, etc).
        /// </summary>
        public static void SetDouble(string key, double value)
        {
            var dict = GetThemeDictionary();
            dict[key] = value;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Get the current theme dictionary.
        /// Looks inside Generic.xaml (index 0) for the ThemeDark.xaml dictionary (also at index 0).
        /// </summary>
        private static ResourceDictionary GetThemeDictionary()
        {
            var appDictionaries = Application.Current.Resources.MergedDictionaries;

            if (appDictionaries.Count == 0)
                throw new InvalidOperationException("No resource dictionaries found in Application.Resources.");

            // Generic.xaml is at index 0
            var genericDictionary = appDictionaries[0];

            if (genericDictionary.MergedDictionaries.Count == 0)
                throw new InvalidOperationException("Generic.xaml has no merged dictionaries.");

            // ThemeDark.xaml is at index 0 inside Generic.xaml
            return genericDictionary.MergedDictionaries[0];
        }

        #endregion

        #region Preset Colors

        /// <summary>
        /// Preset colors for quick testing.
        /// </summary>
        public static class Presets
        {
            // Primary options
            public static readonly Color Blue = ColorFromHex("#2A7AE2");
            public static readonly Color Purple = ColorFromHex("#8B5CF6");
            public static readonly Color Green = ColorFromHex("#27AE60");
            public static readonly Color Red = ColorFromHex("#E74C3C");
            public static readonly Color Orange = ColorFromHex("#E67E22");
            public static readonly Color Pink = ColorFromHex("#E91E63");
            public static readonly Color Teal = ColorFromHex("#1ABC9C");
            public static readonly Color Amber = ColorFromHex("#F39C12");

            private static Color ColorFromHex(string hex)
                => (Color)ColorConverter.ConvertFromString(hex);
        }

        #endregion
    }
}