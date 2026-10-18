import { Component, type ErrorInfo, type ReactNode } from 'react'

interface State {
  error: Error | null
}

/** Catches a crash while rendering any screen and shows a calm message instead of a blank page. */
export class ErrorBoundary extends Component<{ children: ReactNode }, State> {
  state: State = { error: null }

  static getDerivedStateFromError(error: Error): State {
    return { error }
  }

  componentDidCatch(error: Error, info: ErrorInfo) {
    console.error('Unhandled UI error', error, info.componentStack)
  }

  render() {
    if (!this.state.error) return this.props.children
    return (
      <div className="page">
        <div className="empty" role="alert">
          <h1 style={{ fontSize: '2.8rem' }}>משהו השתבש במסך הזה</h1>
          <p className="muted">נסו לרענן את הדף. אם זה חוזר, ספרו לנו מה ניסיתם לעשות.</p>
          <button type="button" className="btn btn-primary" onClick={() => window.location.assign('/')}>
            לדף הבית
          </button>
        </div>
      </div>
    )
  }
}
