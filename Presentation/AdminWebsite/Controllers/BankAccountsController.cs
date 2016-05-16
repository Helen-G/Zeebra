using System;
using System.Linq;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.Core.Payment.ApplicationServices;
using AFT.RegoV2.Core.Payment.Validators;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Domain.BoundedContexts.Security.ApplicationServices;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Domain.Security.Interfaces;
using AutoMapper;
using ServiceStack.Validation;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    public class SaveBankAccountCommand
    {
        public Guid? Id { get; set; }
        public Guid Bank { get; set; }
        public string Currency { get; set; }
        public string AccountId { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public string AccountType { get; set; }
        public string Province { get; set; }
        public string Branch { get; set; }
        public string Remarks { get; set; }
    }

    public class BankAccountsController : BaseController
    {
        private readonly ISecurityProvider _securityProvider;
        private readonly BankAccountCommands _bankAccountCommands;
        private readonly BankAccountQueries _bankAccountQueries;
        private readonly UserService _userService;

        public BankAccountsController(
            BankAccountCommands bankAccountCommands, 
            BankAccountQueries bankAccountQueries, 
            ISecurityProvider securityProvider, 
            UserService userService)
        {
            _bankAccountCommands = bankAccountCommands;
            _bankAccountQueries = bankAccountQueries;
            _securityProvider = securityProvider;
            _userService = userService;
        }

        [HttpGet]
        public ActionResult GetBankAccount(Guid id)
        {
            var data = _bankAccountQueries.GetBankAccountById(id);
            return this.Success(data);
        }

        [HttpGet]
        public ActionResult List()
        {
            return Json(_bankAccountQueries.GetBankAccounts(), JsonRequestBehavior.AllowGet);
        }

        [SearchPackageFilter("searchPackage")]
        public object BankAccounts(SearchPackage searchPackage, string currencyCode)
        {
            var brandFilterSelections = _userService.GetBrandFilterSelections(CurrentUser.UserId);
            
            var bankAccounts = _bankAccountQueries.GetFilteredBankAccounts(CurrentUser.UserId)
                .Where(x => brandFilterSelections.Contains(x.Bank.BrandId));

            if (!string.IsNullOrEmpty(currencyCode))
                bankAccounts = bankAccounts.Where(x => x.CurrencyCode == currencyCode);

            var dataBuilder = new SearchPackageDataBuilder<BankAccount>(searchPackage, bankAccounts.ToArray().AsQueryable());

            dataBuilder.SetFilterRule(x => x.Bank.Brand, value => p => p.Bank.BrandId == new Guid(value))
                .Map(obj => obj.Id, obj =>
                    new[]
                    {
                        obj.CurrencyCode,
                        obj.AccountId,
                        obj.AccountName,
                        obj.AccountNumber,
                        obj.AccountType,
                        obj.Bank.Name,
                        obj.Province,
                        obj.Branch,
                        obj.Status.ToString(),
                        obj.CreatedBy,
                        obj.Created.ToString(),
                        obj.UpdatedBy,
                        obj.Updated.ToString(),
                        obj.PaymentLevels.Any().ToString()
                    }
                );

            var data = dataBuilder.GetPageData(obj => obj.AccountName);

            return new JsonResult
            {
                Data = data, 
                MaxJsonLength = int.MaxValue, 
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [HttpPost]
        public dynamic Save(SaveBankAccountCommand command)
        {
            try
            {
                string message;
                Guid id;

                if (command.Id.HasValue)
                {
                    var editData = Mapper.DynamicMap<EditBankAccountData>(command);
                    _bankAccountCommands.Edit(editData);
                    id = editData.Id;
                    message = "app:bankAccounts.updated";
                }
                else
                {
                    var addData = Mapper.DynamicMap<AddBankAccountData>(command);
                    var bankAccount =_bankAccountCommands.Add(addData);
                    id = bankAccount.Id;
                    message = "app:bankAccounts.created";
                }

                return SerializeJson(new { Result = "success", Data = message, id });
            }
            catch (ValidationError e)
            {
                return ValidationErrorResponseActionResult(e);
            }
            catch (Exception e)
            {
                return this.Failed(e);
            }
        }

        [HttpPost]
        public ActionResult Activate(Guid id, string remarks)
        {
            try
            {
                _bankAccountCommands.Activate(id, remarks);

                return this.Success(
                    new
                    {
                        message = "The bank account has beed successfully activated",
                        messageKey = "app:bankAccounts.bankAccountActivated"
                    });
            }
            catch (Exception ex)
            {
                return this.Failed(ex);
            }
        }

        [HttpPost]
        public ActionResult Deactivate(Guid id, string remarks)
        {
            try
            {
                _bankAccountCommands.Deactivate(id, remarks);
                return this.Success(
                    new
                    {
                        message = "The bank account has beed successfully deactivated",
                        messageKey = "app:bankAccounts.bankAccountDeactivated"
                    });
            }
            catch (Exception ex)
            {
                return this.Failed(ex);
            }
        }

        [HttpGet]
        public string GetCurrencies(Guid brandId)
        {
            var currencies = _bankAccountQueries.GetCurrencies(brandId);
            return SerializeJson(new
            {
                Currencies = currencies
            });
        }

        [HttpGet]
        public string GetBanks(Guid brandId)
        {
            var banks = _bankAccountQueries.GetBanks(brandId)
                .Select(b => new { b.Id, b.Name });

            return SerializeJson(new
            {
                Banks = banks
            });
        }

        [HttpGet]
        public string AccountCurrencies()
        {
            var currencyData = _bankAccountQueries.GetCurrencyData(CurrentUser.UserId);

            return SerializeJson(new
            {
                currencies = currencyData
            });
        }
    }
}