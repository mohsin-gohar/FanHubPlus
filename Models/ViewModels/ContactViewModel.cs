using System.ComponentModel.DataAnnotations;

namespace FanHubPlus.Models.ViewModels;

// Contact form (Views/Contact/Index.cshtml -> ContactController.SendMessage).
// Same shape/validation as the legacy FanHubPlus skeleton model that lived in the
// excluded FanHubPlus\FanHubPlus\ folder.
public class ContactViewModel
{
    [Required(ErrorMessage = "Name is required")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Enter a valid email address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Subject is required")]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Message is required")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Message must be 10-1000 characters")]
    public string Message { get; set; } = string.Empty;
}
