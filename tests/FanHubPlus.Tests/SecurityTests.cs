using System.Text;
using FanHubPlus.Models.Entities;
using FanHubPlus.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Xunit;

namespace FanHubPlus.Tests.Security;

public sealed class FileUploadServiceTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "FanHubPlusTests", Guid.NewGuid().ToString("N"));
    private readonly FileUploadService _service;

    public FileUploadServiceTests()
    {
        Directory.CreateDirectory(Path.Combine(_root, "wwwroot"));
        var env = new TestWebHostEnvironment { WebRootPath = Path.Combine(_root, "wwwroot") };
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["FileUploads:MaxImageBytes"] = "2048",
            ["FileUploads:MaxWidth"] = "100",
            ["FileUploads:MaxHeight"] = "100"
        }).Build();

        _service = new FileUploadService(env, config);
    }

    [Fact]
    public async Task Valid_png_uses_random_name_inside_upload_root()
    {
        var uploaded = await _service.SaveImageAsync(FormFile("avatar.png", "image/png", MinimalPng()), "avatars");

        Assert.StartsWith("/uploads/avatars/", uploaded);
        Assert.NotEqual("avatar.png", Path.GetFileName(uploaded));
        Assert.True(File.Exists(Path.Combine(_root, "wwwroot", uploaded.TrimStart('/').Replace('/', Path.DirectorySeparatorChar))));
    }

    [Theory]
    [InlineData("payload.exe", "image/png")]
    [InlineData("payload.png", "application/octet-stream")]
    public async Task Invalid_extension_mime_or_executable_content_is_rejected(
        string fileName, string contentType)
    {
        var bytes = fileName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
            ? new byte[] { 0x4D, 0x5A, 0x90, 0x00, 0x03, 0, 0, 0, 0x04, 0, 0, 0 }
            : MinimalPng();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.SaveImageAsync(FormFile(fileName, contentType, bytes), "avatars"));
    }

    [Fact]
    public async Task Mime_spoofed_jpeg_with_png_content_is_rejected()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.SaveImageAsync(FormFile("cover.jpg", "image/jpeg", MinimalPng()), "content"));
    }

    [Fact]
    public async Task Oversized_dimensions_are_rejected()
    {
        var png = MinimalPng(width: 101, height: 10);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.SaveImageAsync(FormFile("huge.png", "image/png", png), "content"));
    }

    [Fact]
    public async Task Traversal_in_client_filename_cannot_escape_upload_root()
    {
        var uploaded = await _service.SaveImageAsync(
            FormFile("../payload.png", "image/png", MinimalPng()), "avatars");

        Assert.StartsWith("/uploads/avatars/", uploaded);
        Assert.DoesNotContain("..", uploaded);
        var written = Path.GetFullPath(Path.Combine(_root, "wwwroot", uploaded.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)));
        Assert.StartsWith(Path.GetFullPath(Path.Combine(_root, "wwwroot", "uploads")) +
            Path.DirectorySeparatorChar, written);
    }

    [Fact]
    public void Delete_never_removes_file_outside_uploads_root()
    {
        var secret = Path.Combine(_root, "secret.txt");
        File.WriteAllText(secret, "keep");

        _service.DeleteImage("/secret.txt");
        _service.DeleteImage("/uploads/../secret.txt");

        Assert.True(File.Exists(secret));
    }

    private static FormFile FormFile(string name, string contentType, byte[] bytes) =>
        new(new MemoryStream(bytes), 0, bytes.Length, "file", name)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };

    private static byte[] MinimalPng(int width = 10, int height = 10)
    {
        var png = new byte[32];
        new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }.CopyTo(png, 0);
        Encoding.ASCII.GetBytes("IHDR").CopyTo(png, 12);
        WriteBigEndian(png, 16, width);
        WriteBigEndian(png, 20, height);
        png[24] = 8; // bit depth
        png[25] = 6; // RGBA
        return png;
    }

    private static void WriteBigEndian(byte[] target, int offset, int value)
    {
        target[offset] = (byte)(value >> 24);
        target[offset + 1] = (byte)(value >> 16);
        target[offset + 2] = (byte)(value >> 8);
        target[offset + 3] = (byte)value;
    }

    public void Dispose()
    {
        if (Directory.Exists(_root)) Directory.Delete(_root, recursive: true);
        GC.SuppressFinalize(this);
    }

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public string WebRootPath { get; set; } = string.Empty;
        public IFileProvider WebRootFileProvider { get; set; } = null!;
        public string ApplicationName { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
        public string ContentRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = "Development";
    }
}