import { useState } from 'react'
import { useKit } from '../features/kit/KitContext'
import { formatRange } from '../lib/dates'
import { DateRangePicker } from './DateRangePicker'
import { Dialog } from './ui/Dialog'

/** The event-dates button used across the catalog, item page and order; opens the calendar in a dialog. */
export function RangeButton({ emphasis = false }: { emphasis?: boolean }) {
  const kit = useKit()
  const [open, setOpen] = useState(false)

  return (
    <>
      <button type="button" className={`btn ${emphasis ? 'btn-primary' : ''}`} onClick={() => setOpen(true)}>
        <svg viewBox="0 0 24 24" width="20" height="20" aria-hidden="true" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round">
          <rect x="3.5" y="5" width="17" height="15" rx="2.5" />
          <path d="M3.5 10h17M8 3v4M16 3v4" />
        </svg>
        {kit.hasRange ? (
          <span className="num">
            {formatRange(kit.start!, kit.end!)} · {kit.days} {kit.days === 1 ? 'יום' : 'ימים'}
          </span>
        ) : (
          'בחירת תאריכים'
        )}
      </button>
      <Dialog open={open} onClose={() => setOpen(false)} title="תאריכי האירוע" wide>
        <DateRangePicker
          start={kit.start}
          end={kit.end}
          months={window.innerWidth >= 700 ? 2 : 1}
          onChange={(start, end) => {
            kit.setRange(start, end)
            setOpen(false)
          }}
        />
      </Dialog>
    </>
  )
}
