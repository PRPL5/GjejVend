import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import { Loading } from '@/components/ui'
import { EmptyState, ErrorMessage, StatusDot } from '@/components/Feedback'
import { useMyBookings, type BookingSummary } from '@/features/booking/bookingsApi'
import { openStatuses, statusTones, useFormat } from '@/features/booking/format'

type Summary = BookingSummary

function BookingRow({ booking }: { booking: Summary }) {
  const { t } = useTranslation('bookings')
  const format = useFormat()
  return (
    <li className="border-b border-paper-3 last:border-0">
      <Link
        to={`/bookings/${booking.id}`}
        className="flex min-h-11 flex-col gap-1 py-4 no-underline"
      >
        <span className="flex items-center justify-between gap-4">
          <span className="font-semibold">{booking.locationName}</span>
          <span className="text-right font-semibold">
            {format.money(booking.totalAmount, booking.currency)}
          </span>
        </span>
        <span className="text-ink-2">
          {format.dateTime(booking.startsAt)} – {format.time(booking.endsAt)} ·{' '}
          <span className="font-mono">{booking.vehiclePlate}</span>
        </span>
        <StatusDot tone={statusTones[booking.status]}>{t(`status.${booking.status}`)}</StatusDot>
      </Link>
    </li>
  )
}

export function BookingsPage() {
  const { t } = useTranslation('bookings')
  const bookings = useMyBookings()

  if (bookings.isPending) return <Loading />
  if (bookings.isError)
    return <ErrorMessage error={bookings.error} onRetry={() => void bookings.refetch()} />

  const upcoming = bookings.data.filter((b) => openStatuses.includes(b.status)).reverse()
  const past = bookings.data.filter((b) => !openStatuses.includes(b.status))

  return (
    <div className="flex flex-col gap-8">
      <h1 className="font-display text-3xl font-bold">{t('list.title')}</h1>
      {bookings.data.length === 0 ? (
        <EmptyState>{t('list.empty')}</EmptyState>
      ) : (
        <>
          <section className="flex flex-col gap-2">
            <h2 className="font-display text-xl font-bold">{t('list.upcoming')}</h2>
            {upcoming.length === 0 ? (
              <EmptyState>{t('list.empty')}</EmptyState>
            ) : (
              <ul>
                {upcoming.map((b) => (
                  <BookingRow key={b.id} booking={b} />
                ))}
              </ul>
            )}
          </section>
          <section className="flex flex-col gap-2">
            <h2 className="font-display text-xl font-bold">{t('list.past')}</h2>
            {past.length === 0 ? (
              <EmptyState>{t('list.emptyPast')}</EmptyState>
            ) : (
              <ul>
                {past.map((b) => (
                  <BookingRow key={b.id} booking={b} />
                ))}
              </ul>
            )}
          </section>
        </>
      )}
    </div>
  )
}
