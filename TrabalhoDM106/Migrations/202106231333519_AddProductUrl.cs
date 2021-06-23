namespace TrabalhoDM106.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddProductUrl : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Products",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        nome = c.String(nullable: false),
                        descricao = c.String(),
                        codigo = c.String(nullable: false),
                        modelo = c.String(nullable: false),
                        preco = c.Decimal(nullable: false, precision: 18, scale: 2),
                        cor = c.String(),
                        peso = c.Single(nullable: false),
                        altura = c.Single(nullable: false),
                        largura = c.Single(nullable: false),
                        comprimento = c.Single(nullable: false),
                        diametro = c.Single(nullable: false),
                        url = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Products");
        }
    }
}
