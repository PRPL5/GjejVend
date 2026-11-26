import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render } from '@testing-library/react'
import { createMemoryRouter, RouterProvider } from 'react-router-dom'
import '@/i18n'
import { routes } from '@/app/routes'
import { useSession } from '@/store/session'

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
  useSession.setState({ ready: true, userId: signedIn ? '00000000-0000-0000-0000-000000000001' : null })
  const router = createMemoryRouter(routes, { initialEntries: [path] })
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
