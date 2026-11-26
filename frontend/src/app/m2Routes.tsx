import type { ReactNode } from 'react'
import type { RouteObject } from 'react-router-dom'
import { ArrivalsPage } from '@/features/booking/ArrivalsPage'
import { BookingsPage } from '@/features/booking/BookingsPage'
import { CheckoutPage } from '@/features/booking/CheckoutPage'
import { FavoritesPage } from '@/features/booking/FavoritesPage'
import { NotificationsPage } from '@/features/booking/NotificationsPage'
import { OwnerReviewsPage } from '@/features/booking/OwnerReviewsPage'
import { RequireSession } from '@/features/booking/RequireSession'
import { TicketPage } from '@/features/booking/TicketPage'

const page = (content: ReactNode) => (
  <main className="mx-auto w-full max-w-3xl p-4">
    <RequireSession>{content}</RequireSession>
  </main>
)

export const m2Routes: RouteObject[] = [
  { path: '/checkout/:locationId', element: page(<CheckoutPage />) },
  { path: '/bookings', element: page(<BookingsPage />) },
  { path: '/bookings/:id', element: page(<TicketPage />) },
  { path: '/favorites', element: page(<FavoritesPage />) },
  { path: '/notifications', element: page(<NotificationsPage />) },
  { path: '/owner/arrivals', element: page(<ArrivalsPage />) },
  { path: '/owner/reviews', element: page(<OwnerReviewsPage />) },
]
