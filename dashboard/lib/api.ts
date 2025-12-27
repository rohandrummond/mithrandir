import { getEnvironmentVariable } from './utils'

const API_URL = getEnvironmentVariable('NEXT_PUBLIC_DOTNET_API_URL')
const ADMIN_API_KEY = getEnvironmentVariable('ADMIN_API_KEY')

if (!ADMIN_API_KEY) {
  throw new Error('ADMIN_API_KEY environment variable is not set')
}

class ApiError extends Error {
  constructor(
    public status: number,
    public statusText: string,
    public body?: unknown
  ) {
    super(`HTTP ${status}: ${statusText}`)
    this.name = 'ApiError'
  }
}

interface RequestOptions {
  method: 'GET' | 'POST' | 'PUT' | 'DELETE'
  body?: unknown
}

async function apiFetch<T>(
  endpoint: string,
  options?: RequestOptions
): Promise<T> {
  const { method, body } = options ?? {}
  const res = await fetch(`${API_URL}${endpoint}`, {
    method,
    headers: {
      'Content-Type': 'application/json',
      'X-Admin-Key': ADMIN_API_KEY,
    },
    body: body ? JSON.stringify(body) : undefined,
  })

  if (!res.ok) {
    let body: unknown
    try {
      body = await res.json()
    } catch {}
    throw new ApiError(res.status, res.statusText, body)
  }

  return res.json() as T
}

export const api = {
  get: <T>(endpoint: string) => apiFetch<T>(endpoint, { method: 'GET' }),
  post: <T>(endpoint: string, body: unknown) =>
    apiFetch<T>(endpoint, { method: 'POST', body }),
  put: <T>(endpoint: string, body: unknown) =>
    apiFetch<T>(endpoint, { method: 'PUT', body }),
  delete: <T>(endpoint: string) => apiFetch<T>(endpoint, { method: 'DELETE' }),
}
