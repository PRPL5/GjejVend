import { useTranslation } from 'react-i18next'
import { Link, useParams, useSearchParams } from 'react-router-dom'
import { Card, Notice, PageShell } from '@/components/ui'
import { PaymentStep } from './PaymentStep'

/** Where Stripe sends the driver back: shows the outcome, or the pay button while the booking is unpaid. */
export function PaymentPage() {
  const { t } = useTranslation()
  const { bookingId = '' } = useParams()
  const [search] = useSearchParams()
  const outcome = search.get('payment')

  return (
    <PageShell title={t('payment.title')}>
      {outcome === 'success' ? (
        <Card>
          <div className="flex flex-col gap-3">
            <Notice tone="success">{t('payment.success')}</Notice>
            <p className="text-sm text-slate-600">{t('payment.successHint')}</p>
            <Link className="text-sm font-semibold text-emerald-700 underline" to="/me/invoices">
              {t('payment.viewInvoices')}
            </Link>
          </div>
        </Card>
      ) : (
        <Card>
          <div className="flex flex-col gap-3">
            {outcome === 'cancelled' ? <Notice tone="warning">{t('payment.cancelled')}</Notice> : null}
            <PaymentStep bookingId={bookingId} />
          </div>
        </Card>
      )}
    </PageShell>
  )
}
