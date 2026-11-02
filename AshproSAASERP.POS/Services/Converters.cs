using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AshproSAASERP.Models.Masters;

namespace AshproSAASERP.POS.Services;

public class BoolToVisibilityConverter : IValueConverter
{
    public bool Invert { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool b = value is bool flag && flag;
        bool invert = Invert || (parameter is string p && p.Equals("invert", StringComparison.OrdinalIgnoreCase));
        if (invert) b = !b;
        return b ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is Visibility vis && vis == Visibility.Visible;
    }
}

public class BoolToBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isValid = value is bool b && b;
        return isValid 
            ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(220, 252, 231)) // #DCFCE7
            : new SolidColorBrush(System.Windows.Media.Color.FromRgb(254, 226, 226)); // #FEE2E2
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class ProviderToBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string? current = value?.ToString();
        string? target = parameter?.ToString();
        bool isMatch = string.Equals(current, target, StringComparison.OrdinalIgnoreCase);

        return isMatch
            ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(124, 58, 237)) // #7c3aed (Active Purple)
            : new SolidColorBrush(System.Windows.Media.Color.FromRgb(26, 22, 56));  // #1A1638 (Inactive)
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class BoolToChangeTitleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isValid = value is bool b && b;
        return isValid ? "CHANGE TO RETURN" : "SHORTAGE (INSUFFICIENT)";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class BoolToChangeColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isValid = value is bool b && b;
        return isValid 
            ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(21, 128, 61)) // #15803D (green)
            : new SolidColorBrush(System.Windows.Media.Color.FromRgb(185, 28, 28)); // #B91C1C (red)
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class BoolToChangeHeaderColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isValid = value is bool b && b;
        return isValid 
            ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(22, 101, 52)) // #166534
            : new SolidColorBrush(System.Windows.Media.Color.FromRgb(153, 27, 27)); // #991B1B
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class ZeroToVisibilityConverter : IValueConverter
{
    public bool Invert { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        int count = value switch
        {
            int i => i,
            long l => (int)l,
            decimal d => (int)d,
            _ => 0
        };

        bool isZero = count == 0;
        bool visible = Invert ? !isZero : isZero;
        return visible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class BindingProxy : Freezable
{
    protected override Freezable CreateInstanceCore() => new BindingProxy();

    public object Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register(nameof(Data), typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));
}

public class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b && !b;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b && !b;
    }
}

public class ByteArrayToImageConverter : IValueConverter
{
    private static readonly Dictionary<int, BitmapImage> Cache = new();

    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is byte[] bytes && bytes.Length > 0)
        {
            try
            {
                int hash = ComputeHash(bytes);
                if (Cache.TryGetValue(hash, out var cached))
                {
                    return cached;
                }

                using var stream = new MemoryStream(bytes);
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
                image.Freeze();

                if (Cache.Count < 500)
                {
                    Cache[hash] = image;
                }
                return image;
            }
            catch
            {
                return null;
            }
        }
        return null;
    }

    private static int ComputeHash(byte[] bytes)
    {
        unchecked
        {
            int hash = 17;
            int step = Math.Max(1, bytes.Length / 16);
            for (int i = 0; i < bytes.Length; i += step)
            {
                hash = hash * 31 + bytes[i];
            }
            return hash;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class NullToVisibilityConverter : IValueConverter
{
    public bool Invert { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isNull = value == null || (value is byte[] b && b.Length == 0) || (value is string s && string.IsNullOrEmpty(s));
        bool invert = Invert || (parameter is string p && p.Equals("invert", StringComparison.OrdinalIgnoreCase));
        bool visible = invert ? isNull : !isNull;
        return visible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class CategoryMatchConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length >= 2 && values[0] is CategoryMaster cat && values[1] is CategoryMaster sel)
        {
            return cat.CategoryId == sel.CategoryId;
        }
        return false;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
