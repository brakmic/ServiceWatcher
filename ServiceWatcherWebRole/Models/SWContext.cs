using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace ServiceWatcherWebRole.Models
{
    /// <summary>
    /// Code first db context
    /// </summary>
    public class SWContext : DbContext
    {
   
        public SWContext() : base("name=SWContext")
        {
            //auto init db if model has changed
            Database.SetInitializer(new SWContextInitializer());
        }
        /// <summary>
        /// Custom model creation
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //entities marked as "deleted" will not show up in JSON results
            modelBuilder.Entity<Heartbeat>()
                .Map(m => m.Requires("IsDeleted").HasValue(false))
                .Ignore(m => m.IsDeleted);
            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// override standard SaveChange
        /// </summary>
        /// <returns></returns>
        public override int SaveChanges()
        {
            TrackChanges();
            return base.SaveChanges();
        }
        /// <summary>
        /// Override standard async SaveChange
        /// </summary>
        /// <returns></returns>
        public async override Task<int> SaveChangesAsync()
        {
            return await base.SaveChangesAsync();
        }

        public async override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            TrackChanges();
            return await base.SaveChangesAsync(cancellationToken);
        }
        /// <summary>
        /// Every entry must have a proper ID and no deletion of entries is allowed
        /// Instead of physically deleting an entry the same will be set to "deleted"
        /// </summary>
        private void TrackChanges()
        {
            var Changed = ChangeTracker.Entries();
            if (Changed != null)
            {
                foreach (var entry in Changed.Where(e => e.State != EntityState.Unchanged))
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            {
                                if (entry.Entity is IEntry)
                                {
                                    IEntry entity = GetIEntry(entry);
                                    entity.CreatedAt = DateTime.UtcNow;
                                    if (entity.Id == Guid.Empty)
                                    {
                                        entity.Id = Guid.NewGuid();
                                    }
                                }
                            }
                            break;
                        case EntityState.Deleted:
                            {
                                entry.State = EntityState.Modified;
                                if (entry.Entity is IEntry)
                                {
                                    IEntry entity = GetIEntry(entry);
                                    entity.IsDeleted = true;
                                    entity.ModifiedAt = DateTime.UtcNow;
                                }
                            }
                            break;
                        case EntityState.Modified:
                            {
                                if (entry.Entity is IEntry)
                                {
                                    IEntry entity = GetIEntry(entry);
                                    entity.ModifiedAt = DateTime.UtcNow;
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private IEntry GetIEntry(DbEntityEntry entry)
        {
            return entry.Entity as IEntry;
        }

        public DbSet<Heartbeat> Heartbeats { get; set; }
    
    }
}
