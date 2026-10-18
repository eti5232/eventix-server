import type { ReactNode } from 'react'
import { parseStockConflict, type ApiError } from '../../api/errors'
import { formatLong } from '../../lib/dates'
import styles from './ErrorBanner.module.css'

interface ErrorBannerProps {
  error: ApiError
  title?: string
  /** Buttons such as "update quantity" or "remove from kit". */
  actions?: ReactNode
}

/** A red banner with the server's message and the support code (correlationId) that matches the server log. */
export function ErrorBanner({ error, title, actions }: ErrorBannerProps) {
  const heading =
    title ?? (error.isConflict ? 'הציוד אינו זמין בתאריכים שנבחרו' : error.status === 0 ? 'אין חיבור לשרת' : 'אירעה תקלה')
  const stock = error.isConflict ? parseStockConflict(error.detail) : null
  const message = stock
    ? `אין מספיק ${stock.item} ב${formatLong(stock.day)}: ביקשתם ${stock.requested}, פנויות ${stock.available}.`
    : error.detail

  return (
    <div className={styles.banner} role="alert">
      <div className={styles.text}>
        <strong>{heading}</strong>
        <p>{message}</p>
        {error.correlationId && (
          <p className={styles.code}>
            קוד תקלה: <code>{error.correlationId}</code>
          </p>
        )}
      </div>
      {actions && <div className={styles.actions}>{actions}</div>}
    </div>
  )
}
