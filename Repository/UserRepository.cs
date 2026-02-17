using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ApiEcommerce.Repository;

public class UserRepository : IUserRepository
{
    private readonly string? secretKey;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IMapper _mapper;

    public UserRepository(
        IConfiguration configuration,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IMapper mapper)
    {
        secretKey = configuration.GetValue<string>("ApiSettings:SecretKey");
        _userManager = userManager;
        _roleManager = roleManager;
        _mapper = mapper;
    }

    public async Task<ApplicationUser?> GetUser(string id)
    {
        return await _userManager.FindByIdAsync(id);
    }

    public async Task<ICollection<ApplicationUser>> GetUsers()
    {
        return await _userManager.Users.ToListAsync();
    }

    public async Task<UserLoginResponseDto> Login(UserLoginDto dto)
    {
        var user = await _userManager.FindByNameAsync(dto.Username);

        if (user == null)
            return new UserLoginResponseDto { Message = "Usuario no encontrado" };

        var valid = await _userManager.CheckPasswordAsync(user, dto.Password);

        if (!valid)
            return new UserLoginResponseDto { Message = "Credenciales incorrectas" };

        var roles = await _userManager.GetRolesAsync(user);

        var key = Encoding.UTF8.GetBytes(secretKey!);
        var handler = new JwtSecurityTokenHandler();

        var claims = new List<Claim>
        {
            new Claim("id", user.Id),
            new Claim("username", user.UserName ?? "")
        };

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = handler.CreateToken(tokenDescriptor);

        var dtoUser = _mapper.Map<UserDataDto>(user);
        dtoUser.Role = roles.FirstOrDefault();

        return new UserLoginResponseDto
        {
            Token = handler.WriteToken(token),
            User = dtoUser,
            Message = "Login correcto"
        };
    }

public async Task<UserDataDto> Register(CreateUserDto createUserDto)
{
    if (string.IsNullOrEmpty(createUserDto.Username))
        throw new ArgumentNullException("El Username es requerido");

    if (string.IsNullOrEmpty(createUserDto.Password))
        throw new ArgumentNullException("El Password es requerido");

    var user = new ApplicationUser
    {
        UserName = createUserDto.Username,
        Email = createUserDto.Email ?? createUserDto.Username,
        NormalizedEmail = (createUserDto.Email ?? createUserDto.Username).ToUpper(),
        Name = createUserDto.Name
    };

    var result = await _userManager.CreateAsync(user, createUserDto.Password);

    if (!result.Succeeded)
    {
        // Mostrar los errores REALES de Identity
        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        throw new ApplicationException($"No se pudo realizar el registro: {errors}");
    }

    // Rol por defecto si no se envía
    var userRole = createUserDto.Role ?? "User";

    // Crear rol si no existe
    if (!await _roleManager.RoleExistsAsync(userRole))
    {
        await _roleManager.CreateAsync(new IdentityRole(userRole));
    }

    // Asignar rol al usuario
    await _userManager.AddToRoleAsync(user, userRole);

    // Recuperar usuario recién creado
    var createdUser = await _userManager.FindByNameAsync(createUserDto.Username);

    // Mapear a DTO
    var dto = _mapper.Map<UserDataDto>(createdUser);

    // Asignar el rol manualmente (AutoMapper no lo sabe)
    dto.Role = userRole;

    return dto;
}


}
