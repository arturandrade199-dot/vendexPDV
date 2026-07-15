namespace Vendex.Application.Services;

public record PermissaoModulo(int ModuloId, string NomeModulo, bool PodeVisualizar, bool PodeCriar, bool PodeEditar, bool PodeExcluir);
