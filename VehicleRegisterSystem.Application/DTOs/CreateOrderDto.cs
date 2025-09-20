using System;
using System.ComponentModel.DataAnnotations;

namespace VehicleRegisterSystem.Application.DTOs
{
    public class CreateOrderDto
    {
        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
        [StringLength(100, ErrorMessage = "الاسم الكامل يجب ألا يزيد عن 100 حرف")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "الرقم الوطني مطلوب")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "الرقم الوطني يجب أن يكون 11 رقم")]
        public string NationalNumber { get; set; }

        [Required(ErrorMessage = "اسم الأم مطلوب")]
        [StringLength(50, ErrorMessage = "اسم الأم يجب ألا يزيد عن 50 حرف")]
        public string MotherName { get; set; }

        [Required(ErrorMessage = "اسم السيارة مطلوب")]
        [StringLength(50)]
        public string CarName { get; set; }

        [Required(ErrorMessage = "الموديل مطلوب")]
        [StringLength(50)]
        public string Model { get; set; }

        [Required(ErrorMessage = "سنة الصنع مطلوبة")]
        [Range(1900, 2100, ErrorMessage = "سنة الصنع غير صحيحة")]
        public int YearOfManufacture { get; set; }

        [Required(ErrorMessage = "اللون مطلوب")]
        [StringLength(30)]
        public string Color { get; set; }

        [Required(ErrorMessage = "رقم المحرك مطلوب")]
        [StringLength(50)]
        public string EngineNumber { get; set; }
    }
}
