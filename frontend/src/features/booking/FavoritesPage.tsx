import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import { Button, Loading } from '@/components/ui'
import { EmptyState, ErrorMessage } from '@/components/Feedback'
import { useFavorites, useToggleFavorite } from '@/features/booking/bookingsApi'

export function FavoritesPage() {
  const { t } = useTranslation('bookings')
  const favorites = useFavorites()
  const toggle = useToggleFavorite()

  if (favorites.isPending) return <Loading />
  if (favorites.isError)
    return <ErrorMessage error={favorites.error} onRetry={() => void favorites.refetch()} />

  return (
    <div className="flex flex-col gap-6">
      <h1 className="font-display text-3xl font-bold">{t('favorites.title')}</h1>
      {toggle.isError && <ErrorMessage error={toggle.error} />}
      {favorites.data.length === 0 ? (
        <EmptyState>{t('favorites.empty')}</EmptyState>
      ) : (
        <ul>
          {favorites.data.map((f) => (
            <li
              key={f.locationId}
              className="flex flex-wrap items-center justify-between gap-4 border-b border-paper-3 py-4 last:border-0"
            >
              <div className="flex flex-col gap-1">
                <Link to={`/checkout/${f.locationId}`} className="font-semibold">
                  {f.name}
                </Link>
                <span className="text-ink-2">
                  {[f.addressLine, f.neighbourhood].filter(Boolean).join(', ')}
                </span>
              </div>
              <Button
                variant="secondary"
                type="button"
                loading={toggle.isPending && toggle.variables?.locationId === f.locationId}
                onClick={() => toggle.mutate({ locationId: f.locationId, saved: true })}
              >
                {t('favorites.remove')}
              </Button>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
