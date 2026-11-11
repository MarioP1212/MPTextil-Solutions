namespace MPTextil.Application.Solicitudes.Acciones;

public static class FirmaImagen
{
    public const int BytesNecesarios = 12;

    private static ReadOnlySpan<byte> Jpeg => new byte[] { 0xFF, 0xD8, 0xFF };
    private static ReadOnlySpan<byte> Png => new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
    private static ReadOnlySpan<byte> Riff => "RIFF"u8;
    private static ReadOnlySpan<byte> Webp => "WEBP"u8;

    public static bool Coincide(ReadOnlySpan<byte> cabecera, string? contentType) => contentType?.ToLowerInvariant() switch
    {
        "image/jpeg" => cabecera.StartsWith(Jpeg),
        "image/png" => cabecera.StartsWith(Png),
        "image/webp" => cabecera.Length >= BytesNecesarios && cabecera.StartsWith(Riff) && cabecera[8..BytesNecesarios].SequenceEqual(Webp),
        _ => false
    };
}
