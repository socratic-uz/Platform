using System;
using System.ComponentModel.DataAnnotations;
using SharedKernel.ValueObjects;

namespace Domain.Entities
{
    public class User
    {
        [Display(Name = "Идентификатор")]
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "Номер телефона обязателен для заполнения")]
        [Phone(ErrorMessage = "Неверный формат номера телефона")]
        [Display(Name = "Телефон")]
        public string? Phone { get; set; }

        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "ФИО")]
        public SharedKernel.Protos.Name? Name { get; set; }

        [Display(Name = "Изображение")]
        public File? Image { get; set; }

        [Display(Name = "Адрес")]
        public SharedKernel.Protos.Address? Address { get; set; }

        [Display(Name = "Заблокирован")]
        public bool IsBlocked { get; set; }

        [Display(Name = "Язык")]
        public Language Language { get; set; }

        [Display(Name = "Тема")]
        public Theme Theme { get; set; }

        [Display(Name = "Акцент")]
        public int Accent { get; set; } = 65459;

        [Display(Name = "Истекает в")]
        public DateTime? Expires { get; set; }

        public User Clone()
        {
            return new User
            {
                Id = Id,
                Phone = Phone,
                Email = Email,
                Name = Name != null ? new SharedKernel.Protos.Name { FirstName = Name.FirstName, LastName = Name.LastName, MiddleName = Name.MiddleName } : new SharedKernel.Protos.Name(),
                Address = Address != null ? new SharedKernel.Protos.Address(Address) : new SharedKernel.Protos.Address(),
                Image = Image != null ? new File { Url = Image.Url, FileName = Image.FileName, ContentType = Image.ContentType, Content = Image.Content } : new File(),
                IsBlocked = IsBlocked,
                Language = Language,
                Theme = Theme,
                Accent = Accent,
                Expires = Expires
            };
        }
    }
}
