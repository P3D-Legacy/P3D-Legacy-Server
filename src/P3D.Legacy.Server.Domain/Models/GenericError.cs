namespace P3D.Legacy.Server.Domain.Models;

public record GenericError
{
    public string Code { get; init; } = default!;
    public string Description { get; init; } = default!;

    public GenericError() { }
    public GenericError(string code, string description)
    {
        Code = code;
        Description = description;
    }

    public void Deconstruct(out string code, out string description)
    {
        code = Code;
        description = Description;
    }
}