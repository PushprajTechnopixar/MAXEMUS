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
            CreateMap<UserDetailDTO, ApplicationUser>().ReverseMap();
            CreateMap<ApplicationUser, UserRequestDTO>().ReverseMap();
            CreateMap<Notification, NotificationDTO>().ReverseMap();
            CreateMap<NotificationDTO, Notification>().ReverseMap();
            CreateMap<NotificationSent, NotificationSentDTO>().ReverseMap();
            CreateMap<NotificationSentDTO, NotificationSent>().ReverseMap();
        }
    }
}
