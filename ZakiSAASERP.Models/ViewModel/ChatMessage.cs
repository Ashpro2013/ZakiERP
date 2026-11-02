namespace ZakiSAASERP.Models.ViewModel;

public class ChatMessage
{
    public int Id { get; set; }
    public int ChatHeadId { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsUser { get; set; }
    public string SenderName { get; set; }
    public DateTime Timestamp { get; set; }
}
