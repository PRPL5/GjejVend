import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Button, Card, ErrorNotice, Loading, PageShell } from '@/components/ui'
import { AdminNav } from './AdminNav'
import { useAuditLogs } from './api'

const entityTypes = [
  'payments',
  'refunds',
  'provider_payouts',
  'bookings',
  'parking_locations',
  'parking_providers',
  'reviews',
  'reports',
  'verification_requests',
  'platform_settings',
  'user_roles',
]

/** Metadata arrives as JSON text; show it indented when it parses, as-is when it does not. */
export function prettyMetadata(metadata: string | null | undefined): string {
  if (!metadata) return ''
  try {
    return JSON.stringify(JSON.parse(metadata), null, 2)
  } catch {
    return metadata
  }
}

function startOfDay(value: string): string | undefined {
  return value ? new Date(`${value}T00:00:00`).toISOString() : undefined
}

function dayAfter(value: string): string | undefined {
  if (!value) return undefined
  const date = new Date(`${value}T00:00:00`)
  date.setDate(date.getDate() + 1)
  return date.toISOString()
}

export function AuditLogPage() {
  const { t } = useTranslation()
  const [entityType, setEntityType] = useState('')
  const [from, setFrom] = useState('')
  const [to, setTo] = useState('')
  const [page, setPage] = useState(1)
  const [openRow, setOpenRow] = useState<string | null>(null)

  const logs = useAuditLogs({ entityType: entityType || undefined, from: startOfDay(from), to: dayAfter(to), page })
  const total = logs.data?.total ?? 0
  const pageSize = logs.data?.pageSize ?? 25
  const lastPage = Math.max(1, Math.ceil(total / pageSize))

  function changeFilter(apply: () => void) {
    apply()
    setPage(1)
    setOpenRow(null)
  }

  return (
    <PageShell title={t('admin.audit.title')}>
      <AdminNav />
      <Card>
        <div className="flex flex-wrap gap-3">
          <label className="flex flex-col text-sm">
            {t('admin.audit.entity')}
            <select
              className="min-h-11 rounded-lg border border-slate-300 px-3"
              value={entityType}
              onChange={(event) => changeFilter(() => setEntityType(event.target.value))}
            >
              <option value="">{t('admin.allStatuses')}</option>
              {entityTypes.map((value) => (
                <option key={value} value={value}>
                  {value}
                </option>
              ))}
            </select>
          </label>
          <label className="flex flex-col text-sm">
            {t('admin.from')}
            <input
              type="date"
              className="min-h-11 rounded-lg border border-slate-300 px-3"
              value={from}
              onChange={(event) => changeFilter(() => setFrom(event.target.value))}
            />
          </label>
          <label className="flex flex-col text-sm">
            {t('admin.to')}
            <input
              type="date"
              className="min-h-11 rounded-lg border border-slate-300 px-3"
              value={to}
              onChange={(event) => changeFilter(() => setTo(event.target.value))}
            />
          </label>
        </div>
      </Card>

      {logs.isPending ? <Loading /> : null}
      <ErrorNotice error={logs.error} />
      {logs.data?.items?.length === 0 ? <p className="text-slate-500">{t('admin.audit.empty')}</p> : null}

      <ul className="flex flex-col gap-2">
        {logs.data?.items?.map((entry) => (
          <li key={entry.id}>
            <Card>
              <button
                type="button"
                className="flex w-full flex-wrap items-center justify-between gap-2 text-left text-sm"
                aria-expanded={openRow === entry.id}
                onClick={() => setOpenRow(openRow === entry.id ? null : (entry.id ?? null))}
              >
                <span className="font-semibold">
                  {entry.action} · {entry.entityType}
                </span>
                <span className="text-slate-500">{entry.createdAt ? new Date(entry.createdAt).toLocaleString('sq') : ''}</span>
              </button>
              <p className="mt-1 text-xs text-slate-500">
                {t('admin.audit.actor')}: {entry.actorId ?? t('admin.audit.system')}
              </p>
              {openRow === entry.id ? (
                <pre className="mt-2 max-h-72 overflow-auto rounded-lg bg-slate-100 p-3 text-xs">{prettyMetadata(entry.metadata)}</pre>
              ) : null}
            </Card>
          </li>
        ))}
      </ul>

      <div className="flex items-center justify-between text-sm">
        <Button variant="secondary" disabled={page <= 1} onClick={() => setPage(page - 1)}>
          {t('admin.audit.previous')}
        </Button>
        <span>{t('admin.audit.page', { page, pages: lastPage })}</span>
        <Button variant="secondary" disabled={page >= lastPage} onClick={() => setPage(page + 1)}>
          {t('admin.audit.next')}
        </Button>
      </div>
    </PageShell>
  )
}
