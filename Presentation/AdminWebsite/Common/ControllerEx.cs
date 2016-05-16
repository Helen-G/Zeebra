using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using AFT.RegoV2.Domain.BoundedContexts.Report.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AFT.RegoV2.AdminWebsite.Common
{
    public static class ControllerEx
    {
        private readonly static string style = @"<style> td { mso-number-format:""" + "\\@" + @"""; }</style>";

        public static void ExportGridToExcel<T>(this Controller controller, GridView grid, string fileName)
        {
            var fields = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            grid.RowCreated += delegate(object dsender, GridViewRowEventArgs gvRowEvent)
            {
                if (gvRowEvent.Row.RowType == DataControlRowType.Header)
                {
                    int i = 0;
                    foreach (TableCell cell in gvRowEvent.Row.Cells)
                    {
                        var field = fields[i++];
                        var exportAttr = (ExportAttribute) field.GetCustomAttribute(typeof (ExportAttribute));
                        if (exportAttr != null)
                        {
                            cell.BackColor = Color.LightGray;
                            cell.Text = exportAttr.ColumnName ?? field.Name;
                        }
                        else
                        {
                            ((DataControlFieldCell) cell).ContainingField.Visible = false;
                        }
                    }
                }
                else if (gvRowEvent.Row.RowType == DataControlRowType.DataRow)
                {
                    foreach (TableCell cell in gvRowEvent.Row.Cells)
                    {
                        cell.HorizontalAlign = HorizontalAlign.Left;
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
                        row.Cells[i].Text = ((CheckBox) row.Cells[i].Controls[0]).Checked ? "Yes" : "No";
                    }
                }
            }

            HttpResponseBase response = controller.Response;
            response.ClearContent();

            response.AddHeader("content-disposition", string.Format("attachment; filename={0}", fileName));
            response.ContentType = "application/vnd.ms-excel";
            response.ContentEncoding = Encoding.Unicode;
            response.BinaryWrite(Encoding.Unicode.GetPreamble());

            if (grid.Rows != null && grid.Rows.Count == 0)
            {
                response.Write("There are no record(s)");
            }
            else
            {
                using (StringWriter sw = new StringWriter())
                {
                    HtmlTextWriter htw = new HtmlTextWriter(sw);
                    grid.RenderControl(htw);
                    response.Write(style);
                    response.Write(sw.ToString());

                }
            }

            grid.DataSource = null;
            grid.DataBind();
            grid.Dispose();

            response.Flush();
            response.End();
        }

        public static ActionResult Success(this Controller controller, object data = null)
        {
            return new JsonNetResult
            {
                Data = new { result = "success", data },
                Settings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }
            };
        }

        public static ActionResult Success(this Controller controller, JsonSerializerSettings settings, object data = null)
        {
            return new JsonNetResult
            {
                Data = new { result = "success", data },
                Settings = settings
            };
        }

        public static JsonResult Failed(this Controller controller, Exception exception)
        {
            return new JsonResult { Data = new { result = "failed", data = exception.Message }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public static JsonResult Failed(this Controller controller, object data = null)
        {
            if (data == null)
            {
                data = controller.ModelState.GetErrorsData();
            }
            return new JsonResult { Data = new { result = "failed", data }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public static Dictionary<string, List<string>> GetErrorsData(this ModelStateDictionary modelState)
        {
            var result = new Dictionary<string, List<string>>();
            if (modelState.IsValid)
            {
                return result;
            }

            foreach (var key in modelState.Keys)
            {
                List<string> keyErrors = modelState[key].Errors.Select(e => e.GetErrorDescription()).ToList();
                string dicKey = !String.IsNullOrEmpty(key) ? key : "_";
                result[dicKey] = keyErrors;
            }

            return result;
        }

        private static string GetErrorDescription(this ModelError error)
        {
            var messageList = new List<string>();
            if (!String.IsNullOrEmpty(error.ErrorMessage))
            {
                messageList.Add(error.ErrorMessage);
            }

            var exception = error.Exception;
            if (exception != null)
            {
                if (!String.IsNullOrEmpty(exception.Message))
                {
                    messageList.Add(exception.Message);
                }

                if (!String.IsNullOrEmpty(exception.StackTrace))
                {
                    messageList.Add(exception.StackTrace);
                }
            }

            return String.Join("\r\n", messageList);
        }

        public static byte[] GetBytes(this HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength <= 0)
            {
                return null;
            }
            using (var streamReader = new MemoryStream())
            {
                file.InputStream.CopyTo(streamReader);
                return streamReader.ToArray();
            }
        }

        public static string GetFileName(this HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                return file.FileName;
            }
            return null;
        }

        public static string GetFormattedDate(this Controller controller, DateTimeOffset? dto, bool includeTime = true)
        {
            var dateFormat = includeTime ? "yyyy-MM-dd HH:mm:ss" : "yyyy-MM-dd";
            return dto.HasValue ? dto.Value.ToString(dateFormat) : string.Empty;
        }
    }

    public class JsonNetResult : ActionResult
    {
        public JsonNetResult()
        {
        }

        public JsonNetResult(object data)
        {
            Data = data;
        }

        public JsonNetResult(object data, JsonSerializerSettings settings)
        {
            Data = data;
            Settings = settings;
        }

        /// <summary>Gets or sets the serialiser settings</summary> 
        public JsonSerializerSettings Settings { get; set; }

        /// <summary>Gets or sets the encoding of the response</summary> 
        public Encoding ContentEncoding { get; set; }

        /// <summary>Gets or sets the content type for the response</summary> 
        public string ContentType { get; set; }

        /// <summary>Gets or sets the body of the response</summary> 
        public object Data { get; set; }

        /// <summary>Gets the formatting types depending on whether we are in debug mode</summary> 
        private Formatting Formatting
        {
            get
            {
                return Debugger.IsAttached ? Formatting.Indented : Formatting.None;
            }
        }

        /// <summary> 
        /// Serialises the response and writes it out to the response object 
        /// </summary> 
        /// <param name="context">The execution context</param> 
        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            HttpResponseBase response = context.HttpContext.Response;

            // set content type 
            if (!string.IsNullOrEmpty(ContentType))
            {
                response.ContentType = ContentType;
            }
            else
            {
                response.ContentType = "application/json";
            }

            // set content encoding 
            if (ContentEncoding != null)
            {
                response.ContentEncoding = ContentEncoding;
            }

            if (Data != null)
            {
                response.Write(JsonConvert.SerializeObject(Data, Formatting, Settings));
            }
        }
    }
}