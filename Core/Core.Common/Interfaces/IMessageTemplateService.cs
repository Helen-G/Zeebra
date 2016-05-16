using System;

namespace AFT.RegoV2.Core.Common.Interfaces
{
    public interface IMessageTemplateService
    {
        string GetMessage<T>(Guid templateId, T model);
    }
}