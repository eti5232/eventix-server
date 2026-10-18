import { daysLabel, unitsLabel } from '../lib/plural'
import { useQuery } from '@tanstack/react-query'
import { Link, useParams } from 'react-router-dom'
import { equipmentApi } from '../api/endpoints'
import { toApiError } from '../api/errors'
import { AvailabilityStrip } from '../components/AvailabilityStrip'
import { CategoryIcon } from '../components/CategoryIcon'
import { RangeButton } from '../components/RangeButton'
import { ErrorBanner } from '../components/ui/ErrorBanner'
import { AddToKit } from '../features/catalog/AddToKit'
import { useDayAvailability } from '../features/catalog/useAvailability'
import { useKit } from '../features/kit/KitContext'
import { categoryLook, categoryTone } from '../lib/categories'
import { formatMoney, formatRange } from '../lib/dates'
import { NotFoundPage } from './NotFoundPage'
import styles from './EquipmentPage.module.css'

export function EquipmentPage() {
  const id = Number(useParams().id)
  const kit = useKit()
  const item = useQuery({ queryKey: ['equipment', id], queryFn: () => equipmentApi.get(id), enabled: Number.isFinite(id) })
  const availability = useDayAvailability(id, kit.start, kit.end)

  if (item.isLoading) {
    return (
      <div className="page">
        <div className="skeleton" style={{ height: 480 }} />
      </div>
    )
  }
  if (item.isError) {
    const error = toApiError(item.error)
    if (error.status === 404) return <NotFoundPage what="הפריט" />
    return (
      <div className="page">
        <ErrorBanner error={error} title="לא הצלחנו לטעון את הפריט" />
      </div>
    )
  }
  const equipment = item.data!
  const look = categoryLook(equipment.categoryName)

  return (
    <div className="page">
      <nav className={styles.crumbs} aria-label="מיקום באתר">
        <Link to="/equipment">קטלוג הציוד</Link>
        <span aria-hidden="true">/</span>
        <Link to={`/equipment?category=${equipment.categoryId}`}>{look.label}</Link>
      </nav>

      <div className={styles.layout}>
        <div className={styles.photo} style={categoryTone(equipment.categoryName)}>
          <CategoryIcon name={equipment.categoryName} className={styles.icon} />
        </div>

        <div className={styles.side}>
          <section className={styles.info}>
            <p className={styles.category}>{look.label}</p>
            <h1>{equipment.name}</h1>
            {equipment.description && <p className={styles.desc}>{equipment.description}</p>}
            <p className={styles.price}>
              <strong className="num">{formatMoney(equipment.dailyPrice)}</strong> <span className="muted">ליום</span>
            </p>
            <p className="muted">
              כמות במלאי: <span className="num">{unitsLabel(equipment.totalQuantity)}</span>
            </p>
          </section>

          <section className={`panel ${styles.book}`} aria-labelledby="avail">
            <div className={styles.bookHead}>
              <h2 id="avail">זמינות לתאריכי האירוע</h2>
              <RangeButton emphasis={!kit.hasRange} />
            </div>

            <AvailabilityStrip cells={availability.cells} totalQuantity={equipment.totalQuantity} />

            {kit.hasRange && (
              <p aria-live="polite">
                {availability.minAvailable === undefined ? (
                  'בודקים זמינות…'
                ) : availability.minAvailable === 0 ? (
                  <strong>אין יחידה פנויה בכל הטווח {formatRange(kit.start!, kit.end!)}</strong>
                ) : (
                  <>
                    פנויות בכל הטווח: <strong className="num">{availability.minAvailable}</strong> מתוך{' '}
                    <span className="num">{equipment.totalQuantity}</span>
                  </>
                )}
              </p>
            )}
            {availability.isError && <p className="field-error">לא הצלחנו לבדוק זמינות. נסו שוב.</p>}

            <AddToKit equipment={equipment} minAvailable={availability.minAvailable} />

            {kit.hasRange && availability.minAvailable !== undefined && (
              <p className="muted num">
                {daysLabel(kit.days)} × {formatMoney(equipment.dailyPrice)} = {formatMoney(kit.days * equipment.dailyPrice)} ליחידה
              </p>
            )}
          </section>
        </div>
      </div>
    </div>
  )
}
