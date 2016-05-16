using System.Net;
using System.Web.Mvc;

namespace AFT.RegoV2.GameWebsite.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult NotFound()
        {
            Response.StatusCode = (int)HttpStatusCode.NotFound;
            return View();
        }

        public ActionResult ServerError()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            // Todo: Pass the exception into the view model, which you can make.
            //       That's an exercise, dear reader, for -you-.
            //       In case u want to pass it to the view, if you're admin, etc.
            // if (User.IsAdmin) // <-- I just made that up :) U get the idea...
            // {
            //     var exception = Server.GetLastError();
            //     // etc..
            // }
            var exception = Server.GetLastError();


            var model = new ErrorViewModel
            {
                Exception = exception.ToString(),
                Stack = exception.StackTrace
            };

            return View(model);
        }

    }

    public class ErrorViewModel
    {
        public  string Exception { get; set; }
        public string Stack { get; set; }
    }
}
