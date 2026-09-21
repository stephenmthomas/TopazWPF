using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TopazWPF.Controls;

/// <summary>
/// A two-thumb range slider with an integrated playhead/seeker.
/// Extends the RangeSlider concept with a third draggable indicator
/// for current position (e.g. video playback).
/// 
/// Template parts:
///   PART_Track           – the full-width background track
///   PART_RangeFill       – the filled region between the two range thumbs
///   PART_LowThumb        – the left (minimum) range thumb
///   PART_HighThumb       – the right (maximum) range thumb
///   PART_SeekThumb       – the playhead/seeker thumb (thin line)
///
/// Usage:
///   &lt;controls:RangeSeeker Minimum="0" Maximum="1000"
///                          RangeMin="100" RangeMax="800"
///                          SeekPosition="450" /&gt;
/// </summary>
[TemplatePart(Name = "PART_Track",     Type = typeof(FrameworkElement))]
[TemplatePart(Name = "PART_RangeFill", Type = typeof(FrameworkElement))]
[TemplatePart(Name = "PART_LowThumb",  Type = typeof(Thumb))]
[TemplatePart(Name = "PART_HighThumb", Type = typeof(Thumb))]
[TemplatePart(Name = "PART_SeekThumb", Type = typeof(Thumb))]
public class RangeSeeker : Control
{
    #region Dependency Properties

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(RangeSeeker),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsArrange,
                OnRangeChanged));

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(RangeSeeker),
            new FrameworkPropertyMetadata(100.0, FrameworkPropertyMetadataOptions.AffectsArrange,
                OnRangeChanged));

    public static readonly DependencyProperty RangeMinProperty =
        DependencyProperty.Register(nameof(RangeMin), typeof(double), typeof(RangeSeeker),
            new FrameworkPropertyMetadata(0.0,
                FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnRangeChanged, CoerceRangeMin));

    public static readonly DependencyProperty RangeMaxProperty =
        DependencyProperty.Register(nameof(RangeMax), typeof(double), typeof(RangeSeeker),
            new FrameworkPropertyMetadata(100.0,
                FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnRangeChanged, CoerceRangeMax));

    public static readonly DependencyProperty SeekPositionProperty =
        DependencyProperty.Register(nameof(SeekPosition), typeof(double), typeof(RangeSeeker),
            new FrameworkPropertyMetadata(0.0,
                FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSeekPositionChanged, CoerceSeekPosition));

    public static readonly DependencyProperty StepProperty =
        DependencyProperty.Register(nameof(Step), typeof(double), typeof(RangeSeeker),
            new PropertyMetadata(0.0));

    public static readonly DependencyProperty TrackBrushProperty =
        DependencyProperty.Register(nameof(TrackBrush), typeof(Brush), typeof(RangeSeeker),
            new PropertyMetadata(null));

    public static readonly DependencyProperty RangeFillBrushProperty =
        DependencyProperty.Register(nameof(RangeFillBrush), typeof(Brush), typeof(RangeSeeker),
            new PropertyMetadata(null));

    public static readonly DependencyProperty SeekBrushProperty =
        DependencyProperty.Register(nameof(SeekBrush), typeof(Brush), typeof(RangeSeeker),
            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0xFF, 0xD7, 0x00)))); // Gold

    /// <summary>The absolute minimum of the slider scale.</summary>
    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    /// <summary>The absolute maximum of the slider scale.</summary>
    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    /// <summary>The selected lower value (left thumb).</summary>
    public double RangeMin
    {
        get => (double)GetValue(RangeMinProperty);
        set => SetValue(RangeMinProperty, value);
    }

    /// <summary>The selected upper value (right thumb).</summary>
    public double RangeMax
    {
        get => (double)GetValue(RangeMaxProperty);
        set => SetValue(RangeMaxProperty, value);
    }

    /// <summary>The current playhead/seek position.</summary>
    public double SeekPosition
    {
        get => (double)GetValue(SeekPositionProperty);
        set => SetValue(SeekPositionProperty, value);
    }

    /// <summary>
    /// Optional snap step. When > 0, thumb values snap to multiples.
    /// Set to 0 (default) for continuous movement.
    /// </summary>
    public double Step
    {
        get => (double)GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }

    /// <summary>Optional override for the background track brush.</summary>
    public Brush TrackBrush
    {
        get => (Brush)GetValue(TrackBrushProperty);
        set => SetValue(TrackBrushProperty, value);
    }

    /// <summary>Optional override for the filled range brush.</summary>
    public Brush RangeFillBrush
    {
        get => (Brush)GetValue(RangeFillBrushProperty);
        set => SetValue(RangeFillBrushProperty, value);
    }

    /// <summary>Brush for the seek/playhead indicator.</summary>
    public Brush SeekBrush
    {
        get => (Brush)GetValue(SeekBrushProperty);
        set => SetValue(SeekBrushProperty, value);
    }

    #endregion

    #region Routed Events

    public static readonly RoutedEvent RangeChangedEvent =
        EventManager.RegisterRoutedEvent(nameof(RangeChanged), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(RangeSeeker));

    public static readonly RoutedEvent SeekDragStartedEvent =
        EventManager.RegisterRoutedEvent(nameof(SeekDragStarted), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(RangeSeeker));

    public static readonly RoutedEvent SeekDragCompletedEvent =
        EventManager.RegisterRoutedEvent(nameof(SeekDragCompleted), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(RangeSeeker));

    public static readonly RoutedEvent SeekPositionChangedEvent =
        EventManager.RegisterRoutedEvent(nameof(SeekPositionChangedEv), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(RangeSeeker));

    /// <summary>Raised whenever RangeMin or RangeMax changes.</summary>
    public event RoutedEventHandler RangeChanged
    {
        add => AddHandler(RangeChangedEvent, value);
        remove => RemoveHandler(RangeChangedEvent, value);
    }

    /// <summary>Raised when the user starts dragging the seek thumb.</summary>
    public event RoutedEventHandler SeekDragStarted
    {
        add => AddHandler(SeekDragStartedEvent, value);
        remove => RemoveHandler(SeekDragStartedEvent, value);
    }

    /// <summary>Raised when the user finishes dragging the seek thumb.</summary>
    public event RoutedEventHandler SeekDragCompleted
    {
        add => AddHandler(SeekDragCompletedEvent, value);
        remove => RemoveHandler(SeekDragCompletedEvent, value);
    }

    /// <summary>Raised when SeekPosition changes (by drag or programmatically).</summary>
    public event RoutedEventHandler SeekPositionChangedEv
    {
        add => AddHandler(SeekPositionChangedEvent, value);
        remove => RemoveHandler(SeekPositionChangedEvent, value);
    }

    /// <summary>Returns true while the user is dragging the seek thumb.</summary>
    public bool IsSeekDragging { get; private set; }

    #endregion

    #region Template Parts

    private FrameworkElement? _track;
    private FrameworkElement? _rangeFill;
    private Thumb? _lowThumb;
    private Thumb? _highThumb;
    private Thumb? _seekThumb;

    #endregion

    static RangeSeeker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RangeSeeker),
            new FrameworkPropertyMetadata(typeof(RangeSeeker)));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // Unhook previous thumbs
        if (_lowThumb != null)
            _lowThumb.DragDelta -= OnLowThumbDragDelta;
        if (_highThumb != null)
            _highThumb.DragDelta -= OnHighThumbDragDelta;
        if (_seekThumb != null)
        {
            _seekThumb.DragStarted -= OnSeekThumbDragStarted;
            _seekThumb.DragDelta -= OnSeekThumbDragDelta;
            _seekThumb.DragCompleted -= OnSeekThumbDragCompleted;
        }

        _track     = GetTemplateChild("PART_Track")     as FrameworkElement;
        _rangeFill = GetTemplateChild("PART_RangeFill")  as FrameworkElement;
        _lowThumb  = GetTemplateChild("PART_LowThumb")  as Thumb;
        _highThumb = GetTemplateChild("PART_HighThumb")  as Thumb;
        _seekThumb = GetTemplateChild("PART_SeekThumb")  as Thumb;

        if (_lowThumb != null)
            _lowThumb.DragDelta += OnLowThumbDragDelta;
        if (_highThumb != null)
            _highThumb.DragDelta += OnHighThumbDragDelta;
        if (_seekThumb != null)
        {
            _seekThumb.DragStarted += OnSeekThumbDragStarted;
            _seekThumb.DragDelta += OnSeekThumbDragDelta;
            _seekThumb.DragCompleted += OnSeekThumbDragCompleted;
        }

        // Allow click-on-track to move nearest thumb
        if (_track != null)
            _track.MouseLeftButtonDown += OnTrackMouseDown;

        UpdateVisuals();
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        UpdateVisuals();
    }

    #region Drag Handlers

    private void OnLowThumbDragDelta(object sender, DragDeltaEventArgs e)
    {
        if (_track == null) return;

        double trackWidth = _track.ActualWidth;
        if (trackWidth <= 0) return;

        double range = Maximum - Minimum;
        double deltaValue = (e.HorizontalChange / trackWidth) * range;
        double newValue = RangeMin + deltaValue;

        newValue = Snap(newValue);
        newValue = Clamp(newValue, Minimum, RangeMax);

        RangeMin = newValue;
    }

    private void OnHighThumbDragDelta(object sender, DragDeltaEventArgs e)
    {
        if (_track == null) return;

        double trackWidth = _track.ActualWidth;
        if (trackWidth <= 0) return;

        double range = Maximum - Minimum;
        double deltaValue = (e.HorizontalChange / trackWidth) * range;
        double newValue = RangeMax + deltaValue;

        newValue = Snap(newValue);
        newValue = Clamp(newValue, RangeMin, Maximum);

        RangeMax = newValue;
    }

    private void OnSeekThumbDragStarted(object sender, DragStartedEventArgs e)
    {
        IsSeekDragging = true;
        RaiseEvent(new RoutedEventArgs(SeekDragStartedEvent));
    }

    private void OnSeekThumbDragDelta(object sender, DragDeltaEventArgs e)
    {
        if (_track == null) return;

        double trackWidth = _track.ActualWidth;
        if (trackWidth <= 0) return;

        double range = Maximum - Minimum;
        double deltaValue = (e.HorizontalChange / trackWidth) * range;
        double newValue = SeekPosition + deltaValue;

        newValue = Clamp(newValue, Minimum, Maximum);

        SeekPosition = newValue;
    }

    private void OnSeekThumbDragCompleted(object sender, DragCompletedEventArgs e)
    {
        IsSeekDragging = false;
        RaiseEvent(new RoutedEventArgs(SeekDragCompletedEvent));
    }

    private void OnTrackMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (_track == null) return;

        Point pos = e.GetPosition(_track);
        double trackWidth = _track.ActualWidth;
        if (trackWidth <= 0) return;

        double ratio = pos.X / trackWidth;
        double clickValue = Minimum + ratio * (Maximum - Minimum);
        clickValue = Snap(clickValue);

        // Move whichever thumb is closer
        double distToLow  = Math.Abs(clickValue - RangeMin);
        double distToHigh = Math.Abs(clickValue - RangeMax);
        double distToSeek = Math.Abs(clickValue - SeekPosition);

        // Seek thumb gets priority if it's closest
        if (distToSeek <= distToLow && distToSeek <= distToHigh)
        {
            SeekPosition = Clamp(clickValue, Minimum, Maximum);
        }
        else if (distToLow <= distToHigh)
        {
            RangeMin = Clamp(clickValue, Minimum, RangeMax);
        }
        else
        {
            RangeMax = Clamp(clickValue, RangeMin, Maximum);
        }
    }

    #endregion

    #region Layout / Visuals

    private void UpdateVisuals()
    {
        if (_track == null || _rangeFill == null || _lowThumb == null || _highThumb == null)
            return;

        double trackWidth = _track.ActualWidth;
        if (trackWidth <= 0) return;

        double range = Maximum - Minimum;
        if (range <= 0) return;

        double lowRatio  = (RangeMin - Minimum) / range;
        double highRatio = (RangeMax - Minimum) / range;

        double thumbHalf = _lowThumb.ActualWidth > 0 ? _lowThumb.ActualWidth / 2.0 : 9.0;

        // Position low thumb
        double lowLeft = lowRatio * trackWidth - thumbHalf;
        Canvas.SetLeft(_lowThumb, lowLeft);

        // Position high thumb
        double highLeft = highRatio * trackWidth - thumbHalf;
        Canvas.SetLeft(_highThumb, highLeft);

        // Position and size the filled range bar
        double fillLeft  = lowRatio * trackWidth;
        double fillWidth = (highRatio - lowRatio) * trackWidth;

        Canvas.SetLeft(_rangeFill, fillLeft);
        _rangeFill.Width = Math.Max(0, fillWidth);

        // Position seek thumb
        if (_seekThumb != null)
        {
            double seekRatio = (SeekPosition - Minimum) / range;
            double seekHalf = _seekThumb.ActualWidth > 0 ? _seekThumb.ActualWidth / 2.0 : 1.0;
            double seekLeft = seekRatio * trackWidth - seekHalf;
            Canvas.SetLeft(_seekThumb, seekLeft);
        }
    }

    #endregion

    #region Coercion & Callbacks

    private static void OnRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is RangeSeeker slider)
        {
            slider.CoerceValue(RangeMinProperty);
            slider.CoerceValue(RangeMaxProperty);
            slider.CoerceValue(SeekPositionProperty);
            slider.UpdateVisuals();
            slider.RaiseEvent(new RoutedEventArgs(RangeChangedEvent));
        }
    }

    private static void OnSeekPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is RangeSeeker slider)
        {
            slider.UpdateVisuals();
            slider.RaiseEvent(new RoutedEventArgs(SeekPositionChangedEvent));
        }
    }

    private static object CoerceRangeMin(DependencyObject d, object baseValue)
    {
        var slider = (RangeSeeker)d;
        double val = (double)baseValue;
        val = Math.Max(val, slider.Minimum);
        val = Math.Min(val, slider.RangeMax);
        return val;
    }

    private static object CoerceRangeMax(DependencyObject d, object baseValue)
    {
        var slider = (RangeSeeker)d;
        double val = (double)baseValue;
        val = Math.Min(val, slider.Maximum);
        val = Math.Max(val, slider.RangeMin);
        return val;
    }

    private static object CoerceSeekPosition(DependencyObject d, object baseValue)
    {
        var slider = (RangeSeeker)d;
        double val = (double)baseValue;
        val = Math.Max(val, slider.Minimum);
        val = Math.Min(val, slider.Maximum);
        return val;
    }

    private double Snap(double value)
    {
        if (Step > 0)
            return Math.Round(value / Step) * Step;
        return value;
    }

    private static double Clamp(double value, double min, double max)
        => Math.Max(min, Math.Min(max, value));

    #endregion
}
