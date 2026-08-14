using System.Collections.Generic;
using System.Net.Mail;
using Repo.Dto;

public class CreateEmailDto
{
    public string Type { get; set; }
    public string From { get; set; }
    public string Subject { get; set; }
    public RecipientsFacet Recipients { get; set; }
    public string Message { get; set; }
    public List<Attachment> Attachments { get; set; }
    // public Attachment Attachments { get; set; }
}

public class Attachment
{
    public string FileName { get; set; }
    public string FilePath { get; set; }
}