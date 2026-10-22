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
            // Opret kun testdata, hvis der ikke er posts endnu.
            if (db.Posts.Any())
            {
                return;
            }

            User? user = db.Users.FirstOrDefault();
            if (user == null)
            {
                user = new User { Username = "Jonathan" };
                db.Users.Add(user);
                db.Users.Add(new User { Username = "Maria" });
                db.Users.Add(new User { Username = "Emma" });
            }

            var post = new Post
            {
                Title = "hejhej",
                Content = "Hallo, hvordan har I det?",
                User = user,
                Upvotes = 2
            };

            post.Comments.Add(new Comment
            {
                Content = "Jeg har det godt!",
                User = user
            });

            db.Posts.Add(post);
            db.Posts.Add(new Post
            {
                Title = "Et link til Wikipedia",
                Url = "https://google.com",
                User = user
            });

            db.SaveChanges();
        }
    }
}