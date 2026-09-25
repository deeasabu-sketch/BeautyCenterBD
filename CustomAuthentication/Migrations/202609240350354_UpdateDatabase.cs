namespace CustomAuthentication.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateDatabase : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Products", "Description", c => c.String(maxLength: 1500));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Products", "Description", c => c.String(maxLength: 500));
        }
    }
}
