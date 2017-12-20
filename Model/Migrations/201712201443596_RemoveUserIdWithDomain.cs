namespace Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveUserIdWithDomain : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Users", new[] { "IdWithDomain" });
            DropColumn("dbo.Users", "IdWithDomain");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Users", "IdWithDomain", c => c.Int(nullable: false));
            CreateIndex("dbo.Users", "IdWithDomain", unique: true);
        }
    }
}
