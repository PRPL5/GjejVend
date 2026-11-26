using GjejVend.Application.Bookings;
using GjejVend.Domain.Identity;

namespace GjejVend.Application.UnitTests;

public sealed class BookingValidatorTests
{
    private static readonly DateTimeOffset Start = new(2026, 12, 1, 9, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData("01-ABC-123", true)]
    [InlineData("AB123CD", true)]
    [InlineData("", false)]
    [InlineData("A", false)]
    [InlineData("<script>", false)]
    public void Plate_format_is_checked(string plate, bool valid) =>
        Assert.Equal(valid, new CreateBookingRequestValidator().Validate(Request(plate, Start.AddHours(2))).IsValid);

    [Fact]
    public void End_must_be_after_start() =>
        Assert.False(new CreateBookingRequestValidator().Validate(Request("AB123CD", Start)).IsValid);

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(6, false)]
    public void Rating_is_between_one_and_five(int rating, bool valid) =>
        Assert.Equal(valid, new ReviewRequestValidator().Validate(new ReviewRequest(rating, null)).IsValid);

    private static CreateBookingRequest Request(string plate, DateTimeOffset endsAt) =>
        new(Guid.NewGuid(), Start, endsAt, plate, VehicleType.Car);
}
