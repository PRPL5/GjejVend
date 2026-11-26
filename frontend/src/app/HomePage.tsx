import { useTranslation } from 'react-i18next'
import { HealthBadge } from '@/features/health/HealthBadge'

export function HomePage() {
  const { t } = useTranslation()
  return (
    <section className="flex flex-col items-start gap-4">
      <h1 className="font-display text-4xl font-extrabold">{t('appName')}</h1>
      <p className="text-ink-2">{t('tagline')}</p>
      <HealthBadge />
    </section>
  )
}
