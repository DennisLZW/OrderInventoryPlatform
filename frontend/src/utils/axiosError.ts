import axios from 'axios'

type ApiErrorBody = {
  code?: string
  message?: string
  title?: string
  details?: string[] | null
}

function formatApiBody(data: ApiErrorBody): string | null {
  const msg = data.message ?? data.title
  if (!msg) return null
  let out = data.code ? `${data.code}: ${msg}` : msg
  if (data.details?.length) {
    out += ` (${data.details.join('; ')})`
  }
  return out
}

/** User-facing message from a failed request or thrown value. */
export function getRequestErrorMessage(err: unknown): string {
  if (axios.isAxiosError(err)) {
    const raw = err.response?.data
    if (raw && typeof raw === 'object') {
      const formatted = formatApiBody(raw as ApiErrorBody)
      if (formatted) return formatted
    }
    if (err.message) return err.message
  }
  if (err instanceof Error) return err.message
  return 'An unexpected error occurred.'
}
