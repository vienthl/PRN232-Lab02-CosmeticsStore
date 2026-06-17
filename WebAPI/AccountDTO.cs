namespace WebAPI;

public class AccountRequestDTO
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class AccountResponseDTO
{
    public string Token { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string AccountId { get; set; } = null!;
}
