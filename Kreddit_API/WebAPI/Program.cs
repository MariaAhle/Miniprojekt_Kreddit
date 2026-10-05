using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using shared.Model;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<KredditDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("Kreddit")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


// GET alle posts
app.MapGet("/posts", async (KredditDbContext db) =>
{
    return await db.Posts
        .Include(p => p.User)
        .Include(p => p.Comments)
        .ToListAsync();
});


// GET en post
app.MapGet("/posts/{id}", async (int id, KredditDbContext db) =>
{
    var post = await db.Posts
        .Include(p => p.User)
        .Include(p => p.Comments)
        .FirstOrDefaultAsync(p => p.Id == id);

    if (post == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(post);
});


// Opret ny post
app.MapPost("/posts", async (Post post, KredditDbContext db) =>
{
    db.Posts.Add(post);

    await db.SaveChangesAsync();

    return Results.Created($"/posts/{post.Id}", post);
});


// Opret kommentar
app.MapPost("/posts/{postId}/comments", async (
    int postId,
    CreateCommentRequest request,
    KredditDbContext db) =>
{
    var post = await db.Posts
        .Include(p => p.Comments)
        .FirstOrDefaultAsync(p => p.Id == postId);

    if (post == null)
    {
        return Results.NotFound();
    }

    var user = await db.Users.FindAsync(request.UserId);

    if (user == null)
    {
        return Results.NotFound("User not found");
    }

    var comment = new Comment
    {
        Content = request.Content,
        User = user
    };

    post.Comments.Add(comment);

    await db.SaveChangesAsync();

    return Results.Created(
        $"/posts/{postId}/comments",
        comment);
});


// Upvote post
app.MapPut("/posts/{id}/upvote", async (
    int id,
    KredditDbContext db) =>
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
app.MapPut("/posts/{id}/downvote", async (
    int id,
    KredditDbContext db) =>
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
app.MapPut("/posts/{postId}/comments/{commentId}/upvote", async (
    int postId,
    int commentId,
    KredditDbContext db) =>
{
    var post = await db.Posts
        .Include(p => p.Comments)
        .FirstOrDefaultAsync(p => p.Id == postId);

    if (post == null)
    {
        return Results.NotFound();
    }

    var comment = post.Comments
        .FirstOrDefault(c => c.Id == commentId);

    if (comment == null)
    {
        return Results.NotFound();
    }

    comment.Upvotes++;

    await db.SaveChangesAsync();

    return Results.Ok(comment);
});


// Downvote kommentar
app.MapPut("/posts/{postId}/comments/{commentId}/downvote", async (
    int postId,
    int commentId,
    KredditDbContext db) =>
{
    var post = await db.Posts
        .Include(p => p.Comments)
        .FirstOrDefaultAsync(p => p.Id == postId);

    if (post == null)
    {
        return Results.NotFound();
    }

    var comment = post.Comments
        .FirstOrDefault(c => c.Id == commentId);

    if (comment == null)
    {
        return Results.NotFound();
    }

    comment.Downvotes++;

    await db.SaveChangesAsync();

    return Results.Ok(comment);
});


app.Run();


// Bruges når webappen sender en kommentar
public class CreateCommentRequest
{
    public string Content { get; set; } = "";
    public int UserId { get; set; }
}