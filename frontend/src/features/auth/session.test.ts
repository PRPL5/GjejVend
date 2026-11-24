import { afterEach, beforeEach, expect, test, vi } from 'vitest'
import { getValidToken, signIn, signOut, useSession } from './session'

const NOW = 1_800_000_000

beforeEach(() => signOut())
afterEach(() => vi.restoreAllMocks())

function tokenResponse(access: string, expiresIn = 3600) {
  return new Response(
    JSON.stringify({ access_token: access, refresh_token: `${access}-refresh`, expires_in: expiresIn, user: { email: 'admin@example.test' } }),
    { status: 200, headers: { 'Content-Type': 'application/json' } },
  )
}

test('there is no token while signed out', async () => {
  expect(await getValidToken(() => NOW)).toBeNull()
})

test('signing in stores the session and hands out its token', async () => {
  vi.spyOn(globalThis, 'fetch').mockResolvedValue(tokenResponse('token-1'))

  await signIn('admin@example.test', 'secret')

  expect(useSession.getState().session?.email).toBe('admin@example.test')
  expect(await getValidToken()).toBe('token-1')
})

test('a wrong password is reported as invalid and signs nobody in', async () => {
  vi.spyOn(globalThis, 'fetch').mockResolvedValue(new Response('{}', { status: 400 }))

  await expect(signIn('a@b.test', 'nope')).rejects.toThrow('invalid')
  expect(useSession.getState().session).toBeNull()
})

test('an access token that is about to expire is refreshed before use', async () => {
  useSession.getState().setSession({ accessToken: 'old', refreshToken: 'r1', expiresAt: NOW + 10, email: 'a@b.test' })
  const fetchMock = vi.spyOn(globalThis, 'fetch').mockResolvedValue(tokenResponse('new'))

  expect(await getValidToken(() => NOW)).toBe('new')
  expect(fetchMock.mock.calls[0]?.[0]).toContain('grant_type=refresh_token')
})

test('a token with plenty of time left is used as it is', async () => {
  useSession.getState().setSession({ accessToken: 'fresh', refreshToken: 'r1', expiresAt: NOW + 3000, email: 'a@b.test' })
  const fetchMock = vi.spyOn(globalThis, 'fetch')

  expect(await getValidToken(() => NOW)).toBe('fresh')
  expect(fetchMock).not.toHaveBeenCalled()
})

test('a rejected refresh token signs the user out', async () => {
  useSession.getState().setSession({ accessToken: 'old', refreshToken: 'dead', expiresAt: NOW - 5, email: 'a@b.test' })
  vi.spyOn(globalThis, 'fetch').mockResolvedValue(new Response('{}', { status: 400 }))

  expect(await getValidToken(() => NOW)).toBeNull()
  expect(useSession.getState().session).toBeNull()
})
