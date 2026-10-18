import { api } from './client'
import type {
  Availability,
  AuthResponse,
  Category,
  CategoryRequest,
  CreateReservationRequest,
  Equipment,
  EquipmentRequest,
  LoginRequest,
  Paged,
  RegisterRequest,
  Reservation,
  ReservationStatus,
} from '../types'

export interface EquipmentFilters {
  categoryId?: number
  search?: string
  minPrice?: number
  maxPrice?: number
  sortBy?: 'Name' | 'Price'
  sortDir?: 'Asc' | 'Desc'
  page?: number
  pageSize?: number
}

export const authApi = {
  login: (body: LoginRequest) => api.post<AuthResponse>('/api/auth/login', body).then((r) => r.data),
  register: (body: RegisterRequest) => api.post<AuthResponse>('/api/auth/register', body).then((r) => r.data),
}

export const categoriesApi = {
  list: () => api.get<Category[]>('/api/categories').then((r) => r.data),
  create: (body: CategoryRequest) => api.post<Category>('/api/categories', body).then((r) => r.data),
  update: (id: number, body: CategoryRequest) => api.put<Category>(`/api/categories/${id}`, body).then((r) => r.data),
}

export const equipmentApi = {
  list: (filters: EquipmentFilters) =>
    api.get<Paged<Equipment>>('/api/equipment', { params: filters }).then((r) => r.data),
  get: (id: number) => api.get<Equipment>(`/api/equipment/${id}`).then((r) => r.data),
  availability: (id: number, from: string, to: string) =>
    api.get<Availability>(`/api/equipment/${id}/availability`, { params: { from, to } }).then((r) => r.data),
  create: (body: EquipmentRequest) => api.post<Equipment>('/api/equipment', body).then((r) => r.data),
  update: (id: number, body: EquipmentRequest) => api.put<Equipment>(`/api/equipment/${id}`, body).then((r) => r.data),
  remove: (id: number) => api.delete(`/api/equipment/${id}`).then(() => undefined),
}

export const reservationsApi = {
  list: (params: { status?: ReservationStatus; page?: number; pageSize?: number }) =>
    api.get<Paged<Reservation>>('/api/reservations', { params }).then((r) => r.data),
  get: (id: number) => api.get<Reservation>(`/api/reservations/${id}`).then((r) => r.data),
  create: (body: CreateReservationRequest) => api.post<Reservation>('/api/reservations', body).then((r) => r.data),
  changeStatus: (id: number, status: ReservationStatus) =>
    api.patch<Reservation>(`/api/reservations/${id}`, { status }).then((r) => r.data),
}
