import { create } from 'zustand'
import { supabase } from '@/lib/supabase'

type SessionState = {
  ready: boolean
  userId: string | null
}

export const useSession = create<SessionState>(() => ({ ready: supabase === null, userId: null }))

export function startSessionSync() {
  if (!supabase) return
  void supabase.auth.getSession().then(({ data }) => {
    useSession.setState({ ready: true, userId: data.session?.user.id ?? null })
  })
  supabase.auth.onAuthStateChange((_event, session) => {
    useSession.setState({ ready: true, userId: session?.user.id ?? null })
  })
}
