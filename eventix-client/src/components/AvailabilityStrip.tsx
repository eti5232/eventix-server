import type { DayAvailability } from '../features/catalog/useAvailability'
import { formatDayMonth, formatLong, formatWeekday } from '../lib/dates'
import styles from './AvailabilityStrip.module.css'

interface AvailabilityStripProps {
  cells: DayAvailability[]
  totalQuantity: number
  /** Compact version for catalog cards: no weekday labels. */
  compact?: boolean
  /** Day highlighted as the problem (after a 409). */
  clashDay?: string
}

/**
 * One cell per day of the chosen range showing how many units are free.
 * A full day is striped like gaffer tape. This is a drawing of the DailyStock table.
 */
export function AvailabilityStrip({ cells, totalQuantity, compact, clashDay }: AvailabilityStripProps) {
  if (cells.length === 0) {
    return <p className={styles.hint}>בחרו תאריכים כדי לראות מה פנוי</p>
  }

  return (
    <ol className={`${styles.strip} ${compact ? styles.compact : ''}`} aria-label="זמינות לפי יום">
      {cells.map((cell) => {
        const loading = cell.available === undefined
        const full = cell.available === 0
        const all = cell.available === totalQuantity
        const classes = [
          styles.cell,
          loading ? styles.loading : '',
          full ? styles.full : '',
          all ? styles.all : '',
          cell.date === clashDay ? styles.clash : '',
        ].join(' ')
        return (
          <li
            key={cell.date}
            className={classes}
            title={`${formatLong(cell.date)}: ${loading ? 'נטען' : full ? 'תפוס' : `${cell.available} פנויות`}`}
          >
            <span className={styles.value}>{loading ? '·' : full ? '' : cell.available}</span>
            <span className={styles.day}>{compact ? formatDayMonth(cell.date).split('/')[0] : formatWeekday(cell.date)}</span>
            <span className="sr-only">
              {formatLong(cell.date)}: {loading ? 'נטען' : full ? 'תפוס' : `${cell.available} פנויות`}
            </span>
          </li>
        )
      })}
    </ol>
  )
}
