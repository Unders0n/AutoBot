namespace Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addFieldsToUser : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "PayName", c => c.String());
            AddColumn("dbo.Users", "PaySurname", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "PaySurname");
            DropColumn("dbo.Users", "PayName");
        }
    }
}
