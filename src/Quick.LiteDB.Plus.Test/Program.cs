using LiteDB;
using Quick.LiteDB.Plus;
using Quick.LiteDB.Plus.Test;

//测试ConfigDbContext
ConfigDbContext.Init(@"Config.litedb", modelBuilder =>
{
    modelBuilder.Entity<Customer>(c => c.EnsureIndex(t => t.Id, true));
});
ConfigDbContext.CacheContext.LoadCache();
var a = ConfigDbContext.CacheContext.Find(new Customer() { Id = "f461337e661c43cdb83936da35c95183" });
a.Name="123";
ConfigDbContext.CacheContext.Update(a);

ConfigDbContext.CacheContext.Add(new Customer
{
    Id = Guid.NewGuid().ToString("N"),
    Name = "John Doe_" + DateTime.Now.Ticks,
    Phones = new string[] { "8000-0000", "9000-0000" },
    Age = 39,
    IsActive = true
}
);

//测试备份和还原
using (var db = new LiteDatabase(@"MyData.db"))
{
    var backupContext = new LiteDatabaseBackupContext(new Dictionary<string, string>()
    {
        [typeof(Customer).FullName] = nameof(Customer)
    });

    //backupContext.Restore(db, "backup.d3b");
    //return;

    // Get customer collection
    var col = db.GetCollection<Customer>();
    // Create your new customer instance
    var customer = new Customer
    {
        Id = Guid.NewGuid().ToString("N"),
        Name = "John Doe_" + DateTime.Now.Ticks,
        Phones = new string[] { "8000-0000", "9000-0000" },
        Age = 39,
        IsActive = true
    };

    // Create unique index in Name field
    //col.EnsureIndex(x => x.Name, true);

    // Insert new customer document (Id will be auto-incremented)
    col.Insert(customer);

    //// Update a document inside a collection
    //customer.Name = "Joana Doe";

    //col.Update(customer);

    // Use LINQ to query documents (with no index)
    var results = col.Find(x => x.Age > 20).ToArray();

    backupContext.Backup(db, "backup.d3b");
}