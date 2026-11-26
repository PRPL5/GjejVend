import { useTranslation } from 'react-i18next'
import { Button } from '@/components/ui'
import { useFavorites, useToggleFavorite } from '@/features/booking/bookingsApi'

export function FavoriteButton({ locationId }: { locationId: string }) {
  const { t } = useTranslation('bookings')
  const favorites = useFavorites()
  const toggle = useToggleFavorite()
  const saved = favorites.data?.some((f) => f.locationId === locationId) ?? false

  return (
    <Button
      variant="secondary"
      type="button"
      aria-pressed={saved}
      loading={toggle.isPending}
      disabled={!favorites.isSuccess}
      onClick={() => toggle.mutate({ locationId, saved })}
    >
      {saved ? t('favorites.saved') : t('favorites.save')}
    </Button>
  )
}
