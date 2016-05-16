using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Brand.Data;
using FluentValidation;

namespace AFT.RegoV2.Core.Brand.Validators
{
    public class EditWalletValidator : AbstractValidator<WalletTemplateViewModel>
    {
        public EditWalletValidator()
        {
            RuleFor(x => x)
                .Must(x =>
                {
                    var wallets = new List<WalletViewModel>();
                    wallets.Add(x.MainWallet);
                    wallets.AddRange(x.ProductWallets);
                    return wallets.GroupBy(i => i.Name).All(y => y.Count() == 1);
                })
                    .WithName("Name")
                    .WithMessage("app:wallet.common.nameUnique")
                .Must(x =>
                {
                    var wallets = new List<WalletViewModel> { x.MainWallet };
                    wallets.AddRange(x.ProductWallets);
                    return wallets.GroupBy(i => i.Name).All(y => !string.IsNullOrEmpty(y.Key));
                })
                    .WithName("Name")
                    .WithMessage("app:wallet.common.nameRequired")
                .Must(x => x.ProductWallets.All(y => y.ProductIds.Any()))
                    .WithName("Name")
                    .WithMessage("app:wallet.common.productsRequired")
                .Must(x => x.BrandId != Guid.Empty)
                    .WithName("Name")
                    .WithMessage("app:common.requiredBrand");
        }
    }
}