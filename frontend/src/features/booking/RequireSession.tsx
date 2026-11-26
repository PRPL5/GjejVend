import type { ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useLocation } from 'react-router-dom'
import { EmptyState } from '@/components/Feedback'
import { useSession } from '@/features/auth/session'

export function RequireSession({ children }: { children: ReactNode }) {
  const { t } = useTranslation('bookings')
  const location = useLocation()
  const signedIn = useSession((s) => s.session !== null)
  if (signedIn) return children
  return (
    <EmptyState>
      <Link to="/sign-in" state={{ from: location.pathname }}>
        {t('signInRequired')}
      </Link>
    </EmptyState>
  )
}
