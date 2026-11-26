import { useMutation, useQuery } from '@tanstack/react-query'
import { api, unwrap } from '@/api/http'

export function usePayBooking() {
  return useMutation({
    mutationFn: (bookingId: string) =>
      unwrap(
        api.POST('/api/payments/bookings/{bookingId}/pay', { params: { path: { bookingId } } }),
      ),
  })
}

export function usePaidBookings() {
  return useQuery({
    queryKey: ['payments', 'invoices'],
    queryFn: () => unwrap(api.GET('/api/payments/invoices')),
  })
}

/** Fetches the Stripe-hosted invoice links on demand (the invoice may take a moment to exist). */
export function useOpenInvoice() {
  return useMutation({
    mutationFn: (bookingId: string) =>
      unwrap(
        api.GET('/api/payments/bookings/{bookingId}/invoice', { params: { path: { bookingId } } }),
      ),
  })
}

export function useRefundPreview(bookingId: string) {
  return useQuery({
    queryKey: ['payments', 'refund-preview', bookingId],
    queryFn: () =>
      unwrap(
        api.GET('/api/payments/bookings/{bookingId}/refund-preview', {
          params: { path: { bookingId } },
        }),
      ),
  })
}
