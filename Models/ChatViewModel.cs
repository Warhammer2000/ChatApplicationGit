namespace ChatApplication.Models
{
    public class ChatViewModel
    {
        public string ReceiverId { get; set; } 
        public string SenderId { get; set; }
        public List<Message> Messages { get; set; }
        public Dictionary<string, string> UserEmails { get; set; }

    }
}
