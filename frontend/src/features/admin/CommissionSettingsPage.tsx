import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Button, Card, ErrorNotice, Loading, Notice, PageShell, Stat } from '@/components/ui'
import { formatMoney } from '@/lib/format'
import { AdminNav } from './AdminNav'
import { useAdminSettings, useCommissionSummary, useSetCommissionRate, useSetRefundPolicy } from './api'

/** Turns "12,5" or "12.5" into 0.125, or null when it is not a percentage between 0 and 100. */
export function parsePercent(input: string): number | null {
  const value = Number(input.trim().replace(',', '.'))
  if (input.trim() === '' || !Number.isFinite(value) || value < 0 || value > 100) return null
  return Math.round(value * 100) / 10_000
}

// The mutation lives in the page: the form remounts when the saved rate comes back, which would drop its state.
function CommissionForm({ rate, save }: { rate: number; save: ReturnType<typeof useSetCommissionRate> }) {
  const { t } = useTranslation()
  const [value, setValue] = useState(String(Math.round(rate * 10_000) / 100))
  const [invalid, setInvalid] = useState(false)

  function submit(event: React.FormEvent) {
    event.preventDefault()
    const parsed = parsePercent(value)
    setInvalid(parsed === null)
    if (parsed !== null) save.mutate(parsed)
  }

  return (
    <form onSubmit={submit} className="flex flex-col gap-2">
      <label className="text-sm font-semibold" htmlFor="commission-rate">
        {t('admin.commissionRate')}
      </label>
      <input
        id="commission-rate"
        inputMode="decimal"
        className="min-h-11 rounded-lg border border-slate-300 px-3"
        value={value}
        onChange={(event) => setValue(event.target.value)}
        aria-invalid={invalid}
      />
      <p className="text-xs text-slate-500">{t('admin.commissionHelp')}</p>
      {invalid ? <p role="alert" className="text-sm text-red-700">{t('admin.invalidRate')}</p> : null}
      <ErrorNotice error={save.error} />
      {save.isSuccess ? <Notice tone="success">{t('admin.saved')}</Notice> : null}
      <Button type="submit" disabled={save.isPending}>
        {t('common.save')}
      </Button>
    </form>
  )
}

function RefundPolicyForm({ current, options }: { current: string; options: string[] }) {
  const { t } = useTranslation()
  const save = useSetRefundPolicy()

  return (
    <div className="flex flex-col gap-2">
      <label className="text-sm font-semibold" htmlFor="refund-policy">
        {t('admin.refundPolicy')}
      </label>
      <select
        id="refund-policy"
        className="min-h-11 rounded-lg border border-slate-300 px-3"
        value={current}
        disabled={save.isPending}
        onChange={(event) => save.mutate(event.target.value)}
      >
        {options.map((policy) => (
          <option key={policy} value={policy}>
            {t(`admin.policies.${policy}`, { defaultValue: policy })}
          </option>
        ))}
      </select>
      <ErrorNotice error={save.error} />
    </div>
  )
}

export function CommissionSettingsPage() {
  const { t } = useTranslation()
  const settings = useAdminSettings()
  const saveCommission = useSetCommissionRate()
  const [now] = useState(() => new Date())
  const summary = useCommissionSummary(new Date(now.getTime() - 30 * 86_400_000).toISOString(), now.toISOString())

  return (
    <PageShell title={t('admin.settingsTitle')}>
      <AdminNav />
      {settings.isPending ? <Loading /> : null}
      <ErrorNotice error={settings.error} />

      {settings.data ? (
        <Card>
          <div className="flex flex-col gap-6">
            <CommissionForm key={settings.data.commissionRate} rate={settings.data.commissionRate ?? 0} save={saveCommission} />
            <RefundPolicyForm
              current={settings.data.refundPolicy ?? ''}
              options={settings.data.availableRefundPolicies ?? []}
            />
          </div>
        </Card>
      ) : null}

      <Card title={t('admin.last30')}>
        <ErrorNotice error={summary.error} />
        {summary.data ? (
          <dl className="grid grid-cols-2 gap-4">
            <Stat label={t('admin.platformCommission')} value={formatMoney(summary.data.commission, summary.data.currency)} />
            <Stat label={t('owner.gross')} value={formatMoney(summary.data.gross, summary.data.currency)} />
            <Stat label={t('owner.refunded')} value={formatMoney(summary.data.refunded, summary.data.currency)} />
            <Stat label={t('owner.bookings', { count: summary.data.paidBookings ?? 0 })} value={String(summary.data.paidBookings ?? 0)} />
          </dl>
        ) : null}
      </Card>
    </PageShell>
  )
}
