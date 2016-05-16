using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.AdminWebsite.ViewModels.Content;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Content;
using AFT.RegoV2.Core.Content.ApplicationServices;
using AFT.RegoV2.Core.Content.Data;
using AFT.RegoV2.Core.Content.Validators;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Shared;
using AutoMapper;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    [Authorize]
    public class MessageTemplateController : BaseController
    {
        private readonly UserService _userService;
        private readonly IMessageTemplatesCommands _messageTemplatesCommands;
        private readonly IMessageTemplatesQueries _messageTemplatesQueries;
        private readonly BrandQueries _brandQueries;

        static MessageTemplateController()
        {
            Mapper.CreateMap<AddMessageTemplateModel, AddMessageTemplateData>();
            Mapper.CreateMap<EditMessageTemplateModel, EditMessageTemplateData>();
        }

        public MessageTemplateController(
            UserService userService,
            IMessageTemplatesCommands messageTemplatesCommands,
            IMessageTemplatesQueries messageTemplatesQueries,
            BrandQueries brandQueries)
        {
            _userService = userService;
            _messageTemplatesCommands = messageTemplatesCommands;
            _messageTemplatesQueries = messageTemplatesQueries;
            _brandQueries = brandQueries;
        }

        [SearchPackageFilter("searchPackage")]
        public object List(SearchPackage searchPackage)
        {
            var allowedBrands = _brandQueries
                .GetFilteredBrands(_brandQueries.GetBrands(), CurrentUser.UserId)
                .Select(x => x.Id);

            var templates = _messageTemplatesQueries.GetTemplates()
                .Where(x => allowedBrands.Contains(x.BrandId));

            var dataBuilder = new SearchPackageDataBuilder<MessageTemplate>(searchPackage, templates);

            dataBuilder.Map(template => template.Id, template => new[]
            {
                template.Brand.Name,
                template.Language.Name,
                Enum.GetName(typeof(MessageType), template.MessageType),
                Enum.GetName(typeof(MessageDeliveryMethod), template.MessageDeliveryMethod),
                template.TemplateName,
                Enum.GetName(typeof(Status), template.Status),
                Format.FormatDate(template.Created, true),
                template.CreatedBy,
                Format.FormatDate(template.Updated, true),
                template.UpdatedBy,
                Format.FormatDate(template.Activated, true),
                template.ActivatedBy,
                Format.FormatDate(template.Deactivated, true),
                template.DeactivatedBy
            });

            return new JsonResult
            {
                Data = dataBuilder.GetPageData(record => record.Brand.Name),
                MaxJsonLength = int.MaxValue,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public string Add()
        {
            var selectedLicensees = _userService.GetLicenseeFilterSelections(CurrentUser.UserId);

            var licensees = _brandQueries
                .GetFilteredLicensees(_brandQueries.GetLicensees(), CurrentUser.UserId)
                .Where(x => selectedLicensees.Contains(x.Id))
                .OrderBy(x => x.Name)
                .Select(x => new { x.Id, x.Name});

            return SerializeJson(new
            {
                licensees,
                messageTypes = Enum.GetNames(typeof(MessageType)),
                messageDeliveryMethods = Enum.GetNames(typeof(MessageDeliveryMethod))
            });
        }

        public string Brands(Guid licenseeId)
        {
            var selectedBrands = _userService.GetBrandFilterSelections(CurrentUser.UserId);

            var brands = _brandQueries
                .GetFilteredBrands(_brandQueries.GetBrands(), CurrentUser.UserId)
                .Where(x =>
                    selectedBrands.Contains(x.Id) &&
                    x.LicenseeId == licenseeId)
                .OrderBy(x => x.Name)
                .Select(x => new {x.Id, x.Name});

            return SerializeJson(new {brands});
        }

        public string Languages(Guid brandId)
        {
            var languages = _messageTemplatesQueries
                .GetBrandLanguages(brandId)
                .OrderBy(x => x.Name);

            return SerializeJson(new {languages});
        }

        [HttpPost]
        public ActionResult Add(AddMessageTemplateModel model)
        {
            try
            {
                var data = Mapper.Map<AddMessageTemplateData>(model);
                var validationResult = _messageTemplatesQueries.ValidateCanAdd(data);

                if (!validationResult.IsValid)
                    return ValidationErrorResponseActionResult(validationResult.Errors);

                var id = _messageTemplatesCommands.Add(data);

                return this.Success(id);
            }
            catch (Exception e)
            {
                return this.Failed(e);
            }
        }

        public string Edit(Guid id)
        {
            var allowedBrands = _brandQueries
                .GetFilteredBrands(_brandQueries.GetBrands(), CurrentUser.UserId)
                .Select(x => x.Id);

            var template = _messageTemplatesQueries.GetTemplates().SingleOrDefault(x =>
                x.Id == id &&
                allowedBrands.Contains(x.BrandId));

            if (template == null)
                return null;

            var brand = _brandQueries.GetBrand(template.BrandId);

            return SerializeJson(new
            {
                languages = _messageTemplatesQueries.GetBrandLanguages(brand.Id).OrderBy(x => x.Name),
                messageTypes = Enum.GetNames(typeof (MessageType)),
                messageDeliveryMethods = Enum.GetNames(typeof (MessageDeliveryMethod)),
                template = new
                {
                    licenseeName = _brandQueries.GetLicensee(brand.LicenseeId).Name,
                    brandName = brand.Name,
                    languageCode = template.Language.Code,
                    messageType = Enum.GetName(typeof (MessageType), template.MessageType),
                    messageDeliveryMethod = Enum.GetName(typeof (MessageDeliveryMethod), template.MessageDeliveryMethod),
                    templateName = template.TemplateName,
                    senderName = template.SenderName,
                    senderEmail = template.SenderEmail,
                    subject = template.Subject,
                    senderNumber = template.SenderNumber,
                    messageContent = template.MessageContent
                }
            });
        }

        [HttpPost]
        public ActionResult Edit(EditMessageTemplateModel model)
        {
            try
            {
                var data = Mapper.Map<EditMessageTemplateData>(model);
                var validationResult = _messageTemplatesQueries.ValidateCanEdit(data);

                if (!validationResult.IsValid)
                    return ValidationErrorResponseActionResult(validationResult.Errors);

                _messageTemplatesCommands.Edit(data);

                return this.Success();
            }
            catch (Exception e)
            {
                return this.Failed(e);
            }
        }

        public string View(Guid id)
        {
            var allowedBrands = _brandQueries
                .GetFilteredBrands(_brandQueries.GetBrands(), CurrentUser.UserId)
                .Select(x => x.Id);

            var template = _messageTemplatesQueries.GetTemplates().SingleOrDefault(x =>
                x.Id == id &&
                allowedBrands.Contains(x.BrandId));

            if (template == null)
                return null;

            var brand = _brandQueries.GetBrand(template.BrandId);

            return SerializeJson(new
            {
                licenseeName = _brandQueries.GetLicensee(brand.LicenseeId).Name,
                brandName = brand.Name,
                languageName = template.Language.Name,
                messageType = Enum.GetName(typeof(MessageType), template.MessageType),
                messageDeliveryMethod = Enum.GetName(typeof(MessageDeliveryMethod), template.MessageDeliveryMethod),
                templateName = template.TemplateName,
                senderName = template.SenderName,
                senderEmail = template.SenderEmail,
                subject = template.Subject,
                senderNumber = template.SenderNumber,
                messageContent = template.MessageContent
            });
        }

        #region Actions

        [SearchPackageFilter("searchPackage")]
        public ActionResult Templates(SearchPackage searchPackage)
        {
            var data = SearchTemplate(searchPackage);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Types()
        {
            var response = new
            {
                Types = new List<object>
                {
                    new
                    {
                        Id = (int) MessageDeliveryMethod.Email,
                        Name = MessageDeliveryMethod.Email.ToString()
                    },
                    new
                    {
                        Id = (int) MessageDeliveryMethod.Sms,
                        Name = MessageDeliveryMethod.Sms.ToString()
                    }
                }
            };

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult CreateTemplate(MessageTemplateUiData messageTemplateUiData)
        {
            try
            {
                var id = _messageTemplatesCommands.Update(messageTemplateUiData);   
                return Json(new {Success = true, TemplateId = id});
            }
            catch (RegoException exception)
            {
                return Json(new {Success = false, exception.Message});
            }
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult EditTemplate(MessageTemplateUiData messageTemplateUiData)
        {
            try
            {
                var id = _messageTemplatesCommands.Update(messageTemplateUiData);
                return Json(new {Success = true, TemplateId = id});
            }
            catch (RegoException exception)
            {
                return Json(new {Success = false, exception.Message});
            }
        }

        [HttpPost]
        public ActionResult DeleteTemplate(Guid id)
        {
            try
            {
               _messageTemplatesCommands.Delete(id);
                return Json(new {Success = true});
            }
            catch (RegoException exception)
            {
                return Json(new {Success = false, exception.Message});
            }
        }

        public ActionResult Template(Guid id)
        {
            var template = _messageTemplatesQueries.GetTemplate(id);
            return Json(template, JsonRequestBehavior.AllowGet);
        }

        #endregion

        private object SearchTemplate(SearchPackage searchPackage)
        {
            var queryable = _messageTemplatesQueries.GetTemplates();
            var dataBuilder = new SearchPackageDataBuilder<MessageTemplate>(searchPackage, queryable);

            return dataBuilder
                .Map(x => x.Id,
                    x => MapMessageCell(x))
                .GetPageData(x => x.TemplateName);
        }

        private object MapMessageCell(MessageTemplate messageTemplate)
        {
            return new object[]
            {
                messageTemplate.TemplateName,
                HttpUtility.HtmlEncode(messageTemplate.MessageContent),
                messageTemplate.MessageDeliveryMethod.ToString()
            };
        }
    }
}