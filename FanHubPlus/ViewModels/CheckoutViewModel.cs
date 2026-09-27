using System.ComponentModel.DataAnnotations;

namespace FanHubPlus.ViewModels
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Full name is required")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Enter a valid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone is required")]
        [Phone(ErrorMessage = "Enter a valid phone number")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "PIN code is required")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Enter a valid 6 digit PIN code")]
        public string PinCode { get; set; } = string.Empty;

        [Display(Name = "Payment method")]
        public string PaymentMethod { get; set; } = "Card";

        [Display(Name = "Card number")]
        [RegularExpression(@"^\d{12,19}$", ErrorMessage = "Enter a valid card number")]
        public string CardNumber { get; set; } = string.Empty;

        [Display(Name = "Expiry")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$", ErrorMessage = "Use MM/YY")]
        public string Expiry { get; set; } = string.Empty;

        [Display(Name = "CVV")]
        [RegularExpression(@"^\d{3,4}$", ErrorMessage = "Enter the 3 or 4 digit code")]
        public string Cvv { get; set; } = string.Empty;

        [Display(Name = "Same as billing address")]
        public bool SameAsBilling { get; set; } = true;

        [Display(Name = "Save this address")]
        public bool SaveAddress { get; set; }

        public string PaymentDetail =>
            PaymentMethod switch
            {
                "UPI" => "Paying by UPI - approve the request on your UPI app",
                "Net Banking" => "Net Banking - choose your bank on the next step",
                "Wallet" => "Wallet - FanHub balance will be debited",
                _ => "Card ending " + (CardNumber.Length >= 4 ? CardNumber[^4..] : "0000")
            };
    }
}
