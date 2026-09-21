# TopazWPF Quick Start Guide

This guide walks through adding TopazWPF to a new WPF project and getting it running.

## Prerequisites

- Visual Studio 2022+
- .NET 8.0 SDK
- A new WPF App (.NET 8.0) project created

## Steps

### 1. Add TopazWPF as a Project to Your Solution

1. Right-click the **Solution** in Solution Explorer
2. **Add** → **Existing Project...**
3. Navigate to `TopazWPF.csproj` (wherever it lives on disk)
4. Click **Open**

TopazWPF should now appear as a second project in your solution.

---

### 2. Add TopazWPF as a Project Reference

1. Right-click your app project's **Dependencies** node
2. **Add Project Reference...**
3. Check the box next to **TopazWPF**
4. Click **OK**

---

### 3. Modify `App.xaml`

Replace the contents with:

```xml
<Application x:Class="YourNamespace.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             StartupUri="MainWindow.xaml">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="pack://application:,,,/TopazWPF;component/Themes/Generic.xaml"/>
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

**Replace `YourNamespace.App` with your actual namespace** (e.g., `UltraCoolApp.App`).

---

### 4. Modify `App.xaml.cs`

Ensure it matches:

```csharp
using System.Windows;

namespace YourNamespace
{
    public partial class App : Application
    {
    }
}
```

**Replace `YourNamespace` with your actual namespace.**

---

### 5. Modify `MainWindow.xaml`

Replace the root `<Window>` element with `<topaz:CustomChromeWindow>`:

```xml
<topaz:CustomChromeWindow
    x:Class="YourNamespace.MainWindow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:topaz="clr-namespace:TopazWPF.Windows;assembly=TopazWPF"
    xmlns:controls="clr-namespace:TopazWPF.Controls;assembly=TopazWPF"
    xmlns:themes="clr-namespace:TopazWPF.Themes;assembly=TopazWPF"
    Style="{StaticResource CustomChromeWindowStyle}"
    Title="Your App Title"
    Width="800"
    Height="600">

    <topaz:CustomChromeWindow.CaptionBar>
        <TextBlock Text="Your App Title"
                   Foreground="{DynamicResource Theme.Brush.Text.Primary}"
                   FontSize="{DynamicResource Theme.Font.Size.Body}"
                   FontWeight="SemiBold"
                   VerticalAlignment="Center"
                   Margin="8,0,0,0"/>
    </topaz:CustomChromeWindow.CaptionBar>

    <Grid>
        <!-- Your content here -->
        <Button Style="{DynamicResource PrimaryButtonStyle}"
                Content="Hello World!"
                Width="150"
                Height="40"/>
    </Grid>

</topaz:CustomChromeWindow>
```

**Key changes:**
- Root element changed from `<Window>` to `<topaz:CustomChromeWindow>`
- Added three `xmlns` declarations for TopazWPF namespaces
- Added `Style="{StaticResource CustomChromeWindowStyle}"` — **this is critical**
- Added `<topaz:CustomChromeWindow.CaptionBar>` for the title bar content
- Changed closing tag to `</topaz:CustomChromeWindow>`

**Replace `YourNamespace.MainWindow` with your actual namespace.**

---

### 6. Modify `MainWindow.xaml.cs`

Change the base class from `Window` to `CustomChromeWindow`:

```csharp
using TopazWPF.Windows;

namespace YourNamespace
{
    public partial class MainWindow : CustomChromeWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
```

**Key changes:**
- Added `using TopazWPF.Windows;`
- Changed inheritance from `: Window` to `: CustomChromeWindow`

**Replace `YourNamespace` with your actual namespace.**

---

### 7. Build and Run

Press **F5** or click **Start**.

You should see:
- A dark-themed window with custom chrome (no standard title bar)
- A themed caption bar with your app title
- Native window controls (minimize/maximize/close)
- Mica backdrop effect (on Windows 11)
- Your button styled with the TopazWPF `PrimaryButtonStyle`

---

## Common Issues

**"The name 'CustomChromeWindow' does not exist in the namespace..."**
- Verify TopazWPF project reference is added
- Rebuild the solution (TopazWPF needs to compile first)

**Window appears with no title bar or styling**
- Missing `Style="{StaticResource CustomChromeWindowStyle}"` on the root element
- Missing `xmlns:topaz` / `xmlns:controls` / `xmlns:themes` declarations

**Theme not loading / controls are unstyled**
- Verify `App.xaml` has the `Generic.xaml` merged dictionary
- Check that the pack URI is exactly: `pack://application:,,,/TopazWPF;component/Themes/Generic.xaml`

---

## Next Steps

- Browse TopazWPF button styles: `PrimaryButtonStyle`, `OutlineButtonStyle`, `GhostButtonStyle`, `DangerButtonStyle`
- Use theme tokens: `{DynamicResource Theme.Brush.Primary.Base}`, `{DynamicResource Theme.Radius.Large}`, etc.
- Switch themes at runtime: `ThemeManager.ApplyTheme("Themes/ThemeLight.xaml")`
- Customize primary color: `ThemeManager.SetPrimaryColor("#2A7AE2")`

---

# Basic Topaz Window Customizations

Here is a list of custom properties you can use the customize your Topaz Window:
- ShowCaptionBar
- ShowMinimizeButton
- ShowMaximizeButton
- ShowCloseButton
- ShowStatusBar

All true/false and all need to be set in the window property, NOT in the properties of their container.

**Example: Removing Status Bar**
- use the **ShowStatusBar** property
- `<topaz:CustomChromeWindow ... ShowStatusBar="False">`