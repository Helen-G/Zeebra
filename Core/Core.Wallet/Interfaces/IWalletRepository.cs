using System;
using System.Data.Entity;
using System.Threading.Tasks;

namespace AFT.RegoV2.Core.Wallet.Interfaces
{
    public interface IWalletRepository
    {
        int SaveChanges();
    }
}