import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { reservationsApi } from '../../api/endpoints'
import { toApiError } from '../../api/errors'
import { ErrorBanner } from '../../components/ui/ErrorBanner'
import { Pagination } from '../../components/ui/Pagination'
import { StatusBadge, canCancel, canReturn, isOverdue } from '../../features/reservations/status'
import { StatusChangeDialog } from '../../features/reservations/StatusChangeDialog'
import { formatMoney, formatRange } from '../../lib/dates'
import type { Reservation, ReservationStatus } from '../../types'

type View = 'all' | 'late' | ReservationStatus

const VIEWS: { key: View; label: string }[] = [
  { key: 'all', label: 'הכל' },
  { key: 'Confirmed', label: 'מאושרות' },
  { key: 'late', label: 'באיחור' },
  { key: 'Returned', label: 'הוחזרו' },
  { key: 'Cancelled', label: 'בוטלו' },
]

export function AdminReservationsPage() {
  const [params, setParams] = useSearchParams()
  const view = (VIEWS.find((v) => v.key === params.get('view'))?.key ?? 'all') as View
  const page = Math.max(1, Number(params.get('page')) || 1)
  const [action, setAction] = useState<{ reservation: Reservation; type: 'Cancelled' | 'Returned' } | null>(null)

  // "Late" is not a server status: it is Confirmed with an end date in the past, so we load Confirmed and filter here.
  const late = view === 'late'
  const status: ReservationStatus | undefined = late ? 'Confirmed' : view === 'all' ? undefined : view
  const pageSize = late ? 50 : 15

  const query = useQuery({
    queryKey: ['reservations', 'admin', status ?? 'all', late ? 1 : page, pageSize],
    queryFn: () => reservationsApi.list({ status, page: late ? 1 : page, pageSize }),
    placeholderData: keepPreviousData,
  })

  const rows = query.data ? (late ? query.data.items.filter(isOverdue) : query.data.items) : []

  return (
    <div className="page">
      <div className="page-head">
        <h1>כל ההזמנות</h1>
      </div>

      <div role="group" aria-label="סינון" style={{ display: 'flex', flexWrap: 'wrap', gap: 'var(--space-2)', marginBottom: 'var(--space-5)' }}>
        {VIEWS.map((v) => (
          <button key={v.key} type="button" className="chip" aria-pressed={view === v.key} onClick={() => setParams(v.key === 'all' ? {} : { view: v.key })}>
            {v.label}
          </button>
        ))}
      </div>

      {query.isError && <ErrorBanner error={toApiError(query.error)} title="לא הצלחנו לטעון הזמנות" />}
      {query.isLoading && <div className="skeleton" style={{ height: 240 }} />}

      {query.data && rows.length === 0 && (
        <div className="empty">
          <h2>{late ? 'אין הזמנות באיחור' : 'אין הזמנות להצגה'}</h2>
          <p className="muted">{late ? 'כל הציוד שהסתיימה השכרתו כבר הוחזר.' : 'נסו מסנן אחר.'}</p>
        </div>
      )}

      {rows.length > 0 && (
        <div className="table-wrap">
          <table className="table">
            <thead>
              <tr>
                <th>#</th>
                <th>אירוע</th>
                <th>לקוח</th>
                <th>תאריכים</th>
                <th>סכום</th>
                <th>סטטוס</th>
                <th>פעולות</th>
              </tr>
            </thead>
            <tbody>
              {rows.map((r) => (
                <tr key={r.id}>
                  <td className="num">{r.id}</td>
                  <td>
                    <Link to={`/reservations/${r.id}`}>{r.eventName}</Link>
                    <div className="muted" style={{ fontSize: '0.85rem' }}>
                      {r.items.map((i) => `${i.equipmentName} ×${i.quantity}`).join(' · ')}
                    </div>
                  </td>
                  <td>{r.userFullName}</td>
                  <td className="num">{formatRange(r.startDate, r.endDate)}</td>
                  <td className="num">{formatMoney(r.totalPrice)}</td>
                  <td>
                    <StatusBadge reservation={r} />
                  </td>
                  <td>
                    <div style={{ display: 'flex', gap: 'var(--space-2)' }}>
                      {canReturn(r) && (
                        <button type="button" className="btn btn-small btn-primary" onClick={() => setAction({ reservation: r, type: 'Returned' })}>
                          סימון כהוחזרה
                        </button>
                      )}
                      {canCancel(r) && (
                        <button type="button" className="btn btn-small" onClick={() => setAction({ reservation: r, type: 'Cancelled' })}>
                          ביטול
                        </button>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {query.data && !late && (
        <Pagination
          page={query.data.page}
          pageSize={query.data.pageSize}
          totalCount={query.data.totalCount}
          onPageChange={(p) => setParams({ ...(view === 'all' ? {} : { view }), ...(p > 1 ? { page: String(p) } : {}) })}
        />
      )}

      <StatusChangeDialog reservation={action?.reservation ?? null} action={action?.type ?? 'Cancelled'} onClose={() => setAction(null)} />
    </div>
  )
}
