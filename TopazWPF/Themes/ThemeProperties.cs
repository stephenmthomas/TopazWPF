using System.Windows;
using System.Windows.Media;

namespace TopazWPF.Themes
{
    public enum TopazClass
    {
        Default,
        Primary,
        Secondary,
        Tertiary,
        Success,
        Warning,
        Danger
    }

    public enum TopazStyle
    {
        Default,    // Rounded
        Flat,       // No border
        Square,     // No corner radius
        Pill,       // Circular ends
        Ghost,      // Only text visible until hover
        Outline     // Transparent body, border assumes class color
    }

    public enum TopazBehavior
    {
        Default,    // Standard hover/press
        Lighten,    // Hover brighter, pressed darker
        Darken,     // Hover darker, pressed lighter
        Squeeze,    // Physical dimensions change
        Flash       // Special FX / glow
    }

    public static class Theme
    {
        #region Input Properties

        public static void Refresh(DependencyObject obj)
        {
            Resolve(obj);
        }

        public static readonly DependencyProperty ClassProperty =
            DependencyProperty.RegisterAttached(
                "Class",
                typeof(TopazClass),
                typeof(Theme),
                new FrameworkPropertyMetadata(
                    TopazClass.Default,
                    FrameworkPropertyMetadataOptions.Inherits,
                    OnThemeInputChanged));

        public static readonly DependencyProperty StyleProperty =
            DependencyProperty.RegisterAttached(
                "Style",
                typeof(TopazStyle),
                typeof(Theme),
                new FrameworkPropertyMetadata(
                    TopazStyle.Default,
                    FrameworkPropertyMetadataOptions.Inherits,
                    OnThemeInputChanged));

        public static readonly DependencyProperty BehaviorProperty =
            DependencyProperty.RegisterAttached(
                "Behavior",
                typeof(TopazBehavior),
                typeof(Theme),
                new FrameworkPropertyMetadata(
                    TopazBehavior.Default,
                    FrameworkPropertyMetadataOptions.Inherits,
                    OnThemeInputChanged));

        public static TopazClass GetClass(DependencyObject obj) => (TopazClass)obj.GetValue(ClassProperty);
        public static void SetClass(DependencyObject obj, TopazClass value) => obj.SetValue(ClassProperty, value);

        public static TopazStyle GetStyle(DependencyObject obj) => (TopazStyle)obj.GetValue(StyleProperty);
        public static void SetStyle(DependencyObject obj, TopazStyle value) => obj.SetValue(StyleProperty, value);

        public static TopazBehavior GetBehavior(DependencyObject obj) => (TopazBehavior)obj.GetValue(BehaviorProperty);
        public static void SetBehavior(DependencyObject obj, TopazBehavior value) => obj.SetValue(BehaviorProperty, value);

        #endregion

        #region Output Properties

        public static readonly DependencyProperty BackgroundProperty =
            DependencyProperty.RegisterAttached("Background", typeof(Brush), typeof(Theme), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty HoverBackgroundProperty =
            DependencyProperty.RegisterAttached("HoverBackground", typeof(Brush), typeof(Theme), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty PressedBackgroundProperty =
            DependencyProperty.RegisterAttached("PressedBackground", typeof(Brush), typeof(Theme), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty ForegroundProperty =
            DependencyProperty.RegisterAttached("Foreground", typeof(Brush), typeof(Theme), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty BorderBrushProperty =
            DependencyProperty.RegisterAttached("BorderBrush", typeof(Brush), typeof(Theme), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty HoverBorderBrushProperty =
            DependencyProperty.RegisterAttached("HoverBorderBrush", typeof(Brush), typeof(Theme), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty PressedBorderBrushProperty =
            DependencyProperty.RegisterAttached("PressedBorderBrush", typeof(Brush), typeof(Theme), new FrameworkPropertyMetadata(null));

        public static Brush? GetBackground(DependencyObject obj) => (Brush?)obj.GetValue(BackgroundProperty);
        public static void SetBackground(DependencyObject obj, Brush? value) => obj.SetValue(BackgroundProperty, value);

        public static Brush? GetHoverBackground(DependencyObject obj) => (Brush?)obj.GetValue(HoverBackgroundProperty);
        public static void SetHoverBackground(DependencyObject obj, Brush? value) => obj.SetValue(HoverBackgroundProperty, value);

        public static Brush? GetPressedBackground(DependencyObject obj) => (Brush?)obj.GetValue(PressedBackgroundProperty);
        public static void SetPressedBackground(DependencyObject obj, Brush? value) => obj.SetValue(PressedBackgroundProperty, value);

        public static Brush? GetForeground(DependencyObject obj) => (Brush?)obj.GetValue(ForegroundProperty);
        public static void SetForeground(DependencyObject obj, Brush? value) => obj.SetValue(ForegroundProperty, value);

        public static Brush? GetBorderBrush(DependencyObject obj) => (Brush?)obj.GetValue(BorderBrushProperty);
        public static void SetBorderBrush(DependencyObject obj, Brush? value) => obj.SetValue(BorderBrushProperty, value);

        public static Brush? GetHoverBorderBrush(DependencyObject obj) => (Brush?)obj.GetValue(HoverBorderBrushProperty);
        public static void SetHoverBorderBrush(DependencyObject obj, Brush? value) => obj.SetValue(HoverBorderBrushProperty, value);

        public static Brush? GetPressedBorderBrush(DependencyObject obj) => (Brush?)obj.GetValue(PressedBorderBrushProperty);
        public static void SetPressedBorderBrush(DependencyObject obj, Brush? value) => obj.SetValue(PressedBorderBrushProperty, value);

        #endregion

        #region Resolver

        private static void OnThemeInputChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Resolve(d);
        }

        private static void Resolve(DependencyObject obj)
        {
            if (obj is not FrameworkElement fe)
                return;

            TopazClass cls = GetClass(obj);
            TopazStyle style = GetStyle(obj);
            TopazBehavior behavior = GetBehavior(obj);

            string family = GetFamilyName(cls);

            // Load semantic class colors (expanded palette)
            Brush baseBrush = FindBrush(fe, $"Theme.Brush.{family}.Base");
            Brush hoverBrush = FindBrush(fe, $"Theme.Brush.{family}.Hover");
            Brush pressedBrush = FindBrush(fe, $"Theme.Brush.{family}.Pressed");
            Brush lightBrush = FindBrush(fe, $"Theme.Brush.{family}.Light");
            Brush lighterBrush = FindBrush(fe, $"Theme.Brush.{family}.Lighter");
            Brush darkBrush = FindBrush(fe, $"Theme.Brush.{family}.Dark");
            Brush darkerBrush = FindBrush(fe, $"Theme.Brush.{family}.Darker");
            Brush subtle25 = FindBrush(fe, $"Theme.Brush.{family}.Subtle25");
            Brush subtle50 = FindBrush(fe, $"Theme.Brush.{family}.Subtle50");
            Brush subtle75 = FindBrush(fe, $"Theme.Brush.{family}.Subtle75");

            // Load utility brushes
            Brush surfaceBase = FindBrush(fe, "Theme.Brush.Surface.Base");
            Brush surfaceRaised = FindBrush(fe, "Theme.Brush.Surface.Raised");
            Brush surfaceHover = FindBrush(fe, "Theme.Brush.Surface.Hover");
            Brush surfacePressed = FindBrush(fe, "Theme.Brush.Surface.Pressed");
            Brush borderDefault = FindBrush(fe, "Theme.Brush.Border.Default");
            Brush borderHover = FindBrush(fe, "Theme.Brush.Border.Hover");
            Brush borderSubtle = FindBrush(fe, "Theme.Brush.Border.Subtle");
            Brush textPrimary = FindBrush(fe, "Theme.Brush.Text.Primary");
            Brush white = FindBrush(fe, "Theme.Brush.White");

            Brush normalBackground;
            Brush hoverBackground;
            Brush pressedBackground;
            Brush foreground;
            Brush borderBrush;
            Brush hoverBorderBrush;
            Brush pressedBorderBrush;

            // Determine base behavior based on Class
            if (cls == TopazClass.Default)
            {
                // Default class uses Surface colors
                normalBackground = surfaceBase;
                hoverBackground = surfaceHover;
                pressedBackground = surfacePressed;
                foreground = textPrimary;
                borderBrush = borderDefault;
                hoverBorderBrush = borderHover;
                pressedBorderBrush = borderHover;
            }
            else
            {
                // Semantic classes (Primary, Secondary, etc.)
                normalBackground = baseBrush;
                foreground = white;
                borderBrush = baseBrush;
                hoverBorderBrush = hoverBrush;
                pressedBorderBrush = pressedBrush;

                // Apply Behavior
                switch (behavior)
                {
                    case TopazBehavior.Lighten:
                        hoverBackground = lightBrush;
                        pressedBackground = pressedBrush;
                        break;

                    case TopazBehavior.Darken:
                        hoverBackground = darkBrush;
                        pressedBackground = darkerBrush;
                        break;

                    case TopazBehavior.Flash:
                        hoverBackground = lighterBrush;
                        pressedBackground = lightBrush;
                        break;

                    case TopazBehavior.Squeeze:
                    case TopazBehavior.Default:
                    default:
                        hoverBackground = hoverBrush;
                        pressedBackground = pressedBrush;
                        break;
                }
            }

            // Apply Style modifications
            switch (style)
            {
                case TopazStyle.Ghost:
                    normalBackground = Brushes.Transparent;
                    hoverBackground = subtle75;
                    pressedBackground = subtle50;
                    foreground = cls == TopazClass.Default ? textPrimary : baseBrush;
                    borderBrush = Brushes.Transparent;
                    hoverBorderBrush = Brushes.Transparent;
                    pressedBorderBrush = Brushes.Transparent;
                    break;

                case TopazStyle.Outline:
                    normalBackground = Brushes.Transparent;
                    hoverBackground = subtle75;
                    pressedBackground = subtle50;
                    foreground = cls == TopazClass.Default ? textPrimary : baseBrush;
                    borderBrush = cls == TopazClass.Default ? borderDefault : baseBrush;
                    hoverBorderBrush = cls == TopazClass.Default ? borderHover : hoverBrush;
                    pressedBorderBrush = cls == TopazClass.Default ? borderHover : pressedBrush;
                    break;

                case TopazStyle.Flat:
                    borderBrush = Brushes.Transparent;
                    hoverBorderBrush = Brushes.Transparent;
                    pressedBorderBrush = Brushes.Transparent;
                    break;
            }

            // Set resolved properties
            SetBackground(obj, normalBackground);
            SetHoverBackground(obj, hoverBackground);
            SetPressedBackground(obj, pressedBackground);
            SetForeground(obj, foreground);
            SetBorderBrush(obj, borderBrush);
            SetHoverBorderBrush(obj, hoverBorderBrush);
            SetPressedBorderBrush(obj, pressedBorderBrush);
        }

        private static string GetFamilyName(TopazClass cls)
        {
            return cls switch
            {
                TopazClass.Primary => "Primary",
                TopazClass.Secondary => "Secondary",
                TopazClass.Tertiary => "Tertiary",
                TopazClass.Success => "Success",
                TopazClass.Warning => "Warning",
                TopazClass.Danger => "Error",
                TopazClass.Default => "Default",
                _ => "Default"
            };
        }

        private static Brush FindBrush(FrameworkElement fe, string key)
        {
            return fe.TryFindResource(key) as Brush ?? Brushes.Transparent;
        }

        #endregion
    }
}