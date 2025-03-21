using Microsoft.EntityFrameworkCore;

public class TreasureMapContext : DbContext
{
    public TreasureMapContext(DbContextOptions<TreasureMapContext> options)
        : base(options)
    {
    }

    public DbSet<TreasureMap> TreasureMaps { get; set; }
}