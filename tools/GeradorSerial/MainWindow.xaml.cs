using System.Windows;
using Vendex.Licensing;

namespace GeradorSerial;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void GerarSerial_Click(object sender, RoutedEventArgs e)
    {
        var codigoInstalacao = TxtCodigoInstalacao.Text.Trim();
        if (string.IsNullOrWhiteSpace(codigoInstalacao))
        {
            MessageBox.Show("Informe o código de instalação recebido do cliente.", "Vendex", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        TxtSerialGerado.Text = SerialAlgorithm.GerarSerial(codigoInstalacao);
    }
}
