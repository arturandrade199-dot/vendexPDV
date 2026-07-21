namespace Vendex.Application;

/// <summary>URL base das Edge Functions do Supabase, compartilhada por todos os serviços que
/// conversam com o backend de licenciamento/relatórios (ver supabase/README.md).</summary>
internal static class SupabaseFunctions
{
    public const string BaseUrl = "https://debjnxiglpiqrdtiewrw.supabase.co/functions/v1";
}
