using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceWatcherWebRole.Models
{
    /// <summary>
    /// Base interface for db entries
    /// </summary>
    public interface IEntry
    {
        Guid Id { get; set; }
        DateTimeOffset CreatedAt { get; set; }
        DateTimeOffset? ModifiedAt { get; set; }
        bool? IsDeleted { get; set; }
    }
}
