using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.AdminWebsite.ViewModels;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Core.Security.ApplicationServices.Data.Users;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Domain.BoundedContexts.Security.ApplicationServices;
using AFT.RegoV2.Shared;
using AutoMapper;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    public class AdminManagerController : BaseController
    {
        private readonly UserService _service;
        private readonly RoleService _roleService;
        private readonly BrandQueries _brands;

        public AdminManagerController(
            UserService service, 
            RoleService roleService, 
            BrandQueries brands)
        {
            _service = service;
            _roleService = roleService;
            _brands = brands;
        }

        #region Actions

        [SearchPackageFilter("searchPackage")]
        public ActionResult Data(SearchPackage searchPackage)
        {
            var data = SearchData(searchPackage);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CreateUser(EditUserModel data)
        {
            try
            {
                var userData = Mapper.DynamicMap<AddUserData>(data);

                _service.CreateUser(userData);
                
                return this.Success();
            }
            catch (RegoException exception)
            {
                return this.Failed(exception);
            }
        }

        public ActionResult GetUser(Guid id)
        {
            var user = _service.GetUserById(id);
            var userData = Mapper.Map<EditUserModel>(user);
            return Json(userData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateUser(EditUserModel data)
        {
            try
            {
                var userData = Mapper.DynamicMap<EditUserData>(data);

                _service.UpdateUser(userData);
                
                return this.Success();

            }
            catch (RegoException exception)
            {
                return this.Failed(exception);
            }
        }

        public ActionResult ResetPassword(EditUserModel user)
        {
            if (String.Compare(user.Password, user.PasswordConfirmation, StringComparison.OrdinalIgnoreCase) != 0)
                return this.Failed(new RegoException("Passwords does not match"));

            try
            {
                var updatedUser = _service.ChangePassword(user.Id, user.Password);
                var userData = Mapper.Map<EditUserModel>(updatedUser);

                return this.Success(userData);
            }
            catch (RegoException exception)
            {
                return this.Failed(exception);
            }
        }

        public ActionResult Activate(Guid id)
        {
            try
            {
                _service.Activate(id);
            }
            catch (RegoException exception)
            {
                return this.Failed(exception);
            }

            return this.Success();
        }

        public ActionResult Deactivate(Guid id)
        {
            try
            {
                _service.Deactivate(id);
            }
            catch (RegoException exception)
            {
                return this.Failed(exception);
            }

            return this.Success();
        }

        public JsonResult GetRoles()
        {
            var roles = new List<object>();
            foreach (var role in _roleService.GetRoles())
                roles.Add(new { role.Id, role.Name });
            return Json(roles, JsonRequestBehavior.AllowGet);
        }

        public string GetEditData(Guid? id = null)
        {
            if (!id.HasValue)
            {
                var licenseeFilterSelections = _service.GetLicenseeFilterSelections(CurrentUser.UserId);
                var brandFilterSelections = _service.GetBrandFilterSelections(CurrentUser.UserId);
                
                return SerializeJson(new
                {
                    Licensees = _brands.GetLicensees()
                        .Where(l => licenseeFilterSelections.Contains(l.Id))
                        .Select(l => new { l.Id, l.Name }),
                    Brands = _brands.GetBrands()
                        .Where(b => brandFilterSelections.Contains(b.Id))
                        .Select(b => new { b.Id, b.Code, b.Name }),
                    Currencies = _brands.GetCurrencies().Select(c => new { c.Code, c.Name })                    
                });
            }

            var user = _service.GetUserById(id.Value);

            var userData = Mapper.Map<EditUserModel>(user);

            userData.AssignedLicensees = user.Licensees.Select(l => l.Id).ToList();
            userData.AllowedBrands = user.AllowedBrands.Select(b => b.Id).ToList();
            userData.Currencies = user.Currencies.Select(c => c.Currency).ToList();

            return SerializeJson(new
            {
                User = userData,
                Licensees = _brands.GetLicensees().Select(l => new { l.Id, l.Name }),
                Currencies = _brands.GetCurrencies().Select(c => new { c.Code, c.Name }),
                Brands = _brands.GetBrands().Select(b => new { b.Id, b.Code, b.Name })
            });
        }

        public string GetLicenseeData(IEnumerable<Guid> licensees, bool useBrandFilter = false)
        {
            var filter = licensees ?? new List<Guid>();
            var brands = _brands.GetBrands().Where(b => filter.Contains(b.Licensee.Id));
            var roles = _roleService.GetRoles().Where(r => r.Licensees.Select(l => l.Id).Intersect(filter).Any());
            var currencies = _brands.GetLicensees().Where(l => filter.Contains(l.Id)).SelectMany(l => l.Currencies).Distinct();

            if (useBrandFilter)
            {
                var brandFilterSelections = _service.GetBrandFilterSelections(CurrentUser.UserId);

                brands = brands.Where(b => brandFilterSelections.Contains(b.Id));
            }

            return SerializeJson(new
            {
                Brands = brands.OrderBy(l => l.Name).Select(l => new { l.Id, l.Name }),
                Roles = roles.Select(r => new { r.Id, r.Name }),
                Currencies = currencies.OrderBy(c => c.Code).Select(c => new { c.Code, c.Name })
            });
        }

        [HttpPost]
        public ActionResult SaveBrandFilterSelection(Guid[] brands)
        {
            _service.SetBrandFilterSelections(CurrentUser.UserId, brands);

            return this.Success();
        }

        [HttpPost]
        public ActionResult SaveLicenseeFilterSelection(Guid[] licensees)
        {
            _service.SetLicenseeFilterSelections(CurrentUser.UserId, licensees);

            return this.Success();
        }       

        #endregion

        #region Helper methods

        private object SearchData(SearchPackage searchPackage)
        {
            var brandFilterSelections = _service.GetBrandFilterSelections(CurrentUser.UserId);
            var users = _service.GetUsers().Where(x => x.AllowedBrands.Any(y => brandFilterSelections.Contains(y.Id)));
            var dataBuilder = new SearchPackageDataBuilder<User>(searchPackage, users);

            return dataBuilder
                .Map(user => user.Id, user => GetUserCell(user))
                .GetPageData(user => user.Username);
        }

        private object GetUserCell(User user)
        {
            return new[]
            {
                user.Username,
                user.FirstName,
                user.LastName,
                user.Language,
                user.Role.Name,
                user.Status.ToString(),
                user.Description
            };
        }

        #endregion

    }
}