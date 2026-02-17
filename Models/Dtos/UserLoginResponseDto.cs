namespace ApiEcommerce.Models.Dtos;

public class UserLoginResponseDto
{
    public string Token { get; set; } = "";
    public UserDataDto? User { get; set; }
    public string Message { get; set; } = "";
}
