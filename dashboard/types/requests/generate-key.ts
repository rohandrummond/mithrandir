import { Tier } from '../enums'

export interface GenerateKeyRequest {
  name: string
  tier: Tier
  expiresAt?: string | null
}
