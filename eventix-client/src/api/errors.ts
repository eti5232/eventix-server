import { AxiosError } from 'axios'

/** The server's uniform error body (RFC 7807 ProblemDetails, plus our correlationId). */
interface ProblemDetails {
  status?: number
  title?: string
  detail?: string
  correlationId?: string
  errors?: Record<string, string[]>
}

/** An API error in one shape the whole client understands. */
export class ApiError extends Error {
  status: number
  detail: string
  correlationId?: string
  /** Field name (camelCase) to messages, from 400 validation responses. */
  fieldErrors: Record<string, string[]>

  constructor(status: number, detail: string, correlationId?: string, fieldErrors: Record<string, string[]> = {}) {
    super(detail)
    this.name = 'ApiError'
    this.status = status
    this.detail = detail
    this.correlationId = correlationId
    this.fieldErrors = fieldErrors
  }

  get isConflict() {
    return this.status === 409
  }

  /** First message for a field, e.g. fieldError('email'). */
  fieldError(field: string): string | undefined {
    return this.fieldErrors[field]?.[0]
  }
}

/** The server's "not enough stock" message names the item and the day; this pulls them out for a Hebrew message. */
export function parseStockConflict(detail: string) {
  const match = /Not enough '(.+)' available on (\d{4}-\d{2}-\d{2}): requested (\d+), available (\d+)/.exec(detail)
  if (!match) return null
  return { item: match[1], day: match[2], requested: Number(match[3]), available: Number(match[4]) }
}

const camel = (key: string) => key.charAt(0).toLowerCase() + key.slice(1)

export function toApiError(error: unknown): ApiError {
  if (error instanceof ApiError) return error

  if (error instanceof AxiosError) {
    if (!error.response) {
      return new ApiError(0, 'אין חיבור לשרת. בדקו את החיבור ונסו שוב.')
    }
    const body = error.response.data as ProblemDetails | undefined
    const fieldErrors: Record<string, string[]> = {}
    for (const [key, messages] of Object.entries(body?.errors ?? {})) {
      fieldErrors[camel(key.split('.').pop() ?? key)] = messages
    }
    const detail = body?.detail || body?.title || 'אירעה תקלה בשרת.'
    return new ApiError(error.response.status, detail, body?.correlationId, fieldErrors)
  }

  return new ApiError(-1, error instanceof Error ? error.message : 'אירעה תקלה לא צפויה.')
}
