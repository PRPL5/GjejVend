import { screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, expect, test, vi } from 'vitest'
import { jsonResponse, renderWithProviders } from '@/test-utils'
import { prettyMetadata } from './AuditLogPage'
import { ReportsQueuePage, actionsFor } from './ReportsQueuePage'
import { VerificationQueuePage } from './VerificationQueuePage'

afterEach(() => vi.restoreAllMocks())

/** Answers GETs with `list` and every POST with `{}`, and remembers what was sent. */
function mockApi(list: unknown) {
  const posts: Array<{ url: string; body: unknown }> = []
  vi.spyOn(globalThis, 'fetch').mockImplementation(async (input) => {
    const request = input as Request
    if (request.method === 'POST') {
      posts.push({ url: request.url, body: await request.json() })
      return jsonResponse({})
    }
    return jsonResponse(list)
  })
  return posts
}

const pendingRequest = {
  id: 'v1',
  targetType: 'lot',
  targetId: 'l1',
  targetName: 'Qendra Parking',
  status: 'Pending',
  documentPaths: ['a.pdf', 'b.pdf'],
  createdAt: '2026-11-20T10:00:00Z',
}

test('verification queue lists pending requests and approves one', async () => {
  const posts = mockApi([pendingRequest])

  renderWithProviders(<VerificationQueuePage />)
  expect(await screen.findByText('Qendra Parking')).toBeInTheDocument()
  expect(screen.getByText(/2 dokumente/)).toBeInTheDocument()
  await userEvent.click(screen.getByRole('button', { name: 'Aprovo' }))

  await waitFor(() => expect(posts).toHaveLength(1))
  expect(posts[0]?.url).toContain('/api/admin/verifications/v1/review')
  expect(posts[0]?.body).toEqual({ approve: true })
})

test('a rejection cannot be sent without telling the owner why', async () => {
  const posts = mockApi([pendingRequest])

  renderWithProviders(<VerificationQueuePage />)
  await userEvent.click(await screen.findByRole('button', { name: 'Refuzo' }))
  const confirm = screen.getByRole('button', { name: 'Refuzo kërkesën' })
  expect(confirm).toBeDisabled()

  await userEvent.type(screen.getByLabelText(/pse refuzohet/i), 'Documents are unreadable')
  await userEvent.click(confirm)

  await waitFor(() => expect(posts).toHaveLength(1))
  expect(posts[0]?.body).toEqual({ approve: false, notes: 'Documents are unreadable' })
})

test('reports queue offers only the actions that fit what was reported', async () => {
  const posts = mockApi([
    { id: 'r1', category: 'Fraud', description: 'Fake lot', status: 'Open', locationId: 'l1', createdAt: '2026-11-20T10:00:00Z' },
  ])

  renderWithProviders(<ReportsQueuePage />)
  const card = (await screen.findByText('Fake lot')).closest('section') as HTMLElement
  const options = within(card).getAllByRole('option').map((option) => option.textContent)
  expect(options).toContain('Pezullo parkingun')
  expect(options).not.toContain('Fshih vlerësimin')

  await userEvent.selectOptions(within(card).getByLabelText('Çfarë të bëhet'), 'SuspendLot')
  await userEvent.type(within(card).getByLabelText('Shënim zgjidhjeje'), 'Suspended after review')
  await userEvent.click(within(card).getByRole('button', { name: 'Mbyll raportimin' }))

  await waitFor(() => expect(posts).toHaveLength(1))
  expect(posts[0]?.body).toEqual({ action: 'SuspendLot', resolution: 'Suspended after review' })
})

test('actions depend on the report target', () => {
  expect(actionsFor({ reviewId: 'x' })).toEqual(['Dismiss', 'Resolve', 'HideReview'])
  expect(actionsFor({ locationId: 'x' })).toEqual(['Dismiss', 'Resolve', 'SuspendLot'])
  expect(actionsFor({ reportedUserId: 'x' })).toEqual(['Dismiss', 'Resolve'])
})

test('audit metadata is shown indented, or as it came when it is not JSON', () => {
  expect(prettyMetadata('{"status":{"from":"pending","to":"succeeded"}}')).toContain('\n  "status"')
  expect(prettyMetadata('plain text')).toBe('plain text')
  expect(prettyMetadata(null)).toBe('')
})
