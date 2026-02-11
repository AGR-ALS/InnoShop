using System.Text;

namespace UserService.Application.Models;

public class EmailContents
{
    public EmailContent ForRestoringPassword { get; set; } = null!;
    public EmailContent ForAccountConfirmation { get; set; } = null!;

    public string BuildMessage(params string[] strings)
    {
        var sb = new StringBuilder();
        foreach (var str in strings)
        {
            sb.Append(str);
            sb.Append("\n");
        }
        return sb.ToString();
    }

    public string BuildLink(EmailContent emailContent, string Token)
    {
        return $"{emailContent.Link}?Token={Token}";
    }
}

public class EmailContent
{
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
}