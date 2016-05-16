namespace AFT.RegoV2.Core.Common.Data.Content.MessageTemplateModels
{
    public interface IMessageTemplateModel
    {
        string Username { get; set; }
        string BrandName { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
        string ToEmail { get; set; }
    }
}