namespace ApiPlatform.PortalApi.Domain.Entities;

public sealed class Api
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }

    private Api() { }

    public Api(string name)
    {
        Id = Guid.CreateVersion7();
        Name = name;
    }
}
