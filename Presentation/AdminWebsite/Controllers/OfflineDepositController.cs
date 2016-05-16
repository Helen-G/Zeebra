using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.AdminWebsite.ViewModels;
using AFT.RegoV2.ApplicationServices.Payment;
using AFT.RegoV2.ApplicationServices.Player;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Domain.BoundedContexts.Payment.ApplicationServices.Exceptions;
using AFT.RegoV2.Domain.Payment.Commands;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Domain.Security.Interfaces;
using Newtonsoft.Json;
using ServiceStack.Validation;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    [Authorize]
    public class OfflineDepositController : BaseController
    {
        private readonly OfflineDepositCommands _offlineDepositCommands;
        private readonly PlayerQueries _queries;
        private readonly PaymentQueries _paymentQueries;
        private readonly IBonusQueries _bonusQueries;
        private readonly UserService _userService;
        private readonly ISecurityProvider _securityProvider;

        public string UserName
        {
            get { return Thread.CurrentPrincipal.Identity.Name; }
        }

        public OfflineDepositController(OfflineDepositCommands commands,
            PlayerQueries queries,
            PaymentQueries paymentQueries,
            IBonusQueries bonusQueries,
            UserService userService,
            ISecurityProvider securityProvider)
        {
            if (queries == null)
                throw new ArgumentNullException("queries");

            _offlineDepositCommands = commands;
            _queries = queries;
            _paymentQueries = paymentQueries;

            _bonusQueries = bonusQueries;
            _userService = userService;
            _securityProvider = securityProvider;
        }

        [SearchPackageFilter("searchPackage")]
        public ActionResult RequestedList(SearchPackage searchPackage)
        {
            var data = SearchOfflineDeposits(
               searchPackage,
               new[] { OfflineDepositStatus.New, OfflineDepositStatus.Unverified },
               obj =>
                   new[]
                    {
                        obj.Player.Username,
                        obj.TransactionNumber,
                        LabelHelper.LabelPaymentMethod(obj.PaymentMethod),
                        obj.CurrencyCode,
                        obj.Amount.ToString(CultureInfo.InvariantCulture),
                        obj.Status.ToString(),
                        obj.Brand.Name,
                        obj.Created.GetNormalizedDateTime(),
                        obj.BankAccount.AccountId,
                        obj.BankAccount.Bank.Name,
                        obj.BankAccount.Province,
                        obj.BankAccount.Branch,
                        obj.BankAccount.AccountName,
                        obj.BankAccount.AccountNumber
                    },
               _paymentQueries.GetOfflineDeposits());

            return new JsonResult { Data = data, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [SearchPackageFilter("searchPackage")]
        public ActionResult ApproveList(SearchPackage searchPackage)
        {
            var data = SearchOfflineDeposits(
                searchPackage,
                new[] { OfflineDepositStatus.Verified },
                obj =>
                    new[]
                    {
                        obj.Player.Username,
                        obj.TransactionNumber,
                        LabelHelper.LabelPaymentMethod(obj.PaymentMethod),
                        obj.CurrencyCode,
                        obj.Amount.ToString(CultureInfo.InvariantCulture),
                        obj.Status.ToString(),
                        obj.Brand.Name,
                        obj.Created.GetNormalizedDateTime(),
                        obj.VerifiedBy,
                        obj.DepositType.ToString(),
                        obj.BankAccount.AccountId,
                        obj.BankAccount.Bank.Name,
                        obj.BankAccount.Province,
                        obj.BankAccount.Branch,
                        obj.BankAccount.AccountNumber,
                        obj.BankAccount.AccountName
                    },
                _paymentQueries.GetDepositsAsVerified());
            return new JsonResult { Data = data, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [SearchPackageFilter("searchPackage")]
        public ActionResult VerifyList(SearchPackage searchPackage)
        {
            var data = SearchOfflineDeposits(
              searchPackage,
              new[] { OfflineDepositStatus.Processing },
              obj =>
                  new[] 
                  {
                      obj.Player.Username,
                      obj.Player.FirstName,
                      obj.Player.LastName,
                      obj.TransactionNumber,
                      LabelHelper.LabelPaymentMethod(obj.PaymentMethod),
                      obj.CurrencyCode,
                      obj.Amount.ToString(CultureInfo.InvariantCulture),
                      obj.Status.ToString(),
                      obj.Brand.Name,
                      obj.Created.GetNormalizedDateTime(),
                      obj.DepositType.ToString(),
                      obj.BankAccount.AccountId,
                      obj.BankAccount.Bank.Name,
                      obj.BankAccount.Province,
                      obj.BankAccount.Branch,
                      obj.BankAccount.AccountNumber,
                      obj.BankAccount.AccountName
                  },
              _paymentQueries.GetDepositsAsConfirmed());

            return new JsonResult { Data = data, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpGet]
        public ActionResult Get(Guid id)
        {
            try
            {
                var offlineDeposit = _paymentQueries.GetDepositById(id);
                return this.Success(new OfflineDepositViewModel(offlineDeposit));
            }
            catch (InvalidOperationException exception)
            {
                return this.Failed(exception);
            }
        }

        [HttpGet]
        public ActionResult GetForView(Guid id)
        {
            try
            {
                var offlineDeposit = _paymentQueries.GetDepositByIdForViewRequest(id);
                var viewModel = new OfflineDepositViewModel(offlineDeposit);
                if (string.IsNullOrWhiteSpace(offlineDeposit.BonusCode) == false)
                {
                    viewModel.BonusName = _bonusQueries.GetBonusName(offlineDeposit.BonusCode);
                }

                return this.Success(viewModel);
            }
            catch (InvalidOperationException exception)
            {
                return this.Failed(exception);
            }
        }

        // TODO Edit permission perhaps?
        [HttpGet]
        public ActionResult GetInfoForCreate(Guid playerId)
        {
            var player = _queries.GetPlayer(playerId);
            if (player == null)
            {
                throw new ArgumentException(@"Player was not found", "playerId");
            }

            var bankAccounts = _paymentQueries.GetBankAccountsForAdminOfflineDepositRequest(playerId);
            /*            if (bankAccounts == null)
                        {
                            throw new ArgumentException(@"Player payment level doesn't support offline deposits.");
                        }*/

            var bankData = bankAccounts
                .Select(o => new
                    {
                        Id = o.Id,
                        Name = String.Format("{0} / {1}", o.Bank.Name, o.AccountName),
                        Province = o.Province,
                        Branch = o.Branch,
                        AccountName = o.AccountName,
                        AccountNumber = o.AccountNumber,
                        BankName = o.Bank.Name,
                        AccountId = o.AccountId
                    });

            var qualifiedBonuses = _bonusQueries.GetOfflineDepositQualifiedBonuses(playerId);

            return this.Success(new
            {
                Banks = bankData,
                PlayerId = player.Id,
                player.Username,
                qualifiedBonuses
            });
        }

        [HttpPost]
        public dynamic Create(OfflineDepositRequest depositRequest)
        {
            if (ModelState.IsValid == false)
            {
                return this.Failed();
            }

            try
            {
                depositRequest.RequestedBy = _securityProvider.User.UserName;
                var offlineDeposit = _offlineDepositCommands.Submit(depositRequest);
                return this.Success(offlineDeposit.TransactionNumber);
            }
            catch (ValidationError ex)
            {
                return ValidationErrorResponse(ex);
            }
            catch (Exception exception)
            {
                return this.Failed(exception);
            }
        }

        [HttpGet]
        public ActionResult ViewRequestForConfirm(Guid id)
        {
            try
            {
                var offlineDeposit = _paymentQueries.GetDepositByIdForViewRequest(id);
                return this.Success(new OfflineDepositViewModel(offlineDeposit));
            }
            catch (InvalidOperationException exception)
            {
                return this.Failed(exception);
            }
        }

        [HttpGet]
        public ActionResult Confirm(Guid id)
        {
            try
            {
                var offlineDeposit = _paymentQueries.GetDepositByIdForConfirmation(id);
                return this.Success(new OfflineDepositViewModel(offlineDeposit));
            }
            catch (InvalidOperationException exception)
            {
                return this.Failed(exception);
            }
        }

        [HttpPost]
        public ActionResult ConfirmDeposit(FormCollection form)
        {
            var depositConfirm = form["depositConfirm"];

            var confirm = JsonConvert.DeserializeObject<OfflineDepositConfirm>(depositConfirm);
            var uploadId1 = base.Request.Files["uploadId1"];
            var uploadId2 = base.Request.Files["uploadId2"];
            var receiptUpLoad = base.Request.Files["receiptUpLoad"];
            confirm.IdFrontImage = uploadId1.GetFileName();
            confirm.IdBackImage = uploadId2.GetFileName();
            confirm.ReceiptImage = receiptUpLoad.GetFileName();
            try
            {
                return Confirm(confirm, uploadId1, uploadId2, receiptUpLoad);
            }
            catch (UnauthorizedAccessException)
            {
                // TODO Need to log this for administrator. Insert incident ID into log.
                return this.Failed(new { message = "app:common.systemError", incidentId = Guid.NewGuid() });
            }
            catch (Exception ex)
            {
                return this.Failed(new { message = ex.Message + " " + ex.InnerException, incidentId = Guid.NewGuid() });
            }
        }

        private ActionResult Confirm(OfflineDepositConfirm depositConfirm, HttpPostedFileBase idFrontImage, HttpPostedFileBase idBackImage, HttpPostedFileBase receiptImage)
        {
            if (ModelState.IsValid == false)
            {
                return this.Failed();
            }

            try
            {
                var offlineDeposit = _offlineDepositCommands.Confirm(
                    depositConfirm,
                    idFrontImage.GetBytes(),
                    idBackImage.GetBytes(),
                    receiptImage.GetBytes());

                return this.Success(new { Message = "app:payment.deposit.successfullyConfirmed", offlineDeposit.IdFrontImage, offlineDeposit.IdBackImage, offlineDeposit.ReceiptImage });
            }
            catch (PaymentSettingsViolatedException settingsViolatedException)
            {
                return this.Failed(settingsViolatedException);
            }
            catch (InvalidOperationException exception)
            {
                return this.Failed(exception);
            }
        }

        [HttpPost]
        public ActionResult Verify(Guid id, string remark)
        {
            try
            {
                _offlineDepositCommands.Verify(id, remark);
                return this.Success("app:payment.deposit.successfullyVerified");
            }
            catch (InvalidOperationException exception)
            {
                return this.Failed(exception);
            }
        }

        [HttpPost]
        public ActionResult Approve(OfflineDepositApprove approveCommand)
        {
            if (ModelState.IsValid == false)
            {
                return this.Failed();
            }

            try
            {
                _offlineDepositCommands.Approve(approveCommand);
                return this.Success("app:payment.deposit.successfullyApproved");
            }
            catch (PaymentSettingsViolatedException settingsViolatedException)
            {
                return this.Failed(settingsViolatedException);
            }
            catch (InvalidOperationException exception)
            {
                return this.Failed(exception);
            }
        }

        [HttpPost]
        public ActionResult Reject(Guid id, string remark)
        {
            try
            {
                _offlineDepositCommands.Reject(id, remark);
                return this.Success("app:payment.deposit.rejected");
            }
            catch (InvalidOperationException exception)
            {
                return this.Failed(exception);
            }
        }

        [HttpPost]
        public ActionResult Unverify(Guid id, string remark, UnverifyReasons unverifyReason)
        {
            try
            {
                _offlineDepositCommands.Unverify(id, remark, unverifyReason);
                return this.Success("app:payment.deposit.successfullyUnverified");
            }
            catch (InvalidOperationException exception)
            {
                return this.Failed(exception);
            }
        }

        private SearchPackageResult SearchOfflineDeposits(SearchPackage searchPackage, OfflineDepositStatus[] offlineDepositStatuses,
            Expression<Func<OfflineDeposit, object>> cellExpression, IQueryable<OfflineDeposit> queryable)
        {
            var brandFilterSelections = _userService.GetBrandFilterSelections(CurrentUser.UserId);

            var query = queryable.AsQueryable()
                .Include(p => p.Player)
                .Include(p => p.Brand)
                .Include(p => p.BankAccount.Bank)
                .Where(p => offlineDepositStatuses.Contains(p.Status) && brandFilterSelections.Contains(p.BrandId));

            var dataBuilder = new SearchPackageDataBuilder<OfflineDeposit>(searchPackage, query);

            dataBuilder.SetFilterRule(x => x.BrandId, value => d => d.BrandId == new Guid(value))
                .Map(obj => obj.Id, cellExpression);

            var data = dataBuilder.GetPageData(obj => obj.Created);

            return data;
        }
    }
}