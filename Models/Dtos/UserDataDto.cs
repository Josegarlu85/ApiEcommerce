namespace ApiEcommerce.Models.Dtos;

public class UserDataDto
{
    public string Id { get; set; } = "";
    public string Username { get; set; } = "";
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
}
