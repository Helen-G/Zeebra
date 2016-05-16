using System;
using System.Linq;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Core.Security.Events;
using AFT.RegoV2.Core.Services.Security;
using AFT.RegoV2.Shared;
using AutoMapper;

namespace AFT.RegoV2.Core.Security.ApplicationServices
{
    public class LoggingService : IApplicationService
    {
        private readonly IEventBus _eventBus;
        private readonly ISecurityRepository _repository;

        public LoggingService(
            IEventBus eventBus,
            ISecurityRepository repository)
        {
            _eventBus = eventBus;
            _repository = repository;
        }

        public void Log(Error error)
        {
            var errorEntity = new Entities.Error(error);
            if (errorEntity.IsValidationError)
            {
                var validationEvent = Mapper.DynamicMap<ValidationFailed>(errorEntity);
                _eventBus.Publish(validationEvent);
            }
            else
            {
                var errorEvent = Mapper.DynamicMap<ErrorRaised>(errorEntity);
                _eventBus.Publish(errorEvent);
            }
        }

        public Error GetError(Guid id)
        {
            return _repository.Errors.SingleOrDefault(e => e.Id == id);
        }

        public IQueryable<Error> GetErrors()
        {
            return _repository.Errors.AsQueryable();
        }
    }
}
