using P3D.Legacy.Server.Domain.Commands;

using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Behaviours.Command;

public delegate Task<CommandResult> CommandHandlerDelegate();