namespace AFT.RegoV2.Core.Common.Data.Content.MessageTemplateModels
{
    public abstract class BaseMessageTemplateModel : IMessageTemplateModel
    {
        public string BrandName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ToEmail { get; set; }
        public string Username { get; set; }
    }
}