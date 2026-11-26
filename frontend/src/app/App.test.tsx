import { screen } from '@testing-library/react'
import { mockApi, renderRoute } from '@/test-utils'

test('renders the home page inside the layout', async () => {
  mockApi(() => new Response('Healthy'))

  renderRoute('/')

  expect(await screen.findByRole('heading', { name: 'GjejVend' })).toBeInTheDocument()
  expect(screen.getByRole('link', { name: 'Rezervimet' })).toBeInTheDocument()
})

test('asks to sign in on booking pages without a session', async () => {
  mockApi(() => undefined)

  renderRoute('/bookings', false)

  expect(await screen.findByText('Hyni në llogari për të vazhduar.')).toBeInTheDocument()
})
