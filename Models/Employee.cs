public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public DateTime? HireDate { get; set; }
    public string Position { get; set; } = string.Empty;
    public decimal? Salary { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }

    // Nullable DepartmentId (foreign key)
    public int? DepartmentId { get; set; }

    // Navigation property
    public Department? Department { get; set; }
}
