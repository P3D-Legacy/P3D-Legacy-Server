using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Behaviours.Query
{
    public delegate Task<TQueryResult> QueryHandlerDelegate<TQueryResult>();
}