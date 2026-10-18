import { categoryLook, type CategoryKind } from '../lib/categories'

/** Line-art icons for equipment categories (original vector drawings). Stroke follows `color`; details use the brand orange. */
const ICONS: Record<CategoryKind, React.ReactNode> = {
  speakers: (
    <>
      <rect x="30" y="10" width="60" height="100" rx="10" />
      <circle cx="60" cy="34" r="9" />
      <circle cx="60" cy="78" r="22" />
      <circle cx="60" cy="78" r="8" className="accent" />
    </>
  ),
  microphones: (
    <>
      <circle cx="60" cy="32" r="20" />
      <path d="M42 26c10 4 26 4 36 0M41 38c11 4 27 4 38 0" />
      <path d="M50 51h20l-5 24H55z" />
      <path d="M60 75v28M42 105h36" />
      <path d="M45 66c0 8 6 14 15 14s15-6 15-14" className="accent" />
    </>
  ),
  lighting: (
    <>
      <path d="M26 38v50q0 8 8 8h52q8 0 8-8V38" />
      <circle cx="60" cy="50" r="26" />
      <circle cx="60" cy="50" r="14" />
      <circle cx="60" cy="50" r="5" className="accent" />
      <path d="M44 106h32" />
    </>
  ),
  projectors: (
    <>
      <rect x="12" y="36" width="96" height="48" rx="10" />
      <circle cx="78" cy="60" r="16" />
      <circle cx="78" cy="60" r="6" className="accent" />
      <path d="M26 50h26M26 60h26M26 70h26" />
      <path d="M26 84v8M94 84v8" />
    </>
  ),
  stages: (
    <>
      <path d="M10 28h100M10 62h100" />
      <path d="M10 62 28 28l18 34 18-34 18 34 18-34" />
      <path d="M24 62v22M96 62v22" />
      <rect x="8" y="84" width="104" height="16" rx="3" />
      <path d="M20 92h80" className="accent" />
    </>
  ),
  crate: (
    <>
      <rect x="12" y="26" width="96" height="70" rx="8" />
      <path d="M12 46h96" />
      <rect x="44" y="58" width="32" height="12" rx="5" className="accent" />
    </>
  ),
}

export function CategoryIcon({ name, className }: { name: string; className?: string }) {
  return (
    <svg
      viewBox="0 0 120 120"
      className={className}
      aria-hidden="true"
      focusable="false"
      fill="none"
      stroke="currentColor"
      strokeWidth="3"
      strokeLinecap="round"
      strokeLinejoin="round"
    >
      <style>{`.accent{stroke:#ff7a00}`}</style>
      {ICONS[categoryLook(name).kind]}
    </svg>
  )
}
