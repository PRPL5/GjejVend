import { useState, type FormEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { Button } from '@/components/Button'
import { ErrorMessage } from '@/components/Feedback'
import { TextAreaField } from '@/components/Field'
import { useReviewBooking } from '@/features/booking/api'

export function ReviewForm({ bookingId, onDone }: { bookingId: string; onDone: () => void }) {
  const { t } = useTranslation('bookings')
  const review = useReviewBooking(bookingId)
  const [rating, setRating] = useState(0)
  const [comment, setComment] = useState('')

  function submit(event: FormEvent) {
    event.preventDefault()
    review.mutate({ rating, comment: comment.trim() || null }, { onSuccess: onDone })
  }

  return (
    <form onSubmit={submit} className="flex flex-col gap-4 rounded-2xl bg-paper-2 p-4">
      <h2 className="font-display text-xl font-bold">{t('review.title')}</h2>
      <fieldset className="flex flex-col gap-2">
        <legend className="font-medium">{t('review.rating')}</legend>
        <div className="flex gap-2">
          {[1, 2, 3, 4, 5].map((value) => (
            <button
              key={value}
              type="button"
              aria-pressed={rating === value}
              aria-label={t('review.stars', { count: value })}
              onClick={() => setRating(value)}
              className={`min-h-11 min-w-11 rounded-xl border font-semibold ${rating >= value ? 'border-ink bg-ink text-paper' : 'border-paper-3 bg-paper'}`}
            >
              {value}
            </button>
          ))}
        </div>
      </fieldset>
      <TextAreaField label={t('review.comment')} value={comment} maxLength={1000} onChange={(e) => setComment(e.target.value)} />
      {review.isError && <ErrorMessage error={review.error} />}
      <Button type="submit" variant="primary" disabled={rating === 0} loading={review.isPending}>
        {t('review.submit')}
      </Button>
    </form>
  )
}
