import { Link, NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useAuth } from '../features/auth/AuthContext'
import { KitDrawer } from '../features/kit/KitDrawer'
import { useKit } from '../features/kit/KitContext'
import { formatRange } from '../lib/dates'
import styles from './Layout.module.css'

export function Layout() {
  const { user, isAdmin, logout } = useAuth()
  const kit = useKit()
  const navigate = useNavigate()

  const navClass = ({ isActive }: { isActive: boolean }) => `${styles.link} ${isActive ? styles.active : ''}`

  return (
    <>
      <a href="#main" className={styles.skip}>
        דילוג לתוכן
      </a>
      <header className={styles.header}>
        <div className={styles.bar}>
          <Link to="/" className={styles.logo} aria-label="Eventix, דף הבית">
            <img src="/brand/eventix-logo.png" alt="Eventix" width={163} height={56} />
          </Link>

          <nav className={styles.nav} aria-label="ראשי">
            <NavLink to="/equipment" className={navClass}>
              קטלוג הציוד
            </NavLink>
            {user && !isAdmin && (
              <NavLink to="/reservations" className={navClass}>
                ההזמנות שלי
              </NavLink>
            )}
            {isAdmin && (
              <>
                <NavLink to="/admin/reservations" className={navClass}>
                  הזמנות
                </NavLink>
                <NavLink to="/admin/equipment" className={navClass}>
                  ניהול ציוד
                </NavLink>
                <NavLink to="/admin/categories" className={navClass}>
                  קטגוריות
                </NavLink>
              </>
            )}
          </nav>

          <div className={styles.actions}>
            {kit.hasRange && (
              <button type="button" className={styles.dates} onClick={() => kit.setDrawerOpen(true)}>
                <span className={styles.datesLabel}>תאריכי האירוע</span>
                <span className="num">{formatRange(kit.start!, kit.end!)}</span>
              </button>
            )}

            <button
              type="button"
              className={styles.cart}
              onClick={() => kit.setDrawerOpen(true)}
              aria-label={`ההזמנה שלי, ${kit.unitCount} פריטים`}
            >
              <svg viewBox="0 0 24 24" className={styles.cartIcon} aria-hidden="true">
                <path d="M3 4h2.2l2.1 10.2a1.5 1.5 0 0 0 1.5 1.2h8.2a1.5 1.5 0 0 0 1.5-1.1L20 8H6.2" />
                <circle cx="9.5" cy="19.2" r="1.3" />
                <circle cx="16.5" cy="19.2" r="1.3" />
              </svg>
              <span className={styles.cartText}>ההזמנה שלי</span>
              {/* The key restarts the single animation each time something is added. */}
              <span key={kit.addedTick} className={`${styles.count} ${kit.addedTick > 0 ? styles.bump : ''} num`}>
                {kit.unitCount}
              </span>
            </button>

            {user ? (
              <div className={styles.user}>
                <span className={styles.userName}>{user.fullName}</span>
                <button
                  type="button"
                  className="btn btn-small"
                  onClick={() => {
                    logout()
                    navigate('/')
                  }}
                >
                  יציאה
                </button>
              </div>
            ) : (
              <Link to="/login" className="btn btn-small">
                התחברות
              </Link>
            )}
          </div>
        </div>
      </header>

      <main id="main">
        <Outlet />
      </main>

      <footer className={styles.footer}>
        <div className={styles.footerInner}>
          <div className={styles.brand}>
            <img src="/brand/eventix-logo-dark.png" alt="Eventix" width={163} height={56} />
            <span>השכרת ציוד הפקה לאירועים</span>
          </div>
          <nav className={styles.footerNav} aria-label="תחתית">
            <Link to="/equipment">קטלוג הציוד</Link>
            {!isAdmin && <Link to={user ? '/reservations' : '/login'}>{user ? 'ההזמנות שלי' : 'התחברות'}</Link>}
            {!user && <Link to="/register">פתיחת חשבון</Link>}
          </nav>
        </div>
      </footer>

      <KitDrawer />
    </>
  )
}
