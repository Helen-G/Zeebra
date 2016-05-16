using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.AdminWebsite.ViewModels;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Core.Security.ApplicationServices.Data.IpRegulations;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Domain.Security;
using AFT.RegoV2.Shared;
using AutoMapper;

namespace AFT.RegoV2.AdminWebsite.Controllers.Admin
{
    public class AdminIpRegulationsController : BaseController
    {
        private readonly BackendIpRegulationService _service;
        private readonly BrandQueries _brands;

        public AdminIpRegulationsController(
            BackendIpRegulationService service,
            BrandQueries brands
            )
        {
            _service = service;
            _brands = brands;

            Mapper.CreateMap<AdminIpRegulation, AdminIpRegulationDTO>()
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
            EditAdminIpRegulationModel model = null;
            if (id.HasValue)
            {
                var ipRegulation = _service.GetIpRegulation(id.Value);
                model = Mapper.Map<EditAdminIpRegulationModel>(ipRegulation);
            }

            return SerializeJson(new
            {
                Model = model,
                Licensees = _brands.GetLicensees().Select(l => new { l.Id, l.Name }),
                BlockingTypes = ConstantsHelper.GetConstantsDictionary<IpRegulationConstants.BlockingTypes>()
            });
        }

        public string GetLicenseeBrands(Guid licenseeId)
        {
            var brands = _brands.GetBrands().Where(b => b.Licensee.Id == licenseeId);

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
            var dataBuilder = new SearchPackageDataBuilder<AdminIpRegulationDTO>(searchPackage,
                _service.GetIpRegulations().ToList().Select(Mapper.Map<AdminIpRegulationDTO>).AsQueryable());

            return dataBuilder
                .Map(r => r.Id, r => new object[]
                {
                    r.IpAddress,
                    r.Description,
                    r.CreatedBy,
                    r.CreatedDate,
                    r.UpdatedBy,
                    r.UpdatedDate
                })
                .GetPageData(r => r.IpAddress);
        }

        #endregion

        public ActionResult CreateIpRegulation(EditAdminIpRegulationModel model)
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

                foreach (var address in addreses)
                {
                    var regulationData = Mapper.DynamicMap<AddBackendIpRegulationData>(model);

                    regulationData.IpAddress = address;
                    _service.CreateIpRegulation(regulationData);
                }

                return this.Success();
            }
            catch (RegoException exception)
            {
                return this.Failed(exception);
            }
        }

        public ActionResult UpdateIpRegulation(EditAdminIpRegulationModel model)
        {
            if (!ModelState.IsValid)
            {
                var messages = ModelState.Where(p => p.Value.Errors.Count > 0)
                        .Select(x => new { Name = ToCamelCase(x.Key), Errors = x.Value.Errors.Select(e => e.ErrorMessage) });
                return this.Failed(messages);
            }

            try
            {
                var regulationData = Mapper.DynamicMap<EditBackendIpRegulationData>(model);
                _service.UpdateIpRegulation(regulationData);

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

        private class AdminIpRegulationDTO
            {
            public Guid Id { get; set; }
            public string IpAddress { get; set; }
            public string Description { get; set; }
            public string CreatedBy { get; set; }
            public DateTimeOffset? CreatedDate { get; set; }
            public string UpdatedBy { get; set; }
            public DateTimeOffset? UpdatedDate { get; set; }
        }
    }
}