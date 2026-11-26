import { createClient } from '@supabase/supabase-js'
import { SUPABASE_ANON_KEY, SUPABASE_URL } from '@/lib/env'

export const supabase = SUPABASE_URL && SUPABASE_ANON_KEY ? createClient(SUPABASE_URL, SUPABASE_ANON_KEY) : null

export async function getAccessToken(): Promise<string | undefined> {
  const { data } = (await supabase?.auth.getSession()) ?? { data: { session: null } }
  return data.session?.access_token
}
