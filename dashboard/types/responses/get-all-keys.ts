import { ApiKey } from '../api-key'

export interface GetAllKeysResponse {
  success: boolean
  message: string | null
  keys: ApiKey[]
}
