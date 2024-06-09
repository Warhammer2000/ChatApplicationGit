using ChatApplication.Data;
using ChatApplication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatApplication.Controllers
{
    [Authorize]
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ContactController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User); // Получаем ID текущего пользователя

            // Получаем контакты текущего пользователя
            var contacts = await _context.Contacts
                .Where(c => c.UserId == userId)
                .ToListAsync();

            // Преобразуем контакты в ContactViewModel
            var contactViewModels = contacts.Select(contact => new ContactViewModel
            {
                ContactUserId = contact.ContactUserId,
                ContactUserEmail = contact.ContactUserEmail, // Используем email из таблицы Contacts
                Name = contact.Name,
                PhoneNumber = contact.PhoneNumber,
                IsFavorite = contact.IsFavorite
            }).ToList();

            return View(contactViewModels);
        }


        [HttpGet]
        public IActionResult AddContact()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddContact(ContactViewModel model)
        {
            var userId = _userManager.GetUserId(User); // ID текущего пользователя

            // Проверка, что контакт с таким email уже не существует для данного пользователя
            var existingContact = await _context.Contacts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ContactUserEmail == model.ContactUserEmail);

            if (existingContact != null)
            {
                ModelState.AddModelError(string.Empty, "Контакт с таким email уже существует.");
                return View("Index", await GetContactViewModels());
            }

            // Добавляем новый контакт
            var contact = new Contact
            {
                UserId = userId,
                ContactUserId = Guid.NewGuid().ToString(), // Генерируем новый уникальный идентификатор для контакта
                ContactUserEmail = model.ContactUserEmail,
                Name = model.Name,
                PhoneNumber = model.PhoneNumber,
                IsFavorite = false // Начальная настройка
            };

            _context.Contacts.Add(contact);
            await _context.SaveChangesAsync();

            // Перенаправление на Index для отображения списка контактов
            return RedirectToAction("Index");
        }


        private async Task<List<ContactViewModel>> GetContactViewModels()
        {
            var userId = _userManager.GetUserId(User); // Получаем ID текущего пользователя

            // Получаем контакты текущего пользователя
            var contacts = await _context.Contacts
                .Where(c => c.UserId == userId)
                .ToListAsync();

            var contactViewModels = new List<ContactViewModel>();

            foreach (var contact in contacts)
            {
                var contactUser = await _userManager.FindByIdAsync(contact.ContactUserId);
                if (contactUser != null)
                {
                    contactViewModels.Add(new ContactViewModel
                    {
                        ContactUserId = contact.ContactUserId,
                        ContactUserEmail = contactUser.Email,
                        Name = contact.Name,
                        PhoneNumber = contact.PhoneNumber,
                        IsFavorite = contact.IsFavorite
                    });
                }
            }

            return contactViewModels;
        }
        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(int id)
        {
            var userId = _userManager.GetUserId(User);
            var contact = await _context.Contacts.FirstOrDefaultAsync(c => c.UserId == userId);

            if (contact == null)
            {
                return NotFound("Контакт не найден или не принадлежит текущему пользователю.");
            }

            contact.IsFavorite = !contact.IsFavorite;
            _context.Contacts.Update(contact);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User); // Получаем ID текущего пользователя
            var contact = await _context.Contacts.FirstOrDefaultAsync(c => c.UserId == userId);

            if (contact == null)
            {
                return NotFound("Контакт не найден или не принадлежит текущему пользователю.");
            }

            _context.Contacts.Remove(contact);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
