import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api, unwrap } from '@/api/http'
import type { components } from '@/api/generated/schema'

export type PayoutStatus = components['schemas']['PayoutStatus']
export type VerificationStatus = components['schemas']['VerificationRequestStatus']
export type ReportStatus = components['schemas']['ReportStatus']
export type ReportAction = components['schemas']['ReportAction']

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

export function useVerifications(status: VerificationStatus) {
  return useQuery({
    queryKey: ['admin', 'verifications', status],
    queryFn: () => unwrap(api.GET('/api/admin/verifications', { params: { query: { status } } })),
    retry: false,
  })
}

export function useReviewVerification() {
  const client = useQueryClient()
  return useMutation({
    mutationFn: (input: { id: string; approve: boolean; notes?: string }) =>
      unwrap(
        api.POST('/api/admin/verifications/{id}/review', {
          params: { path: { id: input.id } },
          body: { approve: input.approve, notes: input.notes },
        }),
      ),
    onSuccess: () => client.invalidateQueries({ queryKey: ['admin', 'verifications'] }),
  })
}

export function useReports(status: ReportStatus) {
  return useQuery({
    queryKey: ['admin', 'reports', status],
    queryFn: () => unwrap(api.GET('/api/admin/reports', { params: { query: { status } } })),
    retry: false,
  })
}

export function useResolveReport() {
  const client = useQueryClient()
  return useMutation({
    mutationFn: (input: { id: string; action: ReportAction; resolution: string }) =>
      unwrap(
        api.POST('/api/admin/reports/{id}/resolve', {
          params: { path: { id: input.id } },
          body: { action: input.action, resolution: input.resolution },
        }),
      ),
    onSuccess: () => client.invalidateQueries({ queryKey: ['admin', 'reports'] }),
  })
}

export interface AuditFilters {
  entityType?: string
  from?: string
  to?: string
  page: number
}

export function useAuditLogs(filters: AuditFilters) {
  return useQuery({
    queryKey: ['admin', 'audit', filters],
    queryFn: () => unwrap(api.GET('/api/admin/audit-logs', { params: { query: { ...filters, pageSize: 25 } } })),
    retry: false,
    placeholderData: (previous) => previous,
  })
}
