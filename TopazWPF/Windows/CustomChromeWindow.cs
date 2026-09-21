using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace TopazWPF.Windows
{
    /// <summary>
    /// Base window class providing custom chrome with Mica backdrop and native resizing.
    /// Inherit from this class to create windows with custom title bars.
    /// </summary>
    public class CustomChromeWindow : Window
    {
        private Grid? _captionBar;
        private Button? _minimizeButton;
        private Button? _maximizeButton;
        private Button? _closeButton;

        #region Custom Window Properties
        public static readonly DependencyProperty BaseDraggableProperty =
            DependencyProperty.Register(nameof(BaseDraggable), typeof(bool), typeof(CustomChromeWindow),
                new PropertyMetadata(false));

        /// <summary>
        /// Enables drag-to-move on any empty surface area of the window. Default: false.
        /// </summary>
        public bool BaseDraggable
        {
            get => (bool)GetValue(BaseDraggableProperty);
            set => SetValue(BaseDraggableProperty, value);
        }
        #endregion

        #region Attached Properties for Custom Caption Content

        /// <summary>
        /// Custom content to display in the caption bar (title, navigation, etc.)
        /// </summary>
        public static readonly DependencyProperty CaptionBarProperty =
            DependencyProperty.RegisterAttached(
                "CaptionBar",
                typeof(object),
                typeof(CustomChromeWindow),
                new PropertyMetadata(null));

        public static void SetCaptionBar(DependencyObject element, object value)
            => element.SetValue(CaptionBarProperty, value);

        public static object GetCaptionBar(DependencyObject element)
            => element.GetValue(CaptionBarProperty);

        /// <summary>
        /// Custom buttons to display before the system buttons (settings, help, etc.)
        /// </summary>
        public static readonly DependencyProperty CaptionBarButtonsProperty =
            DependencyProperty.RegisterAttached(
                "CaptionBarButtons",
                typeof(object),
                typeof(CustomChromeWindow),
                new PropertyMetadata(null));

        public static void SetCaptionBarButtons(DependencyObject element, object value) => element.SetValue(CaptionBarButtonsProperty, value);

        public static object GetCaptionBarButtons(DependencyObject element) => element.GetValue(CaptionBarButtonsProperty);

        public static readonly DependencyProperty ShowCaptionBarProperty =
        DependencyProperty.Register(nameof(ShowCaptionBar), typeof(bool), typeof(CustomChromeWindow),
        new PropertyMetadata(true));

        public static readonly DependencyProperty ShowMinimizeButtonProperty =
            DependencyProperty.Register(nameof(ShowMinimizeButton), typeof(bool), typeof(CustomChromeWindow),
                new PropertyMetadata(true));

        public static readonly DependencyProperty ShowMaximizeButtonProperty =
            DependencyProperty.Register(nameof(ShowMaximizeButton), typeof(bool), typeof(CustomChromeWindow),
                new PropertyMetadata(true));

        public static readonly DependencyProperty ShowCloseButtonProperty =
            DependencyProperty.Register(nameof(ShowCloseButton), typeof(bool), typeof(CustomChromeWindow),
                new PropertyMetadata(true));

        /// <summary>
        /// Controls caption bar visibility. Default: true.
        /// </summary>
        public bool ShowCaptionBar
        {
            get => (bool)GetValue(ShowCaptionBarProperty);
            set => SetValue(ShowCaptionBarProperty, value);
        }

        /// <summary>
        /// Controls minimize button visibility. Default: true.
        /// </summary>
        public bool ShowMinimizeButton
        {
            get => (bool)GetValue(ShowMinimizeButtonProperty);
            set => SetValue(ShowMinimizeButtonProperty, value);
        }

        /// <summary>
        /// Controls maximize/restore button visibility. Default: true.
        /// </summary>
        public bool ShowMaximizeButton
        {
            get => (bool)GetValue(ShowMaximizeButtonProperty);
            set => SetValue(ShowMaximizeButtonProperty, value);
        }

        /// <summary>
        /// Controls close button visibility. Default: true.
        /// </summary>
        public bool ShowCloseButton
        {
            get => (bool)GetValue(ShowCloseButtonProperty);
            set => SetValue(ShowCloseButtonProperty, value);
        }

        #endregion

        #region Properties for Custom Status Bar

        public static readonly DependencyProperty StatusBarLeftProperty =
            DependencyProperty.Register(nameof(StatusBarLeft), typeof(object), typeof(CustomChromeWindow));

        public static readonly DependencyProperty StatusBarRightProperty =
            DependencyProperty.Register(nameof(StatusBarRight), typeof(object), typeof(CustomChromeWindow));

        public static readonly DependencyProperty ShowStatusBarProperty =
            DependencyProperty.Register(nameof(ShowStatusBar), typeof(bool), typeof(CustomChromeWindow),
        new PropertyMetadata(true));

        /// <summary>
        /// Controls status bar visibility. Set to false to hide. Default: true.
        /// </summary>
        public bool ShowStatusBar
        {
            get => (bool)GetValue(ShowStatusBarProperty);
            set => SetValue(ShowStatusBarProperty, value);
        }

        /// <summary>
        /// Content for the left side of the status bar
        /// </summary>
        public object StatusBarLeft
        {
            get => GetValue(StatusBarLeftProperty);
            set => SetValue(StatusBarLeftProperty, value);
        }

        /// <summary>
        /// Content for the right side of the status bar
        /// </summary>
        public object StatusBarRight
        {
            get => GetValue(StatusBarRightProperty);
            set => SetValue(StatusBarRightProperty, value);
        }

        #endregion

        /// <summary>
        /// Set to false to prevent WM_NCCALCSIZE interception for modal dialogs, child windows, etc.
        /// </summary>
        protected bool AllowNativeChromeMessages { get; set; } = false;

        /// <summary>
        /// Size of the resize handle area in pixels
        /// </summary>
        protected int ResizeHandleSize { get; set; } = 6;

        public CustomChromeWindow()
        {
            // Essential settings for custom chrome
            WindowStyle = WindowStyle.None;

        }

        private void CaptionBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (e.ClickCount == 2)
                {
                    // Double-click to maximize/restore
                    ToggleMaximizeRestore();
                    return;
                }
                else if (WindowState != WindowState.Maximized)
                {
                    try
                    {
                        DragMove();
                    }
                    catch (InvalidOperationException)
                    {
                        // DragMove can throw if called at the wrong time (i.e. this was just in an else block and you double clicked.)
                    }
                }
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleMaximizeRestore();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void UpdateWindowStateVisuals()
        {
            if (_maximizeButton == null) return;

            bool canMaximize = ResizeMode == ResizeMode.CanResize || ResizeMode == ResizeMode.CanResizeWithGrip;

            _maximizeButton.IsEnabled = canMaximize;
            _maximizeButton.Content = WindowState == WindowState.Maximized ? "\uE923" : "\uE922"; //MDL2 restore icon : MDL2 maximize icon

        }

        /// <summary>
        /// Toggles between maximized and normal states if various checkes allow for it.
        /// Added because some conditions allow for resizing or moving forms that have
        /// resizing disallowed.
        /// </summary>
        private void ToggleMaximizeRestore()
        {
            if (ResizeMode == ResizeMode.CanResize || ResizeMode == ResizeMode.CanResizeWithGrip)
            {
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            }
        }

        #region Overrides
        public override void OnApplyTemplate()
        {
            // Detach old handlers
            if (_captionBar != null) _captionBar.MouseLeftButtonDown -= CaptionBar_MouseLeftButtonDown;
            if (_minimizeButton != null) _minimizeButton.Click -= MinimizeButton_Click;
            if (_maximizeButton != null) _maximizeButton.Click -= MaximizeButton_Click;
            if (_closeButton != null) _closeButton.Click -= CloseButton_Click;

            base.OnApplyTemplate();

            // set new template parts
            _captionBar = GetTemplateChild("PART_CaptionBar") as Grid;
            _minimizeButton = GetTemplateChild("PART_MinimizeButton") as Button;
            _maximizeButton = GetTemplateChild("PART_MaximizeButton") as Button;
            _closeButton = GetTemplateChild("PART_CloseButton") as Button;

            // Attach new handlers
            if (_captionBar != null) _captionBar.MouseLeftButtonDown += CaptionBar_MouseLeftButtonDown;
            if (_minimizeButton != null) _minimizeButton.Click += MinimizeButton_Click;
            if (_maximizeButton != null) _maximizeButton.Click += MaximizeButton_Click;
            if (_closeButton != null) _closeButton.Click += CloseButton_Click;

            //update visual styles
            UpdateWindowStateVisuals();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            UpdateWindowStateVisuals();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var hwnd = new WindowInteropHelper(this).Handle;
            var source = HwndSource.FromHwnd(hwnd);

            if (source != null)
            {
                source.AddHook(WndProc);
                ApplyDwm(hwnd);
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (BaseDraggable && e.ChangedButton == MouseButton.Left)
            {
                if (e.ClickCount == 2)
                {
                    ToggleMaximizeRestore();
                }
                else if (WindowState != WindowState.Maximized)
                {
                    try
                    {
                        DragMove();
                    }
                    catch
                    {
                        // DragMove can throw if not in the right state
                    }
                }
            }
        }

        #endregion

        #region DWM Integration
        internal enum DWMWINDOWATTRIBUTE
        {
            DWMWA_NCRENDERING_ENABLED = 1,
            DWMWA_NCRENDERING_POLICY = 2,
            DWMWA_TRANSITIONS_FORCEDISABLED = 3,
            DWMWA_ALLOW_NCPAINT = 4,
            DWMWA_CAPTION_BUTTON_BOUNDS = 5,
            DWMWA_NONCLIENT_RTL_LAYOUT = 6,
            DWMWA_FORCE_ICONIC_REPRESENTATION = 7,
            DWMWA_FLIP3D_POLICY = 8,
            DWMWA_EXTENDED_FRAME_BOUNDS = 9,
            DWMWA_HAS_ICONIC_BITMAP = 10, // 0x0000000A
            DWMWA_DISALLOW_PEEK = 11, // 0x0000000B
            DWMWA_EXCLUDED_FROM_PEEK = 12, // 0x0000000C
            DWMWA_CLOAK = 13, // 0x0000000D
            DWMWA_CLOAKED = 14, // 0x0000000E
            DWMWA_FREEZE_REPRESENTATION = 15, // 0x0000000F
            DWMWA_PASSIVE_UPDATE_MODE = 16, // 0x00000010
            DWMWA_USE_HOSTBACKDROPBRUSH = 17, // 0x00000011
            DWMWA_USE_IMMERSIVE_DARK_MODE = 20, // 0x00000014
            DWMWA_WINDOW_CORNER_PREFERENCE = 33, // 0x00000021
            DWMWA_BORDER_COLOR = 34, // 0x00000022
            DWMWA_CAPTION_COLOR = 35, // 0x00000023
            DWMWA_TEXT_COLOR = 36, // 0x00000024
            DWMWA_VISIBLE_FRAME_BORDER_THICKNESS = 37, // 0x00000025
            DWMWA_SYSTEMBACKDROP_TYPE = 38, // 0x00000026
            DWMWA_REDIRECTIONBITMAP_ALPHA = 39, // 0x00000027
            DWMWA_LAST = 40, // 0x00000028
        }
        private void ApplyDwm(IntPtr hwnd, int clr = 0x00150A0A)
        {
            if (hwnd == IntPtr.Zero) return;

            try
            {
                DwmIsCompositionEnabled(out bool compositionEnabled);
                if (!compositionEnabled) return;

                // Set Dark Mode
                int darkMode = 1;
                DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int));

                // Border Color
                int borderColor = unchecked((int)clr);
                DwmSetWindowAttribute(hwnd, DWMWA_BORDER_COLOR, ref borderColor, sizeof(int));

                // Set Corner Preference
                int cornerPreference = (int)DwmWindowCornerPreference.Round;
                DwmSetWindowAttribute(hwnd, DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerPreference, sizeof(int));

                // Backdrop - Mica
                int backdrop = (int)OpalSystemBackdrop.Mica;
                DwmSetWindowAttribute(hwnd, DWMWA_SYSTEMBACKDROP_TYPE, ref backdrop, sizeof(int));

                // Extend frame into client area
                MARGINS margins = new MARGINS
                {
                    cxLeftWidth = 0,
                    cxRightWidth = 0,
                    cyTopHeight = 1, // Small value for custom title bar
                    cyBottomHeight = 0
                };
                DwmExtendFrameIntoClientArea(hwnd, ref margins);
            }
            catch
            {
                // Silently fail on Windows 10 or if DWM features unavailable
            }
        }
        private void ApplyDwmBorder(IntPtr hwnd, int clr = 0x00140A0A)
        {
            if (hwnd == IntPtr.Zero) return;

            try
            {
                // Border Color
                int borderColor = unchecked((int)clr);
                DwmSetWindowAttribute(hwnd, DWMWA_BORDER_COLOR, ref borderColor, sizeof(int));
            }
            catch
            {
                // Silently fail on Windows 10 or if DWM features unavailable
            }
        }

        public void SetWindowBorderColor(int color = 0x000000FF)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            if (hwnd != IntPtr.Zero)
            {
                ApplyDwmBorder(hwnd, color);
            }
        }

        /// <summary>
        /// Set window border color using System.Windows.Media.Color
        /// </summary>
        public void SetWindowBorderColor(System.Windows.Media.Color color)
        {
            // Convert ARGB to ABGR (Windows DWM expects 0xAABBGGRR format)
            int dwmColor = unchecked((int)(
                ((uint)color.A << 24) |  // Alpha stays in high byte
                ((uint)color.R << 0) |  // Red goes to low byte
                ((uint)color.G << 8) |  // Green in middle
                ((uint)color.B << 16)    // Blue in high-middle
            ));

            SetWindowBorderColor(dwmColor);
        }

        #endregion

        #region Window Message Processing

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // Always suppress non-client painting to avoid white border
            if (msg == WM_NCPAINT)
            {
                handled = true;
                return IntPtr.Zero;
            }

            // Handle resize hit testing
            if (msg == WM_NCHITTEST)
            {
                // Extract mouse coords from lParam
                int x = unchecked((short)((long)lParam & 0xFFFF));
                int y = unchecked((short)(((long)lParam >> 16) & 0xFFFF));
                var point = PointFromScreen(new Point(x, y));

                bool left = point.X <= ResizeHandleSize;
                bool right = point.X >= ActualWidth - ResizeHandleSize;
                bool top = point.Y <= ResizeHandleSize;
                bool bottom = point.Y >= ActualHeight - ResizeHandleSize;

                if (top && left) { handled = true; return (IntPtr)HTTOPLEFT; }
                if (top && right) { handled = true; return (IntPtr)HTTOPRIGHT; }
                if (bottom && left) { handled = true; return (IntPtr)HTBOTTOMLEFT; }
                if (bottom && right) { handled = true; return (IntPtr)HTBOTTOMRIGHT; }
                if (left) { handled = true; return (IntPtr)HTLEFT; }
                if (right) { handled = true; return (IntPtr)HTRIGHT; }
                if (top) { handled = true; return (IntPtr)HTTOP; }
                if (bottom) { handled = true; return (IntPtr)HTBOTTOM; }
            }

            // Handle NC messages for custom chrome
            if (!AllowNativeChromeMessages)
            {
                if (msg == WM_NCACTIVATE)
                {
                    handled = true;
                    return IntPtr.Zero;
                }

                if (msg == WM_NCCALCSIZE && wParam.ToInt32() == 1)
                {
                    handled = true;
                    return IntPtr.Zero;
                }
            }
            else
            {
                
                // When allowing native messages for modals/dialogs,
                // still handle WM_NCACTIVATE to prevent the white border flash
                if (msg == WM_NCACTIVATE)
                {
                    handled = true;
                    return new IntPtr(1);
                }
            }

            return IntPtr.Zero;
        }

        #endregion

        #region P/Invoke Declarations

        // DWM API
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(
            IntPtr hwnd,
            int dwAttribute,
            ref int pvAttribute,
            int cbAttribute);

        [DllImport("dwmapi.dll")]
        private static extern int DwmIsCompositionEnabled(out bool enabled);

        [DllImport("dwmapi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(
            IntPtr hwnd,
            ref MARGINS pMarInset);

        [StructLayout(LayoutKind.Sequential)]
        public struct MARGINS
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }

        // DWM Attribute Constants
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        private const int DWMWA_BORDER_COLOR = 34;
        private const int DWMWA_SYSTEMBACKDROP_TYPE = 38;

        public enum DwmWindowCornerPreference
        {
            SystemDefault = 0,
            NoRounding = 1,
            Round = 2,
            MinorRounding = 3
        }

        public enum OpalSystemBackdrop
        {
            Auto = 0,
            None = 1,
            Mica = 2,
            Acrylic = 3,
            MicaAlt = 4
        }

        // Window Message Constants
        private const int WM_NCCALCSIZE = 0x0083;
        private const int WM_NCACTIVATE = 0x0086;
        private const int WM_NCHITTEST = 0x84;
        private const int WM_NCPAINT = 0x0085;

        // Hit Test Constants
        private const int HTLEFT = 10;
        private const int HTRIGHT = 11;
        private const int HTTOP = 12;
        private const int HTTOPLEFT = 13;
        private const int HTTOPRIGHT = 14;
        private const int HTBOTTOM = 15;
        private const int HTBOTTOMLEFT = 16;
        private const int HTBOTTOMRIGHT = 17;

        #endregion
    }
}