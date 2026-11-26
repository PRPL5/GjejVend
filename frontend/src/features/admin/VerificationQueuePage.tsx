import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Button, Card, ErrorNotice, Loading, PageShell } from '@/components/ui'
import { formatDate } from '@/lib/format'
import { AdminNav } from './AdminNav'
import { useReviewVerification, useVerifications, type VerificationStatus } from './api'

const statuses: VerificationStatus[] = ['Pending', 'Approved', 'Rejected']

function ReviewActions({ id }: { id: string }) {
  const { t } = useTranslation()
  const review = useReviewVerification()
  const [rejecting, setRejecting] = useState(false)
  const [notes, setNotes] = useState('')

  return (
    <div className="mt-3 flex flex-col gap-2">
      <ErrorNotice error={review.error} />
      {rejecting ? (
        <>
          <label className="flex flex-col gap-1 text-sm font-semibold">
            {t('admin.verification.notes')}
            <textarea
              rows={3}
              className="rounded-lg border border-slate-300 p-3 font-normal"
              value={notes}
              onChange={(event) => setNotes(event.target.value)}
            />
          </label>
          <div className="flex gap-2">
            <Button
              disabled={review.isPending || notes.trim() === ''}
              onClick={() => review.mutate({ id, approve: false, notes })}
            >
              {t('admin.verification.confirmReject')}
            </Button>
            <Button variant="secondary" onClick={() => setRejecting(false)}>
              {t('common.cancel')}
            </Button>
          </div>
        </>
      ) : (
        <div className="flex gap-2">
          <Button disabled={review.isPending} onClick={() => review.mutate({ id, approve: true })}>
            {t('admin.verification.approve')}
          </Button>
          <Button variant="secondary" onClick={() => setRejecting(true)}>
            {t('admin.verification.reject')}
          </Button>
        </div>
      )}
    </div>
  )
}

export function VerificationQueuePage() {
  const { t } = useTranslation()
  const [status, setStatus] = useState<VerificationStatus>('Pending')
  const queue = useVerifications(status)

  return (
    <PageShell title={t('admin.verification.title')}>
      <AdminNav />
      <label className="flex items-center gap-2 text-sm">
        {t('admin.filter')}
        <select
          className="min-h-11 rounded-lg border border-slate-300 px-3"
          value={status}
          onChange={(event) => setStatus(event.target.value as VerificationStatus)}
        >
          {statuses.map((value) => (
            <option key={value} value={value}>
              {t(`admin.verification.status.${value}`)}
            </option>
          ))}
        </select>
      </label>

      {queue.isPending ? <Loading /> : null}
      <ErrorNotice error={queue.error} />
      {queue.data?.length === 0 ? <p className="text-slate-500">{t('admin.verification.empty')}</p> : null}

      <ul className="flex flex-col gap-3">
        {queue.data?.map((item) => (
          <li key={item.id}>
            <Card>
              <div className="flex flex-wrap items-center justify-between gap-2">
                <div>
                  <p className="font-semibold">{item.targetName}</p>
                  <p className="text-sm text-slate-500">
                    {t(`admin.verification.target.${item.targetType ?? 'lot'}`)} · {formatDate(item.createdAt)}
                  </p>
                </div>
                <span className="rounded-full bg-slate-100 px-3 py-1 text-xs">
                  {t(`admin.verification.status.${item.status ?? 'Pending'}`)}
                </span>
              </div>
              <p className="mt-2 text-sm text-slate-600">
                {t('admin.verification.documents', { count: item.documentPaths?.length ?? 0 })}
              </p>
              {item.reviewNotes ? <p className="mt-1 text-sm text-slate-600">“{item.reviewNotes}”</p> : null}
              {item.status === 'Pending' && item.id ? <ReviewActions id={item.id} /> : null}
            </Card>
          </li>
        ))}
      </ul>
    </PageShell>
  )
}
