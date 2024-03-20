using AutoMapper;
using MaxemusAPI.Dtos;
using MaxemusAPI.Models;
using MaxemusAPI.Models.Dtos;
using MaxemusAPI.Repository;
using MaxemusAPI.Repository.IRepository;

namespace MaxemusAPI
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            CreateMap<UserDTO, ApplicationUser>().ReverseMap();
            CreateMap<LoginResponseDTO, ApplicationUser>().ReverseMap();
            CreateMap<DistributorAddressDTO, ApplicationUser>().ReverseMap();
            CreateMap<LoginResponseDTO, ApplicationUser>().ReverseMap();
            CreateMap<DealerDetailDTO, DealerDetail>().ReverseMap();
            CreateMap<DealerProfileDTO, DealerDetail>().ReverseMap();
            CreateMap<DealerDetailDTO, ApplicationUser>().ReverseMap();
            CreateMap<DealerProfileDTO, ApplicationUser>().ReverseMap();
            CreateMap<DistributorAddressDTO, DistributorDetail>().ReverseMap();
            CreateMap<DistributorAddressDTO, DistributorAddress>().ReverseMap();
            CreateMap<DistributorAddress, ApplicationUser>().ReverseMap();
            CreateMap<ApplicationUser, DistributorAddress>().ReverseMap();
            CreateMap<UserDetailDTO, ApplicationUser>().ReverseMap();
            CreateMap<ApplicationUser, UserRequestDTO>().ReverseMap();
            CreateMap<Notification, NotificationDTO>().ReverseMap();
            CreateMap<NotificationDTO, Notification>().ReverseMap();
            CreateMap<NotificationSent, NotificationSentDTO>().ReverseMap();
            CreateMap<NotificationSentDTO, NotificationSent>().ReverseMap();

            CreateMap<DistributorAddressResponseDTO, DistributorAddress>().ReverseMap();
            CreateMap<DistributorAddressResponseDTO, ApplicationUser>().ReverseMap();
            CreateMap<DistributorAddressResponseDTO, DistributorDetail>().ReverseMap();
            CreateMap<DistributorAddressResponseDTO, DistributorAddressDTO>().ReverseMap();

            CreateMap<AddBrandDTO, Brand>().ReverseMap();
            CreateMap<BrandDTO, Brand>().ReverseMap();
            CreateMap<BrandListDTO, Brand>().ReverseMap();


            CreateMap<CategoryRequestDTO, MainCategory>().ReverseMap();
            CreateMap<DeleteCategoryDTO, MainCategory>().ReverseMap();
            CreateMap<GetCategoryRequestDTO, MainCategory>().ReverseMap();
            CreateMap<GetCategoryDetailRequestDTO, MainCategory>().ReverseMap();
            CreateMap<AddCategoryDTO, MainCategory>().ReverseMap();
            CreateMap<CategoryDTO, MainCategory>().ReverseMap();
            CreateMap<UpdateCategoryDTO, MainCategory>().ReverseMap();

            CreateMap<AddCategoryDTO, SubCategory>().ReverseMap();
            CreateMap<DeleteCategoryDTO, SubCategory>().ReverseMap();
            CreateMap<CategoryDTO, SubCategory>().ReverseMap();
            CreateMap<CategoryRequestDTO, SubCategory>().ReverseMap();
            CreateMap<GetCategoryRequestDTO, SubCategory>().ReverseMap();
            CreateMap<GetCategoryDetailRequestDTO, SubCategory>().ReverseMap();
            CreateMap<UpdateCategoryDTO, SubCategory>().ReverseMap();


            CreateMap<AdminResponseDTO, CompanyDetail>().ReverseMap();
            CreateMap<AdminResponseDTO, ApplicationUser>().ReverseMap();
            CreateMap<CompanyDetail, ApplicationUser>().ReverseMap();
            CreateMap<AdminCompanyDTO, CompanyDetail>().ReverseMap();
            CreateMap<AdminCompanyDTO, ApplicationUser>().ReverseMap();
            CreateMap<AdminCompanyResponseDTO, CompanyDetail>().ReverseMap();
            CreateMap<AdminCompanyResponseDTO, ApplicationUser>().ReverseMap();

            CreateMap<AddProductDTO, Product>().ReverseMap();
            CreateMap<ProductResponseDTO, Product>().ReverseMap();

            CreateMap<ProductVariantDTO, Product>().ReverseMap();
            CreateMap<ProductVariantDTO, AccessoriesVariants>().ReverseMap();
            CreateMap<ProductVariantDTO, AudioVariants>().ReverseMap();
            CreateMap<ProductVariantDTO, CameraVariants>().ReverseMap();
            CreateMap<ProductVariantDTO, CertificationVariants>().ReverseMap();
            CreateMap<ProductVariantDTO, EnvironmentVariants>().ReverseMap();
            CreateMap<ProductVariantDTO, GeneralVariants>().ReverseMap();
            CreateMap<ProductVariantDTO, InstallationDocumentVariants>().ReverseMap();
            CreateMap<ProductVariantDTO, LensVariants>().ReverseMap();
            CreateMap<ProductVariantDTO, NetworkVariants>().ReverseMap();
            CreateMap<ProductVariantDTO, PowerVariants>().ReverseMap();
            CreateMap<ProductVariantDTO, VideoVariants>().ReverseMap();

            CreateMap<ProductResponsesDTO, Product>().ReverseMap();
            CreateMap<ProductResponsesDTO, AccessoriesVariants>().ReverseMap();
            CreateMap<ProductResponsesDTO, AudioVariants>().ReverseMap();
            CreateMap<ProductResponsesDTO, CameraVariants>().ReverseMap();
            CreateMap<ProductResponsesDTO, CertificationVariants>().ReverseMap();
            CreateMap<ProductResponsesDTO, EnvironmentVariants>().ReverseMap();
            CreateMap<ProductResponsesDTO, GeneralVariants>().ReverseMap();
            CreateMap<ProductResponsesDTO, InstallationDocumentVariants>().ReverseMap();
            CreateMap<ProductResponsesDTO, LensVariants>().ReverseMap();
            CreateMap<ProductResponsesDTO, NetworkVariants>().ReverseMap();
            CreateMap<ProductResponsesDTO, PowerVariants>().ReverseMap();
            CreateMap<ProductResponsesDTO, VideoVariants>().ReverseMap();





        }
    }
}