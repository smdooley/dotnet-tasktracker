using TaskTrackerApi.DTOs.Auth;
using TaskTrackerApi.DTOs.Tasks;
using TaskTrackerApi.Models;

namespace TaskTrackerApi.Mappings
{
    public class AutoMapperProfile : AutoMapper.Profile
    {
        public AutoMapperProfile()
        {
            // Task mappings
            CreateMap<TaskItem, TaskResponseDto>();
            CreateMap<TaskCreateDto, TaskItem>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
            CreateMap<TaskUpdateDto, TaskItem>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // User mappings
            CreateMap<User, AuthResponse>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.Token, opt => opt.Ignore())
                .ForMember(dest => dest.ExpiresAt, opt => opt.Ignore());
        }
    }
}