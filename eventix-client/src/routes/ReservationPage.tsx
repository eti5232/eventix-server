import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { reservationsApi } from '../api/endpoints'
import { toApiError } from '../api/errors'
import { ErrorBanner } from '../components/ui/ErrorBanner'
import { StatusBadge, canCancel } from '../features/reservations/status'
import { StatusChangeDialog } from '../features/reservations/StatusChangeDialog'
import { formatLong, formatMoney, dayCount } from '../lib/dates'
import { NotFoundPage } from './NotFoundPage'

/** Confirmation after ordering, and the detail view of any reservation. */
export function ReservationPage() {
  const id = Number(useParams().id)
  const [cancelling, setCancelling] = useState(false)
  const query = useQuery({ queryKey: ['reservations', id], queryFn: () => reservationsApi.get(id), enabled: Number.isFinite(id) })

  if (query.isLoading) {
    return (
      <div className="page">
        <div className="skeleton" style={{ height: 360 }} />
      </div>
    )
  }
  if (query.isError) {
    const error = toApiError(query.error)
    if (error.status === 404 || error.status === 403) return <NotFoundPage what="ההזמנה" />
    return (
      <div className="page">
        <ErrorBanner error={error} title="לא הצלחנו לטעון את ההזמנה" />
      </div>
    )
  }
  const r = query.data!
  const days = dayCount(r.startDate, r.endDate)

  return (
    <div className="page" style={{ maxWidth: 760 }}>
      {r.status === 'Confirmed' && (
        <div
          className="panel"
          style={{ marginBottom: 'var(--space-5)', background: 'var(--free-wash)', borderColor: 'var(--free)' }}
          role="status"
        >
          <h1 style={{ color: '#1b5c3f' }}>ההזמנה נקלטה</h1>
          <p>הציוד שמור לכם לכל התאריכים. מספר הזמנה <strong className="num">#{r.id}</strong>.</p>
        </div>
      )}

      <div className="panel" style={{ display: 'grid', gap: 'var(--space-4)' }}>
        <div className="page-head" style={{ marginBottom: 0 }}>
          <h2>{r.eventName}</h2>
          <StatusBadge reservation={r} />
        </div>

        <p>
          {formatLong(r.startDate)} עד {formatLong(r.endDate)} <span className="muted">({days} ימים)</span>
        </p>
        {r.returnedOn && <p className="muted">הוחזרה ב־{formatLong(r.returnedOn)}</p>}

        <div className="table-wrap">
          <table className="table">
            <thead>
              <tr>
                <th>פריט</th>
                <th>כמות</th>
                <th>מחיר ליום</th>
                <th>סה״כ</th>
              </tr>
            </thead>
            <tbody>
              {r.items.map((item) => (
                <tr key={item.equipmentId}>
                  <td>{item.equipmentName}</td>
                  <td className="num">{item.quantity}</td>
                  <td className="num">{formatMoney(item.unitPrice)}</td>
                  <td className="num">{formatMoney(item.unitPrice * item.quantity * days)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        <p style={{ fontSize: '1.4rem', fontWeight: 700 }} className="num">
          סה״כ: {formatMoney(r.totalPrice)}
        </p>

        <div style={{ display: 'flex', flexWrap: 'wrap', gap: 'var(--space-3)' }}>
          <Link to="/reservations" className="btn">
            כל ההזמנות שלי
          </Link>
          <Link to="/equipment" className="btn btn-primary">
            להמשיך לקטלוג
          </Link>
          {canCancel(r) && (
            <button type="button" className="btn btn-danger" onClick={() => setCancelling(true)}>
              ביטול הזמנה
            </button>
          )}
        </div>
      </div>

      <StatusChangeDialog reservation={cancelling ? r : null} action="Cancelled" onClose={() => setCancelling(false)} />
    </div>
  )
}
