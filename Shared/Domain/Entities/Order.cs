using System;
using System.ComponentModel.DataAnnotations;
using SharedKernel.ValueObjects;

namespace Domain.Entities
{
    public class Order
    {
        [Display(Name = "Номер заказа")]
        public Guid? Id { get; set; }

        [Display(Name = "Роль")]
        public Guid? RoleId { get; set; }

        [Display(Name = "Организация")]
        public Guid? OrganizationId { get; set; }

        [Display(Name = "Место")]
        public Guid? PlaceId { get; set; }

        [Display(Name = "Статус")]
        public OrderStatus Status { get; set; }

        [Display(Name = "Тип оплаты")]
        public PaymentMethod PaymentType { get; set; }

        [Range(0.0, 10000000.0, ErrorMessage = "Итоговая сумма должна быть положительной")]
        [Display(Name = "Итоговая сумма")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Требуется OTP")]
        public bool RequiresOtp { get; set; }

        [Display(Name = "OTP Токен")]
        public string? OtpToken { get; set; }

        [Display(Name = "Сообщение об оплате")]
        public string? PaymentMessage { get; set; }

        [Display(Name = "Фискальный признак")]
        public string? FiscalSign { get; set; }

        [Display(Name = "Ссылка на фискальный чек Soliq.uz")]
        public string? FiscalUrl { get; set; }

        [Display(Name = "Создан в")]
        public DateTime CreatedAt { get; set; }

        [ScaffoldColumn(false)]
        public bool Watch { get; set; }
    }
}
