namespace Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addCountries : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "MainConversationReferenceSerialized", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "MainConversationReferenceSerialized");
        }
    }
}
