import { useState, type FormEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { Button, Loading } from '@/components/ui'
import { EmptyState, ErrorMessage } from '@/components/Feedback'
import { TextAreaField } from '@/components/Field'
import {
  useOwnerReviews,
  useRespondToReview,
  type OwnerReview,
} from '@/features/booking/bookingsApi'
import { useFormat } from '@/features/booking/format'

function ReviewItem({ review }: { review: OwnerReview }) {
  const { t } = useTranslation('bookings')
  const format = useFormat()
  const respond = useRespondToReview()
  const [reply, setReply] = useState('')

  function submit(event: FormEvent) {
    event.preventDefault()
    respond.mutate({ id: review.id, response: reply.trim() }, { onSuccess: () => setReply('') })
  }

  return (
    <li className="flex flex-col gap-2 border-b border-paper-3 py-4 last:border-0">
      <p className="flex justify-between gap-4">
        <span className="font-semibold">{review.locationName}</span>
        <span>{t('review.stars', { count: review.rating })}</span>
      </p>
      <p className="text-sm text-ink-2">{format.dateTime(review.createdAt)}</p>
      {review.comment && <p>{review.comment}</p>}
      {review.providerResponse && (
        <p className="text-ink-2">
          {t('ownerReviews.yourReply', { reply: review.providerResponse })}
        </p>
      )}
      <form onSubmit={submit} className="flex flex-col gap-2">
        <TextAreaField
          label={t('ownerReviews.reply')}
          value={reply}
          maxLength={1000}
          onChange={(e) => setReply(e.target.value)}
          required
        />
        {respond.isError && <ErrorMessage error={respond.error} />}
        <Button
          variant="secondary"
          type="submit"
          loading={respond.isPending}
          className="self-start"
        >
          {t('ownerReviews.submit')}
        </Button>
      </form>
    </li>
  )
}

export function OwnerReviewsPage() {
  const { t } = useTranslation('bookings')
  const reviews = useOwnerReviews()

  if (reviews.isPending) return <Loading />
  if (reviews.isError)
    return <ErrorMessage error={reviews.error} onRetry={() => void reviews.refetch()} />

  return (
    <div className="flex flex-col gap-6">
      <h1 className="font-display text-3xl font-bold">{t('ownerReviews.title')}</h1>
      {reviews.data.length === 0 ? (
        <EmptyState>{t('ownerReviews.empty')}</EmptyState>
      ) : (
        <ul>
          {reviews.data.map((review) => (
            <ReviewItem key={review.id} review={review} />
          ))}
        </ul>
      )}
    </div>
  )
}
