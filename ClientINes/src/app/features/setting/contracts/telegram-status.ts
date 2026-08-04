export interface TelegramStatusContract {
  isLinked: boolean;
  botUsername?: string;
  verificationToken?: string;
  telegramChatId?: number;
}