import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render } from '@testing-library/react'
import type { ReactElement } from 'react'
import { createMemoryRouter, MemoryRouter, RouterProvider } from 'react-router-dom'
import { m2Routes } from '@/app/m2Routes'
import { useSession } from '@/features/auth/session'

/** Renders with fresh query state and a router, the way the pages run in the app. */
export function renderWithProviders(ui: ReactElement, route = '/') {
  const client = new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  })
  return render(
    <QueryClientProvider client={client}>
      <MemoryRouter initialEntries={[route]}>{ui}</MemoryRouter>
    </QueryClientProvider>,
  )
}

export function jsonResponse(body: unknown, status = 200): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: { 'Content-Type': 'application/json' },
  })
}

type Handler = (request: Request) => Response | Promise<Response> | undefined

export function json(body: unknown, status = 200): Response {
  return new Response(status === 204 ? null : JSON.stringify(body), {
    status,
    headers: { 'Content-Type': status >= 400 ? 'application/problem+json' : 'application/json' },
  })
}

export function mockApi(handler: Handler) {
  const calls: Request[] = []
  vi.stubGlobal(
    'fetch',
    vi.fn(async (input: Request | string) => {
      const request = typeof input === 'string' ? new Request(input) : input
      calls.push(request.clone())
      return (await handler(request)) ?? json({ title: 'Not mocked' }, 404)
    }),
  )
  return calls
}

export function renderRoute(path: string, signedIn = true) {
  useSession.setState({
    session: signedIn
      ? {
          accessToken: 'token',
          refreshToken: 'refresh',
          expiresAt: 4_102_444_800,
          email: 'driver@example.test',
        }
      : null,
  })
  const router = createMemoryRouter([...m2Routes, { path: '*', element: null }], { initialEntries: [path] })
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return {
    router,
    ...render(
      <QueryClientProvider client={queryClient}>
        <RouterProvider router={router} />
      </QueryClientProvider>,
    ),
  }
}

export function route(request: Request, method: string, pathname: string): boolean {
  return request.method === method && new URL(request.url).pathname === pathname
}
