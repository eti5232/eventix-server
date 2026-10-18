import type { ReactNode } from 'react'
import { parseStockConflict, type ApiError } from '../../api/errors'
import { formatLong } from '../../lib/dates'
import styles from './ErrorBanner.module.css'

interface ErrorBannerProps {
  error: ApiError
  title?: string
  /** Replaces the server's message (which is in English) with a Hebrew one. */
  message?: string
  /** Buttons such as "update quantity" or "remove from order". */
  actions?: ReactNode
}

/**
 * A warm banner with the server's message. The support code (correlationId, matches the server log)
 * is shown only when it helps: unexpected server errors (5xx) and booking conflicts (409).
 * Ordinary mistakes, such as a wrong password, do not show it.
 */
export function ErrorBanner({ error, title, message: messageOverride, actions }: ErrorBannerProps) {
  const heading =
    title ?? (error.isConflict ? 'הציוד אינו זמין בתאריכים שנבחרו' : error.status === 0 ? 'אין חיבור לשרת' : 'אירעה תקלה')
  const stock = error.isConflict ? parseStockConflict(error.detail) : null
  const message =
    messageOverride ??
    (stock
      ? `אין מספיק ${stock.item} ב${formatLong(stock.day)}: ביקשתם ${stock.requested}, פנויות ${stock.available}.`
      : error.detail)
  const showCode = Boolean(error.correlationId) && (error.status >= 500 || error.isConflict)

  return (
    <div className={styles.banner} role="alert">
      <div className={styles.text}>
        <strong>{heading}</strong>
        <p>{message}</p>
        {showCode && (
          <p className={styles.code}>
            קוד תקלה: <code>{error.correlationId}</code>
          </p>
        )}
      </div>
      {actions && <div className={styles.actions}>{actions}</div>}
    </div>
  )
}
