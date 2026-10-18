import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter, Route, Routes } from 'react-router-dom'
import { Layout } from './components/Layout'
import { ToastProvider } from './components/ui/Toast'
import { AuthProvider } from './features/auth/AuthContext'
import { KitProvider } from './features/kit/KitContext'
import { RequireAuth, RequireRole } from './routes/guards'
import { AdminCategoriesPage } from './routes/admin/AdminCategoriesPage'
import { AdminEquipmentPage } from './routes/admin/AdminEquipmentPage'
import { AdminReservationsPage } from './routes/admin/AdminReservationsPage'
import { LoginPage, RegisterPage } from './routes/AuthPages'
import { CatalogPage } from './routes/CatalogPage'
import { CheckoutPage } from './routes/CheckoutPage'
import { EquipmentPage } from './routes/EquipmentPage'
import { ErrorBoundary } from './routes/ErrorBoundary'
import { HomePage } from './routes/HomePage'
import { MyReservationsPage } from './routes/MyReservationsPage'
import { NotFoundPage } from './routes/NotFoundPage'
import { ReservationPage } from './routes/ReservationPage'

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      // A 4xx will not fix itself; only retry network hiccups and server errors once.
      retry: (count, error) => count < 1 && ((error as { status?: number }).status ?? 500) >= 500,
      refetchOnWindowFocus: true,
    },
  },
})

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <ToastProvider>
          <AuthProvider>
            <KitProvider>
              <ErrorBoundary>
                <Routes>
                  <Route element={<Layout />}>
                    <Route index element={<HomePage />} />
                    <Route path="equipment" element={<CatalogPage />} />
                    <Route path="equipment/:id" element={<EquipmentPage />} />
                    <Route path="login" element={<LoginPage />} />
                    <Route path="register" element={<RegisterPage />} />

                    <Route element={<RequireAuth />}>
                      <Route path="reservations/:id" element={<ReservationPage />} />
                    </Route>

                    {/* Only customers order equipment, so checkout is closed to admins up front (the server enforces it too). */}
                    <Route element={<RequireRole role="Producer" />}>
                      <Route path="checkout" element={<CheckoutPage />} />
                      <Route path="reservations" element={<MyReservationsPage />} />
                    </Route>

                    <Route path="admin" element={<RequireRole role="Admin" />}>
                      <Route path="reservations" element={<AdminReservationsPage />} />
                      <Route path="equipment" element={<AdminEquipmentPage />} />
                      <Route path="categories" element={<AdminCategoriesPage />} />
                    </Route>

                    <Route path="*" element={<NotFoundPage />} />
                  </Route>
                </Routes>
              </ErrorBoundary>
            </KitProvider>
          </AuthProvider>
        </ToastProvider>
      </BrowserRouter>
    </QueryClientProvider>
  )
}
