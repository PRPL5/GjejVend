import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import { Button, Loading } from '@/components/ui'
import { EmptyState, ErrorMessage } from '@/components/Feedback'
import { useMarkNotificationsRead, useNotifications } from '@/features/booking/bookingsApi'
import { useFormat } from '@/features/booking/format'

export function NotificationsPage() {
  const { t, i18n } = useTranslation('bookings')
  const format = useFormat()
  const notifications = useNotifications()
  const markRead = useMarkNotificationsRead()

  if (notifications.isPending) return <Loading />
  if (notifications.isError)
    return <ErrorMessage error={notifications.error} onRetry={() => void notifications.refetch()} />

  const { items, unreadCount } = notifications.data

  return (
    <div className="flex flex-col gap-6">
      <header className="flex flex-wrap items-center justify-between gap-4">
        <h1 className="font-display text-3xl font-bold">{t('notifications.title')}</h1>
        {unreadCount > 0 && (
          <Button
            variant="secondary"
            type="button"
            loading={markRead.isPending && markRead.variables === null}
            onClick={() => markRead.mutate(null)}
          >
            {t('notifications.markAllRead')}
          </Button>
        )}
      </header>
      {markRead.isError && <ErrorMessage error={markRead.error} />}
      {items.length === 0 ? (
        <EmptyState>{t('notifications.empty')}</EmptyState>
      ) : (
        <ul>
          {items.map((n) => (
            <li
              key={n.id}
              className="flex flex-wrap items-center justify-between gap-4 border-b border-paper-3 py-4 last:border-0"
            >
              <div className="flex flex-col gap-1">
                <span className={n.readAt ? 'text-ink-2' : 'font-semibold'}>
                  {i18n.exists(`bookings:notifications.types.${n.type}`)
                    ? t(`notifications.types.${n.type}`)
                    : n.title}
                </span>
                <span className="text-sm text-ink-2">
                  {n.body} · {format.dateTime(n.createdAt)}
                </span>
                {n.type === 'review_reminder' && n.entityId && (
                  <Link to={`/bookings/${n.entityId}`}>{t('notifications.openTicket')}</Link>
                )}
              </div>
              {!n.readAt && (
                <Button
                  variant="secondary"
                  type="button"
                  loading={markRead.isPending && markRead.variables === n.id}
                  onClick={() => markRead.mutate(n.id)}
                >
                  {t('notifications.markRead')}
                </Button>
              )}
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}

export function NotificationBadge() {
  const { t } = useTranslation('bookings')
  const notifications = useNotifications()
  const unread = notifications.data?.unreadCount ?? 0
  if (unread === 0) return null
  return (
    <span className="ml-1 inline-flex min-w-6 items-center justify-center rounded-full bg-ink px-2 text-xs font-semibold text-paper">
      <span aria-hidden>{unread}</span>
      <span className="sr-only">{t('notifications.unread', { count: unread })}</span>
    </span>
  )
}
