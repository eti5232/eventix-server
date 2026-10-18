import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react'
import { MAX_RANGE_DAYS, addDays, dayCount, todayIso } from '../../lib/dates'
import type { Equipment } from '../../types'

const STORAGE_KEY = 'eventix.kit'

export interface KitItem {
  equipmentId: number
  name: string
  unitPrice: number
  quantity: number
  /** Only used to pick the photo in the order list; older saved orders may not have it. */
  categoryName?: string
}

interface KitSnapshot {
  start: string | null
  end: string | null
  items: KitItem[]
}

interface KitState extends KitSnapshot {
  /** Number of units in the kit (drives the header counter). */
  unitCount: number
  /** Changes every time something is added, so the header can play its one "loaded" animation. */
  addedTick: number
  days: number
  total: number
  hasRange: boolean
  setRange: (start: string, end: string) => void
  clearRange: () => void
  add: (equipment: Equipment, quantity: number) => void
  setQuantity: (equipmentId: number, quantity: number) => void
  remove: (equipmentId: number) => void
  clear: () => void
  drawerOpen: boolean
  setDrawerOpen: (open: boolean) => void
}

const KitContext = createContext<KitState | null>(null)

function load(): KitSnapshot {
  try {
    const stored = JSON.parse(localStorage.getItem(STORAGE_KEY) ?? 'null') as KitSnapshot | null
    if (stored && Array.isArray(stored.items)) {
      // A range that is already in the past is useless; drop it but keep the items.
      const stale = stored.start != null && stored.start < todayIso()
      return { start: stale ? null : stored.start, end: stale ? null : stored.end, items: stored.items }
    }
  } catch {
    /* corrupted storage: start empty */
  }
  return { start: null, end: null, items: [] }
}

export function KitProvider({ children }: { children: ReactNode }) {
  const [snapshot, setSnapshot] = useState<KitSnapshot>(load)
  const [addedTick, setAddedTick] = useState(0)
  const [drawerOpen, setDrawerOpen] = useState(false)

  useEffect(() => {
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(snapshot))
    } catch {
      /* storage full or blocked: the kit simply will not survive a reload */
    }
  }, [snapshot])

  const setRange = useCallback((start: string, end: string) => {
    // Never allow more than the server's maximum, whatever the caller passes.
    const limitedEnd = dayCount(start, end) > MAX_RANGE_DAYS ? addDays(start, MAX_RANGE_DAYS - 1) : end
    setSnapshot((s) => ({ ...s, start, end: limitedEnd }))
  }, [])

  const clearRange = useCallback(() => setSnapshot((s) => ({ ...s, start: null, end: null })), [])

  const add = useCallback((equipment: Equipment, quantity: number) => {
    setSnapshot((s) => {
      const existing = s.items.find((i) => i.equipmentId === equipment.id)
      const items = existing
        ? s.items.map((i) => (i.equipmentId === equipment.id ? { ...i, quantity: i.quantity + quantity } : i))
        : [
            ...s.items,
            {
              equipmentId: equipment.id,
              name: equipment.name,
              unitPrice: equipment.dailyPrice,
              quantity,
              categoryName: equipment.categoryName,
            },
          ]
      return { ...s, items }
    })
    setAddedTick((t) => t + 1)
  }, [])

  const setQuantity = useCallback((equipmentId: number, quantity: number) => {
    setSnapshot((s) => ({
      ...s,
      items: s.items.map((i) => (i.equipmentId === equipmentId ? { ...i, quantity: Math.max(1, quantity) } : i)),
    }))
  }, [])

  const remove = useCallback((equipmentId: number) => {
    setSnapshot((s) => ({ ...s, items: s.items.filter((i) => i.equipmentId !== equipmentId) }))
  }, [])

  const clear = useCallback(() => setSnapshot({ start: null, end: null, items: [] }), [])

  const value = useMemo<KitState>(() => {
    const hasRange = snapshot.start != null && snapshot.end != null
    const days = hasRange ? dayCount(snapshot.start!, snapshot.end!) : 0
    const perDay = snapshot.items.reduce((sum, i) => sum + i.unitPrice * i.quantity, 0)
    return {
      ...snapshot,
      unitCount: snapshot.items.reduce((sum, i) => sum + i.quantity, 0),
      addedTick,
      days,
      total: perDay * days,
      hasRange,
      setRange,
      clearRange,
      add,
      setQuantity,
      remove,
      clear,
      drawerOpen,
      setDrawerOpen,
    }
  }, [snapshot, addedTick, drawerOpen, setRange, clearRange, add, setQuantity, remove, clear])

  return <KitContext.Provider value={value}>{children}</KitContext.Provider>
}

// eslint-disable-next-line react-refresh/only-export-components
export function useKit(): KitState {
  const context = useContext(KitContext)
  if (!context) throw new Error('useKit must be used inside KitProvider')
  return context
}
