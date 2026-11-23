namespace GjejVend.Domain.Payments;

public sealed class PlatformSetting
{
    public const string CommissionRateKey = "commission_rate";
    public const string RefundPolicyKey = "refund_policy";

    private PlatformSetting() { }

    public PlatformSetting(string key, string value, DateTime now, Guid? updatedBy = null)
    {
        Key = key;
        Value = value;
        UpdatedAt = now;
        UpdatedBy = updatedBy;
    }

    public string Key { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public DateTime UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }

    public void Update(string value, DateTime now, Guid? updatedBy)
    {
        Value = value;
        UpdatedAt = now;
        UpdatedBy = updatedBy;
    }
}
