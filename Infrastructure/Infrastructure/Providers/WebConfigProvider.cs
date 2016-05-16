using System.Web.Configuration;
using AFT.RegoV2.Core.Common.Data;

namespace AFT.RegoV2.Infrastructure.Providers
{
    public interface IWebConfigProvider
    {
        string GetAppSettingByKey(string key);
        MachineDecryptionInfo GetMachineDecryptionInfo();
    }
    public sealed class WebConfigProvider : IWebConfigProvider
    {
        private const string DefaultMachineKey = "77eab1b846234203b09a91cde90904d9";

        string IWebConfigProvider.GetAppSettingByKey(string key)
        {
            return WebConfigurationManager.AppSettings[key];
        }
        MachineDecryptionInfo IWebConfigProvider.GetMachineDecryptionInfo()
        {
            MachineKeySection section = (MachineKeySection) 
                WebConfigurationManager.GetSection ("system.web/machineKey");

            var isDefined = section.DecryptionKey != "AutoGenerate,IsolateApps";

            return new MachineDecryptionInfo
                        {
                            DecryptionKey = isDefined ? section.DecryptionKey : DefaultMachineKey,
                            DecryptionAlgorithm = isDefined ? section.Decryption : CryptoAlgorithm.Rijndael
                        };
        }
    }

}