import useSWR from 'swr'
import { fetcher } from '@/lib/fetcher'
import { GetAllKeysResponse } from '@/types/responses/get-all-keys'

export function useApiKeys() {
  const { data, error, isLoading, mutate } = useSWR(
    '/api/keys',
    fetcher<GetAllKeysResponse>
  )

  return { data, error, isLoading, mutate }
}
