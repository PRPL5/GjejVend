import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, expect, test, vi } from 'vitest'
import { jsonResponse, renderWithProviders } from '@/test-utils'
import { ReportButton } from './ReportButton'

afterEach(() => vi.restoreAllMocks())

test('asks for a description before sending', async () => {
  const fetchMock = vi.spyOn(globalThis, 'fetch')

  renderWithProviders(<ReportButton target={{ locationId: 'lot-1' }} />)
  await userEvent.click(screen.getByRole('button', { name: 'Raporto' }))
  await userEvent.click(screen.getByRole('button', { name: /dërgo raportimin/i }))

  expect(await screen.findByRole('alert')).toHaveTextContent(/përshkruaje problemin/i)
  expect(fetchMock).not.toHaveBeenCalled()
})

test('sends exactly one target with the chosen category', async () => {
  const fetchMock = vi.spyOn(globalThis, 'fetch').mockResolvedValue(jsonResponse({ id: 'r1', status: 'Open' }))

  renderWithProviders(<ReportButton target={{ reviewId: 'rev-9' }} />)
  await userEvent.click(screen.getByRole('button', { name: 'Raporto' }))
  await userEvent.selectOptions(screen.getByLabelText(/çfarë nuk është në rregull/i), 'Fraud')
  await userEvent.type(screen.getByLabelText(/na trego më shumë/i), 'Looks like a scam')
  await userEvent.click(screen.getByRole('button', { name: /dërgo raportimin/i }))

  await waitFor(() => expect(fetchMock).toHaveBeenCalled())
  const request = fetchMock.mock.calls[0]?.[0] as Request
  expect(await request.json()).toEqual({ reviewId: 'rev-9', category: 'Fraud', description: 'Looks like a scam' })
  expect(await screen.findByText(/faleminderit/i)).toBeInTheDocument()
})
