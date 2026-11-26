import type { ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import { ApiError } from '@/api/http'

export type Tone = 'available' | 'warning' | 'danger' | 'muted' | 'ink'

const dotColors: Record<Tone, string> = {
  available: 'bg-available',
  warning: 'bg-warning',
  danger: 'bg-danger',
  muted: 'bg-ink-3',
  ink: 'bg-ink',
}

export function StatusDot({ tone, children }: { tone: Tone; children: ReactNode }) {
  return (
    <span className="inline-flex items-center gap-2 text-sm font-medium">
      <span aria-hidden className={`size-2 rounded-full ${dotColors[tone]}`} />
      {children}
    </span>
  )
}

export function EmptyState({ children }: { children: ReactNode }) {
  return <p className="rounded-2xl bg-paper-2 p-6 text-ink-2">{children}</p>
}

export function useErrorMessage() {
  const { t, i18n } = useTranslation()
  return (error: unknown) => {
    if (error instanceof ApiError) {
      if (error.code && i18n.exists(`bookings:errors.${error.code}`))
        return t(`bookings:errors.${error.code}`)
      if (error.status === 0) return t('common.errors.network')
      if (error.status === 401) return t('common.errors.unauthorized')
      if (error.status < 500 && !error.message.startsWith('HTTP ')) return error.message
    }
    return t('common.errors.generic')
  }
}

export function ErrorMessage({ error, onRetry }: { error: unknown; onRetry?: () => void }) {
  const { t } = useTranslation()
  const message = useErrorMessage()
  return (
    <div
      role="alert"
      className="flex flex-col items-start gap-2 rounded-2xl bg-danger-tint p-4 text-danger"
    >
      <p>{message(error)}</p>
      {onRetry && (
        <button type="button" onClick={onRetry} className="min-h-11 font-semibold underline">
          {t('common.retry')}
        </button>
      )}
    </div>
  )
}
