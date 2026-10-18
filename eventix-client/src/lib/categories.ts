// Presentation only: the server's category names are in English, the UI shows Hebrew names and a line icon.
// An unknown category falls back to the server's name and a generic crate icon, so new categories still render.

export type CategoryKind = 'speakers' | 'microphones' | 'lighting' | 'projectors' | 'stages' | 'crate'

interface CategoryLook {
  kind: CategoryKind
  label: string
}

const LOOKS: Record<string, CategoryLook> = {
  speakers: { kind: 'speakers', label: 'רמקולים ומערכות קול' },
  microphones: { kind: 'microphones', label: 'מיקרופונים' },
  lighting: { kind: 'lighting', label: 'תאורה' },
  projectors: { kind: 'projectors', label: 'מקרנים ומסכים' },
  stages: { kind: 'stages', label: 'במות וטראסים' },
}

export function categoryLook(name: string): CategoryLook {
  const look = LOOKS[name.trim().toLowerCase()]
  return look ?? { kind: 'crate', label: name }
}
