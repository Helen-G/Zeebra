namespace AFT.RegoV2.Core.Common.Data.Content
{
    public enum MessageTemplateValidationError
    {
        Required,
        InvalidId,
        InvalidBrand,
        InvalidLanguage,
        InvalidMessageType,
        InvalidMessageDeliveryMethod,
        TemplateNameInUse,
        InvalidSenderEmail,
        SenderEmailNotApplicable,
        SubjectNotApplicable,
        InvalidSenderNumber,
        SenderNumberNotApplicable,
        InvalidMessageContent,
        AlreadyActive
    }
}