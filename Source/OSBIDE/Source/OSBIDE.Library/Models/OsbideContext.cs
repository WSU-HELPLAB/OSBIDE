using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Data.Objects;
using System.Data.SqlServerCe;
using System.Diagnostics.CodeAnalysis;
using OSBIDE.Library.Events;

namespace OSBIDE.Library.Models
{
    public class OsbideContext : DbContext
    {
        public DbSet<EventLog> EventLogs { get; set; }
        public DbSet<OsbideUser> Users { get; set; }
        public DbSet<BuildEvent> BuildEvents { get; set; }
        public DbSet<DebugEvent> DebugEvents { get; set; }
        public DbSet<EditorActivityEvent> EditorActivityEvents { get; set; }
        public DbSet<ExceptionEvent> ExceptionEvents { get; set; }
        public DbSet<SaveEvent> SaveEvents { get; set; }

        public OsbideContext()
            : base()
        {
        }

        public OsbideContext(DbConnection existingConnection, DbCompiledModel model, bool contextOwnsConnection)
            : base(existingConnection, model, contextOwnsConnection)
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

        /// <summary>
        /// Returns true if the client has SQL Server CE installed.
        /// </summary>
        /// <returns></returns>
        public static bool HasSqlServerCE
        {
            get
            {
                bool success = true;
                try
                {
                    SqlCeConnection conn = new SqlCeConnection(StringConstants.LocalDataConnectionString);
                    OsbideContext localDb = new OsbideContext(conn, true);
                    conn.Close();
                }
                catch (Exception ex)
                {
                    success = false;
                }
                return success;
            }
        }
    }
}
