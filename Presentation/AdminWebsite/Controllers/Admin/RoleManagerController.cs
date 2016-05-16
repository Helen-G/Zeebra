using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.AdminWebsite.ViewModels;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Core.Security.ApplicationServices.Data.Roles;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Domain.BoundedContexts.Security.ApplicationServices;
using AFT.RegoV2.Shared;
using AutoMapper;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    public class RoleManagerController : BaseController
    {
        private readonly RoleService _service;
        private readonly BrandQueries _brands;
        private readonly UserService _userService;

        public RoleManagerController(RoleService service, BrandQueries brands, UserService userService)
        {
            _service = service;
            _brands = brands;
            _userService = userService;
        }

        [HttpGet, SearchPackageFilter("searchPackage")]
        public ActionResult Data(SearchPackage searchPackage)
        {
            return new JsonResult { Data = SearchData(searchPackage), MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public ActionResult CreateRole(EditRoleModel data)
        {
            try
            {
                var roleData = Mapper.DynamicMap<AddRoleData>(data);
                _service.CreateRole(roleData);

                return this.Success();
            }
            catch (RegoException exception)
            {
                return this.Failed(exception);
            }
        }

        public string GetRole(Guid id)
        {
            var role = _service.GetRoleById(id, true);
            var roleData = Mapper.DynamicMap<EditRoleModel>(role);

            roleData.CheckedPermissions = role.Permissions.Select(p => p.PermissionId);
            roleData.AssignedLicensees = role.Licensees.Select(l => l.Id).ToList();

            return SerializeJson(roleData);
        }

        public string GetEditData(Guid? id = null)
        {
            if (!id.HasValue)
            {
                var licenseeFilterSelections = _userService.GetLicenseeFilterSelections(CurrentUser.UserId);

                return SerializeJson(new
                {
                    Licensees = _brands.GetLicensees()
                        .Where(l => licenseeFilterSelections.Contains(l.Id))
                        .OrderBy(l => l.Name)                        
                        .Select(l => new { l.Id, l.Name }),
                });
            }
                

            var role = _service.GetRoleById(id.Value, true);
            var roleData = Mapper.DynamicMap<EditRoleModel>(role);

            roleData.CheckedPermissions = role.Permissions.Select(p => p.PermissionId);
            roleData.AssignedLicensees = role.Licensees.Select(l => l.Id).ToList();            

            return SerializeJson(new
            {
                Role = roleData,
                Licensees = _brands.GetLicensees()                   
                    .OrderBy(l => l.Name)
                    .Select(l => new { l.Id, l.Name })
            });
        }

        public ActionResult UpdateRole(EditRoleModel data)
        {
            try
            {
                var roleData = Mapper.DynamicMap<EditRoleData>(data);
                _service.UpdateRole(roleData);

                return this.Success(new
                {
                    Role = roleData,
                    Licensees = _brands.GetLicensees().Select(l => new { l.Id, l.Name }),
                });

            }
            catch (RegoException exception)
            {
                return this.Failed(exception);
            }
        }

        public string GetLicensees()
        {
            var licensees = _brands.GetLicensees();

            return SerializeJson( new
            {
                Licensees = licensees.OrderBy(l => l.Name).Select(l => new { l.Name, l.Id })
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

        public string GetLicenseeData(IEnumerable<Guid> licensees)
        {
            var filter = licensees ?? new List<Guid>();
            var roles = _service.GetRoles().Where(r => r.Licensees.Select(l => l.Id).Intersect(filter).Any());

            return SerializeJson(new
            {
                Roles = roles.Select(r => new { r.Id, r.Name })
            });
        }

        private object SearchData(SearchPackage searchPackage)
        {
            var licenseeFilterSelections = _userService.GetLicenseeFilterSelections(CurrentUser.UserId);
            var roles = _service.GetRoles().Where(r => r.Licensees.Any(l => licenseeFilterSelections.Contains(l.Id)));
            var dataBuilder = new SearchPackageDataBuilder<Role>(searchPackage, roles);

            return dataBuilder
                .Map(role => role.Id, role => GetRoleCell(role)) 
                .GetPageData(role => role.Name);
        }

        private object GetRoleCell(Role role)
        {
            return new[]
            {
                role.Code,
                role.Name,
                role.Description,
                role.CreatedBy != null ? role.CreatedBy.Username : string.Empty,
                Format.FormatDate(role.CreatedDate),
                role.UpdatedBy != null ? role.UpdatedBy.Username : string.Empty,
                Format.FormatDate(role.UpdatedDate)
            };
        }
    }
}