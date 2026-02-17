using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;

public interface IUserRepository
{
    Task<ApplicationUser?> GetUser(string id);
    Task<ICollection<ApplicationUser>> GetUsers();
    Task<UserLoginResponseDto> Login(UserLoginDto userLoginDto);
    Task<UserDataDto> Register(CreateUserDto createUserDto);
}
