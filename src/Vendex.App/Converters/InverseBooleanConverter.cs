using System.Globalization;
using System.Windows.Data;

namespace Vendex.App.Converters;

/// <summary>Inverte um bool — usado, por exemplo, pra desabilitar um campo enquanto o modo
/// alternativo (outro RadioButton/toggle) está ativo.</summary>
public class InverseBooleanConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture) =>
        value is true ? false : true;

    public object ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture) =>
        value is true ? false : true;
}
