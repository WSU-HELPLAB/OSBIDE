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
        public DbSet<SubmitEvent> SubmitEvents { get; set; }
        /*
        public DbSet<BuildEvent> BuildEvents { get; set; }
        public DbSet<DebugEvent> DebugEvents { get; set; }
        public DbSet<EditorActivityEvent> EditorActivityEvents { get; set; }
        public DbSet<ExceptionEvent> ExceptionEvents { get; set; }
        public DbSet<SaveEvent> SaveEvents { get; set; }
        */

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

        protected override bool ShouldValidateEntity(DbEntityEntry entityEntry)
        {
            // Required to prevent bug - http://stackoverflow.com/questions/5737733
            if (entityEntry.Entity is SubmitEvent)
            {
                return false;
            }
            if (entityEntry.Entity is EventLog)
            {
                return false;
            }
            return base.ShouldValidateEntity(entityEntry);
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

        /// <summary>
        /// Inserts a user that has a preexisting ID into the database context.  Most likely to be used
        /// when inserting a user that already exists in another context.
        /// </summary>
        /// <param name="user">The user to insert</param>
        /// <returns>TRUE if everything went okay
        ///          FALSE if: User ID is 0
        ///                    User already exists
        ///                    Random query error
        /// </returns>
        public bool InsertUserWithId(OsbideUser user)
        {
            //ignore users with an empty ID
            if (user.Id == 0)
            {
                return false;
            }

            //check to see if we already have a user with that ID
            OsbideUser dbUser = Users.Find(user.Id);
            if (dbUser != null)
            {
                return false;
            }

            //finally, we can do a raw insert
            SqlCeConnection conn = new SqlCeConnection(this.Database.Connection.ConnectionString);

            try
            {
                conn.Open();
            }
            catch (Exception ex)
            {
                return false;
            }

            string query = "SET IDENTITY_INSERT OsbideUsers ON";
            SqlCeCommand cmd = new SqlCeCommand(query, conn);

            try
            {
                object result = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                return false;
            }

            query = "INSERT INTO OsbideUsers " +
                  "(Id, FirstName, LastName, InstitutionId, OsbleId) " +
                  "VALUES (@id, @first, @last, @institutionId, @osbleId) ";
            cmd = new SqlCeCommand(query, conn);
            cmd.Parameters.Add(new SqlCeParameter()
            {
                ParameterName = "id",
                Value = user.Id,
                SqlDbType = System.Data.SqlDbType.Int
            });
            cmd.Parameters.Add(new SqlCeParameter()
            {
                ParameterName = "first",
                Value = user.FirstName,
                SqlDbType = System.Data.SqlDbType.NVarChar
            });

            cmd.Parameters.Add(new SqlCeParameter()
            {
                ParameterName = "last",
                Value = user.LastName,
                SqlDbType = System.Data.SqlDbType.NVarChar
            });

            cmd.Parameters.Add(new SqlCeParameter()
            {
                ParameterName = "institutionId",
                Value = user.InstitutionId,
                SqlDbType = System.Data.SqlDbType.NVarChar
            });

            cmd.Parameters.Add(new SqlCeParameter()
            {
                ParameterName = "osbleId",
                Value = user.OsbleId,
                SqlDbType = System.Data.SqlDbType.Int
            });

            try
            {
                cmd.ExecuteNonQuery();

                //turn off identity inserts
                query = "SET IDENTITY_INSERT OsbideUsers OFF";
                cmd = new SqlCeCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Inserts an EventLog that has a preexisting ID into the database context.  Most likely to be used
        /// when inserting a log that already exists in another context.
        /// </summary>
        /// <param name="log">The EventLog to insert</param>
        /// <returns>TRUE if everything went okay
        ///          FALSE if: Log ID is 0
        ///                    Log already exists
        ///                    Random query error
        /// </returns>
        public bool InsertEventLogWithId(EventLog log)
        {
            //log must have a valid id
            if (log.Id == 0)
            {
                return false;
            }

            //log must not already exist
            EventLog dbLog = this.EventLogs.Find(log.Id);
            if (dbLog != null)
            {
                return false;
            }

            //finally, we can do a raw insert
            SqlCeConnection conn = new SqlCeConnection(this.Database.Connection.ConnectionString);

            try
            {
                conn.Open();
            }
            catch (Exception ex)
            {
                return false;
            }

            string query = "SET IDENTITY_INSERT EventLogs ON";
            SqlCeCommand cmd = new SqlCeCommand(query, conn);

            try
            {
                object result = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                return false;
            }

            query = "INSERT INTO EventLogs " +
                  "(Id, SenderId, LogType, DateReceived, Data) " +
                  "VALUES (@id, @senderId, @logType, @dateReceived, @data) ";
            cmd = new SqlCeCommand(query, conn);
            cmd.Parameters.Add(new SqlCeParameter()
            {
                ParameterName = "id",
                Value = log.Id,
                SqlDbType = System.Data.SqlDbType.Int
            });

            cmd.Parameters.Add(new SqlCeParameter()
            {
                ParameterName = "senderId",
                Value = log.SenderId,
                SqlDbType = System.Data.SqlDbType.Int
            });

            cmd.Parameters.Add(new SqlCeParameter()
            {
                ParameterName = "logType",
                Value = log.LogType,
                SqlDbType = System.Data.SqlDbType.NVarChar
            });

            cmd.Parameters.Add(new SqlCeParameter()
            {
                ParameterName = "dateReceived",
                Value = log.DateReceived,
                SqlDbType = System.Data.SqlDbType.DateTime
            });

            cmd.Parameters.Add(new SqlCeParameter()
            {
                ParameterName = "data",
                Value = log.Data,
                SqlDbType = System.Data.SqlDbType.Image
            });

            try
            {
                cmd.ExecuteNonQuery();

                //turn off identity inserts
                query = "SET IDENTITY_INSERT EventLogs OFF";
                cmd = new SqlCeCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

    }
}
