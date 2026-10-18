import axios from 'axios'
import { toApiError } from './errors'

const TOKEN_KEY = 'eventix.token'

export const tokenStore = {
  get: () => localStorage.getItem(TOKEN_KEY),
  set: (token: string) => localStorage.setItem(TOKEN_KEY, token),
  clear: () => localStorage.removeItem(TOKEN_KEY),
}

let onUnauthorized: (() => void) | null = null

/** AuthContext registers what to do when the server rejects the token (log out and go to /login). */
export function setUnauthorizedHandler(handler: (() => void) | null) {
  onUnauthorized = handler
}

export const api = axios.create({
  // Empty in development (Vite proxies /api); the full server address in production.
  baseURL: import.meta.env.VITE_API_URL || '',
  headers: { Accept: 'application/json' },
})

api.interceptors.request.use((config) => {
  const token = tokenStore.get()
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

api.interceptors.response.use(
  (response) => response,
  (error: unknown) => {
    const apiError = toApiError(error)
    // 401 on a request that carried a token means it expired or is invalid; a failed login has no token.
    if (apiError.status === 401 && tokenStore.get()) onUnauthorized?.()
    return Promise.reject(apiError)
  },
)
