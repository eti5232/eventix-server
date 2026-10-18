import { daysLabel, itemsLabel } from '../../lib/plural'
import { useEffect, useRef } from 'react'
import { Link } from 'react-router-dom'
import { RangeButton } from '../../components/RangeButton'
import { formatMoney, formatRange } from '../../lib/dates'
import { KitLines } from './KitLines'
import { useKit } from './KitContext'
import styles from './KitDrawer.module.css'

/** The order cart ("ההזמנה שלי"): a side panel on desktop, a bottom sheet on phones. */
export function KitDrawer() {
  const kit = useKit()
  const ref = useRef<HTMLDialogElement>(null)

  useEffect(() => {
    const dialog = ref.current
    if (!dialog) return
    if (kit.drawerOpen && !dialog.open) dialog.showModal()
    if (!kit.drawerOpen && dialog.open) dialog.close()
  }, [kit.drawerOpen])

  return (
    <dialog
      ref={ref}
      className={styles.drawer}
      aria-label="ההזמנה שלי"
      onClose={() => kit.setDrawerOpen(false)}
      onClick={(event) => {
        if (event.target === ref.current) kit.setDrawerOpen(false)
      }}
    >
      {kit.drawerOpen && (
        <div className={styles.inner}>
          <div className={styles.head}>
            <h2>ההזמנה שלי</h2>
            <button type="button" className="btn btn-ghost btn-small" onClick={() => kit.setDrawerOpen(false)} aria-label="סגירה">
              ✕
            </button>
          </div>

          <div className={styles.range}>
            <span className="muted">תאריכי האירוע</span>
            <RangeButton emphasis={!kit.hasRange} />
          </div>

          {kit.items.length === 0 ? (
            <div className="empty">
              <h3>ההזמנה ריקה</h3>
              <p className="muted">בחרו תאריכים והוסיפו ציוד מהקטלוג.</p>
              <Link to="/equipment" className="btn btn-primary" onClick={() => kit.setDrawerOpen(false)}>
                לקטלוג הציוד
              </Link>
            </div>
          ) : (
            <>
              <KitLines />
              <div className={styles.summary}>
                {kit.hasRange ? (
                  <>
                    <div className={styles.row}>
                      <span>
                        {itemsLabel(kit.unitCount)} × {daysLabel(kit.days)}
                      </span>
                      <span className="muted num">{formatRange(kit.start!, kit.end!)}</span>
                    </div>
                    <div className={`${styles.row} ${styles.total}`}>
                      <span>סה״כ</span>
                      <strong className="num">{formatMoney(kit.total)}</strong>
                    </div>
                    <Link to="/checkout" className="btn btn-primary" onClick={() => kit.setDrawerOpen(false)}>
                      המשך לסיכום ההזמנה
                    </Link>
                  </>
                ) : (
                  <p className="muted">בחרו תאריכים כדי לראות מחיר ולהמשיך להזמנה.</p>
                )}
              </div>
            </>
          )}
        </div>
      )}
    </dialog>
  )
}
