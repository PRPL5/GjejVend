import { QRCodeSVG } from 'qrcode.react'
import { useState, type FormEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { useParams } from 'react-router-dom'
import { Button } from '@/components/Button'
import { ErrorMessage, Loading, StatusDot } from '@/components/Feedback'
import { TextAreaField, TextField } from '@/components/Field'
import { Sheet } from '@/components/Sheet'
import { useCancelBooking, useExtendBooking, useTicket, type Ticket } from '@/features/booking/api'
import { formatDuration, fromLocalInput, mapsLink, statusTones, toLocalInput, useFormat } from '@/features/booking/format'
import { ReviewForm } from '@/features/booking/ReviewForm'
import { useNow } from '@/features/booking/useNow'

function countdown(ticket: Ticket, now: number): { key: string; target: number } | null {
  const starts = Date.parse(ticket.startsAt)
  const ends = Date.parse(ticket.endsAt)
  if (ticket.status === 'pending' && ticket.expiresAt) return { key: 'ticket.expiresIn', target: Date.parse(ticket.expiresAt) }
  if (ticket.status === 'confirmed' && now < starts) return { key: 'ticket.startsIn', target: starts }
  if ((ticket.status === 'confirmed' || ticket.status === 'active') && now < ends) return { key: 'ticket.endsIn', target: ends }
  return null
}

export function TicketPage() {
  const { t } = useTranslation('bookings')
  const { id = '' } = useParams()
  const ticket = useTicket(id)
  const now = useNow(15_000)
  const format = useFormat()
  const [sheet, setSheet] = useState<'extend' | 'cancel' | null>(null)
  const [reviewed, setReviewed] = useState(false)

  if (!ticket.data) {
    return ticket.isError ? <ErrorMessage error={ticket.error} onRetry={() => void ticket.refetch()} /> : <Loading />
  }

  const data = ticket.data
  const timer = countdown(data, now)
  const canExtend = data.status === 'confirmed' || data.status === 'active'
  const canCancel = data.status === 'pending' || data.status === 'confirmed'

  return (
    <article className="flex flex-col gap-6">
      {ticket.isError && (
        <p role="status" className="rounded-2xl bg-warning-tint p-4 text-warning">
          {t('ticket.offline')}
        </p>
      )}

      <header className="flex flex-col gap-2">
        <StatusDot tone={statusTones[data.status]}>{t(`status.${data.status}`)}</StatusDot>
        <h1 className="font-display text-3xl font-bold">{data.locationName}</h1>
        <p className="text-ink-2">{data.addressLine}</p>
        {timer && (
          <p className="font-display text-2xl font-bold" aria-live="polite">
            {t(timer.key, { time: formatDuration(timer.target - now) })}
          </p>
        )}
        {data.status === 'pending' && <p className="text-ink-2">{t('ticket.pendingNote')}</p>}
        {data.cancellationReason && <p className="text-ink-2">{t('ticket.cancellationReason', { reason: data.cancellationReason })}</p>}
      </header>

      {data.ticketToken && (
        <section className="flex flex-col items-start gap-2">
          <div className="rounded-2xl bg-white p-4">
            <QRCodeSVG value={data.ticketToken} size={208} title={t('ticket.qrHint')} />
          </div>
          <p className="text-ink-2">{t('ticket.qrHint')}</p>
        </section>
      )}

      <dl className="grid grid-cols-[auto_1fr] gap-x-6 gap-y-2">
        <dt className="text-ink-2">{t('ticket.reference')}</dt>
        <dd className="font-mono">{data.reference}</dd>
        <dt className="text-ink-2">{t('ticket.period')}</dt>
        <dd>
          {format.dateTime(data.startsAt)} – {format.time(data.endsAt)}
        </dd>
        <dt className="text-ink-2">{t('ticket.plate')}</dt>
        <dd className="font-mono">{data.vehiclePlate}</dd>
        <dt className="text-ink-2">{t('ticket.total')}</dt>
        <dd className="font-semibold">{format.money(data.totalAmount, data.currency)}</dd>
      </dl>

      {data.arrivalInstructions && (
        <section className="flex flex-col gap-1">
          <h2 className="font-display text-xl font-bold">{t('ticket.arrivalInstructions')}</h2>
          <p className="whitespace-pre-line">{data.arrivalInstructions}</p>
        </section>
      )}
      {data.accessInstructions && (
        <section className="flex flex-col gap-1">
          <h2 className="font-display text-xl font-bold">{t('ticket.accessInstructions')}</h2>
          <p className="whitespace-pre-line">{data.accessInstructions}</p>
        </section>
      )}

      {data.status === 'completed' && !data.hasReview && !reviewed && (
        <ReviewForm bookingId={data.id} onDone={() => setReviewed(true)} />
      )}
      {reviewed && <p role="status">{t('review.thanks')}</p>}

      <div className="flex flex-wrap gap-2">
        <a
          href={mapsLink(data.latitude, data.longitude)}
          target="_blank"
          rel="noreferrer"
          className="inline-flex min-h-11 items-center rounded-xl bg-lime px-4 font-semibold text-lime-ink no-underline"
        >
          {t('ticket.navigate')}
        </a>
        {canExtend && (
          <Button type="button" onClick={() => setSheet('extend')}>
            {t('ticket.extend')}
          </Button>
        )}
        {canCancel && (
          <Button type="button" onClick={() => setSheet('cancel')}>
            {t('ticket.cancel')}
          </Button>
        )}
      </div>

      <Sheet open={sheet === 'extend'} title={t('extend.title')} onClose={() => setSheet(null)}>
        <ExtendForm ticket={data} onDone={() => setSheet(null)} />
      </Sheet>
      <Sheet open={sheet === 'cancel'} title={t('cancel.title')} onClose={() => setSheet(null)}>
        <CancelForm ticket={data} onDone={() => setSheet(null)} />
      </Sheet>
    </article>
  )
}

function ExtendForm({ ticket, onDone }: { ticket: Ticket; onDone: () => void }) {
  const { t } = useTranslation('bookings')
  const extend = useExtendBooking(ticket.id)
  const [endsAt, setEndsAt] = useState(() => toLocalInput(new Date(Date.parse(ticket.endsAt) + 3_600_000)))

  function submit(event: FormEvent) {
    event.preventDefault()
    extend.mutate(fromLocalInput(endsAt), { onSuccess: onDone })
  }

  return (
    <form onSubmit={submit} className="flex flex-col gap-4">
      <TextField
        label={t('extend.newEnd')}
        type="datetime-local"
        value={endsAt}
        min={toLocalInput(new Date(ticket.endsAt))}
        onChange={(e) => setEndsAt(e.target.value)}
        required
      />
      {extend.isError && <ErrorMessage error={extend.error} />}
      <Button type="submit" variant="primary" loading={extend.isPending}>
        {t('extend.submit')}
      </Button>
    </form>
  )
}

function CancelForm({ ticket, onDone }: { ticket: Ticket; onDone: () => void }) {
  const { t } = useTranslation('bookings')
  const cancel = useCancelBooking(ticket.id)
  const [reason, setReason] = useState('')

  function submit(event: FormEvent) {
    event.preventDefault()
    cancel.mutate(reason, { onSuccess: onDone })
  }

  return (
    <form onSubmit={submit} className="flex flex-col gap-4">
      <TextAreaField label={t('cancel.reason')} value={reason} maxLength={500} onChange={(e) => setReason(e.target.value)} />
      {cancel.isError && <ErrorMessage error={cancel.error} />}
      <div className="flex flex-col gap-2">
        <Button type="submit" variant="danger" loading={cancel.isPending}>
          {t('cancel.submit')}
        </Button>
        <Button type="button" onClick={onDone}>
          {t('cancel.keep')}
        </Button>
      </div>
    </form>
  )
}
