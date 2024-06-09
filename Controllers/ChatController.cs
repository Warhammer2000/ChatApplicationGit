using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChatApplication.Models;
using System.IO;
using Microsoft.AspNetCore.Http;
using ChatApplication.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using ChatApplication.Hubs;
using Microsoft.AspNetCore.SignalR;



namespace ChatApplication
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly IHubContext<ChatHub> _chatHub;
        public ChatController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment environment, IHubContext<ChatHub> chatHub)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
            _chatHub = chatHub;
        }
        [HttpGet]
        public IActionResult VideoCall(string receiverId)
        {
            // Проверьте, что receiverId передается и обрабатывается корректно
            ViewBag.ReceiverId = receiverId;
            return View();
        }
        public async Task<IActionResult> Index(string receiverId)
        {
            var userId = _userManager.GetUserId(User); // Получаем ID текущего пользователя
            var messages = await _context.Messages
                .Where(m => (m.Sender == userId && m.Receiver == receiverId) ||
                            (m.Sender == receiverId && m.Receiver == userId))
                .OrderBy(m => m.Timestamp)
                .ToListAsync();

            var userEmails = new Dictionary<string, string>();
            foreach (var message in messages)
            {
                if (!userEmails.ContainsKey(message.Sender))
                {
                    var user = await _userManager.FindByIdAsync(message.Sender);
                    if (user != null)
                    {
                        userEmails[message.Sender] = user.Email;
                    }
                }
                if (!userEmails.ContainsKey(message.Receiver))
                {
                    var user = await _userManager.FindByIdAsync(message.Receiver);
                    if (user != null)
                    {
                        userEmails[message.Receiver] = user.Email;
                    }
                }
            }

            var model = new ChatViewModel
            {
                ReceiverId = receiverId,
                Messages = messages,
                UserEmails = userEmails
            };

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> SendMessage([FromForm] string receiverId, [FromForm] string text)
        {
           
            var userId = _userManager.GetUserId(User); 
            var user = await _userManager.FindByIdAsync(userId);
            var email = user?.Email;

            var message = new Message
            {
                Sender = userId,
                UserEmail = email,
                Receiver = receiverId,
                Text = text,
                FilePath = string.Empty,
                Timestamp = DateTime.Now
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            await _chatHub.Clients.Group(receiverId).SendAsync("ReceiveMessage", userId, text);
            await _chatHub.Clients.Group(userId).SendAsync("ReceiveMessage", userId, text); 

            return RedirectToAction("Index", new { receiverId = receiverId });
        }

        [HttpPost]
        public async Task<IActionResult> SendFile(IFormFile file, string receiverId)
        {
            // Проверка, что файл выбран и не пустой
            if (file == null || file.Length == 0)
            {
                return BadRequest("Файл не выбран или пустой.");
            }

            // Получение ID текущего пользователя
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);
            var email = user?.Email;
            // Создание пути для сохранения файла
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            // Генерация уникального имени файла, чтобы избежать конфликтов имен
            var fileName = Path.GetFileName(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Сохранение файла на сервере
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // URL для доступа к файлу
            var fileUrl = $"/uploads/{uniqueFileName}";

            // Создание сообщения о новом файле
            var message = new Message
            {
                Sender = userId,
                UserEmail = email,
                Receiver = receiverId,
                Text = $"Отправил файл: {fileName}",
                FilePath = fileUrl,
                Timestamp = DateTime.Now
            };

            // Сохранение сообщения в базе данных
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // Уведомление клиентов через SignalR о новом файле
            await _chatHub.Clients.Group(receiverId).SendAsync("ReceiveFile", userId, fileName, fileUrl);
            await _chatHub.Clients.Group(userId).SendAsync("ReceiveFile", userId, fileName, fileUrl);

            // Перенаправление обратно на страницу чата
            return RedirectToAction("Index", new { receiverId });
        }
        [HttpPost]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            var message = await _context.Messages.FindAsync(messageId);
            var userId = _userManager.GetUserId(User);
            if (message != null)
            {
                var receiverId = message.Receiver;
                _context.Messages.Remove(message);
                await _context.SaveChangesAsync();

                // Уведомляем клиентов через SignalR
                await _chatHub.Clients.Group(receiverId).SendAsync("DeleteMessage", messageId);
                await _chatHub.Clients.Group(userId).SendAsync("DeleteMessage", messageId);

                return RedirectToAction("Index", new { receiverId = receiverId });
            }
            return NotFound();
        }
        [HttpPost]
        public async Task<IActionResult> EditMessage(int messageId, string receiverId, string newText)
        {
            var message = await _context.Messages.FindAsync(messageId);
            var userId = _userManager.GetUserId(User);
            if (message != null)
            {
                message.Text = newText;
                _context.Messages.Update(message);
                await _context.SaveChangesAsync();

                // Уведомляем клиентов через SignalR
                await _chatHub.Clients.Group(receiverId).SendAsync("EditMessage", messageId, newText);
                await _chatHub.Clients.Group(userId).SendAsync("EditMessage", messageId, newText);
                return RedirectToAction("Index", new { receiverId = message.Receiver });
            }
            return NotFound();
        }
    }
}
