import { unitsLabel } from '../lib/plural'
import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { reservationsApi } from '../api/endpoints'
import { toApiError } from '../api/errors'
import { ErrorBanner } from '../components/ui/ErrorBanner'
import { Pagination } from '../components/ui/Pagination'
import { StatusBadge, canCancel } from '../features/reservations/status'
import { StatusChangeDialog } from '../features/reservations/StatusChangeDialog'
import { formatMoney, formatRange } from '../lib/dates'
import type { Reservation, ReservationStatus } from '../types'

const TABS: { status: ReservationStatus; label: string; empty: string }[] = [
  { status: 'Confirmed', label: 'קרובות', empty: 'אין הזמנות פעילות כרגע.' },
  { status: 'Returned', label: 'הוחזרו', empty: 'עוד לא הוחזר ציוד.' },
  { status: 'Cancelled', label: 'בוטלו', empty: 'לא ביטלתם הזמנות.' },
]

export function MyReservationsPage() {
  const [params, setParams] = useSearchParams()
  const tab = TABS.find((t) => t.status === params.get('tab')) ?? TABS[0]
  const page = Math.max(1, Number(params.get('page')) || 1)
  const [cancelling, setCancelling] = useState<Reservation | null>(null)

  const query = useQuery({
    queryKey: ['reservations', 'mine', tab.status, page],
    queryFn: () => reservationsApi.list({ status: tab.status, page, pageSize: 10 }),
    placeholderData: keepPreviousData,
  })

  return (
    <div className="page">
      <div className="page-head">
        <h1>ההזמנות שלי</h1>
        <Link to="/equipment" className="btn btn-primary">
          הזמנה חדשה
        </Link>
      </div>

      <div role="tablist" aria-label="סטטוס הזמנה" style={{ display: 'flex', gap: 'var(--space-2)', marginBottom: 'var(--space-5)' }}>
        {TABS.map((t) => (
          <button
            key={t.status}
            role="tab"
            type="button"
            className="chip"
            aria-selected={tab.status === t.status}
            aria-pressed={tab.status === t.status}
            onClick={() => setParams(t.status === 'Confirmed' ? {} : { tab: t.status })}
          >
            {t.label}
          </button>
        ))}
      </div>

      {query.isError && <ErrorBanner error={toApiError(query.error)} title="לא הצלחנו לטעון הזמנות" />}
      {query.isLoading && <div className="skeleton" style={{ height: 200 }} />}

      {query.data && query.data.items.length === 0 && (
        <div className="empty">
          <h2>{tab.empty}</h2>
          {tab.status === 'Confirmed' && (
            <Link to="/equipment" className="btn btn-primary">
              לקטלוג
            </Link>
          )}
        </div>
      )}

      {query.data && query.data.items.length > 0 && (
        <>
          <ul style={{ listStyle: 'none', margin: 0, padding: 0, display: 'grid', gap: 'var(--space-4)' }}>
            {query.data.items.map((r) => (
              <li key={r.id} className="panel" style={{ display: 'grid', gap: 'var(--space-3)' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', gap: 'var(--space-3)', flexWrap: 'wrap' }}>
                  <h2 style={{ fontSize: '1.9rem' }}>{r.eventName}</h2>
                  <StatusBadge reservation={r} />
                </div>
                <p className="muted num">
                  {formatRange(r.startDate, r.endDate)} · {unitsLabel(r.items.reduce((sum, i) => sum + i.quantity, 0))} · {formatMoney(r.totalPrice)}
                </p>
                <p>{r.items.map((i) => `${i.equipmentName} ×${i.quantity}`).join(' · ')}</p>
                <div style={{ display: 'flex', gap: 'var(--space-2)' }}>
                  <Link to={`/reservations/${r.id}`} className="btn btn-small">
                    פרטים
                  </Link>
                  {canCancel(r) && (
                    <button type="button" className="btn btn-small btn-danger" onClick={() => setCancelling(r)}>
                      ביטול
                    </button>
                  )}
                </div>
              </li>
            ))}
          </ul>
          <Pagination
            page={query.data.page}
            pageSize={query.data.pageSize}
            totalCount={query.data.totalCount}
            onPageChange={(p) => setParams({ ...(tab.status === 'Confirmed' ? {} : { tab: tab.status }), ...(p > 1 ? { page: String(p) } : {}) })}
          />
        </>
      )}

      <StatusChangeDialog reservation={cancelling} action="Cancelled" onClose={() => setCancelling(null)} />
    </div>
  )
}
