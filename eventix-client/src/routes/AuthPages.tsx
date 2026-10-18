import { useState, type FormEvent } from 'react'
import { Link, Navigate, useLocation, useNavigate } from 'react-router-dom'
import { toApiError, type ApiError } from '../api/errors'
import { ErrorBanner } from '../components/ui/ErrorBanner'
import { useAuth } from '../features/auth/AuthContext'
import { DEMO_ACCOUNTS } from '../features/auth/demoAccounts'
import styles from './AuthPages.module.css'

interface LocationState {
  from?: string
  expired?: boolean
}

function Field({
  id,
  label,
  error,
  ...rest
}: { id: string; label: string; error?: string } & React.InputHTMLAttributes<HTMLInputElement>) {
  return (
    <div className="field">
      <label htmlFor={id}>{label}</label>
      <input
        id={id}
        className="input"
        aria-invalid={error ? true : undefined}
        aria-describedby={error ? `${id}-error` : undefined}
        {...rest}
      />
      {error && (
        <span id={`${id}-error`} className="field-error">
          {error}
        </span>
      )}
    </div>
  )
}

/** Field errors sit next to their field; anything else (wrong password, no connection) goes in a banner. */
function leftover(error: ApiError, fields: string[]) {
  return fields.some((f) => error.fieldError(f)) ? null : error
}

export function LoginPage() {
  const { user, login } = useAuth()
  const navigate = useNavigate()
  const state = (useLocation().state ?? {}) as LocationState
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<ApiError | null>(null)
  const [busy, setBusy] = useState(false)

  if (user) return <Navigate to={state.from ?? '/'} replace />

  async function signIn(credentials: { email: string; password: string }) {
    setBusy(true)
    setError(null)
    try {
      await login(credentials)
      navigate(state.from ?? '/', { replace: true })
    } catch (e) {
      setError(toApiError(e))
    } finally {
      setBusy(false)
    }
  }

  function submit(event: FormEvent) {
    event.preventDefault()
    void signIn({ email: email.trim(), password })
  }

  const banner = error && leftover(error, ['email', 'password'])

  return (
    <div className="page">
      <div className={styles.card}>
        <h1>התחברות</h1>
        {state.expired && <p className="field-error">ההתחברות פגה. התחברו שוב ונחזיר אתכם למקום שבו הייתם.</p>}
        <form onSubmit={submit} className={styles.form} noValidate>
          <Field id="email" label="אימייל" type="email" autoComplete="email" dir="ltr" value={email} onChange={(e) => setEmail(e.target.value)} error={error?.fieldError('email')} required />
          <Field id="password" label="סיסמה" type="password" autoComplete="current-password" dir="ltr" value={password} onChange={(e) => setPassword(e.target.value)} error={error?.fieldError('password')} required />
          {banner && (
            <ErrorBanner
              error={banner}
              title={banner.status === 401 ? 'פרטי ההתחברות שגויים' : undefined}
              message={banner.status === 401 ? 'האימייל או הסיסמה אינם נכונים. בדקו ונסו שוב.' : undefined}
            />
          )}
          <button type="submit" className="btn btn-primary" disabled={busy || !email || !password}>
            {busy ? 'מתחברים…' : 'התחברות'}
          </button>
        </form>
        <p className="muted">
          אין לכם חשבון? <Link to="/register" state={state}>פתיחת חשבון</Link>
        </p>

        <div className={styles.demo}>
          <p className={styles.demoTitle}>אתר הדגמה: כניסה מהירה</p>
          <div className={styles.demoButtons}>
            {DEMO_ACCOUNTS.map((account) => (
              <button
                key={account.email}
                type="button"
                className={`btn ${styles.demoButton}`}
                disabled={busy}
                onClick={() => void signIn(account)}
              >
                <strong>{account.label}</strong>
                <span>{account.hint}</span>
              </button>
            ))}
          </div>
        </div>
      </div>
    </div>
  )
}

export function RegisterPage() {
  const { user, register } = useAuth()
  const navigate = useNavigate()
  const state = (useLocation().state ?? {}) as LocationState
  const [fullName, setFullName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<ApiError | null>(null)
  const [busy, setBusy] = useState(false)

  if (user) return <Navigate to={state.from ?? '/'} replace />

  async function submit(event: FormEvent) {
    event.preventDefault()
    setBusy(true)
    setError(null)
    try {
      await register({ fullName: fullName.trim(), email: email.trim(), password })
      navigate(state.from ?? '/', { replace: true })
    } catch (e) {
      setError(toApiError(e))
    } finally {
      setBusy(false)
    }
  }

  const banner = error && leftover(error, ['fullName', 'email', 'password'])

  return (
    <div className="page">
      <div className={styles.card}>
        <h1>פתיחת חשבון</h1>
        <p className="muted">עם חשבון אפשר להזמין ציוד ולעקוב אחרי ההזמנות.</p>
        <form onSubmit={submit} className={styles.form} noValidate>
          <Field id="fullName" label="שם מלא" autoComplete="name" value={fullName} onChange={(e) => setFullName(e.target.value)} error={error?.fieldError('fullName')} required />
          <Field id="email" label="אימייל" type="email" autoComplete="email" dir="ltr" value={email} onChange={(e) => setEmail(e.target.value)} error={error?.fieldError('email') ?? (error?.status === 409 ? error.detail : undefined)} required />
          <Field id="password" label="סיסמה (לפחות 8 תווים, אות וספרה)" type="password" autoComplete="new-password" dir="ltr" value={password} onChange={(e) => setPassword(e.target.value)} error={error?.fieldError('password')} required />
          {banner && banner.status !== 409 && <ErrorBanner error={banner} />}
          <button type="submit" className="btn btn-primary" disabled={busy || !fullName || !email || !password}>
            {busy ? 'פותחים חשבון…' : 'פתיחת חשבון'}
          </button>
        </form>
        <p className="muted">
          כבר רשומים? <Link to="/login" state={state}>התחברות</Link>
        </p>
      </div>
    </div>
  )
}
