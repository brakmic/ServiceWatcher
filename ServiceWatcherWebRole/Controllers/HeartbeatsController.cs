using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using System.Web.OData;
using ServiceWatcherWebRole.Models;
using System.Diagnostics;
using ServiceWatcherWebRole.Events;
using ServiceWatcherWebRole.Helpers;

namespace ServiceWatcherWebRole.Controllers
{

    public class HeartbeatsController : BaseODataController
    {
        /// <summary>
        /// on each heartbeat POST call the Rx Helper method will be called
        /// this in turn updates the internal Rx Observables
        /// </summary>
        private HeartBeatDelegate heartbeatDelegate = new HeartBeatDelegate(RxStreamHelper.OnHeartBeat);

        public HeartbeatsController(SWContext ctx)
                : base(ctx)
        {
            
        }

        // GET: odata/Heartbeats
        [EnableQuery]
        public IQueryable<Heartbeat> GetHeartbeats()
        {
            return db.Heartbeats;
        }

        // GET: odata/Heartbeats(5)
        [EnableQuery]
        public SingleResult<Heartbeat> GetHeartbeat([FromODataUri] Guid key)
        {
            return SingleResult.Create(db.Heartbeats.Where(heartbeat => heartbeat.Id == key));
        }

        // POST: odata/Heartbeats
        public async Task<IHttpActionResult> Post(Heartbeat heartbeat)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // each heartbeat must be unique
            if (heartbeat.Id == Guid.Empty) heartbeat.Id = Guid.NewGuid();
            //generate event args for delegate
            HeartbeatEventArgs e = RxStreamHelper.GetHeartbeatEventArgs(heartbeat);
            //propagate the heartbeat
            heartbeatDelegate(this, e);
            //insert into db
            db.Heartbeats.Add(heartbeat);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (HeartbeatExists(heartbeat.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return Created(heartbeat);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool HeartbeatExists(Guid key)
        {
            return db.Heartbeats.Count(e => e.Id == key) > 0;
        }
    }
}
