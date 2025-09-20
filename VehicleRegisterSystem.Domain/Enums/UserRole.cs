using System.ComponentModel;

namespace VehicleRegisterSystem.Domain.Enums
{
    /// <summary>
    /// أدوار المستخدمين في النظام
    /// User roles in the system
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// مستخدم عادي - Regular User
        /// يمكنه البحث والاستعارة والإرجاع
        /// Can search, borrow, and return books
        /// </summary>
        [Description("مستخدم عادي - Regular User")]
        User = 1,

        /// <summary>
        /// مدير النظام - Administrator
        /// يمكنه إدارة المستخدمين والكتب وجميع الصلاحيات
        /// Can manage users, books and has all permissions
        /// </summary>
        [Description("مدير النظام - Administrator")]
        Administrator = 2,
        [Description("مدقق البيانات")]

        OrderValidator = 3,
        [Description("مسجل اللوحة")]

        BoardRegistrar = 4,
    }


}
