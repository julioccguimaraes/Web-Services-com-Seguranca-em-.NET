namespace TrabalhoDM106.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OrderSetNullDataEntrega : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Orders", "DataEntrega", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Orders", "DataEntrega", c => c.DateTime(nullable: false));
        }
    }
}
