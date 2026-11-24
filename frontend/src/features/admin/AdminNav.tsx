import { useTranslation } from 'react-i18next'
import { NavLink } from 'react-router-dom'

/** Tabs between the M3 admin screens. The shared admin layout can replace this later. */
export function AdminNav() {
  const { t } = useTranslation()
  const link = ({ isActive }: { isActive: boolean }) =>
    `rounded-lg px-3 py-2 text-sm font-semibold ${isActive ? 'bg-emerald-600 text-white' : 'bg-white text-slate-700 border border-slate-300'}`

  return (
    <nav className="flex flex-wrap gap-2" aria-label="Admin">
      <NavLink to="/admin/verifications" className={link}>
        {t('nav.adminVerifications')}
      </NavLink>
      <NavLink to="/admin/reports" className={link}>
        {t('nav.adminReports')}
      </NavLink>
      <NavLink to="/admin/settings" className={link}>
        {t('nav.adminSettings')}
      </NavLink>
      <NavLink to="/admin/payouts" className={link}>
        {t('nav.adminPayouts')}
      </NavLink>
      <NavLink to="/admin/audit" className={link}>
        {t('nav.adminAudit')}
      </NavLink>
    </nav>
  )
}
