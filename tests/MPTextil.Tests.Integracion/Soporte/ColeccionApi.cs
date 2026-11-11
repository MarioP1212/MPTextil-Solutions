namespace MPTextil.Tests.Integracion.Soporte;

[CollectionDefinition(Nombre)]
public sealed class ColeccionApi : ICollectionFixture<ApiDePrueba>
{
    public const string Nombre = "API con base temporal";
}
