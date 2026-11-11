using System.Diagnostics.CodeAnalysis;
using System.Net.Mail;
using MPTextil.Domain.Excepciones;

namespace MPTextil.Domain.Comun;

internal static class Guardia
{
    public static void Validar([DoesNotReturnIf(false)] bool condicion, string mensaje)
    {
        if (!condicion)
        {
            throw new ReglaNegocioException(mensaje);
        }
    }

    public static string TextoObligatorio(string? valor, string campo, int longitudMaxima)
    {
        Validar(!string.IsNullOrWhiteSpace(valor), $"El campo {campo} es obligatorio.");

        var texto = valor.Trim();
        Validar(texto.Length <= longitudMaxima, $"El campo {campo} no puede superar {longitudMaxima} caracteres.");

        return texto;
    }

    public static string? TextoOpcional(string? valor, string campo, int longitudMaxima) =>
        string.IsNullOrWhiteSpace(valor) ? null : TextoObligatorio(valor, campo, longitudMaxima);

    public static string Correo(string? valor, string campo, int longitudMaxima)
    {
        var correo = TextoObligatorio(valor, campo, longitudMaxima);
        var esValido = MailAddress.TryCreate(correo, out var direccion) && direccion.Address == correo;
        Validar(esValido, $"El campo {campo} no tiene un formato de correo válido.");

        return correo.ToLowerInvariant();
    }

    public static TEnum Definido<TEnum>(TEnum valor, string campo) where TEnum : struct, Enum
    {
        Validar(Enum.IsDefined(valor), $"El valor del campo {campo} no es válido.");

        return valor;
    }
}
