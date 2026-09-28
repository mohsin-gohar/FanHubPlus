using System.ComponentModel.DataAnnotations;

namespace FanHubPlus.Models.ViewModels;

// Chatbot widget request/response (JSON round-trip)
public class ChatRequestViewModel
{
    [Required, StringLength(1000)]
    public string Message { get; set; } = string.Empty;
}

public class ChatResponseViewModel
{
    public string Reply { get; set; } = string.Empty;
    public List<string> Suggestions { get; set; } = new(); // follow-up quick chips
    public List<ChatResultItem> Results { get; set; } = new(); // clickable search results
}

// A single clickable result returned by the chatbot (content, character, article, etc.)
public class ChatResultItem
{
    public string Title { get; set; } = string.Empty;     // e.g. "Naruto"
    public string Url { get; set; } = string.Empty;       // e.g. "/Explore/Details/5"
    public string Subtitle { get; set; } = string.Empty;  // e.g. "Anime · 2002"
    public string Type { get; set; } = string.Empty;      // "content" / "character" / "article" / "event"
}

// Full-page chat history view
public class ChatHistoryViewModel
{
    public List<ChatTurn> Turns { get; set; } = new();
}

public class ChatTurn
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public DateTime At { get; set; }
}
