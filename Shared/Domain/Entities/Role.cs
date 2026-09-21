using System;
using System.ComponentModel.DataAnnotations;
using SharedKernel.ValueObjects;

namespace Domain.Entities
{
    public class Role
    {
        [Display(Name = "Идентификатор")]
        public Guid? Id { get; set; }

        [Display(Name = "Пользователь")]
        public Guid? UserId { get; set; }

        [Display(Name = "Организация")]
        public Guid? OrganizationId { get; set; }

        [Required(ErrorMessage = "Название роли обязательно для заполнения")]
        [StringLength(100, ErrorMessage = "Название роли не должно превышать 100 символов")]
        [Display(Name = "Название")]
        public string? Name { get; set; }

        [Display(Name = "Разрешения")]
        public Permission Permission { get; set; }
    }
}
