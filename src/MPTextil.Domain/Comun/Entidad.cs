namespace MPTextil.Domain.Comun;

public abstract class Entidad<TId> where TId : struct
{
    public TId Id { get; protected set; }
}

public abstract class Entidad : Entidad<int>
{
}
