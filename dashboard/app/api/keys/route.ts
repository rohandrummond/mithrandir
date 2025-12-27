import { api } from '@/lib/api'
import { GetAllKeysResponse } from '@/types/responses/get-all-keys'

export async function GET() {
  try {
    const data = await api.get<GetAllKeysResponse>('/api/admin/keys')
    return Response.json(data)
  } catch (error) {
    console.error('Failed to fetch keys:', error)
    return Response.json(
      { success: false, message: 'Failed to fetch keys', keys: [] },
      { status: 500 }
    )
  }
}
