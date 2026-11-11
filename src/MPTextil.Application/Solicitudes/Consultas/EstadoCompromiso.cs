namespace MPTextil.Application.Solicitudes.Consultas;

public enum EstadoCompromiso
{
    Vigente = 1,
    Vencido = 2,
    EntregadoATiempo = 3,
    EntregadoTarde = 4,
    Reemplazado = 5,
    ReemplazadoVencido = 6
}

public static class DescripcionEstadoCompromiso
{
    public static string Descripcion(this EstadoCompromiso estado) => estado switch
    {
        EstadoCompromiso.Vigente => "Vigente",
        EstadoCompromiso.Vencido => "Vencido sin envío",
        EstadoCompromiso.EntregadoATiempo => "Entregado a tiempo",
        EstadoCompromiso.EntregadoTarde => "Entregado tarde",
        EstadoCompromiso.Reemplazado => "Reemplazado por una nueva fecha",
        EstadoCompromiso.ReemplazadoVencido => "Reemplazado después de vencer",
        _ => estado.ToString()
    };
}
