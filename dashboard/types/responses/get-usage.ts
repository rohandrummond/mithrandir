import { Status, Tier } from '../enums'

export interface EndpointUsage {
  endpoint: string
  count: number
}

export interface StatusCodeSummary {
  statusCode: number
  count: number
}

export interface GetUsageResponse {
  tier: Tier
  status: Status
  createdAt: string
  expiresAt: string | null
  lastUsedAt: string | null
  totalRequests: number
  successfulRequests: number
  failedRequests: number
  endpointUsage: EndpointUsage[]
  statusCodeSummaries: StatusCodeSummary[]
}
