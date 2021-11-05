using System.Collections.Generic;
using System.Linq;

namespace P3D.Legacy.Server.UI.Shared.Models
{
    public record PagingResponse<T> where T : class
    {
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
        public PagingMetadata Metadata { get; set; } = new();
    }
}