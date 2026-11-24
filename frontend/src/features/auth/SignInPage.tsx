import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router-dom'
import { Button, Notice, PageShell } from '@/components/ui'
import { isSignInConfigured, signIn } from './session'

/** Temporary email and password sign-in against Supabase Auth. The shared auth work replaces it. */
export function SignInPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [busy, setBusy] = useState(false)
  const [failed, setFailed] = useState<'invalid' | 'other' | null>(null)

  async function submit(event: React.FormEvent) {
    event.preventDefault()
    setBusy(true)
    setFailed(null)
    try {
      await signIn(email.trim(), password)
      navigate('/')
    } catch (error) {
      setFailed(error instanceof Error && error.message === 'invalid' ? 'invalid' : 'other')
    } finally {
      setBusy(false)
    }
  }

  return (
    <PageShell title={t('auth.title')}>
      {isSignInConfigured() ? (
        <form onSubmit={submit} className="flex max-w-sm flex-col gap-3">
          <label className="flex flex-col gap-1 text-sm font-semibold">
            {t('auth.email')}
            <input
              type="email"
              autoComplete="username"
              required
              className="min-h-11 rounded-lg border border-slate-300 px-3 font-normal"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
            />
          </label>
          <label className="flex flex-col gap-1 text-sm font-semibold">
            {t('auth.password')}
            <input
              type="password"
              autoComplete="current-password"
              required
              className="min-h-11 rounded-lg border border-slate-300 px-3 font-normal"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
            />
          </label>
          {failed ? (
            <p role="alert" className="rounded-lg bg-red-50 p-3 text-sm text-red-800">
              {failed === 'invalid' ? t('auth.invalid') : t('common.errors.generic')}
            </p>
          ) : null}
          <Button type="submit" disabled={busy}>
            {busy ? t('auth.signingIn') : t('auth.submit')}
          </Button>
        </form>
      ) : (
        <Notice tone="warning">{t('auth.notConfigured')}</Notice>
      )}
    </PageShell>
  )
}
