export function formatMoney(amount: number | undefined, currency: string | null | undefined, locale = 'sq'): string {
  return new Intl.NumberFormat(locale, { style: 'currency', currency: currency || 'EUR' }).format(amount ?? 0)
}

export function formatDate(value: string | null | undefined, locale = 'sq'): string {
  return value ? new Date(value).toLocaleDateString(locale, { year: 'numeric', month: 'short', day: 'numeric' }) : '—'
}

/** yyyy-MM-dd in local time, the format date inputs and the payout API use. */
export function toDateInput(date: Date): string {
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')
  return `${date.getFullYear()}-${month}-${day}`
}

/** Monday to Sunday of the last finished week, as date-input values. */
export function lastFinishedWeek(today = new Date()): { start: string; end: string } {
  const sinceMonday = (today.getDay() + 6) % 7
  const thisMonday = new Date(today.getFullYear(), today.getMonth(), today.getDate() - sinceMonday)
  const start = new Date(thisMonday.getFullYear(), thisMonday.getMonth(), thisMonday.getDate() - 7)
  const end = new Date(thisMonday.getFullYear(), thisMonday.getMonth(), thisMonday.getDate() - 1)
  return { start: toDateInput(start), end: toDateInput(end) }
}
