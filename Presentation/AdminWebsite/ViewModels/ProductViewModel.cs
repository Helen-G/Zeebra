using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Game.ApplicationServices;
using AFT.RegoV2.Core.Game.Interfaces;

namespace AFT.RegoV2.AdminWebsite.ViewModels
{
    public class ProductViewModel
    {
        public string Name { get; set; }
        public Guid Id { get; set; }

        public override bool Equals(object obj)
        {
            var product = obj as ProductViewModel;
            return product != null && Id == product.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static IEnumerable<ProductViewModel> BuildFromIds(IGameQueries queries, IEnumerable<Guid> productIds)
        {
            var gameProviders = queries.GetGameProviders().Where(x => productIds.Contains(x.Id));
            return gameProviders.Select(x => new ProductViewModel { Id = x.Id, Name = x.Name });
        }
    }
}