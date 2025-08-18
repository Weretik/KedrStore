namespace Application.Common.Interfaces
{
    /// <summary>
    /// Модель для отправки электронного письма
    /// </summary>
    public class EmailMessage
    {
        /// <summary>
        /// Email отправителя
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// Имя отправителя
        /// </summary>
        public string FromName { get; set; }

        /// <summary>
        /// Адреса получателей
        /// </summary>
        public List<string> To { get; set; } = new List<string>();

        /// <summary>
        /// Адреса получателей копии (CC)
        /// </summary>
        public List<string> Cc { get; set; } = new List<string>();

        /// <summary>
        /// Адреса получателей скрытой копии (BCC)
        /// </summary>
        public List<string> Bcc { get; set; } = new List<string>();

        /// <summary>
        /// Тема письма
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Тело письма
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Флаг, указывающий, является ли тело письма HTML
        /// </summary>
        public bool IsHtml { get; set; } = true;

        /// <summary>
        /// Вложения к письму
        /// </summary>
        public List<EmailAttachment> Attachments { get; set; } = new List<EmailAttachment>();
    }

    /// <summary>
    /// Модель для вложения к электронному письму
    /// </summary>
    public class EmailAttachment
    {
        /// <summary>
        /// Имя файла
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Содержимое файла в виде массива байтов
        /// </summary>
        public byte[] Content { get; set; }

        /// <summary>
        /// MIME-тип содержимого
        /// </summary>
        public string ContentType { get; set; }
    }

    /// <summary>
    /// Результат отправки электронного письма
    /// </summary>
    public class EmailResult
    {
        /// <summary>
        /// Флаг успешной отправки
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Сообщение об ошибке (если есть)
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Идентификатор отправленного письма
        /// </summary>
        public string MessageId { get; set; }
    }

    /// <summary>
    /// Сервис для отправки электронных писем
    /// </summary>
    /// <remarks>
    /// Этот интерфейс предоставляет абстракцию для отправки электронных писем,
    /// позволяя использовать различные реализации (SMTP, SendGrid, MailGun и т.д.).
    /// </remarks>
    public interface IEmailService
    {
        /// <summary>
        /// Отправляет электронное письмо
        /// </summary>
        /// <param name="message">Сообщение для отправки</param>
        /// <returns>Результат отправки</returns>
        EmailResult Send(EmailMessage message);

        /// <summary>
        /// Асинхронно отправляет электронное письмо
        /// </summary>
        /// <param name="message">Сообщение для отправки</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Результат отправки</returns>
        Task<EmailResult> SendAsync(EmailMessage message, CancellationToken cancellationToken = default);

        /// <summary>
        /// Отправляет электронное письмо с использованием шаблона
        /// </summary>
        /// <param name="templateName">Имя шаблона</param>
        /// <param name="templateData">Данные для шаблона</param>
        /// <param name="toAddress">Адрес получателя</param>
        /// <param name="subject">Тема письма</param>
        /// <returns>Результат отправки</returns>
        EmailResult SendTemplate(string templateName, object templateData, string toAddress, string subject);

        /// <summary>
        /// Асинхронно отправляет электронное письмо с использованием шаблона
        /// </summary>
        /// <param name="templateName">Имя шаблона</param>
        /// <param name="templateData">Данные для шаблона</param>
        /// <param name="toAddress">Адрес получателя</param>
        /// <param name="subject">Тема письма</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Результат отправки</returns>
        Task<EmailResult> SendTemplateAsync(string templateName, object templateData, string toAddress, string subject, CancellationToken cancellationToken = default);
    }
}
