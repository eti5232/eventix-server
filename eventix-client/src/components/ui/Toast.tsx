import { createContext, useCallback, useContext, useMemo, useState, type ReactNode } from 'react'
import styles from './Toast.module.css'

type ToastKind = 'ok' | 'error'
interface ToastItem {
  id: number
  kind: ToastKind
  text: string
}

interface ToastApi {
  show: (text: string, kind?: ToastKind) => void
}

const ToastContext = createContext<ToastApi | null>(null)
let nextId = 1

export function ToastProvider({ children }: { children: ReactNode }) {
  const [toasts, setToasts] = useState<ToastItem[]>([])

  const show = useCallback((text: string, kind: ToastKind = 'ok') => {
    const id = nextId++
    setToasts((list) => [...list, { id, kind, text }])
    window.setTimeout(() => setToasts((list) => list.filter((t) => t.id !== id)), 4500)
  }, [])

  const api = useMemo(() => ({ show }), [show])

  return (
    <ToastContext.Provider value={api}>
      {children}
      <div className={styles.region} role="status" aria-live="polite">
        {toasts.map((t) => (
          <div key={t.id} className={`${styles.toast} ${t.kind === 'error' ? styles.error : ''}`}>
            {t.text}
          </div>
        ))}
      </div>
    </ToastContext.Provider>
  )
}

// eslint-disable-next-line react-refresh/only-export-components
export function useToast(): ToastApi {
  const context = useContext(ToastContext)
  if (!context) throw new Error('useToast must be used inside ToastProvider')
  return context
}
