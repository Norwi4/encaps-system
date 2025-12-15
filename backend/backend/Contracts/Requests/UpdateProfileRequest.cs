namespace backend.Contracts.Requests;

public class UpdateProfileRequest
{
    public string Surname { get; set; }
    
    public string Name { get; set; }
    
    public string Patronymic { get; set; }
    
    public string Phone { get; set; }
    
    public string Email { get; set; }
}