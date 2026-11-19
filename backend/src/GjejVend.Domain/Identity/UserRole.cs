namespace GjejVend.Domain.Identity;

public sealed class UserRole
{
    public Guid UserId { get; set; }
    public Role Role { get; set; }
    public Guid? GrantedBy { get; set; }
    public DateTime GrantedAt { get; set; }
}
