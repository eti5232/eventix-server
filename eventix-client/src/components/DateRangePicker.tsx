import { useMemo, useState } from 'react'
import {
  MAX_RANGE_DAYS,
  addDays,
  dayCount,
  formatLong,
  formatMonthYear,
  fromIso,
  toIso,
  todayIso,
} from '../lib/dates'
import styles from './DateRangePicker.module.css'

interface DateRangePickerProps {
  start: string | null
  end: string | null
  onChange: (start: string, end: string) => void
  /** How many months to show side by side (2 on the home hero, 1 in a dialog on phones). */
  months?: 1 | 2
}

const WEEKDAYS = ['א', 'ב', 'ג', 'ד', 'ה', 'ו', 'ש']

/** Cells of one month, padded with nulls so the first day lands under the right weekday (week starts on Sunday). */
function monthCells(year: number, month: number): (string | null)[] {
  const first = new Date(year, month, 1)
  const length = new Date(year, month + 1, 0).getDate()
  const cells: (string | null)[] = Array(first.getDay()).fill(null)
  for (let d = 1; d <= length; d++) cells.push(toIso(new Date(year, month, d)))
  return cells
}

export function DateRangePicker({ start, end, onChange, months = 2 }: DateRangePickerProps) {
  const today = todayIso()
  const [view, setView] = useState(() => {
    const base = start ? fromIso(start) : new Date()
    return new Date(base.getFullYear(), base.getMonth(), 1)
  })
  // The first click of a new range; null when no range is being picked.
  const [anchor, setAnchor] = useState<string | null>(null)
  const [hover, setHover] = useState<string | null>(null)
  const [notice, setNotice] = useState<string | null>(null)

  const maxEnd = anchor ? addDays(anchor, MAX_RANGE_DAYS - 1) : null

  function pick(day: string) {
    setNotice(null)
    if (!anchor) {
      setAnchor(day)
      return
    }
    if (day < anchor) {
      setAnchor(day)
      return
    }
    if (dayCount(anchor, day) > MAX_RANGE_DAYS) {
      setNotice(`אפשר להשכיר עד ${MAX_RANGE_DAYS} ימים ברצף. בחרו תאריך סיום עד ${formatLong(maxEnd!)}.`)
      return
    }
    onChange(anchor, day)
    setAnchor(null)
    setHover(null)
  }

  // What to paint as the selected range: the in-progress pick, or the confirmed range.
  const [paintStart, paintEnd] = useMemo(() => {
    if (anchor) {
      const previewEnd = hover && hover >= anchor && hover <= (maxEnd ?? hover) ? hover : anchor
      return [anchor, previewEnd]
    }
    return [start, end]
  }, [anchor, hover, maxEnd, start, end])

  const shown = Array.from({ length: months }, (_, i) => new Date(view.getFullYear(), view.getMonth() + i, 1))
  const canGoBack = view > new Date(new Date().getFullYear(), new Date().getMonth(), 1)

  return (
    <div className={styles.picker}>
      <div className={styles.nav}>
        <button
          type="button"
          className="btn btn-small"
          onClick={() => setView(new Date(view.getFullYear(), view.getMonth() - 1, 1))}
          disabled={!canGoBack}
          aria-label="החודש הקודם"
        >
          →
        </button>
        <button
          type="button"
          className="btn btn-small"
          onClick={() => setView(new Date(view.getFullYear(), view.getMonth() + 1, 1))}
          aria-label="החודש הבא"
        >
          ←
        </button>
      </div>

      <div className={styles.months}>
        {shown.map((month) => (
          <div key={month.toISOString()} className={styles.month}>
            <h3 className={styles.monthName}>{formatMonthYear(month)}</h3>
            <div className={styles.grid} role="grid" aria-label={formatMonthYear(month)}>
              {WEEKDAYS.map((w) => (
                <span key={w} className={styles.weekday} aria-hidden="true">
                  {w}
                </span>
              ))}
              {monthCells(month.getFullYear(), month.getMonth()).map((day, index) => {
                if (!day) return <span key={`blank-${index}`} />
                const inRange = paintStart != null && paintEnd != null && day >= paintStart && day <= paintEnd
                const isEdge = day === paintStart || day === paintEnd
                const beyond = anchor != null && (day < anchor || day > (maxEnd ?? day)) && day !== anchor
                const disabled = day < today
                const classes = [
                  styles.day,
                  inRange ? styles.inRange : '',
                  isEdge && inRange ? styles.edge : '',
                  beyond && !disabled ? styles.beyond : '',
                  day === today ? styles.today : '',
                ].join(' ')
                return (
                  <button
                    key={day}
                    type="button"
                    className={classes}
                    disabled={disabled}
                    aria-pressed={inRange}
                    aria-label={formatLong(day)}
                    onClick={() => pick(day)}
                    onMouseEnter={() => anchor && setHover(day)}
                    onFocus={() => anchor && setHover(day)}
                  >
                    {fromIso(day).getDate()}
                  </button>
                )
              })}
            </div>
          </div>
        ))}
      </div>

      <p className={styles.help} role="status">
        {notice ??
          (anchor
            ? `התחלה ב־${formatLong(anchor)}. עכשיו בחרו יום סיום (עד ${MAX_RANGE_DAYS} ימים).`
            : 'לחצו על יום ההתחלה ואז על יום הסיום של האירוע.')}
      </p>
    </div>
  )
}
