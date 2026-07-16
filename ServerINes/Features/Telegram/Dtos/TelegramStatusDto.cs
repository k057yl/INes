namespace INest.Features.Telegram.Dtos
{
    public class TelegramStatusDto
    {
        public bool IsLinked { get; set; }
        public string? BotUsername { get; set; }
        public string? VerificationToken { get; set; }
        public long? TelegramChatId { get; set; }
    }
}
