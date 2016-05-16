using System;
using System.Collections;
using System.Configuration;
using AFT.RegoV2.MemberApi.Interface.Proxy;
using AFT.RegoV2.MemberApi.Interface.Security;
using AutoMapper;
using Elmah;
using System.Threading.Tasks;

namespace AFT.RegoV2.MemberWebsite.Security.Logging
{
    public class ElmahErrorLogger : ErrorLog
    {
        private readonly MemberApiProxy _serviceProxy;

        static ElmahErrorLogger()
        {
            Mapper.CreateMap<Error, ApplicationErrorRequest>();
        }

        public ElmahErrorLogger(IDictionary config)
        {
            _serviceProxy = new MemberApiProxy(ConfigurationManager.AppSettings["MemberApiUrl"], Guid.NewGuid().ToString());
        }

        public override string Log(Error error)
        {
            var request = Mapper.Map<ApplicationErrorRequest>(error);
            Task.Run(() => _serviceProxy.ApplicationErrorAsync(request));
            return error.Message;
        }

        public override ErrorLogEntry GetError(string id)
        {
            return null;
        }

        public override int GetErrors(int pageIndex, int pageSize, IList errorEntryList)
        {

            return errorEntryList.Count;
        }
    }
}