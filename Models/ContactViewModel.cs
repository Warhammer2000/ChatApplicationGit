using System.ComponentModel.DataAnnotations;

namespace ChatApplication.Models
{
    public class ContactViewModel
    {
        [Required(ErrorMessage = "Имя обязательно для заполнения.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Номер телефона обязателен для заполнения.")]
        [Phone(ErrorMessage = "Некорректный формат номера телефона.")]
        public string PhoneNumber { get; set; }

        public string ContactUserId { get; set; }


        [Required(ErrorMessage = "Email обязателен для заполнения.")]
        [EmailAddress(ErrorMessage = "Некорректный формат email.")]
        public string ContactUserEmail { get; set; } // Email контакта
        public bool IsFavorite { get; set; } // Является ли контакт избранным
    }
}
