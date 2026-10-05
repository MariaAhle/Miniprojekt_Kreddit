using Microsoft.EntityFrameworkCore;
using shared.Model;

namespace WebAPI.Data;

public class KredditDbContext : DbContext
{
    public KredditDbContext(
        DbContextOptions<KredditDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Comment> Comments => Set<Comment>();
}