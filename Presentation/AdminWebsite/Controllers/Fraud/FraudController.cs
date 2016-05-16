using System;
using System.Linq;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Fraud.Data;
using AFT.RegoV2.Core.Fraud.ApplicationServices;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Domain.BoundedContexts.Security.ApplicationServices;
using ServiceStack.Validation;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    public class FraudController : BaseController
    {
        private readonly IRiskLevelCommands _commands;
        private readonly IRiskLevelQueries _queries;
        private readonly BrandQueries _brandQueries;
        private readonly UserService _userService;

        public FraudController(
            IRiskLevelCommands commands, 
            IRiskLevelQueries queries,
            BrandQueries brandQueries,
            UserService userService)
        {
            _commands = commands;
            _queries = queries;
            _brandQueries = brandQueries;
            _userService = userService;
        }

        [SearchPackageFilter("searchPackage")]
        public ActionResult Search(SearchPackage searchPackage)
        {
            var brandFilterSelections = _userService.GetBrandFilterSelections(CurrentUser.UserId);

            var query = _queries.GetAll().Where(x => brandFilterSelections.Contains(x.BrandId));

            var dataBuilder = new SearchPackageDataBuilder<RiskLevel>(searchPackage, query);

            dataBuilder
                .SetFilterRule(x => x.Brand, (value) => x => x.BrandId == new Guid(value))
                .Map(x => x.Id, x => new object[]
                        {
                          x.Level,
                          x.Name,
                          x.Status,
                          x.Brand.LicenseeName,
                          x.Brand.Name,
                          x.CreatedBy,
                          Format.FormatDate(x.DateCreated, false),
                          //x.UpdatedBy,
                          //Format.FormatDate(x.DateUpdated, false),
                          x.Description
                        });
            var data = dataBuilder.GetPageData(x => x.Name);
            return new JsonResult { Data = data, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public ActionResult AddOrUpdate(RiskLevel model)
        {
            try
            {
                if (model.Id == Guid.Empty)
                {
                    this._commands.Create(model);
                    return this.Success("app:common.entityCreated");
                }
                else
                {
                    this._commands.Update(model);
                    return this.Success("app:common.entityUpdated");
                }
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
        public ActionResult UpdateStatus(Guid id, bool status, string remarks)
        {
            try
            {
                if (status)
                {
                    this._commands.Activate(id, remarks);
                }
                else
                {
                    this._commands.Deactivate(id, remarks);
                }
                return this.Success();
            }
            catch (Exception exception)
            {
                return this.Failed(exception);
            }
        }

        [HttpGet]
        public ActionResult RiskLevel(Guid id)
        {
            var riskLevel = this._queries.GetById(id);
            if (riskLevel != null)
            {
                return this.Success(riskLevel);
            }

            return this.Failed("app:fraud.manager.message.invalidRiskLevelId");
        }

        [HttpGet]
        public ActionResult Brands(bool useBrandFilter)
        {
            var brands = _brandQueries.GetFilteredBrands(_brandQueries.GetBrands(), CurrentUser.UserId);

            if (useBrandFilter)
            {
                var brandFilterSelections = _userService.GetBrandFilterSelections(CurrentUser.UserId);

                brands = brands.Where(x => brandFilterSelections.Contains(x.Id));
            }

            var data = brands.ToArray();

            if (data.Any())
                return this.Success(data.Select(x => new
                {
                    x.Id,
                    x.Name,
                    licenseeId = x.Licensee.Id,
                    licenseeName = x.Licensee.Name
                }));

            return this.Failed("app:fraud.manager.message.brandDoesNotExist");            
        }

        [SearchPackageFilter("searchPackage")]
        public ActionResult Evaluation(SearchPackage searchPackage)
        {
            var dataBuilder = new SearchPackageDataBuilder<PlayerRiskLevel>(searchPackage, this._queries.GetAllPlayerRiskLevels());

            dataBuilder.SetFilterRule(x => x.PlayerId, (value) => x => x.PlayerId == new Guid(value))
                .Map(x => x.Id, x => new object[]
            {
                x.RiskLevel.Level,
                x.RiskLevel.Name,
                Format.FormatDate(x.DateCreated, false),
                x.CreatedBy,
                x.Description
            });

            var data = dataBuilder.GetPageData(x => x.Id);

            return new JsonResult { Data = data, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpGet]
        //[Route("available-risk-level")]
        public ActionResult AvailableRiskLevels(Guid brandId)
        {
            var risks = this._queries.GetByBrand(brandId).ToList();

            if (risks != null && risks.Count > 0)
            {
                var q = from r in risks
                        select new
                        {
                            id = r.Id,
                            name = string.Format("{0} - {1}", r.Level.ToString(), r.Name)
                        };

                var brand = risks.First().Brand;

                return this.Success(new
                {
                    Licensee = brand.LicenseeName,
                    Brand = brand.Name,
                    RiskLevels = q.ToArray()
                });
            }

            return this.Failed("app:fraud.evaluation.message.notAvailableRiskLevel");
        }

        /// <summary>
        /// tag player to specific risk level
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="riskLevel"></param>
        /// <param name="description">remark</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Tag(Guid playerId, Guid riskLevel, string description)
        {
            try
            {
                this._commands.Tag(playerId, riskLevel, description);
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

        /// <summary>
        /// Untag player from specific risk level - mark status inactive
        /// </summary>
        /// <param name="id"></param>
        /// <param name="remarks"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Untag(Guid id, string remarks)
        {
            try
            {
                this._commands.Untag(id, remarks);
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