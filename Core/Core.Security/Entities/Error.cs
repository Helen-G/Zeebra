using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.Core.Security.Entities
{
    public class Error
    {
        private readonly Data.Error _data;

        public Error(Data.Error data)
        {
            _data = data;
        }

        public Guid Id 
        { 
            get { return _data.Id; }
        }

        public string Message
        {
            get { return _data.Message; }
        }

        public string Source
        {
            get { return _data.Source; }
        }

        public string Detail
        {
            get { return _data.Detail; }
        }

        public string User
        {
            get { return _data.User; }
        }

        public string HostName
        {
            get { return _data.HostName; }
        }

        public string Type
        {
            get { return _data.Type; }
        }

        public DateTime Time
        {
            get { return _data.Time; }
        }

        public bool IsValidationError 
        {
            get { return Type == typeof (RegoValidationException).FullName; }
        }
    }
}
