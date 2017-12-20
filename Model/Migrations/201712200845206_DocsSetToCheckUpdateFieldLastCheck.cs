namespace Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DocsSetToCheckUpdateFieldLastCheck : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.DocumentSetToChecks", "ScheduleLastTimeOfCheck", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.DocumentSetToChecks", "ScheduleLastTimeOfCheck", c => c.DateTime(nullable: false));
        }
    }
}
