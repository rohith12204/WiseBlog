using Microsoft.AspNetCore.Mvc;
using WiseBlog.Services;
using WiseBlog.Shared.Models;
using System.Text.Json;

namespace WiseBlog.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlogController : ControllerBase
    {
        private readonly MongoDBService _mongoDBService;

        public BlogController(MongoDBService mongoDBService)
        {
            _mongoDBService = mongoDBService;
        }

        // ✅ Upload Blog (content + metadata)
        [HttpPost("UploadBlog")]
        public async Task<IActionResult> UploadBlog()
        {
            try
            {
                var form = await Request.ReadFormAsync();
                var file = form.Files.FirstOrDefault(f => f.FileName.EndsWith(".html"));
                var metadataFile = form.Files.FirstOrDefault(f => f.FileName.EndsWith(".json"));

                if (file == null || metadataFile == null || file.Length == 0 || metadataFile.Length == 0)
                {
                    return BadRequest("Invalid request. Ensure Title, Description, and Content are provided.");
                }

                // ✅ Read metadata JSON
                string userId, userName, title, description, category, visibility;
                using (var metadataStream = new StreamReader(metadataFile.OpenReadStream()))
                {
                    var metadataJson = await metadataStream.ReadToEndAsync();
                    var metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(metadataJson);

                    if (metadata == null ||
                        !metadata.ContainsKey("userId") ||
                        !metadata.ContainsKey("userName") ||
                        !metadata.ContainsKey("title") ||
                        !metadata.ContainsKey("description") ||
                        !metadata.ContainsKey("category") ||
                        !metadata.ContainsKey("visibility"))
                    {
                        return BadRequest("Invalid metadata.");
                    }

                    userId = metadata["userId"];
                    userName = metadata["userName"];
                    title = metadata["title"];
                    description = metadata["description"];
                    category = metadata["category"];
                    visibility = metadata["visibility"];
                }

                // ✅ Upload content to GridFS
                using var contentStream = file.OpenReadStream();
                using var memoryStream = new MemoryStream();
                await contentStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                string blogId = await _mongoDBService.UploadBlogToGridFS(memoryStream, file.ContentType);

                // ✅ Save metadata document in Blogs collection
                var blogDocument = new Blog
                {
                    userId = userId,
                    userName = userName,
                    title = title,
                    description = description,
                    category = Enum.Parse<BlogCategory>(category, true),
                    visibility = Enum.Parse<VisibilityOptions>(visibility, true),
                    contentId = blogId
                };

                await _mongoDBService.InsertBlogDocument(blogDocument);

                return Ok(new { message = "Blog uploaded successfully", blogId = blogDocument.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error uploading blog: {ex.Message}");
            }
        }

        // ✅ Save summary
        [HttpPost("SaveSummary")]
        public async Task<IActionResult> SaveSummary([FromBody] Summary summary)
        {
            try
            {
                await _mongoDBService.SaveSummary(summary);
                return Ok(new { message = "Summary saved successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error saving summary: {ex.Message}");
            }
        }

        // ✅ Get summaries by user
        [HttpGet("GetSummary/{userid}")]
        public async Task<IActionResult> GetSummary(string userid)
        {
            var summaries = await _mongoDBService.GetSummaries(userid);
            return Ok(summaries);
        }

        // ✅ Get a single blog by ID
        [HttpGet("GetBlog/{id}")]
        public async Task<IActionResult> GetBlog(string id)
        {
            var blog = await _mongoDBService.GetBlog(id);
            if (blog == null) return NotFound("Blog not found");
            return Ok(blog);
        }

        // ✅ Delete blog
        [HttpDelete("DeleteBlog/{id}")]
        public async Task<IActionResult> DeleteBlog(string id)
        {
            await _mongoDBService.DeleteBlogDocument(id);
            return Ok(new { message = "Blog deleted successfully" });
        }

        // ✅ Delete summary
        [HttpDelete("DeleteSummary/{id}")]
        public async Task<IActionResult> DeleteSummary(string id)
        {
            await _mongoDBService.DeleteSummary(id);
            return Ok(new { message = "Summary deleted successfully" });
        }

        // ✅ Get blog HTML content from GridFS
        [HttpGet("GetContent/{id}")]
        public async Task<IActionResult> GetContent(string id)
        {
            var blogStream = await _mongoDBService.DownloadFileFromGridFS(id);
            if (blogStream == null) return NotFound("Content not found");

            using var reader = new StreamReader(blogStream);
            string blogContent = await reader.ReadToEndAsync();
            return Content(blogContent, "text/html");
        }

        // ✅ Get all blogs
        [HttpGet("GetAllBlogs")]
        public async Task<IActionResult> GetAllBlogs()
        {
            var blogs = await _mongoDBService.GetAllBlogs();
            return Ok(blogs);
        }
    }
}
