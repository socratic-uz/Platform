using System;
using System.ComponentModel.DataAnnotations;
using SharedKernel.ValueObjects;

namespace Domain.Entities
{
    public class OrderItem
    {
        [Display(Name = "Идентификатор")]
        public Guid? Id { get; set; }

        [Display(Name = "Номер заказа")]
        public Guid? OrderId { get; set; }

        [Display(Name = "Товар")]
        public Guid? ProductId { get; set; }

        [Range(1, 100000, ErrorMessage = "Количество должно быть от 1 до 100 000")]
        [Display(Name = "Количество")]
        public int Quantity { get; set; }

        [Display(Name = "Название")]
        public string? Name { get; set; }

        [Display(Name = "Производитель")]
        public Guid? MakerId { get; set; }

        [Display(Name = "Статус")]
        public OrderStatus Status { get; set; }

        [Display(Name = "Изменен в")]
        public DateTime ChangedAt { get; set; }

        [ScaffoldColumn(false)]
        public bool Watch { get; set; }
    }
}
