export interface AddToWhitelistResponse {
  success: boolean
  message?: string | null
  whitelistedIps?: string[] | null
}
