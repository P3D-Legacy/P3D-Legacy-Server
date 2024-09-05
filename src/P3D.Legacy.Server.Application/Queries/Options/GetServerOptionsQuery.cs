using P3D.Legacy.Server.Abstractions.Options;
using P3D.Legacy.Server.CQERS.Queries;

namespace P3D.Legacy.Server.Application.Queries.Options;

public sealed record GetServerOptionsQuery : IQuery<ServerOptions>;