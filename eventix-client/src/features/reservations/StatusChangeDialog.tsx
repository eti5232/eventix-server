import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'
import { reservationsApi } from '../../api/endpoints'
import { toApiError, type ApiError } from '../../api/errors'
import { Dialog } from '../../components/ui/Dialog'
import { ErrorBanner } from '../../components/ui/ErrorBanner'
import { useToast } from '../../components/ui/Toast'
import { formatMoney, formatRange } from '../../lib/dates'
import type { Reservation } from '../../types'

interface StatusChangeDialogProps {
  reservation: Reservation | null
  action: 'Cancelled' | 'Returned'
  onClose: () => void
}

const COPY = {
  Cancelled: {
    title: 'ביטול הזמנה',
    question: 'לבטל את ההזמנה? הציוד יחזור למלאי מיד.',
    confirm: 'כן, לבטל',
    done: 'ההזמנה בוטלה',
    danger: true,
  },
  Returned: {
    title: 'סימון כהוחזרה',
    question: 'הציוד חזר למחסן? החזרה מוקדמת משחררת את המלאי מהיום שאחרי.',
    confirm: 'סימון כהוחזרה',
    done: 'ההזמנה סומנה כהוחזרה',
    danger: false,
  },
} as const

/** A confirmation window for cancelling or returning; the server's refusal (409 etc.) is shown inside it. */
export function StatusChangeDialog({ reservation, action, onClose }: StatusChangeDialogProps) {
  const queryClient = useQueryClient()
  const toast = useToast()
  const [error, setError] = useState<ApiError | null>(null)
  const copy = COPY[action]

  useEffect(() => setError(null), [reservation?.id, action])

  const mutation = useMutation({
    mutationFn: () => reservationsApi.changeStatus(reservation!.id, action),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['reservations'] })
      await queryClient.invalidateQueries({ queryKey: ['availability'] })
      toast.show(copy.done)
      onClose()
    },
    onError: (e) => setError(toApiError(e)),
  })

  return (
    <Dialog open={reservation !== null} onClose={onClose} title={copy.title}>
      {reservation && (
        <>
          <p>{copy.question}</p>
          <p className="muted num">
            {reservation.eventName} · {formatRange(reservation.startDate, reservation.endDate)} · {formatMoney(reservation.totalPrice)}
          </p>
          {error && <ErrorBanner error={error} />}
          <div style={{ display: 'flex', gap: 'var(--space-3)', justifyContent: 'flex-end' }}>
            <button type="button" className="btn" onClick={onClose}>
              חזרה
            </button>
            <button
              type="button"
              className={`btn ${copy.danger ? 'btn-danger' : 'btn-primary'}`}
              disabled={mutation.isPending}
              onClick={() => {
                setError(null)
                mutation.mutate()
              }}
            >
              {mutation.isPending ? 'רגע…' : copy.confirm}
            </button>
          </div>
        </>
      )}
    </Dialog>
  )
}
