import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api, unwrap } from '@/api/http'
import type { components } from '@/api/generated/schema'

export type PayoutStatus = components['schemas']['PayoutStatus']

export function useAdminSettings() {
  return useQuery({
    queryKey: ['admin', 'settings'],
    queryFn: () => unwrap(api.GET('/api/admin/settings')),
    retry: false,
  })
}

export function useSetCommissionRate() {
  const client = useQueryClient()
  return useMutation({
    mutationFn: (rate: number) => unwrap(api.PUT('/api/admin/settings/commission-rate', { body: { rate } })),
    onSuccess: (view) => client.setQueryData(['admin', 'settings'], view),
  })
}

export function useSetRefundPolicy() {
  const client = useQueryClient()
  return useMutation({
    mutationFn: (policy: string) => unwrap(api.PUT('/api/admin/settings/refund-policy', { body: { policy } })),
    onSuccess: (view) => client.setQueryData(['admin', 'settings'], view),
  })
}

export function useCommissionSummary(from: string, to: string) {
  return useQuery({
    queryKey: ['admin', 'commission-summary', from, to],
    queryFn: () => unwrap(api.GET('/api/admin/commission-summary', { params: { query: { from, to } } })),
    retry: false,
  })
}

export function useAdminPayouts(status: PayoutStatus | undefined) {
  return useQuery({
    queryKey: ['admin', 'payouts', status ?? 'all'],
    queryFn: () => unwrap(api.GET('/api/admin/payouts', { params: { query: { status } } })),
    retry: false,
  })
}

export function useRunPayouts() {
  const client = useQueryClient()
  return useMutation({
    mutationFn: (period: { periodStart: string; periodEnd: string }) =>
      unwrap(api.POST('/api/admin/payouts/run', { body: period })),
    onSuccess: () => client.invalidateQueries({ queryKey: ['admin', 'payouts'] }),
  })
}
