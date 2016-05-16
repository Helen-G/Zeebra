using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.ApplicationServices.Payment;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Game.Exceptions;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Domain.BoundedContexts.Security.ApplicationServices;
using AFT.RegoV2.Domain.Payment.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ServiceStack.Common.Extensions;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    [Authorize]
    public class OfflineWithdrawController : BaseController
    {
        private readonly WithdrawalService _service;
        private readonly PaymentQueries _paymentQueries;
        private readonly UserService _userService;

        public OfflineWithdrawController(WithdrawalService service, PaymentQueries paymentQueries, UserService userService)
        {
            _service = service;
            _paymentQueries = paymentQueries;
            _userService = userService;
        }

        [SearchPackageFilter("searchPackage")]
        public ActionResult RequestedList(SearchPackage searchPackage)
        {
            var data = SearchOfflineWithdraws(
                searchPackage,
                obj => new[]
                {
                    obj.PlayerBankAccount.Player.Username,
                    obj.PlayerBankAccount.Bank.Brand.Name,
                    obj.PlayerBankAccount.Bank.Name,
                    obj.TransactionNumber,
                    obj.PlayerBankAccount.Province,
                    obj.PlayerBankAccount.City,
                    obj.PlayerBankAccount.Branch,
                    obj.PlayerBankAccount.SwiftCode,
                    obj.PlayerBankAccount.Address,
                    obj.PlayerBankAccount.AccountName,
                    obj.PlayerBankAccount.AccountNumber,
                    obj.Amount.ToString(CultureInfo.InvariantCulture),
                    obj.Created.ToString(CultureInfo.InvariantCulture),
                    obj.CreatedBy,
                    obj.Remarks
                },
                _service.GetWithdrawalsForVerification());

            return new JsonResult
            {
                Data = data,
                MaxJsonLength = int.MaxValue,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [SearchPackageFilter("searchPackage")]
        public ActionResult VerifiedList(SearchPackage searchPackage)
        {
            var data = SearchOfflineWithdraws(
                searchPackage,
                obj => new[]
                {
                    obj.PlayerBankAccount.Player.Username,
                    obj.PlayerBankAccount.Bank.Brand.Name,
                    obj.PlayerBankAccount.Bank.Name,
                    obj.TransactionNumber,
                    obj.PlayerBankAccount.Province,
                    obj.PlayerBankAccount.City,
                    obj.PlayerBankAccount.Branch,
                    obj.PlayerBankAccount.SwiftCode,
                    obj.PlayerBankAccount.Address,
                    obj.PlayerBankAccount.AccountName,
                    obj.PlayerBankAccount.AccountNumber,
                    obj.Amount.ToString(CultureInfo.InvariantCulture),
                    obj.Created.ToString(CultureInfo.InvariantCulture),
                    obj.CreatedBy,
                    obj.Remarks
                },
                _service.GetWithdrawalsForAcceptance());

            return new JsonResult
            {
                Data = data,
                MaxJsonLength = int.MaxValue,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [SearchPackageFilter("searchPackage")]
        public ActionResult AcceptedList(SearchPackage searchPackage)
        {
            var data = SearchOfflineWithdraws(
                searchPackage,
                obj => new[]
                {
                    obj.PlayerBankAccount.Player.Username,
                    obj.PlayerBankAccount.Bank.Brand.Name,
                    obj.PlayerBankAccount.Bank.Name,
                    obj.TransactionNumber,
                    obj.PlayerBankAccount.Province,
                    obj.PlayerBankAccount.City,
                    obj.PlayerBankAccount.Branch,
                    obj.PlayerBankAccount.SwiftCode,
                    obj.PlayerBankAccount.Address,
                    obj.PlayerBankAccount.AccountName,
                    obj.PlayerBankAccount.AccountNumber,
                    obj.Amount.ToString(CultureInfo.InvariantCulture),
                    obj.Created.ToString(CultureInfo.InvariantCulture),
                    obj.CreatedBy,
                    obj.Remarks
                },
                _service.GetWithdrawalsForApproval());

            return new JsonResult
            {
                Data = data,
                MaxJsonLength = int.MaxValue,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [SearchPackageFilter("searchPackage")]
        public ActionResult FailedAutoWagerList(SearchPackage searchPackage)
        {
            var data = SearchOfflineWithdraws(
                searchPackage,
                obj => new[]
                {
                    obj.PlayerBankAccount.Player.Username,
                    obj.PlayerBankAccount.Bank.Brand.Name,
                    obj.PlayerBankAccount.Bank.Name,
                    obj.TransactionNumber,
                    obj.PlayerBankAccount.Province,
                    obj.PlayerBankAccount.City,
                    obj.PlayerBankAccount.Branch,
                    obj.PlayerBankAccount.SwiftCode,
                    obj.PlayerBankAccount.Address,
                    obj.PlayerBankAccount.AccountName,
                    obj.PlayerBankAccount.AccountNumber,
                    obj.Amount.ToString(CultureInfo.InvariantCulture),
                    obj.Created.ToString(CultureInfo.InvariantCulture),
                    obj.CreatedBy,
                    obj.Remarks
                },
                _service.GetWithdrawalsFailedAutoWagerCheck());

            return new JsonResult
            {
                Data = data,
                MaxJsonLength = int.MaxValue,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [SearchPackageFilter("searchPackage")]
        public ActionResult OnHoldList(SearchPackage searchPackage)
        {
            var data = SearchOfflineWithdraws(
                searchPackage,
                obj => new[]
                {
                    obj.PlayerBankAccount.Player.Username,
                    obj.PlayerBankAccount.Bank.Brand.Name,
                    obj.PlayerBankAccount.Bank.Name,
                    obj.TransactionNumber,
                    obj.PlayerBankAccount.Province,
                    obj.PlayerBankAccount.City,
                    obj.PlayerBankAccount.Branch,
                    obj.PlayerBankAccount.SwiftCode,
                    obj.PlayerBankAccount.Address,
                    obj.PlayerBankAccount.AccountName,
                    obj.PlayerBankAccount.AccountNumber,
                    obj.Amount.ToString(CultureInfo.InvariantCulture),
                    obj.Created.ToString(CultureInfo.InvariantCulture),
                    obj.CreatedBy,
                    obj.Remarks
                },
                _service.GetWithdrawalsOnHold());

            return new JsonResult
            {
                Data = data,
                MaxJsonLength = int.MaxValue,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [HttpGet]
        public ActionResult Get(Guid id)
        {
            try
            {
                var offlineWithdraw = _paymentQueries.GetWithdrawById(id);
                return
                    this.Success(
                        new JsonSerializerSettings
                        {
                            ContractResolver = new CamelCasePropertyNamesContractResolver(),
                            PreserveReferencesHandling = PreserveReferencesHandling.Objects
                        }, offlineWithdraw);
            }
            catch (InvalidOperationException exception)
            {
                return this.Failed(exception);
            }
        }

        [HttpPost]
        public string Create(OfflineWithdrawRequest withdrawRequest)
        {
            if (ModelState.IsValid == false)
                return SerializeJson(new {Result = "failed", Data = (string) null});

            withdrawRequest.RequestedBy = System.Threading.Thread.CurrentPrincipal.Identity.Name;


            OfflineWithdrawResponse response;
            try
            {
                response = _service.Request(withdrawRequest);
            }
            catch (Exception ex)
            {
                return SerializeJson(new { Result = "failed", Error = ex.Message });
            }

            return SerializeJson(new {Result = "success", Data = "app:payment.withdraw.requestCreated", Id = response.Id});
        }

        [HttpPost]
        public ActionResult Verify(Guid requestId, string remarks)
        {
            try
            {
                _service.Verify(requestId, remarks);
                return this.Success("app:payment.withdraw.successfullyVerified");
            }
            catch (InvalidOperationException exception)
            {
                return this.Failed(exception);
            }
        }

        [HttpPost]
        public ActionResult Unverify(Guid requestId, string remarks)
        {
            try
            {
                _service.Unverify(requestId, remarks);
                return this.Success("app:payment.withdraw.successfullyUnverified");
            }
            catch (InvalidOperationException exception)
            {
                return this.Failed(exception);
            }
        }

        [HttpPost]
        public ActionResult Approve(Guid requestId, string remarks)
        {
            try
            {
                _service.Approve(requestId, remarks);
                return this.Success("app:payment.withdraw.successfullyApproved");
            }
            catch (InvalidOperationException exception)
            {
                return this.Failed(exception);
            }
            catch (InsufficientFundsException)
            {
                return this.Failed("app:payment.insufficentFund");
            }
        }

        [HttpPost]
        public ActionResult Reject(Guid requestId, string remarks)
        {
            try
            {
                _service.Reject(requestId, remarks);
                return this.Success("app:payment.withdraw.successfullyRejected");
            }
            catch (InvalidOperationException exception)
            {
                return this.Failed(exception);
            }
        }

        [HttpPost]
        public ActionResult PassWager(Guid requestId, string remarks)
        {
            try
            {
                _service.PassWager(requestId, remarks);
                return this.Success("app:payment.withdraw.successfullyUpdated");
            }
            catch (InvalidOperationException exception)
            {
                return this.Failed(exception);
            }
        }

        [HttpPost]
        public ActionResult FailWager(Guid requestId, string remarks)
        {
            try
            {
                _service.FailWager(requestId, remarks);
                return this.Success("app:payment.withdraw.successfullyUpdated");
            }
            catch (InvalidOperationException exception)
            {
                return this.Failed(exception);
            }
        }

        [HttpPost]
        public ActionResult PassInvestigation(Guid requestId, string remarks)
        {
            try
            {
                _service.PassInvestigation(requestId, remarks);
                return this.Success("app:payment.withdraw.successfullyUpdated");
            }
            catch (InvalidOperationException exception)
            {
                return this.Failed(exception);
            }
        }

        [HttpPost]
        public ActionResult FailInvestigation(Guid requestId, string remarks)
        {
            try
            {
                _service.FailInvestigation(requestId, remarks);
                return this.Success("app:payment.withdraw.successfullyUpdated");
            }
            catch (InvalidOperationException exception)
            {
                return this.Failed(exception);
            }
        }

        [HttpPost]
        public ActionResult Accept(Guid requestId, string remarks)
        {
            try
            {
                _service.Accept(requestId, remarks);
                return this.Success("app:payment.withdraw.successfullyAccepted");
            }
            catch (InvalidOperationException exception)
            {
                return this.Failed(exception);
            }
        }

        [HttpGet]
        public string CancelInfo(Guid id)
        {
            var withdrawal = _paymentQueries.GetWithdrawById(id);

            var response = new
            {
                BaseInfo = new
                {
                    Licensee = withdrawal.PlayerBankAccount.Bank.Brand.LicenseeName,
                    Brand = withdrawal.PlayerBankAccount.Bank.Brand.Name,
                    Username = withdrawal.PlayerBankAccount.Player.Username,
                    ReferenceCode = withdrawal.TransactionNumber,
                    Status = withdrawal.Status.ToString(),
                    InternalAccount = "no", //TODO: wtf
                    Currency = withdrawal.PlayerBankAccount.Player.CurrencyCode,
                    PaymentMethod = withdrawal.PaymentMethod.ToString(),
                    Amount = withdrawal.Amount,
                    Submitted = withdrawal.CreatedBy,
                    DateSubmitted = withdrawal.Created.ToString()
                },
                BankInformation = new
                {
                    BankName = withdrawal.PlayerBankAccount.Bank.Name,
                    BankAccountName = withdrawal.PlayerBankAccount.AccountName,
                    BankAccountNumber = withdrawal.PlayerBankAccount.AccountNumber,
                    Branch = withdrawal.PlayerBankAccount.Branch,
                    SwiftCode = withdrawal.PlayerBankAccount.SwiftCode,
                    Address = withdrawal.PlayerBankAccount.Address,
                    City = withdrawal.PlayerBankAccount.City,
                    Province = withdrawal.PlayerBankAccount.Province
                }
            };

            return SerializeJson(response);
        }

        [HttpPost]
        public ActionResult CancelRequest(Guid id, string remark)
        {
            try
            {
                _service.Cancel(id, remark);
                return this.Success("app:payment.withdraw.successfullyCanceled");
            }
            catch (InvalidOperationException exception)
            {
                return this.Failed(exception);
            }
        }


        [HttpPost]
        public ActionResult Revert(Guid requestId, string remarks)
        {
            try
            {
                _service.Revert(requestId, remarks);
                return this.Success("app:payment.withdraw.successfullyReverted");
            }
            catch (InvalidOperationException exception)
            {
                return this.Failed(exception);
            }
        }

        private SearchPackageResult SearchOfflineWithdraws(SearchPackage searchPackage,
            Expression<Func<OfflineWithdraw, object>> cellExpression, IQueryable<OfflineWithdraw> queryable)
        {
            var brandFilterSelections = _userService.GetBrandFilterSelections(CurrentUser.UserId);

            queryable = queryable
                .Include(x => x.PlayerBankAccount.Player)
                .Where(x => brandFilterSelections.Contains(x.PlayerBankAccount.Player.BrandId));

            var dataBuilder = new SearchPackageDataBuilder<OfflineWithdraw>(searchPackage, queryable);

            dataBuilder
                .SetFilterRule(x => x.PlayerBankAccount.Player.BrandId,
                    value => d => d.PlayerBankAccount.Player.BrandId == new Guid(value))
                .Map(obj => obj.Id, cellExpression);
            var data = dataBuilder.GetPageData(obj => obj.Created);
            return data;
        }
    }
}