import type { RouteObject } from 'react-router-dom'
import { AdminPayoutsPage } from '@/features/admin/AdminPayoutsPage'
import { CommissionSettingsPage } from '@/features/admin/CommissionSettingsPage'
import { InvoicesPage } from '@/features/booking/InvoicesPage'
import { PaymentPage } from '@/features/booking/PaymentPage'
import { OwnerDashboardPage } from '@/features/owner/OwnerDashboardPage'

/** Routes of the payments, earnings and admin module. The app shell spreads these into its router. */
export const m3Routes: RouteObject[] = [
  { path: '/bookings/:bookingId/pay', element: <PaymentPage /> },
  { path: '/me/invoices', element: <InvoicesPage /> },
  { path: '/owner', element: <OwnerDashboardPage /> },
  { path: '/admin/settings', element: <CommissionSettingsPage /> },
  { path: '/admin/payouts', element: <AdminPayoutsPage /> },
]
