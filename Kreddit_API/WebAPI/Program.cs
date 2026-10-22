using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using shared.Model;
using WebAPI.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Sætter CORS som i eksemplet.
var AllowSomeStuff = "_AllowSomeStuff";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowSomeStuff, policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<KredditDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Kreddit")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<KredditDbContext>();

    var seedData = new SeedData(db);
    seedData.SeedDatas();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors(AllowSomeStuff);

// GET alle posts
app.MapGet("/posts", async (KredditDbContext db) =>
{
    return await db.Posts.Include(p => p.User).Include(p => p.Comments).ThenInclude(c => c.User).OrderByDescending(p => p.CreatedAt).ToListAsync();
});

// GET en post
app.MapGet("/posts/{id}", async (int id, KredditDbContext db) =>
{
    var post = await db.Posts.Include(p => p.User).Include(p => p.Comments).ThenInclude(c => c.User).FirstOrDefaultAsync(p => p.Id == id);

    if (post == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(post);
});

// Opret ny post
// Opret nyt opslag
app.MapPost("/posts", async (CreatePostRequest request, KredditDbContext db) =>
{
    var user = await db.Users.FindAsync(request.UserId);

    if (user == null)
    {
        return Results.NotFound("User not found.");
    }

    bool hasContent = !string.IsNullOrWhiteSpace(request.Content);
    bool hasUrl = !string.IsNullOrWhiteSpace(request.Url);

    if (string.IsNullOrWhiteSpace(request.Title) || hasContent == hasUrl)
    {
        return Results.BadRequest("Provide a title and either text or a URL.");
    }

    var post = new Post
    {
        Title = request.Title.Trim(),
        Content = request.Content?.Trim() ?? "",
        Url = request.Url?.Trim(),
        User = user,
        CreatedAt = DateTime.UtcNow
    };

    db.Posts.Add(post);
    await db.SaveChangesAsync();

    return Results.Created($"/posts/{post.Id}", post);
});
// Opret kommentar
app.MapPost("/posts/{id}/comments", async (int id, CreateCommentRequest request, KredditDbContext db) =>
{
    var post = await db.Posts
        .FirstOrDefaultAsync(p => p.Id == id);

    if (post == null)
    {
        return Results.NotFound("Post not found.");
    }

    if (string.IsNullOrWhiteSpace(request.Content))
    {
        return Results.BadRequest("Comment content is required.");
    }

    var user = await db.Users.FindAsync(request.UserId);

    if (user == null)
    {
        return Results.NotFound("User not found.");
    }

    var comment = new Comment
    {
        Content = request.Content.Trim(),
        User = user,
        CreatedAt = DateTime.UtcNow
    };

    post.Comments.Add(comment);
    await db.SaveChangesAsync();

    return Results.Created($"/posts/{id}/comments", comment);
});
// Upvote post
app.MapPut("/posts/{id}/upvote", async (int id, KredditDbContext db) =>
{
    var post = await db.Posts.FindAsync(id);
    if (post == null)
    {
        return Results.NotFound();
    }
    post.Upvotes++;
    await db.SaveChangesAsync();
    return Results.Ok(post);
});

// Downvote post
app.MapPut("/posts/{id}/downvote", async (int id, KredditDbContext db) =>
{
    var post = await db.Posts.FindAsync(id);
    if (post == null)
    {
        return Results.NotFound();
    }
    post.Downvotes++;
    await db.SaveChangesAsync();
    return Results.Ok(post);
});

// Upvote kommentar
app.MapPut("/posts/{postId}/comments/{commentId}/upvote", async (int postId, int commentId, KredditDbContext db) =>
{
    var post = await db.Posts
        .Include(p => p.Comments)
        .FirstOrDefaultAsync(p => p.Id == postId);
    if (post == null)
    {
        return Results.NotFound();
    }
    var comment = post.Comments.FirstOrDefault(c => c.Id == commentId);
    if (comment == null)
    {
        return Results.NotFound();
    }
    comment.Upvotes++;
    await db.SaveChangesAsync();
    return Results.Ok(comment);
});

// Downvote kommentar
app.MapPut("/posts/{postId}/comments/{commentId}/downvote", async (int postId, int commentId, KredditDbContext db) =>
{
    var post = await db.Posts
        .Include(p => p.Comments)
        .FirstOrDefaultAsync(p => p.Id == postId);
    if (post == null)
    {
        return Results.NotFound();
    }
    var comment = post.Comments.FirstOrDefault(c => c.Id == commentId);
    if (comment == null)
    {
        return Results.NotFound();
    }
    comment.Downvotes++;
    await db.SaveChangesAsync();
    return Results.Ok(comment);
});

app.Run();

public record CreatePostRequest(
    string Title, string? Content, string? Url, int UserId);

public record CreateCommentRequest(
    string Content, int UserId);