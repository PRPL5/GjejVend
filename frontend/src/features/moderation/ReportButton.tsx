import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation } from '@tanstack/react-query'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { z } from 'zod'
import { api, unwrap } from '@/api/http'
import type { components } from '@/api/generated/schema'
import { Button, ErrorNotice, Notice } from '@/components/ui'

type Category = components['schemas']['ReportCategory']

const categories: Category[] = ['Inaccurate', 'Safety', 'Fraud', 'Inappropriate', 'Other']

/** What is being reported. Exactly one id, matching what the API accepts. */
export type ReportTarget = { locationId: string } | { reviewId: string } | { reportedUserId: string }

const schema = z.object({
  category: z.enum(['Inaccurate', 'Safety', 'Fraud', 'Inappropriate', 'Other']),
  description: z.string().trim().min(1, 'required').max(2000),
})

type FormValues = z.infer<typeof schema>

function useCreateReport() {
  return useMutation({
    mutationFn: (body: components['schemas']['CreateReportRequest']) => unwrap(api.POST('/api/reports', { body })),
  })
}

function ReportDialog({ target, onClose }: { target: ReportTarget; onClose: () => void }) {
  const { t } = useTranslation()
  const create = useCreateReport()
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormValues>({ resolver: zodResolver(schema), defaultValues: { category: 'Inaccurate', description: '' } })

  const submit = handleSubmit((values) => create.mutate({ ...target, ...values }))

  return (
    <div className="fixed inset-0 z-50 flex items-end justify-center bg-black/40 p-4 sm:items-center">
      <div role="dialog" aria-modal="true" aria-labelledby="report-title" className="w-full max-w-md rounded-xl bg-white p-4 shadow-xl">
        <h2 id="report-title" className="mb-3 text-lg font-semibold">
          {t('report.title')}
        </h2>

        {create.isSuccess ? (
          <div className="flex flex-col gap-3">
            <Notice tone="success">{t('report.sent')}</Notice>
            <Button onClick={onClose}>{t('common.close')}</Button>
          </div>
        ) : (
          <form onSubmit={submit} className="flex flex-col gap-3">
            <label className="flex flex-col gap-1 text-sm font-semibold">
              {t('report.category')}
              <select className="min-h-11 rounded-lg border border-slate-300 px-3 font-normal" {...register('category')}>
                {categories.map((category) => (
                  <option key={category} value={category}>
                    {t(`report.categories.${category}`)}
                  </option>
                ))}
              </select>
            </label>

            <label className="flex flex-col gap-1 text-sm font-semibold">
              {t('report.description')}
              <textarea
                rows={4}
                className="rounded-lg border border-slate-300 p-3 font-normal"
                placeholder={t('report.descriptionPlaceholder')}
                aria-invalid={errors.description ? true : undefined}
                {...register('description')}
              />
            </label>
            {errors.description ? (
              <p role="alert" className="text-sm text-red-700">
                {t('report.descriptionRequired')}
              </p>
            ) : null}

            <ErrorNotice error={create.error} />
            <div className="flex gap-2">
              <Button type="submit" disabled={create.isPending}>
                {t('report.submit')}
              </Button>
              <Button variant="secondary" onClick={onClose}>
                {t('common.cancel')}
              </Button>
            </div>
          </form>
        )}
      </div>
    </div>
  )
}

/** Drop this next to a lot, a review or a profile. Opens a small form and sends the report to the admins. */
export function ReportButton({ target }: { target: ReportTarget }) {
  const { t } = useTranslation()
  const [open, setOpen] = useState(false)

  return (
    <>
      <Button variant="secondary" className="min-h-9 px-3 py-1 text-xs" onClick={() => setOpen(true)}>
        {t('report.button')}
      </Button>
      {open ? <ReportDialog target={target} onClose={() => setOpen(false)} /> : null}
    </>
  )
}
