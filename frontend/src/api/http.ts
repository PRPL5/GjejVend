import createClient from 'openapi-fetch'
import { getAccessToken } from '@/lib/auth'
import { API_URL } from '@/lib/env'
import type { paths } from './generated/schema'

/** Typed client generated from the API's OpenAPI document (`npm run gen:api`). */
// fetch is looked up per call (not captured at creation) so tests can replace it.
export const api = createClient<paths>({ baseUrl: API_URL, fetch: (request) => globalThis.fetch(request) })

api.use({
  async onRequest({ request }) {
    const token = await getAccessToken()
    if (token) request.headers.set('Authorization', `Bearer ${token}`)
    return request
  },
})

/** A failed API call. `status` is 0 when the server could not be reached at all. */
export class ApiError extends Error {
  readonly status: number
  readonly code: string | undefined

  constructor(status: number, message: string, code?: string) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.code = code
  }
}

interface ProblemDetails {
  title?: string
  detail?: string
  code?: string
}

type FetchResult<T> = { data?: T; error?: unknown; response: Response }

/** Returns the response body or throws an ApiError carrying the server's problem-details message. */
export async function unwrap<T>(request: Promise<FetchResult<T>>): Promise<T> {
  let result: FetchResult<T>
  try {
    result = await request
  } catch {
    throw new ApiError(0, 'network')
  }

  if (result.error !== undefined || !result.response.ok) {
    const problem = (result.error ?? {}) as ProblemDetails
    throw new ApiError(result.response.status, problem.detail ?? problem.title ?? `HTTP ${result.response.status}`, problem.code)
  }
  return result.data as T
}
