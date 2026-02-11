namespace SharedResources.MessageContracts;

public record MailSendingEvent(string Email, string Subject, string Body);