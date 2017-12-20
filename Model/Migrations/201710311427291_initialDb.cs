namespace Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initialDb : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DocumentSetToChecks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        ScheduleCheck = c.Boolean(nullable: false),
                        ScheduleLastTimeOfCheck = c.DateTime(nullable: false),
                        Sts = c.String(),
                        Vu = c.String(),
                        User_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.User_Id)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IdWithDomain = c.Int(nullable: false),
                        RegistrationDate = c.DateTime(nullable: false),
                        UserIdTelegramm = c.String(),
                        UserIdSkype = c.String(),
                        UserIdFacebook = c.String(),
                        FirstName = c.String(),
                        LastName = c.String(),
                        Surname = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.IdWithDomain, unique: true);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DocumentSetToChecks", "User_Id", "dbo.Users");
            DropIndex("dbo.Users", new[] { "IdWithDomain" });
            DropIndex("dbo.DocumentSetToChecks", new[] { "User_Id" });
            DropTable("dbo.Users");
            DropTable("dbo.DocumentSetToChecks");
        }
    }
}
