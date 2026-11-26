import createClient from 'openapi-fetch'
import type { components, paths } from '@/api/generated/schema'
import { API_URL } from '@/lib/env'
import { getAccessToken } from '@/lib/supabase'

export type Schemas = components['schemas']

export class ApiError extends Error {
  readonly status: number
  readonly code: string | undefined

  constructor(status: number, code: string | undefined, message: string) {
    super(message)
    this.status = status
    this.code = code
  }
}

export const api = createClient<paths>({ baseUrl: API_URL, fetch: (request) => globalThis.fetch(request) })

api.use({
  async onRequest({ request }) {
    const token = await getAccessToken()
    if (token) request.headers.set('Authorization', `Bearer ${token}`)
    return request
  },
})

type Problem = { code?: string; title?: string }

export async function unwrap<T>(call: Promise<{ data?: T; error?: unknown; response: Response }>): Promise<T> {
  const { data, error, response } = await call
  if (!response.ok) {
    const problem = (error ?? {}) as Problem
    throw new ApiError(response.status, problem.code, problem.title ?? `API ${response.status}`)
  }
  return data as T
}

export async function getHealth(): Promise<string> {
  const res = await fetch(`${API_URL}/health`)
  if (!res.ok) throw new Error(`API ${res.status}`)
  return res.text()
}
