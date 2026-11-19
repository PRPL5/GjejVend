namespace GjejVend.Domain.Identity;

public sealed class Profile
{
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? AvatarPath { get; set; }
    public Language PreferredLanguage { get; set; }
    public AccountStatus Status { get; set; }
    public bool NotifyEmail { get; set; } = true;
    public bool NotifyPush { get; set; } = true;
    public bool NotifyBooking { get; set; } = true;
    public bool NotifyMarketing { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
