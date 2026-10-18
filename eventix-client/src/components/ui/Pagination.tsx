interface PaginationProps {
  page: number
  pageSize: number
  totalCount: number
  onPageChange: (page: number) => void
}

export function Pagination({ page, pageSize, totalCount, onPageChange }: PaginationProps) {
  const pages = Math.max(1, Math.ceil(totalCount / pageSize))
  if (pages <= 1) return null

  return (
    <nav
      aria-label="עמודים"
      style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 'var(--space-3)', marginTop: 'var(--space-5)' }}
    >
      <button type="button" className="btn btn-small" disabled={page <= 1} onClick={() => onPageChange(page - 1)}>
        הקודם
      </button>
      <span className="num" aria-current="page">
        עמוד {page} מתוך {pages}
      </span>
      <button type="button" className="btn btn-small" disabled={page >= pages} onClick={() => onPageChange(page + 1)}>
        הבא
      </button>
    </nav>
  )
}
