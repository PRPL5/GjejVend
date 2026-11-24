import { useTranslation } from 'react-i18next'
import { Button, ErrorNotice } from '@/components/ui'
import { formatMoney } from '@/lib/format'
import { usePayBooking } from './api'

interface PaymentStepProps {
  bookingId: string
  /** Shown on the button when the checkout already knows the total. */
  amount?: number
  currency?: string
  /** Replaceable in tests; by default the browser is sent to Stripe Checkout. */
  redirect?: (url: string) => void
}

/** The payment step of checkout: creates the payment, then hands the driver to Stripe's hosted page. */
export function PaymentStep({
  bookingId,
  amount,
  currency,
  redirect = (url) => window.location.assign(url),
}: PaymentStepProps) {
  const { t } = useTranslation()
  const pay = usePayBooking()

  const label = amount === undefined ? t('payment.payNow') : t('payment.payAmount', { amount: formatMoney(amount, currency) })

  function start() {
    pay.mutate(bookingId, {
      onSuccess: (result) => {
        if (result.checkoutUrl) redirect(result.checkoutUrl)
      },
    })
  }

  return (
    <div className="flex flex-col gap-3">
      <ErrorNotice error={pay.error} />
      <Button onClick={start} disabled={pay.isPending || pay.isSuccess}>
        {pay.isPending || pay.isSuccess ? t('payment.redirecting') : label}
      </Button>
      {pay.isError ? (
        <Button variant="secondary" onClick={start}>
          {t('common.retry')}
        </Button>
      ) : null}
      <p className="text-xs text-slate-500">{t('payment.secure')}</p>
    </div>
  )
}
