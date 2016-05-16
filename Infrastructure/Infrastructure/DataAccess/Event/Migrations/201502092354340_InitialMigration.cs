using System.Data.Entity.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Event.Migrations
{
    public partial class InitialMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "event.Acknowledgements",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        State = c.Int(nullable: false),
                        Created = c.DateTimeOffset(nullable: false, precision: 7),
                        Updated = c.DateTimeOffset(precision: 7),
                        Event_Id = c.Guid(),
                        Worker_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("event.Events", t => t.Event_Id)
                .ForeignKey("event.Workers", t => t.Worker_Id)
                .Index(t => new { t.Event_Id, t.Worker_Id }, unique: true, name: "IX_EventWorker");

            
            CreateTable(
                "event.Events",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        DataType = c.String(),
                        Data = c.String(),
                        State = c.Int(nullable: false),
                        Created = c.DateTimeOffset(nullable: false, precision: 7),
                        ReadyToPublish = c.DateTimeOffset(nullable: false, precision: 7),
                        Published = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id, clustered: false)
                .Index(t => t.Created, clustered: true);
            
            CreateTable(
                "event.Workers",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        TypeName = c.String(maxLength: 200),
                        State = c.Int(nullable: false),
                        LastReplayedEvent_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("event.Events", t => t.LastReplayedEvent_Id)
                .Index(t => t.TypeName, unique: true)
                .Index(t => t.LastReplayedEvent_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("event.Acknowledgements", "Worker_Id", "event.Workers");
            DropForeignKey("event.Workers", "LastReplayedEvent_Id", "event.Events");
            DropForeignKey("event.Acknowledgements", "Event_Id", "event.Events");
            DropIndex("event.Workers", new[] { "LastReplayedEvent_Id" });
            DropIndex("event.Workers", new[] { "TypeName" });
            DropIndex("event.Events", new[] { "Created" });
            DropIndex("event.Acknowledgements", new[] { "Worker_Id" });
            DropIndex("event.Acknowledgements", new[] { "Event_Id" });
            DropTable("event.Workers");
            DropTable("event.Events");
            DropTable("event.Acknowledgements");
        }
    }
}
