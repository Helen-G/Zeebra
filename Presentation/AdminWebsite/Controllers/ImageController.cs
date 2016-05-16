using System.Web.Mvc;
using AFT.RegoV2.Core.Common;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    public class ImageController : BaseController
    {
        private readonly IFileStorage _fileSystemStorage;

        public ImageController(IFileStorage fileSystemStorage)
        {
            _fileSystemStorage = fileSystemStorage;
        }

        public ActionResult Show(string fileName)
        {
            var imageData = _fileSystemStorage.Get(fileName);
            return File(imageData, "image/jpg");
        }
    }
}