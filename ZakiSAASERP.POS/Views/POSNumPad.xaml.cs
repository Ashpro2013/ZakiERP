using System;
using System.Windows;
using System.Windows.Controls;

namespace ZakiSAASERP.POS.Views;

public partial class POSNumPad : UserControl
{
    public static readonly RoutedEvent KeyClickEvent = EventManager.RegisterRoutedEvent(
        nameof(KeyClick), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(POSNumPad));

    public static readonly DependencyProperty ButtonHeightProperty =
        DependencyProperty.Register(
            nameof(ButtonHeight),
            typeof(double),
            typeof(POSNumPad),
            new FrameworkPropertyMetadata(42.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, null, CoerceButtonHeight));

    public static readonly DependencyProperty ButtonFontSizeProperty =
        DependencyProperty.Register(
            nameof(ButtonFontSize),
            typeof(double),
            typeof(POSNumPad),
            new FrameworkPropertyMetadata(15.0, FrameworkPropertyMetadataOptions.AffectsRender, OnButtonFontSizeChanged, CoerceButtonFontSize));

    public static readonly DependencyProperty ActionFontSizeProperty =
        DependencyProperty.Register(
            nameof(ActionFontSize),
            typeof(double),
            typeof(POSNumPad),
            new FrameworkPropertyMetadata(13.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public event RoutedEventHandler KeyClick
    {
        add => AddHandler(KeyClickEvent, value);
        remove => RemoveHandler(KeyClickEvent, value);
    }

    public double ButtonHeight
    {
        get => (double)GetValue(ButtonHeightProperty);
        set => SetValue(ButtonHeightProperty, value);
    }

    public double ButtonFontSize
    {
        get => (double)GetValue(ButtonFontSizeProperty);
        set => SetValue(ButtonFontSizeProperty, value);
    }

    public double ActionFontSize
    {
        get => (double)GetValue(ActionFontSizeProperty);
        set => SetValue(ActionFontSizeProperty, value);
    }

    private static void OnButtonFontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is POSNumPad pad && e.NewValue is double fs)
        {
            pad.ActionFontSize = Math.Max(10.0, fs - 2.5);
        }
    }

    private static object CoerceButtonHeight(DependencyObject d, object baseValue)
    {
        if (baseValue is double dVal) return dVal <= 0 ? 42.0 : dVal;
        if (baseValue is int iVal) return iVal <= 0 ? 42.0 : (double)iVal;
        return 42.0;
    }

    private static object CoerceButtonFontSize(DependencyObject d, object baseValue)
    {
        if (baseValue is double dVal) return dVal <= 0 ? 15.0 : dVal;
        if (baseValue is int iVal) return iVal <= 0 ? 15.0 : (double)iVal;
        return 15.0;
    }

    public POSNumPad()
    {
        InitializeComponent();
    }

    private void OnKeyClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string key)
        {
            var args = new RoutedEventArgs(KeyClickEvent, key);
            RaiseEvent(args);
        }
    }
}
