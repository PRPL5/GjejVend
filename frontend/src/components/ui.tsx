import type { ButtonHTMLAttributes, ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import { ApiError } from '@/api/http'

type Variant = 'primary' | 'secondary' | 'danger'

const variants: Record<Variant, string> = {
  primary: 'bg-emerald-600 text-white hover:bg-emerald-700 disabled:bg-emerald-300',
  secondary: 'border border-slate-300 bg-white text-slate-800 hover:bg-slate-100 disabled:text-slate-400',
  danger: 'bg-red-700 text-white hover:bg-red-800 disabled:bg-red-300',
}

export function Button({
  variant = 'primary',
  loading = false,
  disabled,
  className = '',
  ...props
}: ButtonHTMLAttributes<HTMLButtonElement> & { variant?: Variant; loading?: boolean }) {
  return (
    <button
      type="button"
      className={`inline-flex min-h-11 items-center justify-center rounded-lg px-4 py-2 text-sm font-semibold transition disabled:cursor-not-allowed ${variants[variant]} ${className}`}
      {...props}
      disabled={disabled || loading}
      aria-busy={loading}
    />
  )
}

export function Card({ title, children }: { title?: string; children: ReactNode }) {
  return (
    <section className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
      {title ? <h2 className="mb-3 text-base font-semibold">{title}</h2> : null}
      {children}
    </section>
  )
}

export function PageShell({ title, children }: { title: string; children: ReactNode }) {
  return (
    <main className="mx-auto flex w-full max-w-3xl flex-col gap-4 p-4">
      <h1 className="text-2xl font-bold">{title}</h1>
      {children}
    </main>
  )
}

export function Loading() {
  const { t } = useTranslation()
  return (
    <p role="status" className="text-slate-500">
      {t('common.loading')}
    </p>
  )
}

/** A friendly, translated message for any error thrown by the API client. */
export function errorText(error: unknown, t: (key: string) => string): string {
  if (error instanceof ApiError) {
    if (error.status === 0) return t('common.errors.network')
    if (error.status === 401) return t('common.errors.unauthorized')
    if (error.status < 500 && error.message && !error.message.startsWith('HTTP ')) return error.message
  }
  return t('common.errors.generic')
}

export function ErrorNotice({ error }: { error: unknown }) {
  const { t } = useTranslation()
  if (!error) return null
  return (
    <p role="alert" className="rounded-lg bg-red-50 p-3 text-sm text-red-800">
      {errorText(error, t)}
    </p>
  )
}

export function Notice({ tone, children }: { tone: 'success' | 'warning'; children: ReactNode }) {
  const color = tone === 'success' ? 'bg-emerald-50 text-emerald-900' : 'bg-amber-50 text-amber-900'
  return (
    <p role="status" className={`rounded-lg p-3 text-sm ${color}`}>
      {children}
    </p>
  )
}

export function Stat({ label, value, hint }: { label: string; value: string; hint?: string }) {
  return (
    <div>
      <dt className="text-sm text-slate-500">{label}</dt>
      <dd className="text-xl font-semibold">{value}</dd>
      {hint ? <p className="text-xs text-slate-500">{hint}</p> : null}
    </div>
  )
}
