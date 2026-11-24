import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useSearchParams } from 'react-router-dom'
import { ApiError } from '@/api/http'
import { Button, Card, ErrorNotice, Loading, Notice, PageShell, Stat } from '@/components/ui'
import { formatDate, formatMoney } from '@/lib/format'
import { useEarnings, useMyPayouts, useStartPayoutSetup, type Range } from './api'

function startOfToday(now: Date): Date {
  return new Date(now.getFullYear(), now.getMonth(), now.getDate())
}

function EarningsCard({ title, range }: { title: string; range: Range }) {
  const { t } = useTranslation()
  const earnings = useEarnings(range)

  return (
    <Card title={title}>
      {earnings.isPending ? <Loading /> : null}
      <ErrorNotice error={earnings.error} />
      {earnings.data ? (
        <dl className="grid grid-cols-2 gap-4">
          <Stat
            label={t('owner.net')}
            value={formatMoney(earnings.data.net, earnings.data.currency)}
            hint={t('owner.bookings', { count: earnings.data.paidBookings ?? 0 })}
          />
          <Stat label={t('owner.gross')} value={formatMoney(earnings.data.gross, earnings.data.currency)} />
          <Stat label={t('owner.commission')} value={formatMoney(earnings.data.commission, earnings.data.currency)} />
          <Stat label={t('owner.refunded')} value={formatMoney(earnings.data.refunded, earnings.data.currency)} />
        </dl>
      ) : null}
    </Card>
  )
}

export function OwnerDashboardPage() {
  const { t } = useTranslation()
  const [search] = useSearchParams()
  // Fixed when the page opens, so the query keys stay stable while it is displayed.
  const [now] = useState(() => new Date())
  const today: Range = { from: startOfToday(now).toISOString(), to: now.toISOString() }
  const week: Range = { from: new Date(now.getTime() - 7 * 86_400_000).toISOString(), to: now.toISOString() }

  const payouts = useMyPayouts()
  const setup = useStartPayoutSetup()
  const payoutsState = search.get('payouts')

  const notAProvider = payouts.error instanceof ApiError && payouts.error.status === 403

  function startSetup() {
    setup.mutate(undefined, {
      onSuccess: (link) => {
        if (link.url) window.location.assign(link.url)
      },
    })
  }

  if (notAProvider) {
    return (
      <PageShell title={t('owner.title')}>
        <Notice tone="warning">{t('owner.notProvider')}</Notice>
      </PageShell>
    )
  }

  return (
    <PageShell title={t('owner.title')}>
      {payoutsState === 'ready' ? <Notice tone="success">{t('owner.payoutsReady')}</Notice> : null}
      {payoutsState === 'refresh' ? <Notice tone="warning">{t('owner.payoutsRefresh')}</Notice> : null}

      <EarningsCard title={t('owner.today')} range={today} />
      <EarningsCard title={t('owner.week')} range={week} />

      <Card title={t('owner.payouts')}>
        <div className="mb-4 flex flex-col gap-2">
          <p className="text-sm text-slate-600">{t('owner.setupHint')}</p>
          <ErrorNotice error={setup.error} />
          <Button variant="secondary" onClick={startSetup} disabled={setup.isPending}>
            {t('owner.setupPayouts')}
          </Button>
        </div>
        {payouts.isPending ? <Loading /> : null}
        {payouts.data?.length === 0 ? <p className="text-sm text-slate-500">{t('owner.noPayouts')}</p> : null}
        <ul className="flex flex-col divide-y divide-slate-100">
          {payouts.data?.map((payout) => (
            <li key={payout.id} className="flex items-center justify-between py-2 text-sm">
              <span>
                {formatDate(payout.periodStart)} – {formatDate(payout.periodEnd)}
              </span>
              <span className="font-semibold">{formatMoney(payout.amount, payout.currency)}</span>
              <span className="text-slate-500">{t(`admin.payoutStatus.${payout.status ?? 'Pending'}`)}</span>
            </li>
          ))}
        </ul>
      </Card>
    </PageShell>
  )
}
