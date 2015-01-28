using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ServiceWatcherWebRole.Models
{
    /// <summary>
    /// Heartbeat entries for db
    /// </summary>
    public class Heartbeat : IEntry
    {
        [Key]
        public Guid Id { get; set; }
        [Required(ErrorMessage = "CustomerId is required")]
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        [Required(ErrorMessage = "InstanceId is required")]
        public Guid InstanceId { get; set; }
        /// <summary>
        /// 2 types (MasterProcessProc, WebShop)
        /// </summary>
        [Required(ErrorMessage="StreamName is required")]
        public string StreamName { get; set; }
        public string AppName { get; set; }
        public string AppVersion { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? ModifiedAt { get; set; }
        public bool? IsDeleted { get; set; }
    }
}