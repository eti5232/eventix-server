import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useState, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { reservationsApi } from '../api/endpoints'
import { toApiError, type ApiError } from '../api/errors'
import { RangeButton } from '../components/RangeButton'
import { ErrorBanner } from '../components/ui/ErrorBanner'
import { KitLines } from '../features/kit/KitLines'
import { useKit } from '../features/kit/KitContext'
import { formatMoney, formatRange } from '../lib/dates'
import styles from './CheckoutPage.module.css'

export function CheckoutPage() {
  const kit = useKit()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [eventName, setEventName] = useState('')
  const [error, setError] = useState<ApiError | null>(null)

  const create = useMutation({
    mutationFn: reservationsApi.create,
    onSuccess: async (reservation) => {
      // Stock changed for every day of the range, and the reservation lists are stale.
      await queryClient.invalidateQueries({ queryKey: ['availability'] })
      await queryClient.invalidateQueries({ queryKey: ['reservations'] })
      kit.clear()
      navigate(`/reservations/${reservation.id}`, { replace: true })
    },
    onError: (e) => {
      const apiError = toApiError(e)
      setError(apiError)
      // On a conflict the availability we showed is out of date: reload it so the strips and limits update.
      if (apiError.isConflict) void queryClient.invalidateQueries({ queryKey: ['availability'] })
    },
  })

  // Names the server mentioned in its 409 message mark the problem items in the kit.
  const flagged = error?.isConflict ? kit.items.filter((i) => error.detail.includes(i.name)).map((i) => i.name) : []

  function submit(event: FormEvent) {
    event.preventDefault()
    if (!kit.hasRange || create.isPending) return
    setError(null)
    create.mutate({
      eventName: eventName.trim(),
      startDate: kit.start!,
      endDate: kit.end!,
      items: kit.items.map((i) => ({ equipmentId: i.equipmentId, quantity: i.quantity })),
    })
  }

  if (kit.items.length === 0) {
    return (
      <div className="page">
        <div className="empty">
          <h1 style={{ fontSize: '2.2rem' }}>ההזמנה ריקה</h1>
          <p className="muted">עדיין לא נוסף ציוד להזמנה. בחרו תאריכים והוסיפו ציוד מהקטלוג.</p>
          <Link to="/equipment" className="btn btn-primary">
            לקטלוג הציוד
          </Link>
        </div>
      </div>
    )
  }

  const nameError = error?.fieldError('eventName')

  return (
    <div className="page">
      <div className="page-head">
        <h1>סיכום הזמנה</h1>
      </div>

      {error && (
        <div style={{ marginBottom: 'var(--space-5)' }}>
          <ErrorBanner
            error={error}
            actions={
              error.isConflict ? (
                <span style={{ fontSize: '0.9rem' }}>עדכנו כמות או הסירו את הפריט המסומן, ואז נסו שוב. ההזמנה לא נשמרה.</span>
              ) : undefined
            }
          />
        </div>
      )}

      <div className={styles.layout}>
        <section aria-labelledby="items">
          <h2 id="items" style={{ marginBottom: 'var(--space-3)' }}>
            הפריטים בהזמנה
          </h2>
          <KitLines flaggedNames={flagged} />
        </section>

        <form className={`panel ${styles.side}`} onSubmit={submit} noValidate>
          <div className="field">
            <label htmlFor="eventName">שם האירוע</label>
            <input
              id="eventName"
              className="input"
              value={eventName}
              onChange={(e) => setEventName(e.target.value)}
              placeholder="למשל: כנס שנתי, חתונה, השקה"
              maxLength={200}
              aria-invalid={nameError ? true : undefined}
              required
            />
            {nameError && <span className="field-error">{nameError}</span>}
          </div>

          <div className={styles.range}>
            <span className="muted">תאריכי האירוע</span>
            <RangeButton emphasis={!kit.hasRange} />
          </div>

          {kit.hasRange ? (
            <dl className={styles.breakdown}>
              <div>
                <dt>טווח</dt>
                <dd className="num">
                  {formatRange(kit.start!, kit.end!)} ({kit.days} ימים)
                </dd>
              </div>
              {kit.items.map((i) => (
                <div key={i.equipmentId}>
                  <dt>
                    {i.name} <span className="muted">× {i.quantity}</span>
                  </dt>
                  <dd className="num">
                    {i.quantity} × {kit.days} × {formatMoney(i.unitPrice)} = {formatMoney(i.quantity * kit.days * i.unitPrice)}
                  </dd>
                </div>
              ))}
              <div className={styles.total}>
                <dt>סה״כ</dt>
                <dd className="num">{formatMoney(kit.total)}</dd>
              </div>
            </dl>
          ) : (
            <p className="field-error">בחרו תאריכים כדי להזמין.</p>
          )}

          <button
            type="submit"
            className="btn btn-primary"
            disabled={create.isPending || !kit.hasRange || eventName.trim().length < 2}
          >
            {create.isPending ? 'שולחים…' : 'אישור הזמנה'}
          </button>
          <p className="muted" style={{ fontSize: '0.85rem' }}>
            ההזמנה נשמרת כולה או לא נשמרת בכלל. אם פריט אינו זמין, לא יישמר דבר.
          </p>
        </form>
      </div>
    </div>
  )
}
