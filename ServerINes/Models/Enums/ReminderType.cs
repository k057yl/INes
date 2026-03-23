namespace INest.Models.Enums
{
    public enum ReminderType
    {
        WarrantyExpiration = 0, // Гарантия
        Maintenance = 1,        // ТО / Обслуживание
        ReturnItem = 2,         // Вернуть вещь (одалживание)
        Insurance = 3,          // Страховка
        MedicalCheckup = 4,     // Медосмотр
        TaxPayment = 5,         // Налоги 
        Subscription = 6,       // Подписки
        Custom = 7              // Свое
    }
}
