using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Vendex.App.Converters;

/// <summary>Visible somente quando todos os bools de entrada são true — usado para combinar
/// um estado de domínio (ex: PodeMarcarComoPago) com uma permissão de sessão (ex: PodeEditar).</summary>
public class AllTrueToVisibilityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) =>
        values.All(v => v is true) ? Visibility.Visible : Visibility.Collapsed;

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
