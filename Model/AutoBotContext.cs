using System.Diagnostics;
using System.Threading.Tasks;
using Model.Entities;
using Model.Entities.Fines;

namespace Model
{
    using System;
    using System.Data.Entity;
    using System.Linq;

    public class AutoBotContext : DbContext
    {
        // Your context has been configured to use a 'AutoBotContext' connection string from your application's 
        // configuration file (App.config or Web.config). By default, this connection string targets the 
        // 'Model.AutoBotContext' database on your LocalDb instance. 
        // 
        // If you wish to target a different database and/or database provider, modify the 'AutoBotContext' 
        // connection string in the application configuration file.
        public AutoBotContext()
            : base("name=AutoBotContext")
        {
            
        }

        // Add a DbSet for each entity type that you want to include in your model. For more information 
        // on configuring and using a Code First model, see http://go.microsoft.com/fwlink/?LinkId=390109.

        // public virtual DbSet<MyEntity> MyEntities { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<DocumentSetToCheck> DocumentSetsTocheck { get; set; }

        public virtual DbSet<FinesLog> FinesLogs { get; set; }

        public override int SaveChanges()
        {
            try
            {
                return base.SaveChanges();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return 0;
            }
          
        }

        public override Task<int> SaveChangesAsync()
        {
            try
            {
                return base.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
            
        }
    }

    //public class MyEntity
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //}
}