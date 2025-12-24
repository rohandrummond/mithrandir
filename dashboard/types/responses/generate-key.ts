import { Tier } from '../enums'

export interface GenerateKeyResponse {
  key: string
  name: string
  tier: Tier
  createdAt: string
  expiresAt: string | null
}
