const PATHS = {
  edit: (
    <>
      <path d="M4 20h4L19 9a2.8 2.8 0 0 0-4-4L4 16z" />
      <path d="m13.5 6.5 4 4" />
    </>
  ),
  trash: (
    <>
      <path d="M4 7h16" />
      <path d="M9 7V4.5A1.5 1.5 0 0 1 10.5 3h3A1.5 1.5 0 0 1 15 4.5V7" />
      <path d="M6.5 7l.8 12a2 2 0 0 0 2 1.9h5.4a2 2 0 0 0 2-1.9l.8-12" />
      <path d="M10 11v6M14 11v6" />
    </>
  ),
}

interface IconButtonProps {
  icon: keyof typeof PATHS
  /** Spoken by screen readers and shown as a tooltip, since the button has no visible text. */
  label: string
  onClick: () => void
}

/** A small square button with a line icon (edit pencil or trash can). The remove action is tinted on hover. */
export function IconButton({ icon, label, onClick }: IconButtonProps) {
  return (
    <button
      type="button"
      className={`btn btn-small icon-btn ${icon === 'trash' ? 'icon-btn-danger' : ''}`}
      aria-label={label}
      title={label}
      onClick={onClick}
    >
      <svg viewBox="0 0 24 24" aria-hidden="true" focusable="false" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round">
        {PATHS[icon]}
      </svg>
    </button>
  )
}
