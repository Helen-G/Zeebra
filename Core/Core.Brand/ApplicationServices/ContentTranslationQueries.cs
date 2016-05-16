using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Common.Brand.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Security.Common;

namespace AFT.RegoV2.Core.Brand.ApplicationServices
{
    public class ContentTranslationQueries : MarshalByRefObject, IApplicationService
    {
        private readonly IBrandRepository _repository;

        public ContentTranslationQueries(IBrandRepository repository)
        {
            if (repository == null) 
                throw new ArgumentNullException("repository");

            _repository = repository;
        }

        public ContentTranslation GetContentTranslation(Guid Id)
        {
            return _repository.ContentTranslations
                .SingleOrDefault( ct => ct.Id == Id) ;
        }

        public IEnumerable<ContentTranslation> GetAllContentTranslations()
        {
            return _repository.ContentTranslations.ToList();
        }

        [Permission(Permissions.View, Module = Modules.TranslationManager)]
        public IQueryable<ContentTranslation> GetContentTranslations()
        {
            return _repository.ContentTranslations.AsNoTracking();
        }

        public IEnumerable<Culture> GetAllCultureCodes()
        {
            return _repository.Cultures;
        }

        [Permission(Permissions.View, Module = Modules.LanguageManager)]
        public Culture GetCultureByCode(string code)
        {
            return _repository.Cultures.First(c => c.Code == code);
        }

        [Permission(Permissions.View, Module = Modules.LanguageManager)]
        public IDbSet<Culture> GetCultureByCodes()
        {
            return _repository.Cultures;
        }
    }
}
