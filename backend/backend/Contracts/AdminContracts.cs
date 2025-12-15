namespace backend.Contracts
{
    public class AdminUserResponse
    {
        public long Id { get; set; }
        public string Login { get; set; } = "";
        public string Password { get; set; } = "";
        public string Email { get; set; } = "";
        public string Name { get; set; } = "";
        public string Surname { get; set; } = "";
        public string Patronymic { get; set; } = "";
        public string Phone { get; set; }
        public string Role { get; set; } = "";
        public bool IsAdmin { get; set; }
    }

    public class CreateUserRequest
    {
        public string Login { get; set; } = "";
        public string Password { get; set; } = "";
        public string Email { get; set; } = "";
        public string Name { get; set; } = "";
        public string Surname { get; set; } = "";
        public string Patronymic { get; set; } = "";
        public string Phone { get; set; }
        public long RoleId { get; set; } = 1; // По умолчанию обычный пользователь
    }

    public class UpdateUserRequest
    {
        public string Login { get; set; } = "";
        public string Password { get; set; } = "";
        public string Email { get; set; } = "";
        public string Name { get; set; } = "";
        public string Surname { get; set; } = "";
        public string Patronymic { get; set; } = "";
        public string Phone { get; set; }
        public long RoleId { get; set; }
    }

    public class RoleResponse
    {
        public long Id { get; set; }
        public string Name { get; set; } = "";
        public long RoleCode { get; set; }
    }
} 