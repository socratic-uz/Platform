using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SharedKernel.ValueObjects;

namespace Domain.Entities
{
    public class Product
    {
        [Display(Name = "Идентификатор")]
        public Guid? Id { get; set; }

        [Display(Name = "Организация")]
        public Guid? OrganizationId { get; set; }

        [Display(Name = "Родительский ID")]
        public Guid? ParentId { get; set; }

        [Required(ErrorMessage = "Название обязательно для заполнения")]
        [StringLength(100, ErrorMessage = "Название не должно превышать 100 символов")]
        [Display(Name = "Название")]
        public string? Name { get; set; }

        [Range(0.01, 10000000.0, ErrorMessage = "Цена должна быть от 0.01 до 10 000 000")]
        [Display(Name = "Цена")]
        public decimal Price { get; set; }

        [StringLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
        [Display(Name = "Описание")]
        public string? Description { get; set; }

        [Display(Name = "Изображения")]
        public List<File> Images { get; set; } = new();

        [Display(Name = "Единица измерения")]
        public string? Unit { get; set; }

        [Display(Name = "Теги")]
        public List<string> Tags { get; set; } = new();

        [Display(Name = "В списке желаемого")]
        public bool IsWished { get; set; }

        [Display(Name = "Тип товара")]
        public string? Type { get; set; }

        [Display(Name = "График доступности")]
        public WorkSchedule? AvailabilitySchedule { get; set; }

        [Display(Name = "Разрешенные геозоны")]
        public List<string> AllowedZoneIds { get; set; } = new();

        [Display(Name = "Вес (кг)")]
        public double Weight { get; set; }

        [Display(Name = "Объем (л)")]
        public double Volume { get; set; }

        [Display(Name = "Ограничение по возрасту")]
        public bool IsAgeRestricted { get; set; }

        [Display(Name = "UX-Режим взаимодействия")]
        public int ProductUxModeValue { get; set; } = 1;
    }
}
