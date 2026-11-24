import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Button, Card, ErrorNotice, Loading, PageShell } from '@/components/ui'
import { formatDate, formatMoney, lastFinishedWeek } from '@/lib/format'
import { AdminNav } from './AdminNav'
import { useAdminPayouts, useRunPayouts, type PayoutStatus } from './api'

const statuses: PayoutStatus[] = ['Pending', 'Processing', 'Paid', 'Failed']

function RunPayoutsCard() {
  const { t } = useTranslation()
  const run = useRunPayouts()
  const [period, setPeriod] = useState(lastFinishedWeek)

  return (
    <Card title={t('admin.runPayouts')}>
      <p className="mb-3 text-sm text-slate-600">{t('admin.runHint')}</p>
      <div className="mb-3 flex flex-wrap gap-3">
        <label className="flex flex-col text-sm">
          {t('admin.from')}
          <input
            type="date"
            className="min-h-11 rounded-lg border border-slate-300 px-3"
            value={period.start}
            onChange={(event) => setPeriod({ ...period, start: event.target.value })}
          />
        </label>
        <label className="flex flex-col text-sm">
          {t('admin.to')}
          <input
            type="date"
            className="min-h-11 rounded-lg border border-slate-300 px-3"
            value={period.end}
            onChange={(event) => setPeriod({ ...period, end: event.target.value })}
          />
        </label>
      </div>
      <ErrorNotice error={run.error} />
      <Button onClick={() => run.mutate({ periodStart: period.start, periodEnd: period.end })} disabled={run.isPending}>
        {run.isPending ? t('admin.running') : t('admin.runPayouts')}
      </Button>

      {run.data ? (
        <ul className="mt-4 flex flex-col divide-y divide-slate-100 text-sm">
          {run.data.map((item) => (
            <li key={item.providerId} className="flex items-center justify-between gap-3 py-2">
              <span className="font-semibold">{item.providerName}</span>
              <span>{t(`admin.outcomes.${item.outcome ?? 'skipped'}`, { defaultValue: item.outcome ?? '' })}</span>
              <span>{formatMoney(item.amount, 'EUR')}</span>
              {item.detail ? <span className="text-xs text-slate-500">{item.detail}</span> : null}
            </li>
          ))}
        </ul>
      ) : null}
    </Card>
  )
}

export function AdminPayoutsPage() {
  const { t } = useTranslation()
  const [status, setStatus] = useState<PayoutStatus | undefined>()
  const payouts = useAdminPayouts(status)

  return (
    <PageShell title={t('admin.payoutsTitle')}>
      <AdminNav />
      <RunPayoutsCard />

      <Card>
        <label className="mb-3 flex items-center gap-2 text-sm">
          {t('admin.filter')}
          <select
            className="min-h-11 rounded-lg border border-slate-300 px-3"
            value={status ?? ''}
            onChange={(event) => setStatus((event.target.value || undefined) as PayoutStatus | undefined)}
          >
            <option value="">{t('admin.allStatuses')}</option>
            {statuses.map((value) => (
              <option key={value} value={value}>
                {t(`admin.payoutStatus.${value}`)}
              </option>
            ))}
          </select>
        </label>

        {payouts.isPending ? <Loading /> : null}
        <ErrorNotice error={payouts.error} />
        {payouts.data?.length === 0 ? <p className="text-sm text-slate-500">{t('owner.noPayouts')}</p> : null}
        <ul className="flex flex-col divide-y divide-slate-100 text-sm">
          {payouts.data?.map((payout) => (
            <li key={payout.id} className="flex flex-wrap items-center justify-between gap-2 py-2">
              <span className="font-semibold">{payout.providerName}</span>
              <span>
                {formatDate(payout.periodStart)} – {formatDate(payout.periodEnd)}
              </span>
              <span>{formatMoney(payout.amount, payout.currency)}</span>
              <span className="text-slate-500">{t(`admin.payoutStatus.${payout.status ?? 'Pending'}`)}</span>
            </li>
          ))}
        </ul>
      </Card>
    </PageShell>
  )
}
