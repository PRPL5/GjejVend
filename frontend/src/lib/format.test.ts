import { expect, test } from 'vitest'
import { parsePercent } from '@/features/admin/CommissionSettingsPage'
import { lastFinishedWeek, toDateInput } from './format'

test('turns a percentage into a rate', () => {
  expect(parsePercent('10')).toBe(0.1)
  expect(parsePercent('12,5')).toBe(0.125)
  expect(parsePercent(' 0 ')).toBe(0)
  expect(parsePercent('100')).toBe(1)
})

test('rejects anything that is not a percentage', () => {
  expect(parsePercent('')).toBeNull()
  expect(parsePercent('abc')).toBeNull()
  expect(parsePercent('-1')).toBeNull()
  expect(parsePercent('100.5')).toBeNull()
})

test('picks the last finished Monday to Sunday week', () => {
  // Wednesday 2026-11-25: the last finished week is Mon 16 Nov to Sun 22 Nov.
  expect(lastFinishedWeek(new Date(2026, 10, 25))).toEqual({ start: '2026-11-16', end: '2026-11-22' })
  // On a Monday the week before is the one that just ended.
  expect(lastFinishedWeek(new Date(2026, 10, 23))).toEqual({ start: '2026-11-16', end: '2026-11-22' })
  expect(toDateInput(new Date(2026, 0, 5))).toBe('2026-01-05')
})
