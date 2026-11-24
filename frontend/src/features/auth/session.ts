import { create } from 'zustand'
import { setAccessTokenProvider } from '@/lib/auth'
import { SUPABASE_ANON_KEY, SUPABASE_URL } from '@/lib/env'

export interface Session {
  accessToken: string
  refreshToken: string
  /** Unix seconds. */
  expiresAt: number
  email: string
}

interface SessionState {
  session: Session | null
  setSession: (session: Session | null) => void
}

const STORAGE_KEY = 'gjejvend.session'
/** Refresh a little early so a request never leaves with a token that expires in flight. */
const REFRESH_MARGIN_SECONDS = 60

function load(): Session | null {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    return raw ? (JSON.parse(raw) as Session) : null
  } catch {
    return null
  }
}

function persist(session: Session | null): void {
  try {
    if (session) localStorage.setItem(STORAGE_KEY, JSON.stringify(session))
    else localStorage.removeItem(STORAGE_KEY)
  } catch {
    // Storage can be blocked (private mode). The session then lasts until the page closes.
  }
}

export const useSession = create<SessionState>((set) => ({
  session: load(),
  setSession: (session) => {
    persist(session)
    set({ session })
  },
}))

export const isSignInConfigured = (): boolean => SUPABASE_URL !== '' && SUPABASE_ANON_KEY !== ''

interface TokenResponse {
  access_token: string
  refresh_token: string
  expires_in: number
  user?: { email?: string }
}

async function requestToken(grantType: 'password' | 'refresh_token', body: Record<string, string>): Promise<TokenResponse> {
  const response = await fetch(`${SUPABASE_URL.replace(/\/$/, '')}/auth/v1/token?grant_type=${grantType}`, {
    method: 'POST',
    headers: { apikey: SUPABASE_ANON_KEY, 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  })
  if (!response.ok) throw new Error(response.status === 400 || response.status === 401 ? 'invalid' : `HTTP ${response.status}`)
  return (await response.json()) as TokenResponse
}

function toSession(token: TokenResponse, fallbackEmail: string): Session {
  return {
    accessToken: token.access_token,
    refreshToken: token.refresh_token,
    expiresAt: Math.floor(Date.now() / 1000) + token.expires_in,
    email: token.user?.email ?? fallbackEmail,
  }
}

export async function signIn(email: string, password: string): Promise<void> {
  const token = await requestToken('password', { email, password })
  useSession.getState().setSession(toSession(token, email))
}

export function signOut(): void {
  useSession.getState().setSession(null)
}

/** The current access token, refreshed when it is about to expire. Null when signed out. */
export async function getValidToken(now: () => number = () => Date.now() / 1000): Promise<string | null> {
  const { session, setSession } = useSession.getState()
  if (!session) return null
  if (session.expiresAt - now() > REFRESH_MARGIN_SECONDS) return session.accessToken

  try {
    const token = await requestToken('refresh_token', { refresh_token: session.refreshToken })
    const refreshed = toSession(token, session.email)
    setSession(refreshed)
    return refreshed.accessToken
  } catch {
    setSession(null) // the refresh token was rejected: the user has to sign in again
    return null
  }
}

/** Call once at start-up so API requests carry the signed-in user's token. */
export function registerSessionTokenProvider(): void {
  setAccessTokenProvider(() => getValidToken())
}
