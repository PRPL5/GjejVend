using GjejVend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GjejVend.Infrastructure.Bookings;

public interface IBookingAvailabilityQueries
{
    Task<bool> IsOpenAsync(Guid locationId, DateTime startsAt, DateTime endsAt, CancellationToken cancellationToken);

    Task<int> FreeCapacityAsync(Guid locationId, DateTime startsAt, DateTime endsAt, CancellationToken cancellationToken);

    Task<PriceQuote> QuoteAsync(Guid locationId, DateTime startsAt, DateTime endsAt, CancellationToken cancellationToken);
}

public sealed record PriceQuote(Guid RateId, string Currency, int Minutes, decimal TotalAmount);

internal sealed class BookingAvailabilityQueries(GjejVendDbContext context) : IBookingAvailabilityQueries
{
    public Task<bool> IsOpenAsync(Guid locationId, DateTime startsAt, DateTime endsAt, CancellationToken cancellationToken) =>
        DatabaseErrors.GuardAsync(() => context.Database
            .SqlQuery<bool>($"""SELECT public.location_is_open({locationId}, {startsAt}, {endsAt}) AS "Value" """)
            .SingleAsync(cancellationToken));

    public Task<int> FreeCapacityAsync(Guid locationId, DateTime startsAt, DateTime endsAt, CancellationToken cancellationToken) =>
        DatabaseErrors.GuardAsync(() => context.Database
            .SqlQuery<int>($"""SELECT public.location_free_capacity({locationId}, {startsAt}, {endsAt}) AS "Value" """)
            .SingleAsync(cancellationToken));

    public Task<PriceQuote> QuoteAsync(Guid locationId, DateTime startsAt, DateTime endsAt, CancellationToken cancellationToken) =>
        DatabaseErrors.GuardAsync(async () => (await context.Database
            .SqlQuery<PriceQuote>($"""
                SELECT rate_id AS "RateId", currency::text AS "Currency", minutes AS "Minutes", total_amount AS "TotalAmount"
                FROM public.quote_booking({locationId}, NULL::uuid, {startsAt}, {endsAt})
                """)
            .ToListAsync(cancellationToken)).Single());
}
