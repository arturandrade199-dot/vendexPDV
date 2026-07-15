using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Vendex.App.Converters;

/// <summary>Carrega um arquivo de imagem do disco a partir do caminho — usa CacheOption
/// OnLoad pra não travar o arquivo aberto (permite trocar a foto sem reiniciar o app).</summary>
public class PathToImageSourceConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string caminho || string.IsNullOrWhiteSpace(caminho) || !File.Exists(caminho))
            return null;

        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.UriSource = new Uri(caminho, UriKind.Absolute);
        bitmap.EndInit();
        bitmap.Freeze();
        return bitmap;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
