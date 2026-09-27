namespace FanHubPlus.Models
{
    public class Testimonial
    {
        public int Id { get; set; }
        public string Quote { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int ReviewCount { get; set; }
    }
}
