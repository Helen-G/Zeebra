using AFT.RegoV2.Core.Common.Data.Admin;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Payment.ApplicationServices
{
    public interface ICultureCommands : IApplicationService
    {
        string Save(EditCultureData model);
    }
}