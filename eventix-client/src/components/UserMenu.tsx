import { useEffect, useId, useRef, useState } from 'react'
import type { User } from '../types'
import styles from './UserMenu.module.css'

interface UserMenuProps {
  user: User
  onLogout: () => void
}

/** First letters of the first two words of the name, for the avatar circle. */
function initials(fullName: string): string {
  const letters = fullName
    .trim()
    .split(/\s+/)
    .slice(0, 2)
    .map((word) => word.charAt(0))
    .join('')
  return letters.toUpperCase() || '?'
}

/** The signed-in user: an avatar that opens a small menu with the name, the role and the sign-out action. */
export function UserMenu({ user, onLogout }: UserMenuProps) {
  const [open, setOpen] = useState(false)
  const root = useRef<HTMLDivElement>(null)
  const menuId = useId()

  // Close on a click outside or on Escape.
  useEffect(() => {
    if (!open) return
    const onPointer = (event: PointerEvent) => {
      if (!root.current?.contains(event.target as Node)) setOpen(false)
    }
    const onKey = (event: KeyboardEvent) => {
      if (event.key === 'Escape') setOpen(false)
    }
    document.addEventListener('pointerdown', onPointer)
    document.addEventListener('keydown', onKey)
    return () => {
      document.removeEventListener('pointerdown', onPointer)
      document.removeEventListener('keydown', onKey)
    }
  }, [open])

  return (
    <div className={styles.root} ref={root}>
      <button
        type="button"
        className={styles.avatar}
        aria-haspopup="true"
        aria-expanded={open}
        aria-controls={menuId}
        aria-label={`תפריט משתמש, ${user.fullName}`}
        onClick={() => setOpen((value) => !value)}
      >
        {initials(user.fullName)}
      </button>

      {open && (
        <div id={menuId} className={styles.menu} role="group" aria-label="חשבון">
          <div className={styles.who}>
            <strong>{user.fullName}</strong>
            <span>{user.role === 'Admin' ? 'מנהל מערכת' : 'לקוח'}</span>
          </div>
          <button type="button" className={styles.logout} onClick={onLogout}>
            <svg viewBox="0 0 24 24" aria-hidden="true" focusable="false" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round">
              <path d="M15 4h3a2 2 0 0 1 2 2v12a2 2 0 0 1-2 2h-3" />
              <path d="M10 8l-4 4 4 4M6 12h9" />
            </svg>
            יציאה
          </button>
        </div>
      )}
    </div>
  )
}
