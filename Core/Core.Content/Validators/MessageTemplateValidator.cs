using System;
using System.Linq;
using AFT.RegoV2.Core.Common.Data.Content;
using AFT.RegoV2.Core.Common.Extensions;
using AFT.RegoV2.Core.Content.ApplicationServices;
using AFT.RegoV2.Core.Content.Data;
using FluentValidation;
using RazorEngine;
using RazorEngine.Templating;

namespace AFT.RegoV2.Core.Content.Validators
{
    public abstract class MessageTemplateValidator<T> : AbstractValidator<T> where T : BaseMessageTemplateData
    {
        protected readonly IContentRepository Repository;
        protected readonly IMessageTemplatesQueries MessageTemplatesQueries;
        protected Data.Brand Brand;

        protected MessageTemplateValidator(
            IContentRepository repository,
            IMessageTemplatesQueries messageTemplatesQueries)
        {            
            Repository = repository;
            MessageTemplatesQueries = messageTemplatesQueries;
            CascadeMode = CascadeMode.StopOnFirstFailure;
        }

        protected void ValidateLanguage()
        {
            When(x => Brand != null, () =>
                RuleFor(x => x.LanguageCode)
                    .NotEmpty()
                    .WithMessage(MessageTemplateValidationError.Required)
                    .Must(x => Brand.Languages.Any(y => y.Code == x))
                    .WithMessage(MessageTemplateValidationError.InvalidLanguage));
        }

        protected void ValidateMessageType()
        {
            RuleFor(x => x.MessageType)
                .Must(x => Enum.IsDefined(typeof (MessageType), x))
                .WithMessage(MessageTemplateValidationError.InvalidMessageType);
        }

        protected void ValidateMessageDeliveryMethod()
        {
            RuleFor(x => x.MessageDeliveryMethod)
                .Must(x => Enum.IsDefined(typeof(MessageDeliveryMethod), x))
                .WithMessage(MessageTemplateValidationError.InvalidMessageDeliveryMethod);
        }

        protected virtual void ValidateTemplateName()
        {
            When(x => Brand != null, () =>
                RuleFor(x => x.TemplateName)
                    .NotEmpty()
                    .WithMessage(MessageTemplateValidationError.Required)
                    .Must((data, name) => !Repository.MessageTemplates
                        .Any(y =>
                            y.BrandId == Brand.Id &&
                            y.MessageType == data.MessageType &&
                            y.LanguageCode == data.LanguageCode &&
                            y.TemplateName == name))
                    .WithMessage(MessageTemplateValidationError.TemplateNameInUse));
        }

        protected void ValidateFromName()
        {
            When(x => x.MessageDeliveryMethod == MessageDeliveryMethod.Email, () =>
                RuleFor(y => y.SenderName)
                    .NotEmpty()
                    .WithMessage(MessageTemplateValidationError.Required));

            When(x => x.MessageDeliveryMethod == MessageDeliveryMethod.Sms, () =>
                RuleFor(y => y.SenderName)
                    .Must(string.IsNullOrEmpty)
                    .WithMessage(MessageTemplateValidationError.Required));
        }

        protected void ValidateFromEmail()
        {
            When(x => x.MessageDeliveryMethod == MessageDeliveryMethod.Email, () =>
                RuleFor(y => y.SenderEmail)
                    .NotEmpty()
                    .WithMessage(MessageTemplateValidationError.Required)
                    .Matches("")
                    .WithMessage(MessageTemplateValidationError.InvalidSenderEmail));

            When(x => x.MessageDeliveryMethod == MessageDeliveryMethod.Sms, () =>
                RuleFor(y => y.SenderEmail)
                    .Must(string.IsNullOrEmpty)
                    .WithMessage(MessageTemplateValidationError.SenderEmailNotApplicable));
        }

        protected void ValidateSubject()
        {
            When(x => x.MessageDeliveryMethod == MessageDeliveryMethod.Email, () =>
                RuleFor(y => y.Subject)
                    .NotEmpty()
                    .WithMessage(MessageTemplateValidationError.Required));

            When(x => x.MessageDeliveryMethod == MessageDeliveryMethod.Sms, () =>
                RuleFor(y => y.Subject)
                    .Must(string.IsNullOrEmpty)
                    .WithMessage(MessageTemplateValidationError.SubjectNotApplicable));
        }

        protected void ValidateFromNumber()
        {
            When(x => x.MessageDeliveryMethod == MessageDeliveryMethod.Email, () =>
                RuleFor(y => y.SenderNumber)
                    .Must(string.IsNullOrEmpty)
                    .WithMessage(MessageTemplateValidationError.SenderNumberNotApplicable));

            When(x => x.MessageDeliveryMethod == MessageDeliveryMethod.Sms, () =>
                RuleFor(y => y.SenderNumber)
                    .NotEmpty()
                    .WithMessage(MessageTemplateValidationError.Required)
                    .Matches("")
                    .WithMessage(MessageTemplateValidationError.InvalidSenderNumber));
        }

        protected void ValidateMessageContent()
        {
            var stackTrace = string.Empty;

            When(x => Enum.IsDefined(typeof (MessageType), x.MessageType), () =>
                RuleFor(x => x.MessageContent)
                    .NotEmpty()
                    .WithMessage(MessageTemplateValidationError.Required)
                    .Must((data, content) =>
                    {
                        try
                        {
                            var modelType = MessageTemplatesQueries.GetMessageTemplateModelType(data.MessageType);
                            Engine.Razor.Compile(content, Guid.NewGuid().ToString(), modelType);
                            return true;
                        }
                        catch (Exception e)
                        {
                            stackTrace = e.StackTrace;
                            return false;
                        }
                    })
                    .WithMessage(MessageTemplateValidationError.InvalidMessageContent)
                    .WithState(x => stackTrace));
        }
    }
}