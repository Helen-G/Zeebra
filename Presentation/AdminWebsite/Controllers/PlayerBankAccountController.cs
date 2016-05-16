using System;
using System.Linq;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.ApplicationServices.Payment;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Domain.BoundedContexts.Security.ApplicationServices;
using AFT.RegoV2.Domain.Payment.Data;
using ServiceStack.Validation;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    public class PlayerBankAccountController : BaseController
    {
        private readonly PlayerBankAccountCommands _commands;
        private readonly PlayerBankAccountQueries _queries;
        private readonly UserService _userService;

        public PlayerBankAccountController(PlayerBankAccountCommands commands, PlayerBankAccountQueries queries, UserService userService)
        {
            _commands = commands;
            _queries = queries;
            _userService = userService;
        }

        [HttpGet]
        [SearchPackageFilter("searchPackage")]
        public object PlayerList(SearchPackage searchPackage)
        {
            var playerBankAccounts = _queries.GetPlayerBankAccounts();

            var dataBuilder = new SearchPackageDataBuilder<PlayerBankAccount>(searchPackage, playerBankAccounts);

            dataBuilder.SetFilterRule(x => x.Player.Id, (value) => p => p.Player.Id == new Guid(value))
                .Map(obj => obj.Id,
                    obj => new[]
                    {
                        obj.AccountName,
                        obj.AccountNumber,
                        obj.Bank.Name,
                        obj.Province,
                        obj.City,
                        obj.Branch,
                        obj.SwiftCode,
                        obj.Address,
                        obj.IsCurrent ? "Yes" : "No",
                        Enum.GetName(typeof(BankAccountStatus), obj.Status),
                        obj.CreatedBy,
                        Format.FormatDate(obj.Created, false),
                        obj.UpdatedBy,
                        Format.FormatDate(obj.Updated, false),
                        obj.VerifiedBy,
                        Format.FormatDate(obj.Verified, false),
                        obj.RejectedBy,
                        Format.FormatDate(obj.Rejected, false)
                    }
                );

            var data = dataBuilder.GetPageData(obj => obj.AccountName);

            var result = new JsonResult
            {
                Data = data,
                MaxJsonLength = int.MaxValue,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            return result;
        }

        [HttpGet]
        [SearchPackageFilter("searchPackage")]
        public object PendingList(SearchPackage searchPackage)
        {
            var brandFilterSelections = _userService.GetBrandFilterSelections(CurrentUser.UserId);
            var pendingPlayerBankAccounts = _queries.GetPendingPlayerBankAccounts()
                .Where(x => brandFilterSelections.Contains(x.Player.BrandId));

            var dataBuilder = new SearchPackageDataBuilder<PlayerBankAccount>(searchPackage, pendingPlayerBankAccounts);

            dataBuilder.SetFilterRule(x => x.Player.BrandId, (value) => y => y.Player.BrandId == new Guid(value))
                .Map(obj => obj.Id,
                    obj => new[]
                    {
                        obj.AccountName,
                        obj.AccountNumber,
                        obj.Bank.Name,
                        obj.Province,
                        obj.City,
                        obj.Branch,
                        obj.SwiftCode,
                        obj.Address,
                        obj.IsCurrent ? "Yes" : "No",
                        Enum.GetName(typeof(BankAccountStatus), obj.Status),
                        obj.CreatedBy,
                        Format.FormatDate(obj.Created, false),
                        obj.UpdatedBy,
                        Format.FormatDate(obj.Updated, false)
                    }
                );

            var data = dataBuilder.GetPageData(obj => obj.AccountName);

            var result = new JsonResult
            {
                Data = data,
                MaxJsonLength = int.MaxValue,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            return result;
        }       

        [HttpPost]
        public ActionResult Verify(Guid id, string remarks)
        {
            try
            {
                _commands.Verify(id, remarks);
                return this.Success();
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
        public ActionResult Reject(Guid id, string remarks)
        {
            try
            {
                _commands.Reject(id, remarks);
                return this.Success();
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
    }
}