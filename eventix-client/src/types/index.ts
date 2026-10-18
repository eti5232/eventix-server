import type { components } from './api'

// The generator marks every property optional because the server's Swagger does not declare required fields.
// `Strict` makes them required again, except the ones listed in `Optional`, which really can be null.
type Strict<T, Optional extends keyof T = never> = {
  [K in Exclude<keyof T, Optional>]-?: NonNullable<T[K]>
} & {
  [K in Optional]?: T[K]
}

type Schemas = components['schemas']

export type ReservationStatus = Schemas['ReservationStatus']

export type Category = Strict<Schemas['CategoryResponseDto'], 'description'>
export type CategoryRequest = Strict<Schemas['CategoryRequestDto'], 'description'>

export type Equipment = Strict<Schemas['EquipmentResponseDto'], 'description'>
export type EquipmentRequest = Strict<Schemas['EquipmentRequestDto'], 'description'>
export type Availability = Strict<Schemas['AvailabilityResponseDto']>

export type ReservationItem = Strict<Schemas['ReservationItemResponseDto']>
export type Reservation = Omit<Strict<Schemas['ReservationResponseDto'], 'returnedOn'>, 'items' | 'status'> & {
  status: ReservationStatus
  items: ReservationItem[]
}
export type CreateReservationRequest = Omit<Strict<Schemas['CreateReservationRequestDto']>, 'items'> & {
  items: { equipmentId: number; quantity: number }[]
}

export type User = Strict<Schemas['UserResponseDto']>
export type AuthResponse = Omit<Strict<Schemas['AuthResponseDto']>, 'user'> & { user: User }
export type LoginRequest = Strict<Schemas['LoginRequestDto']>
export type RegisterRequest = Strict<Schemas['RegisterRequestDto']>

export interface Paged<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
}
