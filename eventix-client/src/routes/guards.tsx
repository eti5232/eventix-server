import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { useAuth } from '../features/auth/AuthContext'

/** Signed-in users only; everyone else goes to /login and comes back here afterwards. */
export function RequireAuth() {
  const { user } = useAuth()
  const location = useLocation()
  if (!user) {
    return <Navigate to="/login" replace state={{ from: location.pathname + location.search }} />
  }
  return <Outlet />
}

/** The role comes from the token and only decides what is shown; the server enforces it for real. */
export function RequireRole({ role }: { role: 'Admin' | 'Producer' }) {
  const { user } = useAuth()
  const location = useLocation()
  if (!user) {
    return <Navigate to="/login" replace state={{ from: location.pathname + location.search }} />
  }
  if (user.role !== role) {
    return (
      <div className="page">
        <div className="empty">
          <h2>אין הרשאה לעמוד הזה</h2>
          <p className="muted">
            {role === 'Admin' ? 'העמוד הזה מיועד למנהלי המערכת.' : 'עמוד ההזמנות מיועד ללקוחות. חשבון מנהל אינו מזמין ציוד.'}
          </p>
        </div>
      </div>
    )
  }
  return <Outlet />
}
