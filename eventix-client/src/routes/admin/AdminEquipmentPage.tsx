import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState, type FormEvent } from 'react'
import { useSearchParams } from 'react-router-dom'
import { categoriesApi, equipmentApi } from '../../api/endpoints'
import { toApiError, type ApiError } from '../../api/errors'
import { Dialog } from '../../components/ui/Dialog'
import { ErrorBanner } from '../../components/ui/ErrorBanner'
import { IconButton } from '../../components/ui/IconButton'
import { Pagination } from '../../components/ui/Pagination'
import { useToast } from '../../components/ui/Toast'
import { formatMoney } from '../../lib/dates'
import type { Equipment, EquipmentRequest } from '../../types'

const EMPTY: EquipmentRequest = { name: '', description: '', dailyPrice: 0, totalQuantity: 1, categoryId: 0 }

export function AdminEquipmentPage() {
  const queryClient = useQueryClient()
  const toast = useToast()
  const [params, setParams] = useSearchParams()
  const page = Math.max(1, Number(params.get('page')) || 1)

  const [editing, setEditing] = useState<Equipment | 'new' | null>(null)
  const [removing, setRemoving] = useState<Equipment | null>(null)
  const [form, setForm] = useState<EquipmentRequest>(EMPTY)
  const [error, setError] = useState<ApiError | null>(null)

  const categories = useQuery({ queryKey: ['categories'], queryFn: categoriesApi.list })
  const list = useQuery({
    queryKey: ['equipment', 'admin', page],
    queryFn: () => equipmentApi.list({ page, pageSize: 15 }),
  })

  useEffect(() => {
    setError(null)
    if (editing === 'new') setForm({ ...EMPTY, categoryId: categories.data?.[0]?.id ?? 0 })
    else if (editing) {
      setForm({
        name: editing.name,
        description: editing.description ?? '',
        dailyPrice: editing.dailyPrice,
        totalQuantity: editing.totalQuantity,
        categoryId: editing.categoryId,
      })
    }
  }, [editing, categories.data])

  async function refresh() {
    await queryClient.invalidateQueries({ queryKey: ['equipment'] })
    await queryClient.invalidateQueries({ queryKey: ['availability'] })
  }

  const save = useMutation({
    mutationFn: () =>
      editing === 'new'
        ? equipmentApi.create({ ...form, description: form.description || null })
        : equipmentApi.update((editing as Equipment).id, { ...form, description: form.description || null }),
    onSuccess: async () => {
      await refresh()
      toast.show(editing === 'new' ? 'הפריט נוצר' : 'הפריט עודכן')
      setEditing(null)
    },
    onError: (e) => setError(toApiError(e)),
  })

  const remove = useMutation({
    mutationFn: () => equipmentApi.remove(removing!.id),
    onSuccess: async () => {
      await refresh()
      toast.show('הפריט הוסר מהקטלוג')
      setRemoving(null)
    },
    onError: (e) => setError(toApiError(e)),
  })

  function submit(event: FormEvent) {
    event.preventDefault()
    setError(null)
    save.mutate()
  }

  const set = <K extends keyof EquipmentRequest>(key: K, value: EquipmentRequest[K]) => setForm((f) => ({ ...f, [key]: value }))

  return (
    <div className="page">
      <div className="page-head">
        <h1>ניהול ציוד</h1>
        <button type="button" className="btn btn-primary" onClick={() => setEditing('new')}>
          פריט חדש
        </button>
      </div>

      {list.isError && <ErrorBanner error={toApiError(list.error)} title="לא הצלחנו לטעון את הציוד" />}
      {list.isLoading && <div className="skeleton" style={{ height: 260 }} />}

      {list.data && list.data.items.length === 0 && (
        <div className="empty">
          <h2>אין ציוד בקטלוג</h2>
          <button type="button" className="btn btn-primary" onClick={() => setEditing('new')}>
            הוספת הפריט הראשון
          </button>
        </div>
      )}

      {list.data && list.data.items.length > 0 && (
        <>
          <div className="table-wrap">
            <table className="table">
              <thead>
                <tr>
                  <th>פריט</th>
                  <th>קטגוריה</th>
                  <th>מחיר ליום</th>
                  <th>כמות במחסן</th>
                  <th>פעולות</th>
                </tr>
              </thead>
              <tbody>
                {list.data.items.map((item) => (
                  <tr key={item.id}>
                    <td>
                      <strong>{item.name}</strong>
                    </td>
                    <td>{item.categoryName}</td>
                    <td className="num">{formatMoney(item.dailyPrice)}</td>
                    <td className="num">{item.totalQuantity}</td>
                    <td>
                      <div style={{ display: 'flex', gap: 'var(--space-2)' }}>
                        <IconButton icon="edit" label="עריכת פריט" onClick={() => setEditing(item)} />
                        <IconButton
                          icon="trash"
                          label="הסרת פריט מהקטלוג"
                          onClick={() => {
                            setError(null)
                            setRemoving(item)
                          }}
                        />
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <Pagination
            page={list.data.page}
            pageSize={list.data.pageSize}
            totalCount={list.data.totalCount}
            onPageChange={(p) => setParams(p > 1 ? { page: String(p) } : {})}
          />
        </>
      )}

      <Dialog open={editing !== null} onClose={() => setEditing(null)} title={editing === 'new' ? 'פריט חדש' : 'עריכת פריט'}>
        <form onSubmit={submit} style={{ display: 'grid', gap: 'var(--space-4)' }} noValidate>
          <div className="field">
            <label htmlFor="name">שם</label>
            <input id="name" className="input" value={form.name} onChange={(e) => set('name', e.target.value)} aria-invalid={error?.fieldError('name') ? true : undefined} required />
            {error?.fieldError('name') && <span className="field-error">{error.fieldError('name')}</span>}
          </div>
          <div className="field">
            <label htmlFor="desc">תיאור</label>
            <textarea id="desc" className="textarea" value={form.description ?? ''} onChange={(e) => set('description', e.target.value)} maxLength={500} />
          </div>
          <div className="field">
            <label htmlFor="cat">קטגוריה</label>
            <select id="cat" className="select" value={form.categoryId} onChange={(e) => set('categoryId', Number(e.target.value))}>
              {categories.data?.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.name}
                </option>
              ))}
            </select>
          </div>
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 'var(--space-3)' }}>
            <div className="field">
              <label htmlFor="price">מחיר ליום (₪)</label>
              <input id="price" className="input" type="number" min={0.01} step="0.01" value={form.dailyPrice} onChange={(e) => set('dailyPrice', Number(e.target.value))} />
              {error?.fieldError('dailyPrice') && <span className="field-error">{error.fieldError('dailyPrice')}</span>}
            </div>
            <div className="field">
              <label htmlFor="qty">כמות במחסן</label>
              <input id="qty" className="input" type="number" min={0} value={form.totalQuantity} onChange={(e) => set('totalQuantity', Number(e.target.value))} />
              {error?.fieldError('totalQuantity') && <span className="field-error">{error.fieldError('totalQuantity')}</span>}
            </div>
          </div>
          {/* A 409 here means the quantity is below what is already booked; the server's message names the day. */}
          {error && <ErrorBanner error={error} />}
          <button type="submit" className="btn btn-primary" disabled={save.isPending || form.name.trim().length < 2 || !form.categoryId}>
            {save.isPending ? 'שומרים…' : 'שמירה'}
          </button>
        </form>
      </Dialog>

      <Dialog open={removing !== null} onClose={() => setRemoving(null)} title="הסרת פריט">
        {removing && (
          <>
            <p>
              להסיר את <strong>{removing.name}</strong> מהקטלוג? הפריט יוסתר מהקטלוג ולא יימחק מהמערכת. אם יש לו הזמנות פעילות, לא ניתן להסיר אותו.
            </p>
            {error && <ErrorBanner error={error} />}
            <div style={{ display: 'flex', gap: 'var(--space-3)', justifyContent: 'flex-end' }}>
              <button type="button" className="btn" onClick={() => setRemoving(null)}>
                חזרה
              </button>
              <button
                type="button"
                className="btn btn-danger"
                disabled={remove.isPending}
                onClick={() => {
                  setError(null)
                  remove.mutate()
                }}
              >
                {remove.isPending ? 'רגע…' : 'הסרה'}
              </button>
            </div>
          </>
        )}
      </Dialog>
    </div>
  )
}
