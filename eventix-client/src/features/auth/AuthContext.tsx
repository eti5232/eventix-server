import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQueryClient } from '@tanstack/react-query'
import { setUnauthorizedHandler, tokenStore } from '../../api/client'
import { authApi } from '../../api/endpoints'
import type { AuthResponse, LoginRequest, RegisterRequest, User } from '../../types'

const USER_KEY = 'eventix.user'
const EXPIRY_KEY = 'eventix.expires'

interface AuthState {
  user: User | null
  isAdmin: boolean
  login: (body: LoginRequest) => Promise<User>
  register: (body: RegisterRequest) => Promise<User>
  logout: () => void
}

const AuthContext = createContext<AuthState | null>(null)

function readStoredUser(): User | null {
  try {
    const expires = localStorage.getItem(EXPIRY_KEY)
    if (!tokenStore.get() || !expires || new Date(expires).getTime() < Date.now()) return null
    return JSON.parse(localStorage.getItem(USER_KEY) ?? 'null') as User | null
  } catch {
    return null
  }
}

function persist(response: AuthResponse) {
  tokenStore.set(response.token)
  localStorage.setItem(USER_KEY, JSON.stringify(response.user))
  // The server sends UTC without an offset; make it explicit so Date parses it as UTC.
  const utc = response.expiresAtUtc.endsWith('Z') ? response.expiresAtUtc : `${response.expiresAtUtc}Z`
  localStorage.setItem(EXPIRY_KEY, utc)
}

function forget() {
  tokenStore.clear()
  localStorage.removeItem(USER_KEY)
  localStorage.removeItem(EXPIRY_KEY)
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(() => readStoredUser())
  const navigate = useNavigate()
  const queryClient = useQueryClient()

  const logout = useCallback(() => {
    forget()
    setUser(null)
    queryClient.removeQueries({ queryKey: ['reservations'] })
  }, [queryClient])

  useEffect(() => {
    if (!user) forget()
  }, [user])

  // A 401 from the server means the token is no longer valid: log out and come back here after signing in.
  useEffect(() => {
    setUnauthorizedHandler(() => {
      logout()
      const from = window.location.pathname + window.location.search
      navigate('/login', { replace: true, state: { from, expired: true } })
    })
    return () => setUnauthorizedHandler(null)
  }, [logout, navigate])

  const accept = useCallback((response: AuthResponse) => {
    persist(response)
    setUser(response.user)
    return response.user
  }, [])

  const value = useMemo<AuthState>(
    () => ({
      user,
      isAdmin: user?.role === 'Admin',
      login: async (body) => accept(await authApi.login(body)),
      register: async (body) => accept(await authApi.register(body)),
      logout,
    }),
    [user, accept, logout],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

// eslint-disable-next-line react-refresh/only-export-components
export function useAuth(): AuthState {
  const context = useContext(AuthContext)
  if (!context) throw new Error('useAuth must be used inside AuthProvider')
  return context
}
