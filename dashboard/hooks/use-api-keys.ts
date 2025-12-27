import useSWR from 'swr'
import { fetcher } from '@/lib/fetcher'
import { GetAllKeysResponse } from '@/types/responses/get-all-keys'

export function useApiKeys() {
  return useSWR('/api/keys', fetcher<GetAllKeysResponse>)
}
