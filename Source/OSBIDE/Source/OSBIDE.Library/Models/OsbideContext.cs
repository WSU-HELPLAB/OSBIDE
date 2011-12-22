using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Data.Objects;

namespace OSBIDE.Library.Models
{
    public class OsbideContext : DbContext
    {
        public DbSet<EventLog> EventLogs { get; set; }
        public DbSet<OsbideUser> Users { get; set; }

        public OsbideContext() : base()
        {
        }

        public OsbideContext(DbConnection existingConnection, DbCompiledModel model, bool contextOwnsConnection)
            :base(existingConnection, model, contextOwnsConnection)
        {
        }

        public OsbideContext(string nameOrConnectionString, DbCompiledModel model)
            : base(nameOrConnectionString, model)
        {
        }

        public OsbideContext(ObjectContext objectContext, bool dbContextOwnsObjectContext)
            : base(objectContext, dbContextOwnsObjectContext)
        {
        }

        public OsbideContext(DbConnection existingConnection, bool contextOwnsConnection)
            : base(existingConnection, contextOwnsConnection)
        {
        }

        public OsbideContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        public OsbideContext(DbCompiledModel model)
            : base(model)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
#if !DEBUG
            modelBuilder.Conventions.Remove<IncludeMetadataConvention>();
#endif
        }
    }
}
