import type { RouteObject } from 'react-router-dom'
import { AdminPayoutsPage } from '@/features/admin/AdminPayoutsPage'
import { AuditLogPage } from '@/features/admin/AuditLogPage'
import { CommissionSettingsPage } from '@/features/admin/CommissionSettingsPage'
import { ReportsQueuePage } from '@/features/admin/ReportsQueuePage'
import { VerificationQueuePage } from '@/features/admin/VerificationQueuePage'
import { InvoicesPage } from '@/features/booking/InvoicesPage'
import { PaymentPage } from '@/features/booking/PaymentPage'
import { OwnerDashboardPage } from '@/features/owner/OwnerDashboardPage'

/** Routes of the payments, earnings and admin module. The app shell spreads these into its router. */
export const m3Routes: RouteObject[] = [
  { path: '/bookings/:bookingId/pay', element: <PaymentPage /> },
  { path: '/me/invoices', element: <InvoicesPage /> },
  { path: '/owner', element: <OwnerDashboardPage /> },
  { path: '/admin/verifications', element: <VerificationQueuePage /> },
  { path: '/admin/reports', element: <ReportsQueuePage /> },
  { path: '/admin/settings', element: <CommissionSettingsPage /> },
  { path: '/admin/audit', element: <AuditLogPage /> },
  { path: '/admin/payouts', element: <AdminPayoutsPage /> },
]
