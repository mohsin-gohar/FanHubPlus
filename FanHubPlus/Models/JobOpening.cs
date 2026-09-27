namespace FanHubPlus.Models
{
    /// <summary>An open role advertised on the careers pages.</summary>
    public class JobOpening
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;     // Engineering | Content | Design | Support
        public string Location { get; set; } = "Mumbai (Hybrid)";
        public string Type { get; set; } = "Full Time";           // Full Time | Part Time | Internship | Contract
        public string Experience { get; set; } = "2-4 years";
        public string Salary { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string Responsibilities { get; set; } = string.Empty;   // newline separated
        public string Requirements { get; set; } = string.Empty;       // newline separated
        public string Benefits { get; set; } = string.Empty;           // newline separated
        public bool IsUrgent { get; set; }
        public DateTime PostedOn { get; set; } = DateTime.Now;

        public string PostedDisplay => PostedOn.ToString("dd MMM yyyy");
        public IEnumerable<string> ResponsibilityList =>
            (Responsibilities ?? string.Empty).Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
        public IEnumerable<string> RequirementList =>
            (Requirements ?? string.Empty).Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
        public IEnumerable<string> BenefitList =>
            (Benefits ?? string.Empty).Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
    }
}
