import { useMutation, useQuery } from '@tanstack/react-query'
import { api, unwrap } from '@/api/http'

export interface Range {
  from: string
  to: string
}

export function useEarnings(range: Range) {
  return useQuery({
    queryKey: ['owner', 'earnings', range.from, range.to],
    queryFn: () => unwrap(api.GET('/api/owner/earnings', { params: { query: range } })),
    retry: false,
  })
}

export function useMyPayouts() {
  return useQuery({
    queryKey: ['owner', 'payouts'],
    queryFn: () => unwrap(api.GET('/api/owner/payouts')),
    retry: false,
  })
}

export function useStartPayoutSetup() {
  return useMutation({
    mutationFn: () => unwrap(api.POST('/api/owner/stripe/onboarding', { body: {} })),
  })
}
