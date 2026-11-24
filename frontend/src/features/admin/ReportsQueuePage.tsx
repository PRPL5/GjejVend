import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Button, Card, ErrorNotice, Loading, PageShell } from '@/components/ui'
import { formatDate } from '@/lib/format'
import { AdminNav } from './AdminNav'
import { useReports, useResolveReport, type ReportAction, type ReportStatus } from './api'

const statuses: ReportStatus[] = ['Open', 'Reviewing', 'Resolved', 'Dismissed']

interface ReportTargets {
  locationId?: string | null
  reviewId?: string | null
  reportedUserId?: string | null
}

/** Only offer actions that make sense for what was reported. */
export function actionsFor(report: ReportTargets): ReportAction[] {
  const actions: ReportAction[] = ['Dismiss', 'Resolve']
  if (report.reviewId) actions.push('HideReview')
  if (report.locationId) actions.push('SuspendLot')
  return actions
}

function targetKind(report: ReportTargets): 'lot' | 'review' | 'user' {
  return report.locationId ? 'lot' : report.reviewId ? 'review' : 'user'
}

function ResolveForm({ id, report }: { id: string; report: ReportTargets }) {
  const { t } = useTranslation()
  const resolve = useResolveReport()
  const options = actionsFor(report)
  const [action, setAction] = useState<ReportAction>(options[0] ?? 'Dismiss')
  const [resolution, setResolution] = useState('')

  return (
    <div className="mt-3 flex flex-col gap-2">
      <label className="flex flex-col gap-1 text-sm font-semibold">
        {t('admin.reports.action')}
        <select
          className="min-h-11 rounded-lg border border-slate-300 px-3 font-normal"
          value={action}
          onChange={(event) => setAction(event.target.value as ReportAction)}
        >
          {options.map((value) => (
            <option key={value} value={value}>
              {t(`admin.reports.actions.${value}`)}
            </option>
          ))}
        </select>
      </label>
      <label className="flex flex-col gap-1 text-sm font-semibold">
        {t('admin.reports.resolution')}
        <textarea
          rows={2}
          className="rounded-lg border border-slate-300 p-3 font-normal"
          value={resolution}
          onChange={(event) => setResolution(event.target.value)}
        />
      </label>
      <ErrorNotice error={resolve.error} />
      <Button disabled={resolve.isPending || resolution.trim() === ''} onClick={() => resolve.mutate({ id, action, resolution })}>
        {t('admin.reports.submit')}
      </Button>
    </div>
  )
}

export function ReportsQueuePage() {
  const { t } = useTranslation()
  const [status, setStatus] = useState<ReportStatus>('Open')
  const reports = useReports(status)

  return (
    <PageShell title={t('admin.reports.title')}>
      <AdminNav />
      <label className="flex items-center gap-2 text-sm">
        {t('admin.filter')}
        <select
          className="min-h-11 rounded-lg border border-slate-300 px-3"
          value={status}
          onChange={(event) => setStatus(event.target.value as ReportStatus)}
        >
          {statuses.map((value) => (
            <option key={value} value={value}>
              {t(`admin.reports.status.${value}`)}
            </option>
          ))}
        </select>
      </label>

      {reports.isPending ? <Loading /> : null}
      <ErrorNotice error={reports.error} />
      {reports.data?.length === 0 ? <p className="text-slate-500">{t('admin.reports.empty')}</p> : null}

      <ul className="flex flex-col gap-3">
        {reports.data?.map((report) => (
          <li key={report.id}>
            <Card>
              <div className="flex flex-wrap items-center justify-between gap-2">
                <p className="font-semibold">
                  {t(`report.categories.${report.category ?? 'Other'}`)} · {t(`admin.reports.target.${targetKind(report)}`)}
                </p>
                <span className="text-sm text-slate-500">{formatDate(report.createdAt)}</span>
              </div>
              <p className="mt-2 text-sm">{report.description}</p>
              {report.resolution ? (
                <p className="mt-2 text-sm text-slate-600">
                  {t('admin.reports.resolved')}: {report.resolution}
                </p>
              ) : null}
              {(report.status === 'Open' || report.status === 'Reviewing') && report.id ? (
                <ResolveForm id={report.id} report={report} />
              ) : null}
            </Card>
          </li>
        ))}
      </ul>
    </PageShell>
  )
}
