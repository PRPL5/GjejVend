import { useState, type FormEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate, useParams, useSearchParams } from 'react-router-dom'
import { Button, Loading } from '@/components/ui'
import { ErrorMessage, StatusDot } from '@/components/Feedback'
import { SelectField, TextField } from '@/components/Field'
import {
  useCheckoutPreview,
  useCreateBooking,
  type VehicleType,
} from '@/features/booking/bookingsApi'
import { FavoriteButton } from '@/features/booking/FavoriteButton'
import { fromLocalInput, toLocalInput, useFormat } from '@/features/booking/format'

const vehicleTypes: VehicleType[] = ['Car', 'Motorcycle', 'Van', 'Suv']

function defaultStart(): Date {
  const start = new Date(Date.now() + 15 * 60_000)
  start.setMinutes(Math.ceil(start.getMinutes() / 15) * 15, 0, 0)
  return start
}

function initialPeriod(params: URLSearchParams): [string, string] {
  const from = params.get('from')
  const to = params.get('to')
  const start = from ? new Date(from) : defaultStart()
  const end = to ? new Date(to) : new Date(start.getTime() + 2 * 3_600_000)
  return [toLocalInput(start), toLocalInput(end)]
}

export function CheckoutPage() {
  const { t } = useTranslation('bookings')
  const { locationId = '' } = useParams()
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()
  const format = useFormat()
  const [[startsAt, endsAt], setPeriod] = useState(() => initialPeriod(searchParams))
  const [plate, setPlate] = useState<string | null>(null)
  const [vehicleType, setVehicleType] = useState<VehicleType | null>(null)

  const validPeriod = startsAt !== '' && endsAt !== '' && new Date(endsAt) > new Date(startsAt)
  const preview = useCheckoutPreview(
    locationId,
    validPeriod ? fromLocalInput(startsAt) : '',
    validPeriod ? fromLocalInput(endsAt) : '',
    validPeriod,
  )
  const create = useCreateBooking()
  const defaultVehicle = preview.data?.defaultVehicle
  const plateValue = plate ?? defaultVehicle?.plate ?? ''
  const vehicleTypeValue = vehicleType ?? defaultVehicle?.vehicleType ?? 'Car'

  const free = preview.data?.freeCapacity ?? 0
  const bookable = preview.isSuccess && preview.data.isOpen && free > 0

  function submit(event: FormEvent) {
    event.preventDefault()
    if (!bookable) return
    create.mutate(
      {
        locationId,
        startsAt: fromLocalInput(startsAt),
        endsAt: fromLocalInput(endsAt),
        vehiclePlate: plateValue.trim(),
        vehicleType: vehicleTypeValue,
      },
      { onSuccess: (ticket) => navigate(`/bookings/${ticket.id}/pay`) },
    )
  }

  return (
    <form onSubmit={submit} className="flex flex-col gap-6 pb-28">
      <header className="flex items-start justify-between gap-4">
        <div className="flex flex-col gap-1">
          <h1 className="font-display text-3xl font-bold">
            {preview.data?.locationName ?? t('checkout.title')}
          </h1>
          {preview.data && <p className="text-ink-2">{preview.data.addressLine}</p>}
        </div>
        <FavoriteButton locationId={locationId} />
      </header>

      <section className="grid gap-4 sm:grid-cols-2">
        <TextField
          label={t('checkout.from')}
          type="datetime-local"
          value={startsAt}
          onChange={(e) => setPeriod([e.target.value, endsAt])}
          required
        />
        <TextField
          label={t('checkout.to')}
          type="datetime-local"
          value={endsAt}
          min={startsAt}
          onChange={(e) => setPeriod([startsAt, e.target.value])}
          aria-invalid={!validPeriod}
          required
        />
        {!validPeriod && <p className="text-danger sm:col-span-2">{t('checkout.invalidPeriod')}</p>}
      </section>

      {preview.isLoading && <Loading />}
      {preview.isError && (
        <ErrorMessage error={preview.error} onRetry={() => void preview.refetch()} />
      )}
      {preview.isSuccess && (
        <p>
          {!preview.data.isOpen ? (
            <StatusDot tone="muted">{t('availability.closed')}</StatusDot>
          ) : free > 0 ? (
            <StatusDot tone="available">{t('availability.free', { count: free })}</StatusDot>
          ) : (
            <StatusDot tone="muted">{t('availability.full')}</StatusDot>
          )}
        </p>
      )}

      <section className="grid gap-4 sm:grid-cols-2">
        <TextField
          label={t('checkout.plate')}
          value={plateValue}
          onChange={(e) => setPlate(e.target.value.toUpperCase())}
          autoCapitalize="characters"
          autoComplete="off"
          pattern="[A-Za-z0-9][A-Za-z0-9 \-]{1,14}"
          className="font-mono"
          required
        />
        <SelectField
          label={t('checkout.vehicleType')}
          value={vehicleTypeValue}
          onChange={(e) => setVehicleType(e.target.value as VehicleType)}
        >
          {vehicleTypes.map((type) => (
            <option key={type} value={type}>
              {t(`vehicleType.${type}`)}
            </option>
          ))}
        </SelectField>
      </section>

      <section className="flex flex-col gap-1">
        <h2 className="font-display text-xl font-bold">{t('checkout.payment')}</h2>
        <p className="text-ink-2">{t('checkout.paymentNote')}</p>
      </section>

      {create.isError && <ErrorMessage error={create.error} />}

      <div className="fixed inset-x-0 bottom-0 border-t border-paper-3 bg-paper p-4 shadow-sh-1">
        <div className="mx-auto flex max-w-2xl items-center justify-between gap-4">
          <p className="flex flex-col">
            <span className="text-sm text-ink-2">{t('checkout.total')}</span>
            <span className="font-display text-2xl font-bold">
              {preview.isSuccess
                ? format.money(preview.data.totalAmount, preview.data.currency)
                : '–'}
            </span>
          </p>
          <Button
            type="submit"
            variant="primary"
            loading={create.isPending}
            disabled={!bookable}
            className="min-w-40"
          >
            {create.isPending ? t('checkout.submitting') : t('checkout.submit')}
          </Button>
        </div>
      </div>
    </form>
  )
}
