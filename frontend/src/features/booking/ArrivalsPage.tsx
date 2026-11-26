import { useMemo, useState, type FormEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { Button } from '@/components/Button'
import { EmptyState, ErrorMessage, Loading, StatusDot } from '@/components/Feedback'
import { SelectField, TextField } from '@/components/Field'
import { useArrivals, useDeskAction } from '@/features/booking/api'
import { statusTones, useFormat } from '@/features/booking/format'

function today(): [string, string] {
  const start = new Date()
  start.setHours(0, 0, 0, 0)
  const end = new Date(start)
  end.setDate(end.getDate() + 1)
  return [start.toISOString(), end.toISOString()]
}

export function ArrivalsPage() {
  const { t } = useTranslation('bookings')
  const format = useFormat()
  const [[from, to]] = useState(today)
  const arrivals = useArrivals(from, to)
  const desk = useDeskAction()
  const [locationId, setLocationId] = useState('')
  const [token, setToken] = useState('')
  const [scanMode, setScanMode] = useState<'scan-in' | 'scan-out'>('scan-in')

  const locations = useMemo(
    () => [...new Map((arrivals.data ?? []).map((a) => [a.locationId, a.locationName])).entries()],
    [arrivals.data],
  )
  const selectedLocation = locationId || locations[0]?.[0] || ''

  function scan(event: FormEvent) {
    event.preventDefault()
    desk.mutate({ kind: scanMode, locationId: selectedLocation, token: token.trim() }, { onSuccess: () => setToken('') })
  }

  if (arrivals.isPending) return <Loading />
  if (arrivals.isError) return <ErrorMessage error={arrivals.error} onRetry={() => void arrivals.refetch()} />

  return (
    <div className="flex flex-col gap-8">
      <h1 className="font-display text-3xl font-bold">{t('arrivals.title')}</h1>

      {desk.isError && <ErrorMessage error={desk.error} />}
      {desk.isSuccess && (
        <p role="status" className="rounded-2xl bg-success-tint p-4 text-available">
          {t('arrivals.done', { plate: desk.data.vehiclePlate, status: t(`status.${desk.data.status}`) })}
        </p>
      )}

      {arrivals.data.length === 0 ? (
        <EmptyState>{t('arrivals.empty')}</EmptyState>
      ) : (
        <>
          <ul>
            {arrivals.data.map((arrival) => (
              <li key={arrival.id} className="flex flex-wrap items-center justify-between gap-4 border-b border-paper-3 py-4 last:border-0">
                <div className="flex flex-col gap-1">
                  <span className="font-mono text-lg">{arrival.vehiclePlate}</span>
                  <span className="text-ink-2">
                    {format.time(arrival.startsAt)} – {format.time(arrival.endsAt)} · {arrival.locationName}
                  </span>
                  <StatusDot tone={statusTones[arrival.status]}>{t(`status.${arrival.status}`)}</StatusDot>
                </div>
                {arrival.status === 'confirmed' && (
                  <Button
                    type="button"
                    loading={desk.isPending && desk.variables?.kind === 'check-in' && desk.variables.bookingId === arrival.id}
                    onClick={() => desk.mutate({ kind: 'check-in', bookingId: arrival.id })}
                  >
                    {t('arrivals.checkIn')}
                  </Button>
                )}
                {arrival.status === 'active' && (
                  <Button
                    type="button"
                    loading={desk.isPending && desk.variables?.kind === 'check-out' && desk.variables.bookingId === arrival.id}
                    onClick={() => desk.mutate({ kind: 'check-out', bookingId: arrival.id })}
                  >
                    {t('arrivals.checkOut')}
                  </Button>
                )}
              </li>
            ))}
          </ul>

          <form onSubmit={scan} className="flex flex-col gap-4 rounded-2xl bg-paper-2 p-4">
            <h2 className="font-display text-xl font-bold">{t('arrivals.scanTitle')}</h2>
            <SelectField label={t('arrivals.location')} value={selectedLocation} onChange={(e) => setLocationId(e.target.value)}>
              {locations.map(([id, name]) => (
                <option key={id} value={id}>
                  {name}
                </option>
              ))}
            </SelectField>
            <TextField
              label={t('arrivals.token')}
              value={token}
              onChange={(e) => setToken(e.target.value)}
              autoComplete="off"
              className="font-mono"
              required
            />
            <div className="flex gap-2">
              <Button type="submit" variant="primary" onClick={() => setScanMode('scan-in')} loading={desk.isPending && scanMode === 'scan-in'}>
                {t('arrivals.scanCheckIn')}
              </Button>
              <Button type="submit" onClick={() => setScanMode('scan-out')} loading={desk.isPending && scanMode === 'scan-out'}>
                {t('arrivals.scanCheckOut')}
              </Button>
            </div>
          </form>
        </>
      )}
    </div>
  )
}
