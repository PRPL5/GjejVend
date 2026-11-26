import { useTranslation } from 'react-i18next'
import { NavLink, Outlet } from 'react-router-dom'
import { NotificationBadge } from '@/features/booking/NotificationsPage'
import { useSession } from '@/store/session'

const navLink = ({ isActive }: { isActive: boolean }) =>
  `inline-flex min-h-11 items-center rounded-xl px-3 no-underline ${isActive ? 'bg-ink text-paper' : 'hover:bg-paper-2'}`

export function MainLayout() {
  const { t, i18n } = useTranslation()
  const signedIn = useSession((s) => s.userId !== null)

  return (
    <div className="min-h-screen">
      <header className="border-b border-paper-3">
        <nav className="mx-auto flex max-w-2xl flex-wrap items-center gap-2 px-4 py-2">
          <NavLink to="/" className="mr-auto font-display text-xl font-extrabold no-underline">
            {t('appName')}
          </NavLink>
          <NavLink to="/bookings" className={navLink}>
            {t('nav.bookings')}
          </NavLink>
          <NavLink to="/favorites" className={navLink}>
            {t('nav.favorites')}
          </NavLink>
          <NavLink to="/notifications" className={navLink}>
            {t('nav.notifications')}
            {signedIn && <NotificationBadge />}
          </NavLink>
          <NavLink to="/owner/arrivals" className={navLink}>
            {t('nav.arrivals')}
          </NavLink>
          <NavLink to="/owner/reviews" className={navLink}>
            {t('nav.ownerReviews')}
          </NavLink>
          <button
            type="button"
            className="min-h-11 rounded-xl px-3 underline"
            onClick={() => void i18n.changeLanguage(i18n.language === 'sq' ? 'en' : 'sq')}
          >
            {t('nav.language')}
          </button>
        </nav>
      </header>
      <main className="mx-auto max-w-2xl px-4 py-8">
        <Outlet />
      </main>
    </div>
  )
}
