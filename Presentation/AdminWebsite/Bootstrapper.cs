using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.ViewModels;
using AFT.RegoV2.AdminWebsite.ViewModels.GameIntegration;
using AFT.RegoV2.Core.Brand.Validators;
using AFT.RegoV2.Core.Game.Data;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Core.Services.Security;
using AutoMapper;
using Microsoft.Practices.Unity;
using Unity.Mvc4;

namespace AFT.RegoV2.AdminWebsite
{
    public static class Bootstrapper
    {
        public static IUnityContainer Initialise()
        {
            var container = BuildUnityContainer();

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
           
            return container;
        }

        private static IUnityContainer BuildUnityContainer()
        {
            var container = new AdminWebsiteContainerFactory().CreateWithRegisteredTypes();

            RegisterMappings();

            return container;
        }

        public static void RegisterMappings()
        {
            Mapper.CreateMap<EditBrandData, Core.Brand.Data.Brand>().ForMember(b => b.Licensee, opt => opt.Ignore());
            
            RegisterUserMappings();

            RegisterIpRegulationMappings();

            RegisterGameProviderMappings();
        }

        public static void RegisterUserMappings()
        {
            Mapper.CreateMap<EditUserModel, User>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(data => data.GetStatus()))
                .ForMember(dest => dest.PasswordEncrypted,
                    opt => opt.MapFrom(data => PasswordHelper.EncryptPassword(data.Id, data.Password)))
                .ForMember(dest => dest.AllowedBrands, opt => opt.Ignore())
                .ForMember(dest => dest.Currencies, opt => opt.Ignore());

            Mapper.CreateMap<User, EditUserModel>()
                .ForMember(dest => dest.RoleId, opt => opt.MapFrom(data => data.Role.Id))
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(data => data.Role.Name))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(data => data.Status.ToString()))
                .ForMember(dest => dest.AllowedBrands, opt => opt.Ignore())
                .ForMember(dest => dest.Currencies, opt => opt.Ignore());
        }

        public static void RegisterIpRegulationMappings()
        {
            Mapper.CreateMap<EditAdminIpRegulationModel, AdminIpRegulation>();
            Mapper.CreateMap<AdminIpRegulation, EditAdminIpRegulationModel>();

            Mapper.CreateMap<AddBrandIpRegulationModel, BrandIpRegulation>();
            Mapper.CreateMap<BrandIpRegulation, AddBrandIpRegulationModel>();

            Mapper.CreateMap<EditBrandIpRegulationModel, BrandIpRegulation>();
            Mapper.CreateMap<BrandIpRegulation, EditBrandIpRegulationModel>();
        }
        
        private static void RegisterGameProviderMappings()
        {
            Mapper.CreateMap<EditGameProviderModel, GameProvider>();
        }

    }
}