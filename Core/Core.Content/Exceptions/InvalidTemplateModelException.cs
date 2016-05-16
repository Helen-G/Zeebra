using System;
using AFT.RegoV2.Core.Common.Data.Content;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.Core.Content.Exceptions
{
    public class InvalidTemplateModelException : RegoException
    {
        public InvalidTemplateModelException(MessageType messageType, Type expectedModelType, Type providedModelType)
            : base(string.Format(
                "{0} messages require a model of type {1}. Provided model is of type {2}",
                messageType, expectedModelType, providedModelType)) { }
    }
}