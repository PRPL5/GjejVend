import { useTranslation } from 'react-i18next'
import { createBrowserRouter, NavLink, Outlet } from 'react-router-dom'
import App from './App'
import { m3Routes } from './m3Routes'

function AppShell() {
  const { t } = useTranslation()
  const link = ({ isActive }: { isActive: boolean }) => (isActive ? 'font-semibold text-emerald-700' : 'text-slate-600')

  return (
    <>
      <header className="border-b border-slate-200 bg-white">
        <nav className="mx-auto flex max-w-3xl flex-wrap gap-4 p-4 text-sm" aria-label="Main">
          <NavLink to="/" end className={link}>
            {t('nav.home')}
          </NavLink>
          <NavLink to="/me/invoices" className={link}>
            {t('nav.invoices')}
          </NavLink>
          <NavLink to="/owner" className={link}>
            {t('nav.owner')}
          </NavLink>
          <NavLink to="/admin/settings" className={link}>
            Admin
          </NavLink>
        </nav>
      </header>
      <Outlet />
    </>
  )
}

export const router = createBrowserRouter([
  {
    element: <AppShell />,
    children: [{ path: '/', element: <App /> }, ...m3Routes],
  },
])
