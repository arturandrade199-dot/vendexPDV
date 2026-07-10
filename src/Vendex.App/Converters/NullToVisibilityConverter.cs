using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Vendex.App.Converters;

/// <summary>Oculta o elemento quando o valor vinculado é nulo ou uma string vazia.</summary>
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture) =>
        value is null || (value is string texto && string.IsNullOrEmpty(texto))
            ? Visibility.Collapsed
            : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
