import { Tier } from '../enums'

export interface ValidateKeyResponse {
  isValid: boolean
  reason?: string | null
  tier?: Tier | null
}

export interface AuthenticateKeyResponse {
  isValid: boolean
  reason?: string | null
  id?: number | null
  tier?: Tier | null
  ipWhitelist?: string[] | null
}
