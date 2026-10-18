import { Link } from 'react-router-dom'

export function NotFoundPage({ what = 'העמוד' }: { what?: string }) {
  return (
    <div className="page">
      <div className="empty">
        <p style={{ fontSize: '5rem', fontWeight: 900, lineHeight: 1, color: 'var(--brand)' }}>404</p>
        <h1 style={{ fontSize: '2.2rem' }}>{what} לא נמצא</h1>
        <p className="muted">ייתכן שהקישור ישן או שהפריט הוסר מהקטלוג.</p>
        <Link to="/equipment" className="btn btn-primary">
          לקטלוג הציוד
        </Link>
      </div>
    </div>
  )
}
