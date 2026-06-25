namespace ApiPlatform.PortalApi.Domain.Entities;

public sealed class Scope
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }

    private Scope() { }

    public Scope(string name)
    {
        Id = Guid.CreateVersion7();
        Name = name;
    }
}
