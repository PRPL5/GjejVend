import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, expect, test, vi } from 'vitest'
import { jsonResponse, renderWithProviders } from '@/test-utils'
import { PaymentStep } from './PaymentStep'

afterEach(() => vi.restoreAllMocks())

test('sends the driver to Stripe checkout after creating the payment', async () => {
  const fetchMock = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
    jsonResponse({ paymentId: 'p1', reference: 'cs_1', checkoutUrl: 'https://checkout.stripe.test/c/cs_1', amount: 8, currency: 'EUR' }),
  )
  const redirect = vi.fn()

  renderWithProviders(<PaymentStep bookingId="b1" amount={8} currency="EUR" redirect={redirect} />)
  await userEvent.click(screen.getByRole('button', { name: /8/ }))

  await waitFor(() => expect(redirect).toHaveBeenCalledWith('https://checkout.stripe.test/c/cs_1'))
  const request = fetchMock.mock.calls[0]?.[0] as Request
  expect(request.method).toBe('POST')
  expect(request.url).toContain('/api/payments/bookings/b1/pay')
})

test('shows the server message when the booking can no longer be paid', async () => {
  vi.spyOn(globalThis, 'fetch').mockResolvedValue(
    jsonResponse({ title: 'Business rule violated', detail: 'The payment window for this booking has expired.' }, 400),
  )

  renderWithProviders(<PaymentStep bookingId="b1" redirect={vi.fn()} />)
  await userEvent.click(screen.getByRole('button', { name: /paguaj tani/i }))

  expect(await screen.findByRole('alert')).toHaveTextContent('The payment window for this booking has expired.')
  expect(screen.getByRole('button', { name: /provo përsëri/i })).toBeEnabled()
})

test('shows a network message when the server cannot be reached', async () => {
  vi.spyOn(globalThis, 'fetch').mockRejectedValue(new TypeError('Failed to fetch'))

  renderWithProviders(<PaymentStep bookingId="b1" redirect={vi.fn()} />)
  await userEvent.click(screen.getByRole('button', { name: /paguaj tani/i }))

  expect(await screen.findByRole('alert')).toHaveTextContent(/lidhemi me serverin/i)
})
