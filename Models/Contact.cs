namespace ChatApplication.Models
{
    public class Contact
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string UserId { get; set; } 
        public string ContactUserEmail { get; set; }
        public string ContactUserId { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsFavorite { get; set; }
    }

}
