import { useState } from 'react'
import { useKit } from '../kit/KitContext'
import { useToast } from '../../components/ui/Toast'
import type { Equipment } from '../../types'

interface AddToKitProps {
  equipment: Equipment
  /** Lowest free count over the chosen range; undefined while loading or with no range. */
  minAvailable: number | undefined
}

/** Quantity picker limited to what is free, and the add button (disabled with a reason when it cannot work). */
export function AddToKit({ equipment, minAvailable }: AddToKitProps) {
  const kit = useKit()
  const toast = useToast()
  const [quantity, setQuantity] = useState(1)

  const inOrder = kit.items.find((i) => i.equipmentId === equipment.id)?.quantity ?? 0
  const room = minAvailable === undefined ? 0 : Math.max(0, minAvailable - inOrder)
  const chosen = Math.min(quantity, Math.max(1, room))

  let reason: string | null = null
  if (!kit.hasRange) reason = 'בחרו תאריכים כדי להוסיף להזמנה'
  else if (minAvailable === undefined) reason = 'בודקים זמינות…'
  else if (minAvailable === 0) reason = 'הפריט תפוס בתאריכים שנבחרו'
  else if (room === 0) reason = 'הוספתם להזמנה את כל הכמות הפנויה'

  return (
    <div style={{ display: 'grid', gap: 'var(--space-2)' }}>
      <div style={{ display: 'flex', flexWrap: 'wrap', alignItems: 'center', gap: 'var(--space-3)' }}>
        <div
          role="group"
          aria-label={`כמות עבור ${equipment.name}`}
          style={{ display: 'inline-flex', alignItems: 'center', gap: 'var(--space-2)' }}
        >
          <button
            type="button"
            className="btn btn-small"
            style={{ padding: 0, width: 38 }}
            onClick={() => setQuantity(Math.max(1, chosen - 1))}
            disabled={reason !== null || chosen <= 1}
            aria-label="הפחתת כמות"
          >
            −
          </button>
          <span className="num" style={{ minWidth: '2ch', textAlign: 'center', fontWeight: 800 }}>
            {chosen}
          </span>
          <button
            type="button"
            className="btn btn-small"
            style={{ padding: 0, width: 38 }}
            onClick={() => setQuantity(Math.min(room, chosen + 1))}
            disabled={reason !== null || chosen >= room}
            aria-label="הגדלת כמות"
          >
            +
          </button>
        </div>
        <button
          type="button"
          className="btn btn-primary"
          disabled={reason !== null}
          style={{ flex: '1 1 auto' }}
          onClick={() => {
            kit.add(equipment, chosen)
            setQuantity(1)
            toast.show(`${equipment.name} נוסף להזמנה`)
          }}
        >
          הוסף להזמנה
        </button>
      </div>
      {reason && (
        <span className="muted" style={{ fontSize: '0.85rem' }}>
          {reason}
        </span>
      )}
    </div>
  )
}
