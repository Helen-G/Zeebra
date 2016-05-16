using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Core.Security.Events;
using AFT.RegoV2.Core.Services.Security;
using AutoMapper;

namespace AFT.RegoV2.Core.Security.ApplicationServices
{
    public class SecuritySubscriber : IBusSubscriber,
         IConsumes<ErrorRaised>
    {
        private readonly ISecurityRepository _repository;

        static SecuritySubscriber()
        {
            Mapper.CreateMap<ErrorRaised, Error>();
        }

        public SecuritySubscriber(
            ISecurityRepository repository
            )
        {
            _repository = repository;
        }

        public void Consume(ErrorRaised @event)
        {
            var error = Mapper.Map<Error>(@event);

            if (error.Id == Guid.Empty)
                error.Id = Guid.NewGuid();

            _repository.Errors.Add(error);
            _repository.SaveChanges();
        }
    }
}
