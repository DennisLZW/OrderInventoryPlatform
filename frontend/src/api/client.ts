import axios from 'axios'

/**
 * Base URL for API requests (no trailing slash).
 *
 * - **Development:** leave empty to use Vite's `/api` proxy (`vite.config.ts` → backend).
 * - **Direct to API:** set `VITE_API_BASE_URL` (e.g. `http://localhost:5241`).
 *
 * `VITE_API_URL` is supported as a legacy alias.
 */
function resolveBaseUrl(): string {
  const fromEnv =
    import.meta.env.VITE_API_BASE_URL ?? import.meta.env.VITE_API_URL ?? ''
  return typeof fromEnv === 'string' ? fromEnv.trim().replace(/\/$/, '') : ''
}

export const apiBaseUrl = resolveBaseUrl()

export const api = axios.create({
  baseURL: apiBaseUrl,
  headers: {
    'Content-Type': 'application/json',
  },
})
