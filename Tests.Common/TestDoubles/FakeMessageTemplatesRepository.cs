using System;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Content;
using AFT.RegoV2.Core.Content.Data;

namespace AFT.RegoV2.Tests.Common.TestDoubles
{

    public class FakeContentsRepository : IContentRepository
    {
        private readonly FakeDbSet<MessageTemplate> _messageTemplatesDbSetFake = new FakeDbSet<MessageTemplate>();
        private readonly FakeDbSet<Brand> _brandDbSetFake = new FakeDbSet<Brand>();
        private readonly FakeDbSet<Language> _languageDbSetFake = new FakeDbSet<Language>();
        private readonly FakeDbSet<Player> _playerDbSetFake = new FakeDbSet<Player>();

        public IDbSet<MessageTemplate> MessageTemplates { get { return _messageTemplatesDbSetFake; } }
        public IDbSet<Brand> Brands { get { return _brandDbSetFake; } }
        public IDbSet<Language> Languages { get { return _languageDbSetFake; } }
        public IDbSet<Player> Players { get { return _playerDbSetFake; } } 

        public FakeContentsRepository()
        {
            const string content = "Hi @Model.Username! <br/> <br/> Bonus reward: @Model.Amount @Model.Currency. <br/> <br/> Best regards,@Model.Brand team";

            MessageTemplates.Add(new MessageTemplate
            {
                TemplateName = "Email message template",
                MessageContent = content,
                MessageDeliveryMethod = MessageDeliveryMethod.Email
            });

            MessageTemplates.Add(new MessageTemplate
            {
                TemplateName = "Sms message template",
                MessageContent = content,
                MessageDeliveryMethod = MessageDeliveryMethod.Sms
            });

            MessageTemplates.Add(new MessageTemplate
            {
                Id = Guid.Parse("B6433819-5665-4E2F-BC5B-BA42B844B56D"),
                TemplateName = "High Deposit Reminder",
                MessageContent =
                "Hi @Model.Username, you are about to get @Model.Currency @Model.BonusAmount by depositing @Model.Currency @Model.RemainingAmount more to reach monthly deposit of @Model.Currency @Model.DepositAmountRequired bonus. @Model.Brand team",
                MessageDeliveryMethod = MessageDeliveryMethod.Email
            });

            MessageTemplates.Add(new MessageTemplate
            {
                Id = Guid.Parse("4564BEB7-24B7-4041-A76B-BABF92591CDD"),
                TemplateName = "Refer Friend Notification",
                MessageContent =
                "Hi, your friend @Model.ReferalName recommends you to play in our member site. Please click @Model.ReferalLink to register. @Model.Brand team.",
                MessageDeliveryMethod = MessageDeliveryMethod.Email
            });

            MessageTemplates.Add( new MessageTemplate
            {
                Id = Guid.Parse("F44435B3-9F3C-4962-B28D-39B3AE5E61F0"),
                TemplateName = "New password",
                MessageContent =
                "Hi @Model.Username!<br/><br/>Your new password is @Model.Newpassword.<br/><br/>Best regards,<br/>@Model.Brand team",
                MessageDeliveryMethod = MessageDeliveryMethod.Email
            });

            MessageTemplates.Add(new MessageTemplate
            {
                Id = Guid.Parse("0EBFC3C2-8506-4EB9-86E3-CE7EEC789D53"),
                TemplateName = "Activation link message",
                MessageContent =
                "Dear @Model.Username, To confirm your email address, follow the link bellow: <a href=\"@Model.ActivationLink\">Activate</a>",
                MessageDeliveryMethod = MessageDeliveryMethod.Email
            });

            MessageTemplates.Add( new MessageTemplate
            {
                Id = Guid.Parse("042DC6D5-1E23-475F-9C8A-E29AFF85DC60"),
                TemplateName = "Mobile verification code",
                MessageContent =
                "Hi, @Model.Username. Your mobile verification code is: @Model.VerificationCode.",
                MessageDeliveryMethod = MessageDeliveryMethod.Sms
            });
        }

        public MessageTemplate FindMessageTemplateById(Guid id)
        {
            return MessageTemplates.FirstOrDefault(x => x.Id == id);
        }

        public int SaveChanges()
        {
            return 0;
        }
    }
}