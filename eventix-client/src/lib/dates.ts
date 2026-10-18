// Dates travel as "yyyy-MM-dd" strings (the server's DateOnly); no time zones are involved.

export const MAX_RANGE_DAYS = 7

const pad = (n: number) => String(n).padStart(2, '0')

export function toIso(date: Date): string {
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`
}

export function fromIso(iso: string): Date {
  const [y, m, d] = iso.split('-').map(Number)
  return new Date(y, m - 1, d)
}

export function todayIso(): string {
  return toIso(new Date())
}

export function addDays(iso: string, days: number): string {
  const date = fromIso(iso)
  date.setDate(date.getDate() + days)
  return toIso(date)
}

/** Every day from start to end, inclusive. */
export function daysBetween(start: string, end: string): string[] {
  const days: string[] = []
  for (let day = start; day <= end; day = addDays(day, 1)) days.push(day)
  return days
}

export function dayCount(start: string, end: string): number {
  return daysBetween(start, end).length
}

const dayMonth = new Intl.DateTimeFormat('he-IL', { day: 'numeric', month: 'numeric' })
const longDate = new Intl.DateTimeFormat('he-IL', { weekday: 'long', day: 'numeric', month: 'long' })
const weekdayShort = new Intl.DateTimeFormat('he-IL', { weekday: 'narrow' })
const monthYear = new Intl.DateTimeFormat('he-IL', { month: 'long', year: 'numeric' })

/** Numbers with dashes and slashes get scrambled inside right-to-left text; an isolate keeps them left-to-right. */
const ltr = (text: string) => `⁦${text}⁩`

export const formatDayMonth = (iso: string) => ltr(dayMonth.format(fromIso(iso)).replace(/\./g, '/'))
export const formatLong = (iso: string) => longDate.format(fromIso(iso))
export const formatWeekday = (iso: string) => weekdayShort.format(fromIso(iso))
export const formatMonthYear = (date: Date) => monthYear.format(date)

/** "20–23/11", or just "20/11" for a single day. */
export function formatRange(start: string, end: string): string {
  if (start === end) return formatDayMonth(start)
  const s = fromIso(start)
  const e = fromIso(end)
  if (s.getMonth() === e.getMonth()) return ltr(`${s.getDate()}–${e.getDate()}/${e.getMonth() + 1}`)
  return ltr(`${dayMonth.format(s).replace(/\./g, '/')}–${dayMonth.format(e).replace(/\./g, '/')}`)
}

const shekel = new Intl.NumberFormat('he-IL', {
  style: 'currency',
  currency: 'ILS',
  maximumFractionDigits: 2,
})
export const formatMoney = (value: number) => shekel.format(value)
