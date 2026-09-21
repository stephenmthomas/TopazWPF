using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace TopazWPF.Themes
{
    /// <summary>
    /// Provides smooth color transitions for WPF controls.
    /// Usage: Add local:AnimationHelper.AnimateBrush="True" to any Border/Control.
    /// </summary>
    public static class AnimationHelper
    {
        private static readonly DependencyProperty IsAnimatingProperty =
            DependencyProperty.RegisterAttached("IsAnimating", typeof(bool), typeof(AnimationHelper), new PropertyMetadata(false));

        public static readonly DependencyProperty AnimateBrushProperty =
            DependencyProperty.RegisterAttached(
                "AnimateBrush",
                typeof(bool),
                typeof(AnimationHelper),
                new PropertyMetadata(false, OnAnimateBrushChanged));

        public static bool GetAnimateBrush(DependencyObject obj)
            => (bool)obj.GetValue(AnimateBrushProperty);

        public static void SetAnimateBrush(DependencyObject obj, bool value)
            => obj.SetValue(AnimateBrushProperty, value);

        private static void OnAnimateBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Border border && (bool)e.NewValue)
            {
                // Wire up Background color animation
                var bgDescriptor = DependencyPropertyDescriptor.FromProperty(
                    Border.BackgroundProperty, typeof(Border));
                bgDescriptor?.AddValueChanged(border, OnBackgroundChanged);

                // Wire up BorderBrush color animation
                var borderDescriptor = DependencyPropertyDescriptor.FromProperty(
                    Border.BorderBrushProperty, typeof(Border));
                borderDescriptor?.AddValueChanged(border, OnBorderBrushChanged);
            }
        }

        private static void OnBackgroundChanged(object sender, EventArgs e)
        {
            if (sender is Border border)
            {
                // Prevent recursion
                if ((bool)border.GetValue(IsAnimatingProperty))
                    return;

                if (border.Background is SolidColorBrush brush)
                {
                    border.SetValue(IsAnimatingProperty, true);

                    // Clone the brush to make it animatable
                    var animatedBrush = brush.CloneCurrentValue();
                    border.Background = animatedBrush;

                    // Create animation
                    var animation = new ColorAnimation
                    {
                        To = brush.Color,
                        Duration = TimeSpan.FromMilliseconds(150),
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                    };

                    animation.Completed += (s, args) => border.SetValue(IsAnimatingProperty, false);

                    // Animate the cloned brush
                    animatedBrush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
                }
            }
        }

        private static void OnBorderBrushChanged(object sender, EventArgs e)
        {
            if (sender is Border border)
            {
                // Prevent recursion
                if ((bool)border.GetValue(IsAnimatingProperty))
                    return;

                if (border.BorderBrush is SolidColorBrush brush)
                {
                    border.SetValue(IsAnimatingProperty, true);

                    // Clone the brush to make it animatable
                    var animatedBrush = brush.CloneCurrentValue();
                    border.BorderBrush = animatedBrush;

                    // Create animation
                    var animation = new ColorAnimation
                    {
                        To = brush.Color,
                        Duration = TimeSpan.FromMilliseconds(350),
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                    };

                    animation.Completed += (s, args) => border.SetValue(IsAnimatingProperty, false);

                    // Animate the cloned brush
                    animatedBrush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
                }
            }
        }
    }
}