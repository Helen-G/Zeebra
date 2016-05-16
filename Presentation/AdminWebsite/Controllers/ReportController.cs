using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.ApplicationServices.Payment;
using AFT.RegoV2.ApplicationServices.Report;
using AFT.RegoV2.BoundedContexts.Report.Data;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Game.ApplicationServices;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Report.Data;
using AFT.RegoV2.Core.Report.Data.Brand;
using AFT.RegoV2.Core.Report.Events;
using AFT.RegoV2.Domain.BoundedContexts.Report.Attributes;
using AFT.RegoV2.Shared;
using ServiceStack.Text;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    [Authorize]
    public class ReportController : BaseController
    {
        private readonly ReportQueries _queries;
        private readonly BrandQueries _brandQueries;
        private readonly IGameQueries _gameQueries;
        private readonly PaymentQueries _paymentQueries;
        private readonly IEventBus _eventBus;

        public ReportController(
            ReportQueries queries,
            BrandQueries brandQueries,
            GameQueries gameQueries,
            PaymentQueries paymentQueries,
            IEventBus eventBus)
        {
            _queries = queries;
            _brandQueries = brandQueries;
            _gameQueries = gameQueries;
            _paymentQueries = paymentQueries;
            _eventBus = eventBus;
        }

        #region Player Reports
        [HttpGet, SearchPackageFilter("searchPackage")]
        public JsonResult PlayerData(SearchPackage searchPackage)
        {
            var dataBuilder = new SearchPackageDataBuilder<PlayerRecord>(searchPackage, _queries.GetPlayerRecords());
            var data = dataBuilder
                .Map(r => r.PlayerId, r => new object[]
                {
                    r.Licensee,
                    r.Brand,
                    r.Username,
                    r.Mobile,
                    r.Email,
                    r.Birthday,
                    r.IsInternalAccount,
                    r.RegistrationDate,
                    r.PlayerStatus,
                    r.Language,
                    r.Currency,
                    r.SignUpIP,
                    r.VipLevel,
                    r.Country,
                    r.PlayerName,
                    r.Title,
                    r.Gender,
                    r.StreetAddress,
                    r.PostCode,
                    r.Created,
                    r.CreatedBy,
                    r.Updated,
                    r.UpdatedBy,
                    r.Activated,
                    r.ActivatedBy,
                    r.Deactivated,
                    r.DeactivatedBy
                })
                .GetPageData(r => r.RegistrationDate);
            return new JsonResult { Data = data, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpGet]
        public ActionResult ExportPlayerReport(PlayerRecord filters, string sortColumnName, string sortOrder, string hiddenColumns = null)
        {
            return ExportReport(_queries.GetPlayerRecordsForExport(), filters, sortColumnName, sortOrder, hiddenColumns);
        }

        [HttpGet, SearchPackageFilter("searchPackage")]
        public JsonResult PlayerBetHistoryData(SearchPackage searchPackage)
        {
            var dataBuilder = new SearchPackageDataBuilder<PlayerBetHistoryRecord>(searchPackage, _queries.GetPlayerBetHistoryRecords());
            var data = dataBuilder
                .Map(r => r.RoundId, r => new object[]
                {
                    r.Licensee,
                    r.Brand,
                    r.RoundId,
                    r.LoginName,
                    r.UserIP,
                    r.GameName,
                    r.DateBet,
                    r.BetAmount,
                    r.TotalWinLoss,
                    r.Currency
                })
                .GetPageData(r => r.DateBet);
            return new JsonResult { Data = data, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpGet]
        public ActionResult ExportPlayerBetHistoryReport(PlayerBetHistoryRecord filters, string sortColumnName, string sortOrder, string hiddenColumns = null)
        {
            return ExportReport(_queries.GetPlayerBetHistoryRecordsForExport(), filters, sortColumnName, sortOrder, hiddenColumns);
        }

        #endregion
        #region Payment Reports
        [HttpGet, SearchPackageFilter("searchPackage")]
        public JsonResult DepositData(SearchPackage searchPackage)
        {
            var dataBuilder = new SearchPackageDataBuilder<DepositRecord>(searchPackage, _queries.GetDepositRecords());
            var data = dataBuilder
                .Map(r => r.DepositId, r => new object[]
                {
                    r.Licensee,
                    r.Brand,
                    r.Username,
                    r.IsInternalAccount,
                    r.VipLevel,
                    r.TransactionId,
                    r.DepositId,
                    r.PaymentMethod,
                    r.Currency,
                    r.Amount,
                    r.ActualAmount,
                    r.Fee,
                    r.Status,
                    r.Submitted,
                    r.SubmittedBy,
                    r.Approved,
                    r.ApprovedBy,
                    r.Rejected,
                    r.RejectedBy,
                    r.Verified,
                    r.VerifiedBy,
                    r.DepositType,
                    r.BankAccountName,
                    r.BankAccountId,
                    r.BankName,
                    r.BankProvince,
                    r.BankBranch,
                    r.BankAccountNumber
                })
                .GetPageData(r => r.Submitted);
            return new JsonResult { Data = data, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpGet]
        public ActionResult ExportDepositReport(DepositRecord filters, string sortColumnName, string sortOrder, string hiddenColumns = null)
        {
            return ExportReport(_queries.GetDepositRecordsForExport(), filters, sortColumnName, sortOrder, hiddenColumns);
        }

        #endregion
        #region Brand Reports
        [HttpGet, SearchPackageFilter("searchPackage")]
        public JsonResult BrandData(SearchPackage searchPackage)
        {
            var dataBuilder = new SearchPackageDataBuilder<BrandRecord>(searchPackage, _queries.GetBrandRecords());
            var data = dataBuilder
                .Map(r => r.BrandId, r => new object[]
                {
                    r.Licensee,
                    r.BrandCode,
                    r.Brand,
                    r.BrandType,
                    r.PlayerPrefix,
                    r.AllowedInternalAccountsNumber,
                    r.BrandStatus,
                    r.BrandTimeZone,
                    r.CreatedBy,
                    r.Created,
                    r.UpdatedBy,
                    r.Updated,
                    r.ActivatedBy,
                    r.Activated,
                    r.DeactivatedBy,
                    r.Deactivated,
                    r.Remarks
                })
                .GetPageData(r => r.Created);
            return new JsonResult { Data = data, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpGet]
        public ActionResult ExportBrandReport(BrandRecord filters, string sortColumnName, string sortOrder, string hiddenColumns = null)
        {
            return ExportReport(_queries.GetBrandRecordsForExport(), filters, sortColumnName, sortOrder, hiddenColumns);
        }

        [HttpGet, SearchPackageFilter("searchPackage")]
        public JsonResult LicenseeData(SearchPackage searchPackage)
        {
            var dataBuilder = new SearchPackageDataBuilder<LicenseeRecord>(searchPackage, _queries.GetLicenseeRecords());
            var data = dataBuilder
                .Map(r => r.LicenseeId, r => new object[]
                {
                    r.Name,
                    r.CompanyName,
                    r.EmailAddress,
                    r.AffiliateSystem,
                    r.ContractStart,
                    r.ContractEnd,
                    r.Status,
                    r.CreatedBy,
                    r.Created,
                    r.UpdatedBy,
                    r.Updated,
                    r.ActivatedBy,
                    r.Activated,
                    r.DeactivatedBy,
                    r.Deactivated
                })
                .GetPageData(r => r.Created);
            return new JsonResult { Data = data, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpGet]
        public ActionResult ExportLicenseeReport(LicenseeRecord filters, string sortColumnName, string sortOrder, string hiddenColumns = null)
        {
            return ExportReport(_queries.GetLicenseeRecordsForExport(), filters, sortColumnName, sortOrder, hiddenColumns);
        }

        [HttpGet, SearchPackageFilter("searchPackage")]
        public JsonResult LanguageData(SearchPackage searchPackage)
        {
            var dataBuilder = new SearchPackageDataBuilder<LanguageRecord>(searchPackage, _queries.GetLanguageRecords());
            var data = dataBuilder
                .Map(r => r.Code, r => new object[]
                {
                    r.Code,
                    r.Name,
                    r.NativeName,
                    r.Status,
                    r.Licensee,
                    r.Brand,
                    r.CreatedBy,
                    r.Created,
                    r.UpdatedBy,
                    r.Updated,
                    r.ActivatedBy,
                    r.Activated,
                    r.DeactivatedBy,
                    r.Deactivated
                })
                .GetPageData(r => r.Created);
            return new JsonResult { Data = data, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpGet]
        public ActionResult ExportLanguageReport(LanguageRecord filters, string sortColumnName, string sortOrder, string hiddenColumns = null)
        {
            return ExportReport(_queries.GetLanguageRecordsForExport(), filters, sortColumnName, sortOrder, hiddenColumns);
        }

        [HttpGet, SearchPackageFilter("searchPackage")]
        public JsonResult VipLevelData(SearchPackage searchPackage)
        {
            var dataBuilder = new SearchPackageDataBuilder<VipLevelRecord>(searchPackage, _queries.GetVipLevelRecords());
            var data = dataBuilder
                .Map(r => r.VipLevelId, r => new object[]
                {
                    r.Licensee,
                    r.Brand,
                    r.Code,
                    r.Rank,
                    r.Status,
                    r.GameProvider,
                    r.Currency,
                    r.BetLevel,
                    r.CreatedBy,
                    r.Created,
                    r.UpdatedBy,
                    r.Updated,
                    r.ActivatedBy,
                    r.Activated,
                    r.DeactivatedBy,
                    r.Deactivated
                })
                .GetPageData(r => r.Created);
            return new JsonResult { Data = data, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpGet]
        public ActionResult ExportVipLevelReport(VipLevelRecord filters, string sortColumnName, string sortOrder, string hiddenColumns = null)
        {
            return ExportReport(_queries.GetVipLevelRecordsForExport(), filters, sortColumnName, sortOrder, hiddenColumns);
        }

        #endregion
        #region Lists

        public JsonResult LanguageList()
        {
            var cultures = _brandQueries.GetCultures().OrderBy(c => c.Name).Select(c => c.Name);
            return Json(cultures, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LanguageCodeList()
        {
            var cultures = _brandQueries.GetCultures().OrderBy(c => c.Code).Select(c => c.Code);
            return Json(cultures, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CurrencyList()
        {
            var currencies = _brandQueries.GetBrands().Select(brand => brand.Id).ToList()
                .SelectMany(brandId => _brandQueries.GetCurrenciesByBrand(brandId)).Distinct()
                .OrderBy(currency => currency.Code)
                .Select(currency => currency.Code);
            return Json(currencies, JsonRequestBehavior.AllowGet);
        }
        public JsonResult CountryList()
        {
            var countries = _brandQueries.GetCountries().OrderBy(c => c.Name).Select(c => c.Name);
            return Json(countries, JsonRequestBehavior.AllowGet);
        }

        public JsonResult VipLevelList()
        {
            var vipLevels = _brandQueries.GetVipLevels().OrderBy(vl => vl.Code).Select(vl => vl.Code);
            return Json(vipLevels, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ProductList()
        {
            var products = _gameQueries.GetGameProviders().OrderBy(p => p.Name).Select(p => p.Name);
            return Json(products, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BankList()
        {
            var banks = _brandQueries.GetBrands().Select(brand => brand.Id).ToList()
                .SelectMany(brandId => _paymentQueries.GetBanksByBrand(brandId)).Distinct()
                .OrderBy(bank => bank.Name)
                .Select(bank => bank.Name);
            return Json(banks, JsonRequestBehavior.AllowGet);
        }

        public JsonResult TimeZoneList()
        {
            var timeZones = TimeZoneInfo.GetSystemTimeZones().Select(tz => tz.DisplayName);
            return Json(timeZones, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GameProviderList()
        {
            var gameProviders = _gameQueries.GetGameProviderDtos().OrderBy(gs => gs.Name).Select(gs => gs.Name);
            return Json(gameProviders, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LicenseeList()
        {
            var licensees = _brandQueries.GetLicensees().OrderBy(l => l.Name).Select(l => l.Name);
            return Json(licensees, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BrandList()
        {
            var brands = _brandQueries.GetBrands().OrderBy(b => b.Name).Select(b => b.Name);
            return Json(brands, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Common
        public ActionResult ExportReport<T>(IQueryable<T> records, T filters, string sortColumnName, string sortOrder, string hiddenColumns)
        {
            try
            {
                var filteredRecords = FilterAndOrder(records, filters, sortColumnName, sortOrder);
                var bytes = ExportToExcel(filteredRecords, hiddenColumns != null ? hiddenColumns.Split(',') : null);
                var reportName = typeof (T).Name.Replace("Record", "Report");
                var outputFilename = reportName + ".xls";
                _eventBus.Publish(new ReportExported(reportName, ExportFormat.Excel));
                return File(bytes, "binary/octet-stream", outputFilename);
            }
            catch (RegoException exception)
            {
                return this.Failed(exception);
            }
        }

        public static List<T> FilterAndOrder<T>(IQueryable<T> records, T filters, string sortColumnName, string sortOrder)
        {
            var filterDict = new Dictionary<string, string>();
            var fields = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var keyField = fields.First();
            foreach (var field in fields)
            {
                var filterValue = field.GetMethod.Invoke(filters, new object[0]);
                var filterTypeDefaultValue = field.PropertyType.GetDefaultValue();
                if (filterValue != null &&
                    (filterTypeDefaultValue == null || filterValue.ToString() != filterTypeDefaultValue.ToString()))
                {
                    filterDict.Add(field.Name, filterValue.ToString());
                }
            }
            SearchPackage searchPackage = jqGridHelper.GetExportSearchPackage(filterDict, sortColumnName, sortOrder);

            var dataBuilder = new SearchPackageDataBuilder<T>(searchPackage, records);
            var param = Expression.Parameter(typeof(T), "r");
            var keyFieldExpr =
                Expression.Lambda<Func<T, object>>(
                    Expression.Convert(Expression.Property(param, keyField), typeof(object)),
                    param);
            var sortFieldExpr =
                Expression.Lambda<Func<T, object>>(
                    Expression.Convert(Expression.Property(param, sortColumnName ?? keyField.Name), typeof(object)),
                    param);
            return dataBuilder
                .Map(keyFieldExpr, r => fields.Select(f => f.GetMethod.Invoke(r, new object[0])).ToArray())
                .GetExportResult(sortFieldExpr);
        }

        public static byte[] ExportToExcel<T>(IEnumerable<T> data, string[] hiddenColumns = null)
        {
            var grid = new GridView();
            grid.DataSource = data;
            var fields = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            grid.RowCreated += (dsender, e) =>
            {
                if (e.Row.RowType == DataControlRowType.Header)
                {
                    int i = 0;
                    foreach (TableCell cell in e.Row.Cells)
                    {
                        var field = fields[i++];
                        var exportAttr = (ExportAttribute)field.GetCustomAttribute(typeof(ExportAttribute));
                        if (exportAttr != null && (hiddenColumns == null || !hiddenColumns.Contains(field.Name)))
                        {
                            cell.BackColor = Color.LightGray;
                            cell.Text = exportAttr.ColumnName ?? field.Name;
                        }
                        else
                        {
                            ((DataControlFieldCell)cell).ContainingField.Visible = false;
                        }
                    }
                }
                else if (e.Row.RowType == DataControlRowType.DataRow)
                {
                    foreach (TableCell cell in e.Row.Cells)
                    {
                        cell.HorizontalAlign = HorizontalAlign.Left;
                    }
                }
            };

            grid.RowDataBound += (sender, e) =>
            {
                if (e.Row.RowType == DataControlRowType.DataRow)
                {
                    for (var i = 0; i < fields.Count(); i++)
                    {
                        if (String.IsNullOrWhiteSpace(e.Row.Cells[i].Text.Replace("&nbsp;", " ")))
                        {
                            continue;
                        }
                        if (fields[i].PropertyType == typeof(DateTimeOffset) ||
                            fields[i].PropertyType == typeof(DateTimeOffset?))
                        {
                            e.Row.Cells[i].Text = DateTimeOffset.Parse(e.Row.Cells[i].Text).ToString("yyyy-MM-dd");
                        }
                        if (fields[i].PropertyType == typeof(Decimal) ||
                            fields[i].PropertyType == typeof(Decimal?))
                        {
                            e.Row.Cells[i].Text = ((Double) Decimal.Parse(e.Row.Cells[i].Text)).ToString("");
                        }
                    }
                }
            };

            grid.DataBind();

            for (var i = 0; i < fields.Count(); i++)
            {
                if (fields[i].PropertyType == typeof (bool))
                {
                    foreach (TableRow row in grid.Rows)
                    {
                        row.Cells[i].Text = ((CheckBox)row.Cells[i].Controls[0]).Checked ? "Yes" : "No";
                    }
                }
            }

            string excelContent;
            using (var sw = new StringWriter())
            {
                sw.Write(@"<style> td { mso-number-format:""" + "\\@" + @"""; }</style>");
                var htw = new HtmlTextWriter(sw);
                grid.RenderControl(htw);
                excelContent = sw.ToString();
            }

            grid.DataSource = null;
            grid.DataBind();
            grid.Dispose();

            var preamble = Encoding.Unicode.GetPreamble();
            var byteContent = Encoding.Unicode.GetBytes(excelContent);
            var result = new byte[preamble.Length + byteContent.Length];
            Buffer.BlockCopy(preamble, 0, result, 0, preamble.Length);
            Buffer.BlockCopy(byteContent, 0, result, preamble.Length, byteContent.Length);

            return result;
        }
        #endregion
    }
}