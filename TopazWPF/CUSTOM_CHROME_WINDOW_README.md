# CustomChromeWindow Template

A reusable WPF window base class providing custom chrome with Mica backdrop, native resizing, and flexible caption bar layout.

## Features

- ✅ **Custom Chrome** with `WindowStyle="None"` + `ResizeMode="CanResize"`
- ✅ **Mica Backdrop** (Windows 11) via DWM API
- ✅ **Native Edge Resizing** - no WM_NCHITTEST needed
- ✅ **No Grey Bar** - WM_NCCALCSIZE interception removes it
- ✅ **Flexible Caption Bar** - inject custom content and buttons
- ✅ **Theme-Aware** - uses DynamicResource for all colors
- ✅ **Drag to Move** - caption bar handles window dragging
- ✅ **Double-Click Maximize** - built-in maximize/restore

## File Structure

Add these to your TopazWPF library:

```
TopazWPF/
├── Windows/
│   └── CustomChromeWindow.cs          ← Base class
└── Styles/
    └── CustomChromeWindowStyle.xaml   ← Window template + button styles
```

## Integration Steps

### 1. Add Files to TopazWPF Library

Place `CustomChromeWindow.cs` in a `Windows` folder.
Place `CustomChromeWindowStyle.xaml` in your `Styles` folder.

### 2. Update TopazWPF Theme Dictionary

In `TopazWPF/Themes/ThemeDark.xaml`, ensure these keys exist:

```xml
<!-- Required color keys for custom chrome -->
<Color x:Key="Theme.Surface.Base">#1E1E22</Color>
<Color x:Key="Theme.Surface.Chrome">#252529</Color>
<Color x:Key="Theme.Text.Default">#FFFFFF</Color>
<Color x:Key="Theme.FocusGlow30">#30FFFFFF</Color>
<Color x:Key="Theme.FocusGlow50">#50FFFFFF</Color>

<!-- Create brushes from colors -->
<SolidColorBrush x:Key="Theme.Brush.Surface.Base" Color="{StaticResource Theme.Surface.Base}"/>
<SolidColorBrush x:Key="Theme.Brush.Surface.Chrome" Color="{StaticResource Theme.Surface.Chrome}"/>
<SolidColorBrush x:Key="Theme.Brush.Text.Primary" Color="{StaticResource Theme.Text.Default}"/>
<SolidColorBrush x:Key="Theme.Brush.FocusGlow30" Color="{StaticResource Theme.FocusGlow30}"/>
<SolidColorBrush x:Key="Theme.Brush.FocusGlow50" Color="{StaticResource Theme.FocusGlow50}"/>
```

### 3. Merge Style in Your App

In your consuming application's `App.xaml`:

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <!-- Theme tokens first -->
            <ResourceDictionary Source="pack://application:,,,/TopazWPF;component/Themes/ThemeDark.xaml"/>
            
            <!-- Custom chrome window style -->
            <ResourceDictionary Source="pack://application:,,,/TopazWPF;component/Styles/CustomChromeWindowStyle.xaml"/>
            
            <!-- Other styles -->
            <ResourceDictionary Source="pack://application:,,,/TopazWPF;component/Styles/ButtonStyles.xaml"/>
            <!-- ... etc ... -->
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

## Usage Example

### Your Window XAML

```xml
<Window x:Class="YourApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:topaz="clr-namespace:TopazWPF.Windows;assembly=TopazWPF"
        Style="{StaticResource CustomChromeWindowStyle}"
        Title="My App" Height="700" Width="900">

    <!-- Custom Title/Navigation Content -->
    <topaz:CustomChromeWindow.CaptionBar>
        <StackPanel Orientation="Horizontal">
            <TextBlock Text="My Application" Style="{StaticResource CaptionTitleStyle}"/>
            <Button Content="Home" Style="{StaticResource TabNavButton}" Click="Home_Click"/>
        </StackPanel>
    </topaz:CustomChromeWindow.CaptionBar>

    <!-- Optional: Custom Buttons Before Min/Max/Close -->
    <topaz:CustomChromeWindow.CaptionBarButtons>
        <Button Content="&#xE713;" Style="{StaticResource CaptionBarButtonStyle}" 
                ToolTip="Settings" Click="Settings_Click"/>
    </topaz:CustomChromeWindow.CaptionBarButtons>

    <!-- Your Content -->
    <Grid>
        <!-- ... -->
    </Grid>
</Window>
```

### Your Code-Behind

```csharp
using TopazWPF.Windows;

namespace YourApp
{
    public partial class MainWindow : CustomChromeWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            // Your logic
        }
    }
}
```

## Customization Options

### Modal Dialogs / Child Windows

For dialogs or child windows where you DON'T want custom chrome:

```csharp
public partial class MyDialog : CustomChromeWindow
{
    public MyDialog()
    {
        AllowNativeChromeMessages = false; // Disables WM_NCCALCSIZE hook
        InitializeComponent();
    }
}
```

### Custom Caption Bar Buttons

Available Segoe MDL2 Asset icons:
- `&#xE713;` - Settings gear
- `&#xE897;` - Help/Question mark
- `&#xE710;` - Search
- `&#xE72D;` - Folder open
- `&#xE74E;` - Info

### Caption Bar Height

Modify the first `RowDefinition` in the template:

```xml
<Grid.RowDefinitions>
    <RowDefinition Height="40"/> <!-- Change from 32 -->
    <RowDefinition Height="*"/>
</Grid.RowDefinitions>
```

### System Button Width

Change `Width` in `CaptionBarButtonStyle`:

```xml
<Setter Property="Width" Value="50"/> <!-- Default is 46 -->
```

## Theme Requirements

The template expects these DynamicResource keys:

| Key | Purpose |
|-----|---------|
| `Theme.Brush.Surface.Base` | Window background |
| `Theme.Brush.Surface.Chrome` | Caption bar background |
| `Theme.Brush.Text.Primary` | Text/icon color |
| `Theme.Brush.FocusGlow30` | Button hover (light) |
| `Theme.Brush.FocusGlow50` | Button pressed |

## Advanced: Tab Navigation Pattern

See the SettingsWindow example for implementing caption-bar navigation with TabControl:

```xml
<!-- In CaptionBar -->
<Button Content="Display" Tag="0" Click="NavItem_Click" Style="{StaticResource TabNavButton}"/>
<Button Content="Color" Tag="1" Click="NavItem_Click" Style="{StaticResource TabNavButton}"/>

<!-- Main content -->
<TabControl x:Name="MainTabControl" Style="{StaticResource TabPageControlStyle}">
    <TabItem>...</TabItem>
    <TabItem>...</TabItem>
</TabControl>
```

```csharp
private void NavItem_Click(object sender, RoutedEventArgs e)
{
    if (sender is Button button && int.TryParse(button.Tag as string, out int index))
    {
        MainTabControl.SelectedIndex = index;
    }
}
```

## Troubleshooting

**Grey bar appears at top:**
- Ensure `CustomChromeWindow.AllowNativeChromeMessages = true` (default)
- Verify `WindowStyle="None"` and `ResizeMode="CanResize"`

**Can't resize window:**
- `ResizeMode="CanResize"` provides native edge resizing automatically
- No custom WM_NCHITTEST code needed

**Mica backdrop doesn't show:**
- Requires Windows 11
- Fails silently on Windows 10 (falls back to solid color)

**Theme colors not applying:**
- Verify theme is loaded at index 0 in App.xaml MergedDictionaries
- Check that color keys exist in your theme dictionary
