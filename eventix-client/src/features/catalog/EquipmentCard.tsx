import { Link } from 'react-router-dom'
import { AvailabilityStrip } from '../../components/AvailabilityStrip'
import { CategoryIcon } from '../../components/CategoryIcon'
import { categoryLook } from '../../lib/categories'
import { formatMoney } from '../../lib/dates'
import type { Equipment } from '../../types'
import { useKit } from '../kit/KitContext'
import { AddToKit } from './AddToKit'
import { useDayAvailability } from './useAvailability'
import styles from './EquipmentCard.module.css'

/** One catalog item: large photo, name, daily price, day-by-day availability and the add button. */
export function EquipmentCard({ equipment }: { equipment: Equipment }) {
  const kit = useKit()
  const availability = useDayAvailability(equipment.id, kit.start, kit.end)
  const look = categoryLook(equipment.categoryName)
  const soldOut = availability.minAvailable === 0

  return (
    <article className={`${styles.card} ${soldOut ? styles.soldOut : ''}`}>
      <Link to={`/equipment/${equipment.id}`} className={styles.media} tabIndex={-1} aria-hidden="true">
        <CategoryIcon name={equipment.categoryName} className={styles.icon} />
        <span className={styles.category}>{look.label}</span>
        {soldOut && <span className={styles.badge}>לא פנוי בתאריכים שנבחרו</span>}
      </Link>

      <div className={styles.body}>
        <h3 className={styles.name}>
          <Link to={`/equipment/${equipment.id}`} className={styles.link}>
            {equipment.name}
          </Link>
        </h3>
        {equipment.description && <p className={`${styles.desc} muted`}>{equipment.description}</p>}

        <p className={styles.price}>
          <strong className="num">{formatMoney(equipment.dailyPrice)}</strong>
          <span className="muted"> ליום</span>
        </p>

        <div className={styles.avail}>
          <AvailabilityStrip cells={availability.cells} totalQuantity={equipment.totalQuantity} compact />
          {kit.hasRange && (
            <p className={styles.summary} aria-live="polite">
              {availability.minAvailable === undefined ? (
                'בודקים זמינות…'
              ) : availability.minAvailable === 0 ? (
                <strong>אין יחידה פנויה בכל הטווח</strong>
              ) : (
                <>
                  פנויות בכל הטווח: <strong className="num">{availability.minAvailable}</strong> מתוך{' '}
                  <span className="num">{equipment.totalQuantity}</span>
                </>
              )}
            </p>
          )}
        </div>

        <AddToKit equipment={equipment} minAvailable={availability.minAvailable} />
      </div>
    </article>
  )
}
