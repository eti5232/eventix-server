import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState, type FormEvent } from 'react'
import { categoriesApi } from '../../api/endpoints'
import { toApiError, type ApiError } from '../../api/errors'
import { Dialog } from '../../components/ui/Dialog'
import { ErrorBanner } from '../../components/ui/ErrorBanner'
import { useToast } from '../../components/ui/Toast'
import type { Category } from '../../types'

export function AdminCategoriesPage() {
  const queryClient = useQueryClient()
  const toast = useToast()
  const [editing, setEditing] = useState<Category | 'new' | null>(null)
  const [name, setName] = useState('')
  const [description, setDescription] = useState('')
  const [error, setError] = useState<ApiError | null>(null)

  const list = useQuery({ queryKey: ['categories'], queryFn: categoriesApi.list })

  useEffect(() => {
    setError(null)
    setName(editing && editing !== 'new' ? editing.name : '')
    setDescription(editing && editing !== 'new' ? (editing.description ?? '') : '')
  }, [editing])

  const save = useMutation({
    mutationFn: () => {
      const body = { name: name.trim(), description: description.trim() || null }
      return editing === 'new' ? categoriesApi.create(body) : categoriesApi.update((editing as Category).id, body)
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['categories'] })
      await queryClient.invalidateQueries({ queryKey: ['equipment'] })
      toast.show(editing === 'new' ? 'הקטגוריה נוצרה' : 'הקטגוריה עודכנה')
      setEditing(null)
    },
    onError: (e) => setError(toApiError(e)),
  })

  function submit(event: FormEvent) {
    event.preventDefault()
    setError(null)
    save.mutate()
  }

  return (
    <div className="page">
      <div className="page-head">
        <h1>קטגוריות</h1>
        <button type="button" className="btn btn-primary" onClick={() => setEditing('new')}>
          קטגוריה חדשה
        </button>
      </div>

      {list.isError && <ErrorBanner error={toApiError(list.error)} title="לא הצלחנו לטעון קטגוריות" />}
      {list.isLoading && <div className="skeleton" style={{ height: 200 }} />}

      {list.data && (
        <div className="table-wrap">
          <table className="table">
            <thead>
              <tr>
                <th>שם</th>
                <th>תיאור</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {list.data.map((c) => (
                <tr key={c.id}>
                  <td>
                    <strong>{c.name}</strong>
                  </td>
                  <td className="muted">{c.description}</td>
                  <td>
                    <button type="button" className="btn btn-small" onClick={() => setEditing(c)}>
                      עריכה
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      <Dialog open={editing !== null} onClose={() => setEditing(null)} title={editing === 'new' ? 'קטגוריה חדשה' : 'עריכת קטגוריה'}>
        <form onSubmit={submit} style={{ display: 'grid', gap: 'var(--space-4)' }} noValidate>
          <div className="field">
            <label htmlFor="cname">שם</label>
            <input id="cname" className="input" value={name} onChange={(e) => setName(e.target.value)} aria-invalid={error?.fieldError('name') ? true : undefined} required />
            {error?.fieldError('name') && <span className="field-error">{error.fieldError('name')}</span>}
          </div>
          <div className="field">
            <label htmlFor="cdesc">תיאור</label>
            <textarea id="cdesc" className="textarea" value={description} onChange={(e) => setDescription(e.target.value)} maxLength={500} />
          </div>
          {error && <ErrorBanner error={error} />}
          <button type="submit" className="btn btn-primary" disabled={save.isPending || name.trim().length < 2}>
            {save.isPending ? 'שומרים…' : 'שמירה'}
          </button>
        </form>
      </Dialog>
    </div>
  )
}
