using System;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http.Results;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.ApplicationServices.Payment;
using AFT.RegoV2.ApplicationServices.Player;
using AFT.RegoV2.ApplicationServices.Report;
using AFT.RegoV2.BoundedContexts.Report.Data;
using AFT.RegoV2.Core.ApplicationServices.Player;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Domain.Player.Data;
using AFT.RegoV2.Core.Game.ApplicationServices;
using AFT.RegoV2.Core.Player.Data;
using AFT.RegoV2.Core.Services.Security;
using AFT.RegoV2.Domain.BoundedContexts.Payment.Data;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Infrastructure.Extensions;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Constants;
using Newtonsoft.Json;
using ServiceStack.Validation;
using TransactionType = AFT.RegoV2.Core.Common.Data.Wallet.TransactionType;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    [Authorize]
    public class PlayerInfoController : BaseController
    {
        private const string DateFormat = "yyyy/MM/dd";

        private readonly WalletQueries _walletQueries;
        private readonly PlayerCommands _commands;
        private readonly BrandQueries _brandQueries;
        private readonly WithdrawalService _withdrawalService;
        private readonly PaymentQueries _paymentQueries;
        private readonly ReportQueries _reportQueries;
        private readonly PlayerQueries _queries;
        private readonly SecurityRepository _securityRepository;

        public PlayerInfoController(
            WalletQueries walletQueries,
            PlayerCommands commands,
            BrandQueries brandQueries,
            WithdrawalService withdrawalService,
            PaymentQueries paymentQueries,
            ReportQueries reportQueries,
            PlayerQueries queries,
            SecurityRepository securityRepository)
        {
            _walletQueries = walletQueries;
            _commands = commands;
            _brandQueries = brandQueries;
            _withdrawalService = withdrawalService;
            _paymentQueries = paymentQueries;
            _reportQueries = reportQueries;
            _queries = queries;
            _securityRepository = securityRepository;
        }

        [SearchPackageFilter("searchPackage")]
        public ActionResult DepositTransactions(SearchPackage searchPackage)
        {
            var query = _paymentQueries.GetOfflineDeposits()
                .Include(p => p.Player)
                .Include(p => p.Brand)
                .Include(p => p.BankAccount.Bank);
            var dataBuilder = new SearchPackageDataBuilder<OfflineDeposit>(searchPackage, query);

            dataBuilder = dataBuilder.SetFilterRule(x => x.PlayerId, (value) => p => p.PlayerId == new Guid(value));
            dataBuilder.Map(obj => obj.Id, od => new[]
                {
                    od.Created.GetNormalizedDateTime(),
                    od.TransactionNumber,
                    od.BankAccount.AccountNumber,
                    LabelHelper.LabelOfflineDepositType(od.DepositMethod),
                    od.Status == OfflineDepositStatus.Approved? od.ActualAmount.ToString(CultureInfo.InvariantCulture): od.Amount.ToString(CultureInfo.InvariantCulture),
                    od.Status.ToString()
                });
            var data = dataBuilder.GetPageData(obj => obj.Created);
            return new JsonResult { Data = data, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [SearchPackageFilter("searchPackage")]
        public ActionResult WithdrawTransactions(SearchPackage searchPackage)
        {
            var query = _paymentQueries.GetOfflineWithdraws()
                .Include(p => p.PlayerBankAccount.Player)
                .Where(x =>
                    x.Status == WithdrawalStatus.Approved ||
                    x.Status == WithdrawalStatus.Rejected ||
                    x.Status == WithdrawalStatus.Unverified ||
                    x.Status == WithdrawalStatus.Canceled);

            var dataBuilder = new SearchPackageDataBuilder<OfflineWithdraw>(searchPackage, query);

            dataBuilder = dataBuilder.SetFilterRule(x => x.PlayerBankAccount.Player.Id, (value) => p => p.PlayerBankAccount.Player.Id == new Guid(value));
            dataBuilder.Map(obj => obj.Id, od => new object[]
            {
                od.Created.LocalDateTime.GetNormalizedDateTime(),
                od.TransactionNumber,
                od.PlayerBankAccount.AccountNumber,
                Enum.GetName(typeof(PaymentMethod), od.PaymentMethod),
                od.Amount.ToString(CultureInfo.InvariantCulture),
                od.Status
            });

            var data = dataBuilder.GetPageData(obj => obj.Created);
            return new JsonResult { Data = data, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [SearchPackageFilter("searchPackage")]
        public ActionResult TrnasferFundTransfers(SearchPackage searchPackage)
        {
            var query = _paymentQueries.GetTransferFunds()
                //.Include(p => p.PlayerBankAccount.Player)
                .Where(x =>
                    //x.CreatedBy == searchPackage.
                    x.Status == TransferFundStatus.Approved ||
                    x.Status == TransferFundStatus.Rejected);

            var dataBuilder = new SearchPackageDataBuilder<TransferFund>(searchPackage, query);

            dataBuilder = dataBuilder.SetFilterRule(x => x.CreatedBy, (value) => p => p.CreatedBy == value);

            dataBuilder.Map(obj => obj.Id, od => new object[]
            {
                od.TransactionNumber,
                LabelHelper.LabelTransferType(od.TransferType),
                _brandQueries.GetWalletTemplate(new Guid(od.WalletId)).Name,
                od.Amount.ToString(CultureInfo.InvariantCulture),
                LabelHelper.LabelTransferStatus(od.Status),
                od.Created.LocalDateTime.GetNormalizedDateTime(),
                od.Remarks
            });

            var data = dataBuilder.GetPageData(obj => obj.Created);
            return new JsonResult { Data = data, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [SearchPackageFilter("searchPackage")]
        public ActionResult Transactions(SearchPackage searchPackage, Guid playerId)
        {
            var dataBuilder = new SearchPackageDataBuilder<PlayerTransactionRecord>(searchPackage, _reportQueries.GetPlayerTransactionRecords(playerId));
            var data = dataBuilder
                .Map(r => r.TransactionId, r => new object[]
                {
                    r.Type,
                    r.MainBalanceAmount + r.BonusBalanceAmount + r.TemporaryBalanceAmount 
                        + r.LockBonusAmount + r.LockFraudAmount + r.LockWithdrawalAmount,
                    r.MainBalance + r.BonusBalance + r.TemporaryBalance,
                    r.CreatedOn,
                    r.Wallet,
                    r.CurrencyCode,
                    r.Description,
                    r.PerformedBy,
                    r.TransactionNumber
                })
                .GetPageData(r => r.TransactionId);
            return new JsonResult { Data = data, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [SearchPackageFilter("searchPackage")]
        public ActionResult ActivityLog(SearchPackage searchPackage)
        {
            var query = _queries.GetPlayerActivityLog();

            var dataBuilder = new SearchPackageDataBuilder<PlayerActivityLog>(searchPackage, query);

            dataBuilder.Map(obj => obj.Id, od => new object[]
            {
                od.Category,
                od.ActivityDone,
                od.PerformedBy,
                od.DatePerformed.LocalDateTime.GetNormalizedDate(),
                od.Remarks,
                od.UpdatedBy,
                od.DateUpdated.HasValue ? od.DateUpdated.Value.LocalDateTime.GetNormalizedDate() : null
            });

            var data = dataBuilder.GetPageData(obj => obj.DatePerformed);
            return new JsonResult { Data = data, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpGet]
        [SearchPackageFilter("searchPackage")]
        public object IdentityVerification(SearchPackage searchPackage, Guid playerId)
        {
            var identityVerifications = _queries.GetPlayerIdentityVerifications(playerId);

            var dataBuilder = new SearchPackageDataBuilder<IdentityVerification>(searchPackage, identityVerifications);

            dataBuilder.Map(obj => obj.Id, obj => new object[]
            {
                obj.DocumentType.ToString(),
                obj.CardNumber,
                Format.FormatDate(obj.ExpirationDate, false),
                obj.VerificationStatus.ToString(),
                obj.VerifiedBy,
                Format.FormatDate(obj.DateVerified, false),
                obj.UnverifiedBy,
                Format.FormatDate(obj.DateUnverified, false),
                obj.UploadedBy,
                Format.FormatDate(obj.DateUploaded, false)
            });

            var data = dataBuilder.GetPageData(obj => obj.DocumentType);

            var result = new JsonResult
            {
                Data = data,
                MaxJsonLength = int.MaxValue,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            return result;
        }

        public ActionResult EditLogRemark(Guid id, string remarks)
        {
            try
            {
                _commands.UpdateLogRemark(id, remarks);
            }
            catch (RegoException exception)
            {
                return this.Failed(exception);
            }

            return this.Success();
        }

        public async Task<ActionResult> GetBalances(Guid playerId)
        {
            var player = await _queries.GetPlayerAsync(playerId);
            var wallet = await _walletQueries.GetPlayerBalanceAsync(playerId);
            var offlineDeposits = _paymentQueries.GetOfflineDeposits().Where(x => x.PlayerId == playerId);
            var offlineDepositsWithoutWageringCompleted = await offlineDeposits.Where(x => x.DepositWagering != 0).ToListAsync();
            var playerBetStatistics = await _queries.GetPlayerBetStatistics().FirstOrDefaultAsync(s => s.PlayerId == playerId);

            decimal totalWin = 0;
            decimal totalLoss = 0;
            decimal totalAdjustments = 0;

            if (playerBetStatistics != null)
            {
                totalWin = playerBetStatistics.TotalWon;
                totalLoss = playerBetStatistics.TotalLoss;
                totalAdjustments = playerBetStatistics.TotlAdjusted;
            }

            return Json(new
            {
                Balance = new
                {
                    Currency = player.CurrencyCode,
                    MainBalance = wallet.Main,
                    BonusBalance = wallet.Bonus,
                    PlayableBalance = wallet.Playable,
                    FreeBalance = wallet.Free,
                    TotalBonus = "-",
                    DepositCount = offlineDeposits.Count(),
                    TotalDeposit = "-",
                    WithdrawalCount = "-",
                    TotalWithdrawal = "-",
                    TotalWin = totalWin,
                    TotalLoss = totalLoss,
                    TotalAdjustments = totalAdjustments,
                    TotalCreditsRefund = "-",
                    TotalCreditsCancellation = "-",
                    TotalChargeback = "-",
                    TotalChargebackReversals = "-",
                    TotalWager = "-",
                    AverageWagering = "-",
                    AverageDeposit = "-",
                    MaxBalance = "-"
                },

                GameBalance = new
                {
                    Product = "-",
                    Balance = "-",
                    BonusBalance = "-",
                    BettingBalance = "-",
                    TotalBonus = "-"
                },

                DepositWagering = new
                {
                    TotalWagering = offlineDepositsWithoutWageringCompleted.Any() ? offlineDepositsWithoutWageringCompleted.Select(x => x.Amount).Aggregate((x, y) => x + y) : 0,
                    WageringRequired = offlineDepositsWithoutWageringCompleted.Any() ? offlineDepositsWithoutWageringCompleted.Select(x => x.DepositWagering).Aggregate((x, y) => x + y) : 0,
                },

            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetTransactionTypes()
        {
            var transactionTypeNames = Enum.GetNames(typeof(TransactionType)).ToList();

            return Json(transactionTypeNames.Select(name => new
            {
                Name = name
            }), JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetWalletTemplates(Guid playerId)
        {
            var player = await _queries.GetPlayerAsync(playerId);
            var walletTemplates = _brandQueries.GetWalletTemplates(player.BrandId);

            return Json(walletTemplates.Select(w => new
            {
                w.Id,
                w.Name
            }), JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetPlayerWallets(Guid playerId)
        {
            var player = await _queries.GetPlayerAsync(playerId);

            var walletTemplateIDs = await
                 _walletQueries.GetProductWalletsOfPlayer(playerId).Select(x => x.Template.Id).ToArrayAsync();

            var playerWallets =
                _brandQueries.GetWalletTemplates(player.BrandId)
                    .Where(wallet => !wallet.IsMain && walletTemplateIDs.Contains(wallet.Id));

            return Json(playerWallets.Select(w => new
            {
                w.Id,
                w.Name
            }), JsonRequestBehavior.AllowGet);

        }

        public async Task<ActionResult> GetStatus(Guid playerId)
        {
            // do not lock
            using (var scope = CustomTransactionScope.GetTransactionScopeAsync(IsolationLevel.ReadUncommitted))
            {
                var player = await _queries.GetPlayerAsync(playerId);
                scope.Complete();
                return Json(player.IsOnline, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<ActionResult> GetPlayerTitle(Guid id)
        {
            var player = await _queries.GetPlayerAsync(id);

            return Json(new
            {
                player.Username,
                player.FirstName,
                player.LastName
            }, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public string GetIdentificationDocumentEditData(Guid id)
        {
            var player = _paymentQueries.GetPlayerHelper(id, p => p.Include(o => o.Brand.Licensee));

            return SerializeJson(new
            {
                Username = player.Username,
                BrandName = player.Brand.Name,
                LicenseeName = player.Brand.LicenseeName,
                DocumentTypes = Enum.GetNames(typeof(DocumentType))
            });
        }

        [Authorize]
        [HttpPost]
        public string UploadId()
        {
            var request = System.Web.HttpContext.Current.Request;

            var data = request.Form["data"];
            var playerId = Guid.Parse(request.Form["playerId"]);

            var uploadData = JsonConvert.DeserializeObject<IdUploadData>(data);
            var uploadIdFront = request.Files["uploadId1"];
            var uploadIdBack = request.Files["uploadId2"];

            var frontFileName = uploadIdFront != null ? uploadIdFront.FileName : null;
            var backFileName = uploadIdBack != null ? uploadIdBack.FileName : null;

            uploadData.FrontIdFile = uploadIdFront != null ? uploadIdFront.InputStream.ToByteArray() : null;
            uploadData.BackIdFile = uploadIdBack != null ? uploadIdBack.InputStream.ToByteArray() : null;
            uploadData.FrontName = frontFileName;
            uploadData.BackName = backFileName;

            IdentityVerification identity;
            try
            {
                identity = _commands.UploadIdentificationDocuments(uploadData, playerId, CurrentUser.UserName);
            }
            catch (Exception ex)
            {
                return SerializeJson(new { Result = "failed", Data = ex.Message });
            }

            return SerializeJson(new
            {
                Result = "success",
                Data = new
                {
                    FrontIdFilename = identity.FrontFilename,
                    BackIdFilename = identity.BackFilename
                }
            });
        }

        public IQueryable GetLicensees()
        {
            var licensees = _brandQueries.GetLicensees();

            var user = _securityRepository.Users
                .Include(x => x.Role)
                .Include(x => x.Licensees)
                .Single(u => u.Id == CurrentUser.UserId);

            if (user.Role.Id == RoleIds.LicenseeId || user.Role.Id == RoleIds.SingleBrandManagerId)
            {
                licensees = licensees
                    .Where(l => user.Licensees.Any(x => l.Id == x.Id) && l.Status == LicenseeStatus.Active)
                    .AsQueryable();
            }

            return licensees
                .OrderBy(l => l.Name)
                .Select(l => new { l.Name, l.Id });
        }

        public async Task<ActionResult> Get(Guid id)
        {
            var paymentPlayer = _paymentQueries.GetPlayer(id);

            var player = await _queries.GetPlayerAsync(id);
            var paymentId = await _paymentQueries.GetPlayerPaymentLevels().Include(p => p.PaymentLevel).SingleAsync(p => p.PlayerId == id);
            var brand = await _brandQueries.GetBrandOrNullAsync(player.BrandId);

            var brandTimeZone = TimeZoneInfo.FindSystemTimeZoneById(brand.TimezoneId);
            var now = DateTimeOffset.Now;
            var exemptFrom = paymentPlayer.ExemptWithdrawalFrom.HasValue ? paymentPlayer.ExemptWithdrawalFrom.Value : now;
            var exemptTo = paymentPlayer.ExemptWithdrawalTo.HasValue ? paymentPlayer.ExemptWithdrawalTo.Value : now;
            var exemptFromText = TimeZoneInfo.ConvertTime(exemptFrom, brandTimeZone).ToString(DateFormat);
            var exemptToText = TimeZoneInfo.ConvertTime(exemptTo, brandTimeZone).ToString(DateFormat);

            var question = await _queries.GetSecurityQuestions().SingleOrDefaultAsync(x => x.Id == player.SecurityQuestionId);

            return Json(new
            {
                player.BrandId,
                player.Username,
                player.FirstName,
                player.LastName,
                DateOfBirth = Format.FormatDate(player.DateOfBirth, false),
                ParsedDateOfBirth = player.DateOfBirth.ToString("MMMM d, y"),
                Title = Enum.GetName(typeof(Title), player.Title),
                Gender = Enum.GetName(typeof(Gender), player.Gender),
                player.Email,
                player.PhoneNumber,
                player.MailingAddressLine1,
                player.MailingAddressLine2,
                player.MailingAddressLine3,
                player.MailingAddressLine4,
                player.MailingAddressCity,
                player.MailingAddressPostalCode,
                player.PhysicalAddressLine1,
                player.PhysicalAddressLine2,
                player.PhysicalAddressLine3,
                player.PhysicalAddressLine4,
                player.PhysicalAddressCity,
                player.PhysicalAddressPostalCode,
                player.CountryCode,
                PaymentLevel = paymentId != null ? (Guid?)paymentId.PaymentLevel.Id : null,
                ContactPreference = Enum.GetName(typeof(ContactMethod), player.ContactPreference),
                DateRegistered = player.DateRegistered.ToString(DateFormat),
                Brand = brand.Name,
                player.CurrencyCode,
                VipLevel = player.VipLevel == null ? (Guid?)null : player.VipLevel.Id,
                Active = player.AccountStatus == AccountStatus.Active,
                ExemptWithdrawalVerification = paymentPlayer.ExemptWithdrawalVerification.HasValue && paymentPlayer.ExemptWithdrawalVerification.Value,
                ExemptFrom = exemptFromText,
                ExemptTo = exemptToText,
                ExemptLimit = paymentPlayer.ExemptLimit.HasValue ? paymentPlayer.ExemptLimit.Value : -1,
                SecurityQuestion = question != null ? question.Question : "",
                player.SecurityAnswer,
                player.IsOnline,
                player.AccountAlertEmail,
                player.AccountAlertSms,
                player.MarketingAlertEmail,
                player.MarketingAlertSms,
                player.MarketingAlertPhone
            }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetExemptionData(Guid id)
        {
            var paymentPlayer = _paymentQueries.GetPlayer(id);

            var player = await _queries.GetPlayerAsync(id);
            var brand = await _brandQueries.GetBrandOrNullAsync(player.BrandId);

            var brandTimeZone = TimeZoneInfo.FindSystemTimeZoneById(brand.TimezoneId);
            var now = DateTimeOffset.Now;
            var exemptFrom = paymentPlayer.ExemptWithdrawalFrom ?? now;
            var exemptTo = paymentPlayer.ExemptWithdrawalTo ?? now;
            var exemptFromText = TimeZoneInfo.ConvertTime(exemptFrom, brandTimeZone).ToString(DateFormat);
            var exemptToText = TimeZoneInfo.ConvertTime(exemptTo, brandTimeZone).ToString(DateFormat);

            return Json(new
            {
                ExemptWithdrawalVerification = paymentPlayer.ExemptWithdrawalVerification.HasValue && paymentPlayer.ExemptWithdrawalVerification.Value,
                ExemptFrom = exemptFromText,
                ExemptTo = exemptToText,
                ExemptLimit = paymentPlayer.ExemptLimit ?? -1,
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public string Edit(EditPlayerCommand command)
        {
            if (ModelState.IsValid == false)
            {
                return ErrorResponse();
            }

            try
            {
                var edit = new EditPlayerData
                {
                    PlayerId = command.PlayerId,
                    FirstName = command.FirstName,
                    LastName = command.LastName,
                    DateOfBirth = command.DateOfBirth,
                    Title = command.Title,
                    Gender = command.Gender,
                    Email = command.Email,
                    PhoneNumber = command.PhoneNumber,
                    MailingAddressLine1 = command.MailingAddressLine1,
                    MailingAddressLine2 = command.MailingAddressLine2,
                    MailingAddressLine3 = command.MailingAddressLine3,
                    MailingAddressLine4 = command.MailingAddressLine4,
                    MailingAddressCity = command.MailingAddressCity,
                    MailingAddressPostalCode = command.MailingAddressPostalCode,
                    PhysicalAddressLine1 = command.PhysicalAddressLine1,
                    PhysicalAddressLine2 = command.PhysicalAddressLine2,
                    PhysicalAddressLine3 = command.PhysicalAddressLine3,
                    PhysicalAddressLine4 = command.PhysicalAddressLine4,
                    PhysicalAddressCity = command.PhysicalAddressCity,
                    PhysicalAddressPostalCode = command.PhysicalAddressPostalCode,
                    CountryCode = command.CountryCode,
                    ContactPreference = command.ContactPreference,
                    AccountAlertEmail = command.AccountAlertEmail,
                    AccountAlertSms = command.AccountAlertSms,
                    PaymentLevelId = command.PaymentLevelId,
                    MarketingAlertEmail = command.MarketingAlertEmail,
                    MarketingAlertSms = command.MarketingAlertSms,
                    MarketingAlertPhone = command.MarketingAlertPhone
                };

                _commands.Edit(edit);

                return SerializeJson(new { Result = "success" });
            }
            catch (ValidationError e)
            {
                return ValidationErrorResponse(e);
            }
            catch (Exception exception)
            {
                return SerializeJson(new { Result = "failed", Data = exception.Message });
            }
        }

        [HttpPost]
        public string SubmitExemption(Exemption exemption)
        {
            if (!ModelState.IsValid)
            {
                return ErrorResponse();
            }

            try
            {
                _withdrawalService.SaveExemption(exemption);

                return SerializeJson(new { Result = "success" });
            }
            catch (ValidationError e)
            {
                return ValidationErrorResponse(e);
            }
            catch (Exception exception)
            {
                return SerializeJson(new { Result = "failed", Data = exception.Message });
            }
        }

        [HttpPost]
        public string SetStatus(Guid id, bool active)
        {
            try
            {
                _commands.SetStatus(id, active);

                return SerializeJson(new { Result = "success", active = active });
            }
            catch (Exception exception)
            {
                return SerializeJson(new { Result = "failed", Data = exception.Message });
            }
        }

        [Authorize]
        [HttpPost]
        public string VerifyIdDocument(Guid id)
        {
            try
            {
                _commands.VerifyIdDocument(id, CurrentUser.UserName);
            }
            catch (Exception exception)
            {
                return SerializeJson(new { Result = "failed", Data = exception.Message });
            }

            return SerializeJson(new
            {
                result = "success"
            });
        }

        [HttpPost]
        public string UnverifyIdDocument(Guid id)
        {
            try
            {
                _commands.UnverifyIdDocument(id, CurrentUser.UserName);
            }
            catch (Exception exception)
            {
                return SerializeJson(new { Result = "failed", Data = exception.Message });
            }

            return SerializeJson(new
            {
                result = "success"
            });
        }

        [HttpPost]
        public string ResendActivationEmail(Guid id)
        {
            try
            {
                _commands.ResendActivationEmail(id);

                return SerializeJson(new { Result = "success" });
            }
            catch (Exception exception)
            {
                return SerializeJson(new { Result = "failed", Data = exception.Message });
            }
        }
    }
}