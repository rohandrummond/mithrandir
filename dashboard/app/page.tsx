'use client'

import { useApiKeys } from '@/hooks/use-api-keys'

export default function Home() {
  const { data, error, isLoading } = useApiKeys()

  console.log('Data: ', data)

  if (isLoading) return <div>Loading...</div>
  if (error) return <div>Error loading keys</div>

  return <div>{data?.keys.length} keys found</div>
}
