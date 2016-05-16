using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.AdminWebsite.ViewModels;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Core.Security.ApplicationServices.Data.IpRegulations;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Domain.BoundedContexts.Security.ApplicationServices;
using AFT.RegoV2.Domain.Security;
using AFT.RegoV2.Shared;
using AutoMapper;

namespace AFT.RegoV2.AdminWebsite.Controllers.Admin
{
    public class BrandIpRegulationsController : BaseController
    {
        private readonly BrandIpRegulationService _service;
        private readonly BrandQueries _brands;
        private readonly UserService _userService;

        public BrandIpRegulationsController(
            BrandIpRegulationService service,
            BrandQueries brands, 
            UserService userService)
        {
            _service = service;
            _brands = brands;
            _userService = userService;

            Mapper.CreateMap<BrandIpRegulation, BrandIpRegulationDTO>()
                .ForMember(dest => dest.Licensee, opt => opt.ResolveUsing(src => _brands.GetLicensees().First(l => l.Id == src.LicenseeId).Name))
                .ForMember(dest => dest.Brand, opt => opt.ResolveUsing(src => _brands.GetBrands().Where(b => b.Licensee.Id == src.LicenseeId).First(b => b.Id == src.BrandId).Name))
                .ForMember(dest => dest.RedirectionUrl, opt => opt.ResolveUsing(src => src.BlockingType == "Redirection" ? src.RedirectionUrl : ""))
                .ForMember(dest => dest.CreatedBy, opt => opt.ResolveUsing(src => (src.CreatedBy != null ? src.CreatedBy.Username : null) ?? String.Empty))
                .ForMember(dest => dest.UpdatedBy, opt => opt.ResolveUsing(src => (src.UpdatedBy != null ? src.UpdatedBy.Username : null) ?? String.Empty));
        }

        #region Query Actions

        public bool IsIpAddressUnique(string ipAddress)
        {
            return _service.IsIpAddressUnique(ipAddress);
        }

        public bool IsIpAddressBatchUnique(string ipAddressBatch)
        {
            var ipAddresses = ipAddressBatch.Replace("\n", string.Empty).Split(';');

            return ipAddresses.Select(ip => _service.IsIpAddressUnique(ip))
                .Aggregate(true, (current, result) => current && result);
        }

        public string GetEditData(Guid? id = null)
        {
            if (!id.HasValue)
            {
                var licenseeFilterSelections = _userService.GetLicenseeFilterSelections(CurrentUser.UserId);

                return SerializeJson(new
                {
                    Model = (EditBrandIpRegulationModel)null,
                    Licensees = _brands.GetLicensees()
                        .Where(l => l.Brands.Any() && licenseeFilterSelections.Contains(l.Id))
                        .OrderBy(l => l.Name)
                        .Select(l => new { l.Id, l.Name }),
                    BlockingTypes = ConstantsHelper.GetConstantsDictionary<IpRegulationConstants.BlockingTypes>()
                });
            }
                
            var ipRegulation = _service.GetIpRegulation(id.Value);
            var model = Mapper.Map<EditBrandIpRegulationModel>(ipRegulation);

            return SerializeJson(new
            {
                Model = model,
                Licensees = _brands.GetLicensees()
                    .OrderBy(l => l.Name)
                    .Select(l => new { l.Id, l.Name }),
                BlockingTypes = ConstantsHelper.GetConstantsDictionary<IpRegulationConstants.BlockingTypes>()
            });
        }

        public string GetLicenseeBrands(Guid licenseeId, bool useBrandFilter)
        {
            var brands = _brands.GetBrands().Where(b => b.Licensee.Id == licenseeId);

            if (useBrandFilter)
            {
                var brandFilterSelections = _userService.GetBrandFilterSelections(CurrentUser.UserId);

                brands = brands.Where(b => brandFilterSelections.Contains(b.Id));
            }

            return SerializeJson(new
            {
                Brands = brands.OrderBy(l => l.Name).Select(l => new { l.Name, l.Id })
            });
        }

        [HttpGet, SearchPackageFilter("searchPackage")]
        public ActionResult List(SearchPackage searchPackage)
        {
            return new JsonResult { Data = SearchData(searchPackage), MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        private object SearchData(SearchPackage searchPackage)
        {
            var brandFilterSelections = _userService.GetBrandFilterSelections(CurrentUser.UserId);
            var ipRegulations = _service.GetIpRegulations().Where(x => brandFilterSelections.Contains(x.BrandId));
            var brandIpReulationDto = ipRegulations.Select(Mapper.Map<BrandIpRegulationDTO>).AsQueryable();
            var dataBuilder = new SearchPackageDataBuilder<BrandIpRegulationDTO>(searchPackage, brandIpReulationDto);

            return dataBuilder
                .Map(r => r.Id, r => new object[]
                {
                    r.Licensee,
                    r.Brand,
                    r.IpAddress,
                    r.Description,
                    r.BlockingType,
                    r.RedirectionUrl,
                    r.CreatedBy,
                    r.CreatedDate,
                    r.UpdatedBy,
                    r.UpdatedDate
                })
                .GetPageData(r => r.IpAddress);
        }

        #endregion

        public ActionResult CreateIpRegulation(AddBrandIpRegulationModel model)
        {
            try
            {
                var addreses = new List<string>();
                if (string.IsNullOrEmpty(model.IpAddressBatch))
                    addreses.Add(model.IpAddress);
                else
                {
                    addreses.AddRange(model.IpAddressBatch.Split(';').Select(ip => ip.Trim(new[] { ' ', '\n' })));
                }

                if (!addreses.TrueForAll(ip => _service.IsIpAddressUnique(ip)))
                {
                    ModelState.AddModelError("IpAddress", "{\"text\": \"app:admin.messages.duplicateIp\"}");
                }

                if (!ModelState.IsValid)
                {
                    var messages = ModelState.Where(p => p.Value.Errors.Count > 0)
                            .Select(x => new { Name = ToCamelCase(x.Key), Errors = x.Value.Errors.Select(e => e.ErrorMessage) });
                    return this.Failed(messages);
                }

                foreach (var assignedBrand in model.AssignedBrands)
                {
                    foreach (var address in addreses)
                    {
                        var data = Mapper.DynamicMap<AddBrandIpRegulationData>(model);

                        data.IpAddress = address;
                        data.BrandId = assignedBrand;

                        _service.CreateIpRegulation(data);
                    }
                }
                
                return this.Success();
            }
            catch (RegoException exception)
            {
                return this.Failed(exception);
            }
        }

        public ActionResult UpdateIpRegulation(EditBrandIpRegulationModel model)
        {
            if (!ModelState.IsValid)
            {
                var messages = ModelState.Where(p => p.Value.Errors.Count > 0)
                        .Select(x => new { Name = ToCamelCase(x.Key), Errors = x.Value.Errors.Select(e => e.ErrorMessage) });
                return this.Failed(messages);
            }

            try
            {
                var data = Mapper.DynamicMap<EditBrandIpRegulationData>(model);
                _service.UpdateIpRegulation(data);

                return this.Success();
            }
            catch (RegoException exception)
            {
                return this.Failed(exception);
            }
        }

        public ActionResult DeleteIpRegulation(Guid id)
        {
            try
            {
                _service.DeleteIpRegulation(id);
            }
            catch (Exception exception)
            {
                return this.Failed(exception);
            }
            return this.Success();
        }

        private class BrandIpRegulationDTO
        {
            public Guid Id { get; set; }
            public string Licensee { get; set; }
            public string Brand { get; set; }
            public string IpAddress { get; set; }
            public string Restriction { get; set; }
            public string BlockingType { get; set; }
            public string RedirectionUrl { get; set; }
            public string Description { get; set; }
            public string CreatedBy { get; set; }
            public DateTimeOffset? CreatedDate { get; set; }
            public string UpdatedBy { get; set; }
            public DateTimeOffset? UpdatedDate { get; set; }
        }
    }
}