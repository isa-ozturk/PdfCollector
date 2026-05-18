using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using PdfCollector.Core.Models;

namespace PdfCollector.Presentation.Converters;

// Avoid naming collision with System.Windows.Controls.BooleanToVisibilityConverter
public class AppBoolToVisConverter : IValueConverter
{
    public bool Invert { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var b = value is bool and true;
        if (Invert) b = !b;
        return b ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var vis = value is Visibility v && v == Visibility.Visible;
        return Invert ? !vis : vis;
    }
}

public class AppLogLevelToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not LogLevel level) return Brushes.Gray;
        switch (level)
        {
            case LogLevel.Success:
                return new SolidColorBrush(Color.FromRgb(0x10, 0x7C, 0x10)); // Windows green
            case LogLevel.Warning:
                return new SolidColorBrush(Color.FromRgb(0xCA, 0x50, 0x10)); // Windows orange
            case LogLevel.Error:
                return new SolidColorBrush(Color.FromRgb(0xC4, 0x2B, 0x1C)); // Windows red
            case LogLevel.Info:
            default:
                return new SolidColorBrush(Color.FromRgb(0x61, 0x61, 0x61)); // gray-600
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class AppLogLevelToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not LogLevel level) return "·";
        switch (level)
        {
            case LogLevel.Success: return "✔";
            case LogLevel.Warning: return "⚠";
            case LogLevel.Error: return "✖";
            case LogLevel.Info:
            default: return "·";
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class AppIntToVisConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var i = value is int iv ? iv : 0;
        return i > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class AppInverseBoolToVisConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b && b ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is Visibility v && v == Visibility.Collapsed;
}