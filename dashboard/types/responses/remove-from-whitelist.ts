export interface RemoveFromWhitelistResponse {
  success: boolean
  message?: string | null
  whitelistedIps?: string[] | null
}
