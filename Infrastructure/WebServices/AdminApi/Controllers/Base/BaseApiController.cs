using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Results;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Infrastructure.Attributes;
using FluentValidation.Results;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AFT.RegoV2.AdminApi.Controllers.Base
{
    [ForceJsonFormatter]
    [Authorize]
    public class BaseApiController : ApiController
    {
        private readonly IPermissionService _permissionService;

        public BaseApiController()
        {
        }

        public BaseApiController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        protected string Username
        {
            get
            {
                var principal = (ClaimsPrincipal)User;
                var username = principal.Claims.Any() ? (from c in principal.Claims where c.Type == ClaimTypes.Name select c.Value).Single() : "";
                return username;
            }
        }

        protected Guid UserId
        {
            get
            {
                var principal = (ClaimsPrincipal)User;
                var userId = principal.Claims.Any(c => c.Type == ClaimTypes.NameIdentifier) ? (from c in principal.Claims where c.Type == ClaimTypes.NameIdentifier select c.Value).Single() : Guid.Empty.ToString();
                return new Guid(userId);
            }
        }

        protected void VerifyPermission(string permissionName, string module)
        {
            if (!_permissionService.VerifyPermission(UserId, permissionName, module))
                throw new Exception("Access forbidden");
        }

        protected string SerializeJson(object response)
        {
            var jsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            return JsonConvert.SerializeObject(response, jsonSettings);
        }

        protected static string ToCamelCase(string str)
        {
            return Char.ToLowerInvariant(str[0]) + str.Substring(1);
        }

        public object ValidationExceptionResponse(IList<ValidationFailure> e)
        {
            var fields = e
                .GroupBy(x => x.PropertyName)
                .Select(x => new
                {
                    Name = x.Key,
                    Errors = x.Select(y => y.ErrorMessage).ToArray()
                }).ToArray();

            return new { Result = "failed", Fields = fields };
        }
    }
}
