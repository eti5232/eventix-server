import { useEffect, useId, useRef, type ReactNode } from 'react'
import styles from './Dialog.module.css'

interface DialogProps {
  open: boolean
  onClose: () => void
  title: string
  children: ReactNode
  /** Wider layout, used for the calendar. */
  wide?: boolean
}

/** A modal on the native <dialog> element: focus trapping, Esc and the backdrop come from the browser. */
export function Dialog({ open, onClose, title, children, wide }: DialogProps) {
  const ref = useRef<HTMLDialogElement>(null)
  const titleId = useId()

  useEffect(() => {
    const dialog = ref.current
    if (!dialog) return
    if (open && !dialog.open) dialog.showModal()
    if (!open && dialog.open) dialog.close()
  }, [open])

  return (
    <dialog
      ref={ref}
      className={`${styles.dialog} ${wide ? styles.wide : ''}`}
      aria-labelledby={titleId}
      onClose={onClose}
      onClick={(event) => {
        if (event.target === ref.current) onClose()
      }}
    >
      {open && (
        <div className={styles.body}>
          <div className={styles.head}>
            <h2 id={titleId}>{title}</h2>
            <button type="button" className="btn btn-ghost btn-small" onClick={onClose} aria-label="סגירה">
              ✕
            </button>
          </div>
          {children}
        </div>
      )}
    </dialog>
  )
}
