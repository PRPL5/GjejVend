import type { ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import { EmptyState, Loading } from '@/components/Feedback'
import { useSession } from '@/store/session'

export function RequireSession({ children }: { children: ReactNode }) {
  const { t } = useTranslation()
  const { ready, userId } = useSession()
  if (!ready) return <Loading />
  if (!userId) return <EmptyState>{t('signInRequired')}</EmptyState>
  return children
}
