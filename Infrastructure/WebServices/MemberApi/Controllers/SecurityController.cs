using System.Linq;
using System.Web.Http;
using AFT.RegoV2.Core.Brand;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.MemberApi.Interface.Security;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Shared;
using AutoMapper;

namespace AFT.RegoV2.MemberApi.Controllers
{
    public class SecurityController : BaseApiController
    {
        private readonly BrandIpRegulationService _regulations;
        private readonly IBrandRepository _brands;
        private readonly LoggingService _logging;

        static SecurityController()
        {
            Mapper.CreateMap<ApplicationErrorRequest, Error>();
        }

        public SecurityController(
            BrandIpRegulationService regulations, 
            IBrandRepository brands,
            LoggingService logging)
        {
            _regulations = regulations;
            _brands = brands;
            _logging = logging;
        }

        [AllowAnonymous]
        [HttpPost]
        public VerifyIpResponse VerifyIp(VerifyIpRequest request)
        {
            var brand = _brands.Brands.SingleOrDefault(b => b.Code == request.BrandName);
            if (brand == null)
                throw new RegoException("Unrecognized brand code");

            var result = _regulations.VerifyIpAddress(request.IpAddress, brand.Id);

            return new VerifyIpResponse(result);
        }



        [AllowAnonymous]
        [HttpPost]
        public ApplicationErrorResponse LogApplicationError(ApplicationErrorRequest request)
        {
            var error = Mapper.Map<Error>(request);
            _logging.Log(error);
            return new ApplicationErrorResponse();
        }
    }
}