using System.Windows;
using System.Windows.Controls;

namespace Vendex.App.Behaviors;

public enum TipoMascara
{
    Telefone,
    Cpf,
    CpfCnpj
}

/// <summary>Attached property que formata Telefone/CPF/CNPJ enquanto o usuário digita.
/// Sempre reposiciona o cursor no fim — aceitável aqui porque a digitação nesses campos
/// é sempre da esquerda pra direita, sem edição no meio do texto.</summary>
public static class Mascara
{
    public static readonly DependencyProperty TipoProperty =
        DependencyProperty.RegisterAttached("Tipo", typeof(TipoMascara?), typeof(Mascara),
            new PropertyMetadata(null, OnTipoChanged));

    public static void SetTipo(DependencyObject elemento, TipoMascara? valor) => elemento.SetValue(TipoProperty, valor);
    public static TipoMascara? GetTipo(DependencyObject elemento) => (TipoMascara?)elemento.GetValue(TipoProperty);

    private static void OnTipoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBox textBox)
            return;

        textBox.TextChanged -= TextBox_TextChanged;
        if (e.NewValue is TipoMascara)
            textBox.TextChanged += TextBox_TextChanged;
    }

    private static void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var textBox = (TextBox)sender;
        var tipo = GetTipo(textBox);
        if (tipo is null)
            return;

        var digitos = new string(textBox.Text.Where(char.IsDigit).ToArray());
        var maxDigitos = tipo == TipoMascara.Telefone ? 11 : tipo == TipoMascara.Cpf ? 11 : 14;
        if (digitos.Length > maxDigitos)
            digitos = digitos[..maxDigitos];

        var formatado = tipo switch
        {
            TipoMascara.Telefone => FormatarTelefone(digitos),
            TipoMascara.Cpf => FormatarCpf(digitos),
            TipoMascara.CpfCnpj => digitos.Length > 11 ? FormatarCnpj(digitos) : FormatarCpf(digitos),
            _ => digitos
        };

        if (textBox.Text == formatado)
            return;

        textBox.TextChanged -= TextBox_TextChanged;
        textBox.Text = formatado;
        textBox.CaretIndex = formatado.Length;
        textBox.TextChanged += TextBox_TextChanged;
    }

    private static string FormatarTelefone(string d)
    {
        if (d.Length == 0) return d;
        if (d.Length <= 2) return $"({d}";
        if (d.Length <= 6) return $"({d[..2]}) {d[2..]}";
        // Fixo: (XX) XXXX-XXXX — Celular: (XX) XXXXX-XXXX
        var tamanhoPrefixo = d.Length <= 10 ? 4 : 5;
        var fimPrefixo = Math.Min(2 + tamanhoPrefixo, d.Length);
        if (d.Length <= fimPrefixo) return $"({d[..2]}) {d[2..]}";
        return $"({d[..2]}) {d[2..fimPrefixo]}-{d[fimPrefixo..]}";
    }

    private static string FormatarCpf(string d)
    {
        if (d.Length <= 3) return d;
        if (d.Length <= 6) return $"{d[..3]}.{d[3..]}";
        if (d.Length <= 9) return $"{d[..3]}.{d[3..6]}.{d[6..]}";
        return $"{d[..3]}.{d[3..6]}.{d[6..9]}-{d[9..]}";
    }

    private static string FormatarCnpj(string d)
    {
        if (d.Length <= 2) return d;
        if (d.Length <= 5) return $"{d[..2]}.{d[2..]}";
        if (d.Length <= 8) return $"{d[..2]}.{d[2..5]}.{d[5..]}";
        if (d.Length <= 12) return $"{d[..2]}.{d[2..5]}.{d[5..8]}/{d[8..]}";
        return $"{d[..2]}.{d[2..5]}.{d[5..8]}/{d[8..12]}-{d[12..]}";
    }
}
