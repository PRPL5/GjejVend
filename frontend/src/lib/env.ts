export const API_URL: string = import.meta.env.VITE_API_URL ?? 'http://localhost:5185'

/** Supabase project used for sign-in. Both are public values (the anon key is meant for browsers). */
export const SUPABASE_URL: string = import.meta.env.VITE_SUPABASE_URL ?? ''
export const SUPABASE_ANON_KEY: string = import.meta.env.VITE_SUPABASE_ANON_KEY ?? ''
