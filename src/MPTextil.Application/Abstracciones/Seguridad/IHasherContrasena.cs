namespace MPTextil.Application.Abstracciones.Seguridad;

public interface IHasherContrasena
{
    string Hashear(string contrasena);

    bool Verificar(string contrasena, string hashAlmacenado);
}
