using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

using shared.Model;

namespace kreddit_app.Data;

public class ApiService
{
    private readonly HttpClient http;
    private readonly IConfiguration configuration;
    private readonly string baseAPI = "";

    public ApiService(HttpClient http, IConfiguration configuration)
    {
        this.http = http;
        this.configuration = configuration;
        this.baseAPI = configuration["base_api"];
    }

    public async Task<Post[]> GetPosts()
    {
        string url = $"{baseAPI}posts/";
        return await http.GetFromJsonAsync<Post[]>(url);
    }

    public async Task<Post> GetPost(int id)
    {
        string url = $"{baseAPI}posts/{id}/";
        return await http.GetFromJsonAsync<Post>(url);
    }

    public async Task<Comment> CreateComment(string content, int postId, string username)
    {
        string url = $"{baseAPI}posts/{postId}/comments";
     
        // Post JSON to API, save the HttpResponseMessage
        HttpResponseMessage msg = await http.PostAsJsonAsync(url, new { content, username });

        // Get the JSON string from the response
        msg.EnsureSuccessStatusCode();
        string json = await msg.Content.ReadAsStringAsync();

        // Deserialize the JSON string to a Comment object
        Comment? newComment = JsonSerializer.Deserialize<Comment>(json, new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true // Ignore case when matching JSON properties to C# properties 
        });

        // Return the new comment 
        return newComment;
    }

    public async Task<Post> UpvotePost(int id)
    {
        string url = $"{baseAPI}posts/{id}/upvote/";

        // Post JSON to API, save the HttpResponseMessage
        HttpResponseMessage msg = await http.PutAsJsonAsync(url, "");

        // Get the JSON string from the response
        msg.EnsureSuccessStatusCode();
        string json = await msg.Content.ReadAsStringAsync();

        // Deserialize the JSON string to a Post object
        Post? updatedPost = JsonSerializer.Deserialize<Post>(json, new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true // Ignore case when matching JSON properties to C# properties
        });

        // Return the updated post (vote increased)
        return updatedPost;
    }

    public async Task<Post> CreatePost(string title, string content, string username)
    {
        string url = $"{baseAPI}posts";

        string? postUrl = null;
        if (Uri.TryCreate(content.Trim(), UriKind.Absolute, out var link)
            && (link.Scheme == "http" || link.Scheme == "https"))
        {
            postUrl = content.Trim();
            content = "";
        }

        using var response = await http.PostAsJsonAsync(
            url, new { title, content, url = postUrl, username });
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Post>();
    }
    public async Task DownvotePost(int id)
    {
        using var response = await http.PutAsync($"{baseAPI}posts/{id}/downvote", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpvoteComment(int postId, int commentId)
    {
        using var response = await http.PutAsync(
            $"{baseAPI}posts/{postId}/comments/{commentId}/upvote", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task DownvoteComment(int postId, int commentId)
    {
        using var response = await http.PutAsync(
            $"{baseAPI}posts/{postId}/comments/{commentId}/downvote", null);
        response.EnsureSuccessStatusCode();
    }
}
