import { useTranslation } from 'react-i18next'
import { Button } from '@/components/Button'
import { useFavorites, useToggleFavorite } from '@/features/booking/api'

export function FavoriteButton({ locationId }: { locationId: string }) {
  const { t } = useTranslation('bookings')
  const favorites = useFavorites()
  const toggle = useToggleFavorite()
  const saved = favorites.data?.some((f) => f.locationId === locationId) ?? false

  return (
    <Button
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
