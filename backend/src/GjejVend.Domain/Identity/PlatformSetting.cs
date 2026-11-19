namespace GjejVend.Domain.Identity;

public sealed class PlatformSetting
{
    public required string Key { get; set; }
    public required string Value { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
}
