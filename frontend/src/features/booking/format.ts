import { useTranslation } from 'react-i18next'
import type { Tone } from '@/components/Feedback'
import type { BookingStatus } from '@/features/booking/bookingsApi'

const locales: Record<string, string> = { sq: 'sq-AL', en: 'en-GB' }

export const statusTones: Record<BookingStatus, Tone> = {
  Pending: 'warning',
  Confirmed: 'available',
  Active: 'ink',
  Completed: 'muted',
  Cancelled: 'danger',
  Expired: 'muted',
  NoShow: 'danger',
}

export const openStatuses: BookingStatus[] = ['Pending', 'Confirmed', 'Active']

export function useFormat() {
  const { i18n } = useTranslation()
  const locale = locales[i18n.language] ?? 'sq-AL'
  return {
    money: (amount: number, currency: string) =>
      new Intl.NumberFormat(locale, { style: 'currency', currency }).format(amount),
    dateTime: (value: string) =>
      new Intl.DateTimeFormat(locale, {
        weekday: 'short',
        day: 'numeric',
        month: 'short',
        hour: '2-digit',
        minute: '2-digit',
      }).format(new Date(value)),
    time: (value: string) =>
      new Intl.DateTimeFormat(locale, { hour: '2-digit', minute: '2-digit' }).format(
        new Date(value),
      ),
  }
}

export function formatDuration(milliseconds: number): string {
  const totalMinutes = Math.max(0, Math.ceil(milliseconds / 60_000))
  const days = Math.floor(totalMinutes / 1440)
  const hours = Math.floor((totalMinutes % 1440) / 60)
  const minutes = totalMinutes % 60
  if (days > 0) return `${days}d ${hours}h`
  if (hours > 0) return `${hours}h ${minutes}min`
  return `${minutes}min`
}

export function toLocalInput(date: Date): string {
  const shifted = new Date(date.getTime() - date.getTimezoneOffset() * 60_000)
  return shifted.toISOString().slice(0, 16)
}

export function fromLocalInput(value: string): string {
  return new Date(value).toISOString()
}

export function mapsLink(latitude: number, longitude: number): string {
  return `https://www.google.com/maps/dir/?api=1&destination=${latitude},${longitude}`
}
