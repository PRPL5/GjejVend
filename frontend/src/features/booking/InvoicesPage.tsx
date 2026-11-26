import { useTranslation } from 'react-i18next'
import { Button, Card, ErrorNotice, Loading, PageShell } from '@/components/ui'
import { formatDate, formatMoney } from '@/lib/format'
import { useOpenInvoice, usePaidBookings } from './api'

/** Opens the Stripe-hosted invoice for one booking. Reusable from the My bookings cards. */
export function InvoiceButton({ bookingId }: { bookingId: string }) {
  const { t } = useTranslation()
  const invoice = useOpenInvoice()

  function open() {
    invoice.mutate(bookingId, {
      onSuccess: (result) => {
        const url = result.hostedUrl ?? result.pdfUrl
        if (url) window.open(url, '_blank', 'noopener,noreferrer')
      },
    })
  }

  return (
    <div className="flex flex-col items-end gap-1">
      <Button variant="secondary" onClick={open} disabled={invoice.isPending}>
        {invoice.isPending ? t('common.loading') : t('invoices.open')}
      </Button>
      {invoice.isError ? <ErrorNotice error={invoice.error} /> : null}
      {invoice.isSuccess && invoice.data.pdfUrl ? (
        <a
          className="text-xs text-emerald-700 underline"
          href={invoice.data.pdfUrl}
          target="_blank"
          rel="noopener noreferrer"
        >
          {t('invoices.pdf')}
        </a>
      ) : null}
    </div>
  )
}

export function InvoicesPage() {
  const { t } = useTranslation()
  const bookings = usePaidBookings()

  return (
    <PageShell title={t('invoices.title')}>
      {bookings.isPending ? <Loading /> : null}
      <ErrorNotice error={bookings.error} />
      {bookings.data?.length === 0 ? <p className="text-slate-500">{t('invoices.empty')}</p> : null}
      <ul className="flex flex-col gap-3">
        {bookings.data?.map((booking) => (
          <li key={booking.paymentId}>
            <Card>
              <div className="flex items-center justify-between gap-3">
                <div>
                  <p className="font-semibold">{formatMoney(booking.amount, booking.currency)}</p>
                  <p className="text-sm text-slate-500">
                    {t('invoices.booking', { id: (booking.bookingId ?? '').slice(0, 8) })} ·{' '}
                    {t('invoices.paidOn', { date: formatDate(booking.paidAt) })}
                  </p>
                  <p className="text-xs text-slate-500">
                    {t(`invoices.status.${booking.status ?? 'Succeeded'}`)}
                  </p>
                </div>
                {booking.bookingId ? <InvoiceButton bookingId={booking.bookingId} /> : null}
              </div>
            </Card>
          </li>
        ))}
      </ul>
    </PageShell>
  )
}
