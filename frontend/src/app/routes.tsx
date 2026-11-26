import type { ReactNode } from 'react'
import type { RouteObject } from 'react-router-dom'
import { HomePage } from '@/app/HomePage'
import { MainLayout } from '@/app/layouts/MainLayout'
import { RequireSession } from '@/app/RequireSession'
import { ArrivalsPage } from '@/features/booking/ArrivalsPage'
import { BookingsPage } from '@/features/booking/BookingsPage'
import { CheckoutPage } from '@/features/booking/CheckoutPage'
import { FavoritesPage } from '@/features/booking/FavoritesPage'
import { NotificationsPage } from '@/features/booking/NotificationsPage'
import { OwnerReviewsPage } from '@/features/booking/OwnerReviewsPage'
import { TicketPage } from '@/features/booking/TicketPage'

const signedIn = (page: ReactNode) => <RequireSession>{page}</RequireSession>

export const routes: RouteObject[] = [
  {
    element: <MainLayout />,
    children: [
      { index: true, element: <HomePage /> },
      { path: 'checkout/:locationId', element: signedIn(<CheckoutPage />) },
      { path: 'bookings', element: signedIn(<BookingsPage />) },
      { path: 'bookings/:id', element: signedIn(<TicketPage />) },
      { path: 'favorites', element: signedIn(<FavoritesPage />) },
      { path: 'notifications', element: signedIn(<NotificationsPage />) },
      { path: 'owner/arrivals', element: signedIn(<ArrivalsPage />) },
      { path: 'owner/reviews', element: signedIn(<OwnerReviewsPage />) },
    ],
  },
]
