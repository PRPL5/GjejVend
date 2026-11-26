import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api, unwrap, type Schemas } from '@/api/client'
import { useSession } from '@/store/session'

export type Ticket = Schemas['TicketView']
export type BookingStatus = Schemas['BookingStatus']
export type VehicleType = Schemas['VehicleType']

const keys = {
  bookings: ['bookings'] as const,
  ticket: (id: string) => ['bookings', id] as const,
  preview: (locationId: string, startsAt: string, endsAt: string) => ['preview', locationId, startsAt, endsAt] as const,
  arrivals: (from: string, to: string) => ['arrivals', from, to] as const,
  ownerReviews: ['owner-reviews'] as const,
  notifications: ['notifications'] as const,
  favorites: ['favorites'] as const,
}

const ticketStorageKey = (id: string) => `gjejvend:ticket:${id}`

function readSavedTicket(id: string): Ticket | undefined {
  try {
    const saved = localStorage.getItem(ticketStorageKey(id))
    return saved ? (JSON.parse(saved) as Ticket) : undefined
  } catch {
    return undefined
  }
}

function saveTicket(ticket: Ticket) {
  try {
    localStorage.setItem(ticketStorageKey(ticket.id), JSON.stringify(ticket))
  } catch {
    return
  }
}

function useSignedIn() {
  return useSession((s) => s.userId !== null)
}

export function useCheckoutPreview(locationId: string, startsAt: string, endsAt: string, enabled: boolean) {
  const signedIn = useSignedIn()
  return useQuery({
    queryKey: keys.preview(locationId, startsAt, endsAt),
    queryFn: () => unwrap(api.GET('/bookings/preview', { params: { query: { LocationId: locationId, StartsAt: startsAt, EndsAt: endsAt } } })),
    enabled: enabled && signedIn,
    retry: false,
  })
}

export function useCreateBooking() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (body: Schemas['CreateBookingRequest']) => unwrap(api.POST('/bookings', { body })),
    onSuccess: (ticket) => {
      saveTicket(ticket)
      queryClient.setQueryData(keys.ticket(ticket.id), ticket)
      void queryClient.invalidateQueries({ queryKey: keys.bookings, exact: true })
    },
  })
}

export function useMyBookings() {
  return useQuery({
    queryKey: keys.bookings,
    queryFn: () => unwrap(api.GET('/bookings')),
    enabled: useSignedIn(),
  })
}

export function useTicket(id: string) {
  return useQuery({
    queryKey: keys.ticket(id),
    queryFn: async () => {
      const ticket = await unwrap(api.GET('/bookings/{id}', { params: { path: { id } } }))
      saveTicket(ticket)
      return ticket
    },
    initialData: () => readSavedTicket(id),
    initialDataUpdatedAt: 0,
    enabled: useSignedIn(),
    refetchInterval: 60_000,
  })
}

function useTicketMutation<TBody>(id: string, run: (body: TBody) => Promise<unknown>) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: run,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: keys.bookings })
      void queryClient.invalidateQueries({ queryKey: keys.notifications })
    },
    onError: () => void queryClient.invalidateQueries({ queryKey: keys.ticket(id) }),
  })
}

export function useExtendBooking(id: string) {
  return useTicketMutation(id, (endsAt: string) =>
    unwrap(api.POST('/bookings/{id}/extend', { params: { path: { id } }, body: { endsAt } })),
  )
}

export function useCancelBooking(id: string) {
  return useTicketMutation(id, (reason: string) =>
    unwrap(api.POST('/bookings/{id}/cancel', { params: { path: { id } }, body: { reason: reason.trim() || null } })),
  )
}

export function useReviewBooking(id: string) {
  return useTicketMutation(id, (body: Schemas['ReviewRequest']) =>
    unwrap(api.POST('/bookings/{id}/review', { params: { path: { id } }, body })),
  )
}

export function useArrivals(from: string, to: string) {
  return useQuery({
    queryKey: keys.arrivals(from, to),
    queryFn: () => unwrap(api.GET('/owner/arrivals', { params: { query: { From: from, To: to } } })),
    enabled: useSignedIn(),
    refetchInterval: 30_000,
  })
}

type DeskAction = { kind: 'check-in' | 'check-out'; bookingId: string } | { kind: 'scan-in' | 'scan-out'; locationId: string; token: string }

export function useDeskAction() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (action: DeskAction) => {
      switch (action.kind) {
        case 'check-in':
          return unwrap(api.POST('/bookings/{id}/check-in', { params: { path: { id: action.bookingId } } }))
        case 'check-out':
          return unwrap(api.POST('/bookings/{id}/check-out', { params: { path: { id: action.bookingId } } }))
        case 'scan-in':
          return unwrap(api.POST('/bookings/check-in', { body: { locationId: action.locationId, token: action.token } }))
        case 'scan-out':
          return unwrap(api.POST('/bookings/check-out', { body: { locationId: action.locationId, token: action.token } }))
      }
    },
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: ['arrivals'] }),
  })
}

export function useOwnerReviews() {
  return useQuery({ queryKey: keys.ownerReviews, queryFn: () => unwrap(api.GET('/owner/reviews')), enabled: useSignedIn() })
}

export function useRespondToReview() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, response }: { id: string; response: string }) =>
      unwrap(api.POST('/reviews/{id}/response', { params: { path: { id } }, body: { response } })),
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: keys.ownerReviews }),
  })
}

export function useNotifications() {
  return useQuery({
    queryKey: keys.notifications,
    queryFn: () => unwrap(api.GET('/me/notifications')),
    enabled: useSignedIn(),
    refetchInterval: 60_000,
  })
}

export function useMarkNotificationsRead() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string | null) =>
      id
        ? unwrap(api.POST('/me/notifications/{id}/read', { params: { path: { id } } }))
        : unwrap(api.POST('/me/notifications/read-all')),
    onSettled: () => void queryClient.invalidateQueries({ queryKey: keys.notifications }),
  })
}

export function useFavorites() {
  return useQuery({ queryKey: keys.favorites, queryFn: () => unwrap(api.GET('/me/favorites')), enabled: useSignedIn() })
}

export function useToggleFavorite() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ locationId, saved }: { locationId: string; saved: boolean }) =>
      saved
        ? unwrap(api.DELETE('/me/favorites/{locationId}', { params: { path: { locationId } } }))
        : unwrap(api.PUT('/me/favorites/{locationId}', { params: { path: { locationId } } })),
    onSettled: () => void queryClient.invalidateQueries({ queryKey: keys.favorites }),
  })
}
