using System.Collections.Generic;
using System.Linq;

namespace P3D.Legacy.Server.Infrastructure.Models;

public class AccountResult
{
    public static AccountResult Success { get; } = new() { Succeeded = true };
    public static AccountResult Failed(params GenericError[] errors)
    {
        var result = new AccountResult { Succeeded = false };
        result._errors.AddRange(errors);
        return result;
    }

    private readonly List<GenericError> _errors = new();

    public bool Succeeded { get; protected set; }
    public IEnumerable<GenericError> Errors => _errors;

    public override string ToString() => Succeeded
        ? "Succeeded"
        : $"Failed : {string.Join(",", Errors.Select(static x => x.Code).ToList())}";
}