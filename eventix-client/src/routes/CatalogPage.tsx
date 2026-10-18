import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { useState, type FormEvent } from 'react'
import { useSearchParams } from 'react-router-dom'
import { categoriesApi, equipmentApi, type EquipmentFilters } from '../api/endpoints'
import { toApiError } from '../api/errors'
import { RangeButton } from '../components/RangeButton'
import { ErrorBanner } from '../components/ui/ErrorBanner'
import { Pagination } from '../components/ui/Pagination'
import { EquipmentCard } from '../features/catalog/EquipmentCard'
import { useKit } from '../features/kit/KitContext'
import { categoryLook } from '../lib/categories'
import { formatRange } from '../lib/dates'
import styles from './CatalogPage.module.css'

const PAGE_SIZE = 9

/** Every filter lives in the URL, so a filtered catalog can be shared or reloaded. */
export function CatalogPage() {
  const kit = useKit()
  const [params, setParams] = useSearchParams()

  const categoryId = params.get('category') ? Number(params.get('category')) : undefined
  const search = params.get('q') ?? ''
  const minPrice = params.get('min') ? Number(params.get('min')) : undefined
  const maxPrice = params.get('max') ? Number(params.get('max')) : undefined
  const sort = params.get('sort') === 'price' ? 'Price' : 'Name'
  const dir = params.get('dir') === 'desc' ? 'Desc' : 'Asc'
  const page = Math.max(1, Number(params.get('page')) || 1)

  const [searchText, setSearchText] = useState(search)

  const filters: EquipmentFilters = {
    categoryId,
    search: search || undefined,
    minPrice,
    maxPrice,
    sortBy: sort,
    sortDir: dir,
    page,
    pageSize: PAGE_SIZE,
  }

  const categories = useQuery({ queryKey: ['categories'], queryFn: categoriesApi.list, staleTime: 5 * 60_000 })
  const equipment = useQuery({
    queryKey: ['equipment', filters],
    queryFn: () => equipmentApi.list(filters),
    placeholderData: keepPreviousData,
  })

  function update(changes: Record<string, string | null>) {
    const next = new URLSearchParams(params)
    for (const [key, value] of Object.entries(changes)) {
      if (value === null || value === '') next.delete(key)
      else next.set(key, value)
    }
    // Any filter change returns to the first page.
    if (!('page' in changes)) next.delete('page')
    setParams(next)
  }

  function submitSearch(event: FormEvent) {
    event.preventDefault()
    update({ q: searchText.trim() })
  }

  function clearAll() {
    setSearchText('')
    setParams(new URLSearchParams())
  }

  const hasFilters = Boolean(categoryId || search || minPrice !== undefined || maxPrice !== undefined)
  const priceActive = minPrice !== undefined || maxPrice !== undefined

  return (
    <>
      <div className={styles.intro}>
        <div className={styles.introInner}>
          <h1>קטלוג הציוד</h1>
          <p>
            {kit.hasRange ? (
              <>
                מציגים זמינות לתאריכים <strong className="num">{formatRange(kit.start!, kit.end!)}</strong>
              </>
            ) : (
              'בחרו תאריכי אירוע כדי לראות מה פנוי ולהוסיף להזמנה.'
            )}
          </p>
        </div>
      </div>

      <div className={styles.toolbar}>
        <div className={styles.toolbarInner}>
          <RangeButton emphasis={!kit.hasRange} />

          <form className={styles.search} onSubmit={submitSearch} role="search">
            <label className="sr-only" htmlFor="q">
              חיפוש ציוד
            </label>
            <input
              id="q"
              className="input"
              type="search"
              placeholder="חיפוש ציוד"
              value={searchText}
              onChange={(e) => setSearchText(e.target.value)}
            />
            <button type="submit" className="btn">
              חיפוש
            </button>
          </form>

          <div className={styles.sort}>
            <label className="sr-only" htmlFor="sort">
              מיון
            </label>
            <select
              id="sort"
              className="select"
              value={`${sort}-${dir}`}
              onChange={(e) => {
                const [s, d] = e.target.value.split('-')
                update({ sort: s === 'Price' ? 'price' : null, dir: d === 'Desc' ? 'desc' : null })
              }}
            >
              <option value="Name-Asc">מיון: שם (א–ת)</option>
              <option value="Name-Desc">מיון: שם (ת–א)</option>
              <option value="Price-Asc">מיון: מחיר מהנמוך לגבוה</option>
              <option value="Price-Desc">מיון: מחיר מהגבוה לנמוך</option>
            </select>
          </div>
        </div>
      </div>

      <div className="page" style={{ paddingTop: 'var(--space-5)' }}>
        <div className={styles.filters}>
          <div className={styles.chips} role="group" aria-label="קטגוריות">
            <button type="button" className="chip" aria-pressed={!categoryId} onClick={() => update({ category: null })}>
              הכל
            </button>
            {categories.data?.map((c) => (
              <button
                key={c.id}
                type="button"
                className="chip"
                aria-pressed={categoryId === c.id}
                onClick={() => update({ category: String(c.id) })}
              >
                {categoryLook(c.name).label}
              </button>
            ))}
          </div>

          <details className={styles.price} open={priceActive}>
            <summary>טווח מחירים ליום{priceActive ? ' (פעיל)' : ''}</summary>
            <div className={styles.priceRow}>
              <div className="field">
                <label htmlFor="min">מ־₪</label>
                <input
                  id="min"
                  className="input"
                  type="number"
                  min={0}
                  inputMode="numeric"
                  defaultValue={minPrice ?? ''}
                  key={`min-${minPrice ?? ''}`}
                  onBlur={(e) => update({ min: e.target.value })}
                />
              </div>
              <div className="field">
                <label htmlFor="max">עד ₪</label>
                <input
                  id="max"
                  className="input"
                  type="number"
                  min={0}
                  inputMode="numeric"
                  defaultValue={maxPrice ?? ''}
                  key={`max-${maxPrice ?? ''}`}
                  onBlur={(e) => update({ max: e.target.value })}
                />
              </div>
            </div>
          </details>

          {hasFilters && (
            <button type="button" className="btn btn-ghost btn-small" onClick={clearAll}>
              ניקוי סינון
            </button>
          )}
        </div>

        {equipment.isError && <ErrorBanner error={toApiError(equipment.error)} title="לא הצלחנו לטעון את הקטלוג" />}

        {equipment.isLoading && (
          <div className={styles.grid}>
            {Array.from({ length: 6 }, (_, i) => (
              <div key={i} className="skeleton" style={{ height: 460 }} />
            ))}
          </div>
        )}

        {equipment.data && equipment.data.items.length === 0 && (
          <div className="empty">
            <h2>לא נמצא ציוד</h2>
            <p className="muted">נסו חיפוש אחר או הרחיבו את טווח המחירים.</p>
            {hasFilters && (
              <button type="button" className="btn btn-primary" onClick={clearAll}>
                ניקוי סינון
              </button>
            )}
          </div>
        )}

        {equipment.data && equipment.data.items.length > 0 && (
          <>
            <p className="muted" aria-live="polite" style={{ marginBottom: 'var(--space-3)' }}>
              <span className="num">{equipment.data.totalCount}</span> פריטים
            </p>
            <div className={`${styles.grid} ${equipment.isPlaceholderData ? styles.stale : ''}`}>
              {equipment.data.items.map((item) => (
                <EquipmentCard key={item.id} equipment={item} />
              ))}
            </div>
            <Pagination
              page={equipment.data.page}
              pageSize={equipment.data.pageSize}
              totalCount={equipment.data.totalCount}
              onPageChange={(p) => update({ page: p === 1 ? null : String(p) })}
            />
          </>
        )}
      </div>
    </>
  )
}
