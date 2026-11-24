import { useTranslation } from 'react-i18next'
import { createBrowserRouter, NavLink, Outlet } from 'react-router-dom'
import { SignInPage } from '@/features/auth/SignInPage'
import { signOut, useSession } from '@/features/auth/session'
import App from './App'
import { m3Routes } from './m3Routes'

function AppShell() {
  const { t } = useTranslation()
  const session = useSession((state) => state.session)
  const link = ({ isActive }: { isActive: boolean }) => (isActive ? 'font-semibold text-emerald-700' : 'text-slate-600')

  return (
    <>
      <header className="border-b border-slate-200 bg-white">
        <nav className="mx-auto flex max-w-3xl flex-wrap items-center gap-4 p-4 text-sm" aria-label="Main">
          <NavLink to="/" end className={link}>
            {t('nav.home')}
          </NavLink>
          <NavLink to="/me/invoices" className={link}>
            {t('nav.invoices')}
          </NavLink>
          <NavLink to="/owner" className={link}>
            {t('nav.owner')}
          </NavLink>
          <NavLink to="/admin/verifications" className={link}>
            {t('nav.admin')}
          </NavLink>
          <span className="ml-auto flex items-center gap-3">
            {session ? (
              <>
                <span className="text-slate-500">{session.email}</span>
                <button type="button" className="font-semibold text-emerald-700 underline" onClick={signOut}>
                  {t('nav.signOut')}
                </button>
              </>
            ) : (
              <NavLink to="/sign-in" className={link}>
                {t('nav.signIn')}
              </NavLink>
            )}
          </span>
        </nav>
      </header>
      <Outlet />
    </>
  )
}

export const router = createBrowserRouter([
  {
    element: <AppShell />,
    children: [{ path: '/', element: <App /> }, { path: '/sign-in', element: <SignInPage /> }, ...m3Routes],
  },
])
