using MPTextil.Domain.Enumeraciones;

namespace MPTextil.Domain.Flujo;

public sealed record Actor(int UsuarioId, Rol Rol, int? ProveedorId);
