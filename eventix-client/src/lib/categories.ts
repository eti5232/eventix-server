import type { CSSProperties } from 'react'

// Presentation only: the server's category names are in English, the UI shows Hebrew names, a line icon and a color.
// An unknown category falls back to the server's name, a generic crate icon and the brand color, so new categories still render.

export type CategoryKind = 'speakers' | 'microphones' | 'lighting' | 'projectors' | 'stages' | 'crate'
type Tone = 'yellow' | 'teal' | 'coral' | 'brand'

interface CategoryLook {
  kind: CategoryKind
  label: string
  tone: Tone
}

const LOOKS: Record<string, CategoryLook> = {
  speakers: { kind: 'speakers', label: 'רמקולים ומערכות קול', tone: 'yellow' },
  microphones: { kind: 'microphones', label: 'מיקרופונים', tone: 'teal' },
  lighting: { kind: 'lighting', label: 'תאורה', tone: 'coral' },
  projectors: { kind: 'projectors', label: 'מקרנים ומסכים', tone: 'brand' },
  stages: { kind: 'stages', label: 'במות וטראסים', tone: 'yellow' },
}

// Soft background and strong accent for each tone (the strong color is used for the icon detail).
const TONES: Record<Tone, { wash: string; strong: string }> = {
  yellow: { wash: 'var(--yellow-wash)', strong: 'var(--yellow-strong)' },
  teal: { wash: 'var(--teal-wash)', strong: 'var(--teal)' },
  coral: { wash: 'var(--coral-wash)', strong: 'var(--coral)' },
  brand: { wash: 'var(--brand-wash)', strong: 'var(--brand)' },
}

export function categoryLook(name: string): CategoryLook {
  return LOOKS[name.trim().toLowerCase()] ?? { kind: 'crate', label: name, tone: 'brand' }
}

/** CSS variables (`--tone-wash`, `--icon-accent`) to put on the element that holds a category's icon. */
export function categoryTone(name: string): CSSProperties {
  const tone = TONES[categoryLook(name).tone]
  return { ['--tone-wash' as string]: tone.wash, ['--icon-accent' as string]: tone.strong }
}
