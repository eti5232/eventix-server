import { useKit, type KitItem } from './KitContext'
import { useRangeAvailability } from '../catalog/useAvailability'
import { CategoryIcon } from '../../components/CategoryIcon'
import { IconButton } from '../../components/ui/IconButton'
import { categoryTone } from '../../lib/categories'
import { formatMoney } from '../../lib/dates'
import styles from './KitLines.module.css'

interface KitLineProps {
  item: KitItem
  /** True for the item the server named in a 409; it is marked and gets the fix buttons. */
  flagged?: boolean
}

function KitLine({ item, flagged }: KitLineProps) {
  const kit = useKit()
  const availability = useRangeAvailability(item.equipmentId, kit.start, kit.end)
  const max = availability.data?.availableQuantity
  const over = max !== undefined && item.quantity > max
  const soldOut = max === 0

  return (
    <li className={`${styles.line} ${over || flagged ? styles.problem : ''}`} data-equipment-id={item.equipmentId}>
      <span className={styles.thumb} style={categoryTone(item.categoryName ?? '')} aria-hidden="true">
        <CategoryIcon name={item.categoryName ?? ''} className={styles.thumbIcon} />
      </span>
      <div className={styles.main}>
        <strong>{item.name}</strong>
        <span className="muted num">
          {formatMoney(item.unitPrice)} ליום
          {kit.days > 0 && ` · ${formatMoney(item.unitPrice * item.quantity * kit.days)} לכל התקופה`}
        </span>
      </div>

      <div className={styles.controls}>
        <div className={styles.stepper} role="group" aria-label={`כמות עבור ${item.name}`}>
          <button
            type="button"
            className="btn btn-small"
            style={{ padding: 0, width: 38 }}
            onClick={() => kit.setQuantity(item.equipmentId, item.quantity - 1)}
            disabled={item.quantity <= 1}
            aria-label="הפחתת כמות"
          >
            −
          </button>
          <span className={`${styles.qty} num`}>{item.quantity}</span>
          <button
            type="button"
            className="btn btn-small"
            style={{ padding: 0, width: 38 }}
            onClick={() => kit.setQuantity(item.equipmentId, item.quantity + 1)}
            disabled={max !== undefined && item.quantity >= max}
            aria-label="הגדלת כמות"
          >
            +
          </button>
        </div>
        <IconButton icon="trash" label="הסרה מההזמנה" onClick={() => kit.remove(item.equipmentId)} />
      </div>

      {(over || flagged) && max !== undefined && (
        <div className={styles.warning} role="alert">
          {soldOut ? 'אין יחידות פנויות בטווח הזה.' : `בתאריכים שנבחרו נשארו רק ${max} יחידות פנויות.`}
          {!soldOut && (
            <button type="button" className="btn btn-small" onClick={() => kit.setQuantity(item.equipmentId, max)}>
              עדכון לכמות {max}
            </button>
          )}
          <IconButton icon="trash" label="הסרה מההזמנה" onClick={() => kit.remove(item.equipmentId)} />
        </div>
      )}
    </li>
  )
}

export function KitLines({ flaggedNames = [] }: { flaggedNames?: string[] }) {
  const kit = useKit()
  return (
    <ul className={styles.list}>
      {kit.items.map((item) => (
        <KitLine key={item.equipmentId} item={item} flagged={flaggedNames.includes(item.name)} />
      ))}
    </ul>
  )
}
