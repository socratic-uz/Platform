using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SharedKernel.Protos;
using SharedKernel.ValueObjects;

namespace Domain.Entities
{
    public class Organization
    {
        [Display(Name = "Идентификатор")]
        public Guid? Id { get; set; }

        [Display(Name = "Родительская организация")]
        public Guid? ParentId { get; set; }

        [Required(ErrorMessage = "Название обязательно для заполнения")]
        [StringLength(100, ErrorMessage = "Название не должно превышать 100 символов")]
        [Display(Name = "Название")]
        public string? Name { get; set; }

        [StringLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
        [Display(Name = "Описание")]
        public string? Description { get; set; }

        [Display(Name = "Изображения")]
        public List<File> Images { get; set; } = new();

        [Display(Name = "Адрес")]
        public SharedKernel.Protos.Address? Address { get; set; }

        [Display(Name = "Активен до")]
        public DateTime? ActiveUntil { get; set; }

        [Display(Name = "График работы")]
        public WorkSchedule? DefaultSchedule { get; set; }

        [Display(Name = "Зоны доставки")]
        public List<SharedKernel.ValueObjects.Zone> Zones { get; set; } = new();

        [Display(Name = "ИНН (STIR)")]
        public string? TaxId { get; set; }

        [Display(Name = "Настройки оплаты")]
        public OrganizationPaymentConfig PaymentConfig { get; set; } = new();

        [Display(Name = "Подписка")]
        public OrganizationSubscription Subscription { get; set; } = new();

        [ScaffoldColumn(false)]
        public SharedKernel.Protos.Options? Options { get; set; }

        [ScaffoldColumn(false)]
        [Display(Name = "Зона доставки")]
        public SharedKernel.Protos.ZoneDto? Zone { get; set; }
    }
}
