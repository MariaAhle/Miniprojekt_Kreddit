using shared.Model;
using WebAPI.Data;

namespace WebAPI.Service
{
    public class SeedData
    {
        public KredditDbContext db { get; }
        public SeedData(KredditDbContext db)
        {
            this.db = db;
        }
        public void SeedDatas()
        {
            User user = db.Users.FirstOrDefault()!;
            if (user == null)
            {
                user = new User { Username = "Jonathan" };
                db.Users.Add(user);
                db.Users.Add(new User { Username = "Maria" });
                db.Users.Add(new User { Username = "Emma" });
            }

            Post post = db.Posts.FirstOrDefault()!;
            if (post == null)
            {
                db.Posts.Add(new Post { Id = 1, Title = "hejhej", Content = "Hallo, hvordan har i det?" });
            }
        }
    }
}
