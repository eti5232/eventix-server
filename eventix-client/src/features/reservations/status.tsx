import { todayIso } from '../../lib/dates'
import type { Reservation } from '../../types'

export const STATUS_LABEL: Record<Reservation['status'], string> = {
  Confirmed: 'מאושרת',
  Returned: 'הוחזרה',
  Cancelled: 'בוטלה',
}

/** Confirmed, but the end date has passed and the equipment is not back yet. */
export const isOverdue = (r: Reservation) => r.status === 'Confirmed' && r.endDate < todayIso()

/** The server allows cancelling only before the start date. */
export const canCancel = (r: Reservation) => r.status === 'Confirmed' && r.startDate > todayIso()

/** The server allows marking as returned only once the reservation has started. */
export const canReturn = (r: Reservation) => r.status === 'Confirmed' && r.startDate <= todayIso()

export function StatusBadge({ reservation }: { reservation: Reservation }) {
  if (isOverdue(reservation)) return <span className="badge badge-late">באיחור</span>
  const kind = reservation.status.toLowerCase()
  return <span className={`badge badge-${kind}`}>{STATUS_LABEL[reservation.status as Reservation['status']]}</span>
}
