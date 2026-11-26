import { screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import type { Schemas } from '@/api/client'
import { json, mockApi, renderRoute, route } from '@/test-utils'

const locationId = '11111111-1111-1111-1111-111111111111'
const bookingId = '22222222-2222-2222-2222-222222222222'

function ticket(overrides: Partial<Schemas['TicketView']> = {}): Schemas['TicketView'] {
  return {
    id: bookingId,
    reference: 'GVAB12CD34',
    status: 'confirmed',
    locationId,
    locationName: 'Parkingu Qendra',
    addressLine: 'Rr. Agim Ramadani 1',
    latitude: 42.66,
    longitude: 21.16,
    arrivalInstructions: null,
    accessInstructions: 'Kodi i portës 4321',
    vehiclePlate: '01ABC123',
    vehicleType: 'car',
    startsAt: new Date(Date.now() + 3_600_000).toISOString(),
    endsAt: new Date(Date.now() + 3 * 3_600_000).toISOString(),
    expiresAt: null,
    totalAmount: 4.5,
    currency: 'EUR',
    ticketToken: 'abc123token',
    checkedInAt: null,
    checkedOutAt: null,
    cancelledAt: null,
    cancellationReason: null,
    hasReview: false,
    ...overrides,
  }
}

function preview(overrides: Partial<Schemas['CheckoutPreview']> = {}): Schemas['CheckoutPreview'] {
  return {
    locationId,
    locationName: 'Parkingu Qendra',
    addressLine: 'Rr. Agim Ramadani 1',
    isOpen: true,
    freeCapacity: 4,
    totalAmount: 3,
    currency: 'EUR',
    minutes: 120,
    defaultVehicle: { id: '33333333-3333-3333-3333-333333333333', plate: '01XYZ999', vehicleType: 'suv' },
    ...overrides,
  }
}

beforeEach(() => localStorage.clear())
afterEach(() => vi.unstubAllGlobals())

describe('checkout', () => {
  test('prefills the default vehicle, books and lands on the ticket', async () => {
    const calls = mockApi((request) => {
      if (route(request, 'GET', '/bookings/preview')) return json(preview())
      if (route(request, 'GET', '/me/favorites')) return json([])
      if (route(request, 'POST', '/bookings')) return json(ticket({ status: 'pending', ticketToken: null }), 201)
      if (route(request, 'GET', `/bookings/${bookingId}`)) return json(ticket({ status: 'pending', ticketToken: null }))
    })
    const { router } = renderRoute(`/checkout/${locationId}`)

    expect(await screen.findByText('4 vende të lira')).toBeInTheDocument()
    expect(screen.getByLabelText('Targa')).toHaveValue('01XYZ999')
    expect(screen.getByLabelText('Lloji i automjetit')).toHaveValue('suv')

    await userEvent.click(screen.getByRole('button', { name: 'Rezervo' }))

    await waitFor(() => expect(router.state.location.pathname).toBe(`/bookings/${bookingId}`))
    const body = await calls.find((r) => route(r, 'POST', '/bookings'))!.json()
    expect(body).toMatchObject({ locationId, vehiclePlate: '01XYZ999', vehicleType: 'suv' })
  })

  test('shows a full lot and blocks booking', async () => {
    mockApi((request) => {
      if (route(request, 'GET', '/bookings/preview')) return json(preview({ freeCapacity: 0 }))
      if (route(request, 'GET', '/me/favorites')) return json([])
    })
    renderRoute(`/checkout/${locationId}`)

    expect(await screen.findByText('Plot')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Rezervo' })).toBeDisabled()
  })

  test('explains a conflict from the API', async () => {
    mockApi((request) => {
      if (route(request, 'GET', '/bookings/preview')) return json(preview())
      if (route(request, 'GET', '/me/favorites')) return json([])
      if (route(request, 'POST', '/bookings')) return json({ code: 'booking_no_capacity', title: 'Full' }, 409)
    })
    renderRoute(`/checkout/${locationId}`)

    await userEvent.click(await screen.findByRole('button', { name: 'Rezervo' }))

    expect(await screen.findByRole('alert')).toHaveTextContent('Parkingu është plot në këtë kohë.')
  })
})

describe('ticket', () => {
  test('shows QR, access instructions and countdown for a confirmed booking', async () => {
    mockApi((request) => (route(request, 'GET', `/bookings/${bookingId}`) ? json(ticket()) : undefined))
    renderRoute(`/bookings/${bookingId}`)

    expect(await screen.findByText('E konfirmuar')).toBeInTheDocument()
    expect(screen.getByTitle('Tregojeni këtë kod te hyrja.')).toBeInTheDocument()
    expect(screen.getByText('Kodi i portës 4321')).toBeInTheDocument()
    expect(screen.getByText(/^Fillon pas/)).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Navigo' })).toHaveAttribute('href', expect.stringContaining('42.66,21.16'))
  })

  test('shows the last saved ticket when offline', async () => {
    localStorage.setItem(`gjejvend:ticket:${bookingId}`, JSON.stringify(ticket()))
    mockApi(() => {
      throw new TypeError('Failed to fetch')
    })
    renderRoute(`/bookings/${bookingId}`)

    expect(await screen.findByText('Pa lidhje. Po shihni të dhënat e fundit të ruajtura.')).toBeInTheDocument()
    expect(screen.getByText('Parkingu Qendra')).toBeInTheDocument()
  })

  test('cancels with a reason', async () => {
    let current = ticket()
    const calls = mockApi((request) => {
      if (route(request, 'GET', `/bookings/${bookingId}`)) return json(current)
      if (route(request, 'POST', `/bookings/${bookingId}/cancel`)) {
        current = ticket({ status: 'cancelled', ticketToken: null, accessInstructions: null, cancellationReason: 'Ndryshim plani' })
        return json(null, 204)
      }
    })
    renderRoute(`/bookings/${bookingId}`)

    await userEvent.click(await screen.findByRole('button', { name: 'Anulo' }))
    const sheet = screen.getByRole('dialog', { name: 'Anulo rezervimin?' })
    await userEvent.type(within(sheet).getByLabelText('Arsyeja (opsionale)'), 'Ndryshim plani')
    await userEvent.click(within(sheet).getByRole('button', { name: 'Po, anuloje' }))

    expect(await screen.findByText('E anuluar')).toBeInTheDocument()
    expect(await calls.find((r) => route(r, 'POST', `/bookings/${bookingId}/cancel`))!.json()).toEqual({ reason: 'Ndryshim plani' })
  })

  test('asks for a review after a completed booking', async () => {
    const calls = mockApi((request) => {
      if (route(request, 'GET', `/bookings/${bookingId}`)) return json(ticket({ status: 'completed', ticketToken: null }))
      if (route(request, 'POST', `/bookings/${bookingId}/review`)) return json({ id: 'r1', rating: 5 }, 201)
    })
    renderRoute(`/bookings/${bookingId}`)

    await userEvent.click(await screen.findByRole('button', { name: '5 yje' }))
    await userEvent.click(screen.getByRole('button', { name: 'Dërgo vlerësimin' }))

    expect(await screen.findByText('Faleminderit për vlerësimin.')).toBeInTheDocument()
    expect(await calls.find((r) => route(r, 'POST', `/bookings/${bookingId}/review`))!.json()).toEqual({ rating: 5, comment: null })
  })
})

describe('lists', () => {
  test('my bookings splits upcoming and past with status words', async () => {
    mockApi((request) =>
      route(request, 'GET', '/bookings')
        ? json([
            { ...ticket(), id: 'a', status: 'confirmed', locationName: 'Lot A' },
            { ...ticket(), id: 'b', status: 'completed', locationName: 'Lot B' },
          ])
        : undefined,
    )
    renderRoute('/bookings')

    const upcoming = (await screen.findByRole('heading', { name: 'Të ardhshme' })).closest('section')!
    const past = screen.getByRole('heading', { name: 'Të kaluara' }).closest('section')!
    expect(within(upcoming).getByRole('link', { name: /Lot A/ })).toHaveAttribute('href', '/bookings/a')
    expect(within(past).getByText('E përfunduar')).toBeInTheDocument()
  })

  test('my bookings shows the empty history state', async () => {
    mockApi((request) => (route(request, 'GET', '/bookings') ? json([]) : undefined))
    renderRoute('/bookings')

    expect(await screen.findByText('Ende nuk keni rezervime.')).toBeInTheDocument()
  })

  test('notifications can be marked read', async () => {
    let unread = true
    const calls = mockApi((request) => {
      if (route(request, 'GET', '/me/notifications')) {
        return json({
          unreadCount: unread ? 1 : 0,
          items: [
            {
              id: 'n1',
              type: 'booking_cancelled',
              title: 'Booking cancelled',
              body: 'Booking GVAB12CD34',
              entityType: 'booking',
              entityId: bookingId,
              readAt: unread ? null : new Date().toISOString(),
              createdAt: new Date().toISOString(),
            },
          ],
        })
      }
      if (route(request, 'POST', '/me/notifications/n1/read')) {
        unread = false
        return json(null, 204)
      }
    })
    renderRoute('/notifications')

    expect(await screen.findByText('Rezervimi u anulua')).toBeInTheDocument()
    await userEvent.click(screen.getByRole('button', { name: 'Shëno si të lexuar' }))

    await waitFor(() => expect(screen.queryByRole('button', { name: 'Shëno si të lexuar' })).not.toBeInTheDocument())
    expect(calls.some((r) => route(r, 'POST', '/me/notifications/n1/read'))).toBe(true)
  })

  test('owner checks in an arrival', async () => {
    let status: Schemas['BookingStatus'] = 'confirmed'
    const arrival = () => ({
      id: bookingId,
      reference: 'GVAB12CD34',
      status,
      locationId,
      locationName: 'Parkingu Qendra',
      vehiclePlate: '01ABC123',
      vehicleType: 'car',
      startsAt: new Date().toISOString(),
      endsAt: new Date(Date.now() + 3_600_000).toISOString(),
      checkedInAt: null,
      checkedOutAt: null,
    })
    mockApi((request) => {
      if (route(request, 'GET', '/owner/arrivals')) return json([arrival()])
      if (route(request, 'POST', `/bookings/${bookingId}/check-in`)) {
        status = 'active'
        return json(arrival())
      }
    })
    renderRoute('/owner/arrivals')

    await userEvent.click(await screen.findByRole('button', { name: 'Regjistro hyrjen' }))

    expect(await screen.findByRole('button', { name: 'Regjistro daljen' })).toBeInTheDocument()
    expect(screen.getByRole('status')).toHaveTextContent('01ABC123: Në parking')
  })
})
