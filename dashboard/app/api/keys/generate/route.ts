import { api } from '@/lib/api'
import { GenerateKeyRequest } from '@/types/requests/generate-key'
import { GenerateKeyResponse } from '@/types/responses/generate-key'

export async function POST(request: Request) {
  try {
    const body: GenerateKeyRequest = await request.json()
    const data = await api.post<GenerateKeyResponse>(
      '/api/admin/keys/generate',
      body
    )
    return Response.json(data)
  } catch (error) {
    console.error('Failed to generate key:', error)
    return Response.json(
      { success: false, message: 'Failed to generate key' },
      { status: 500 }
    )
  }
}
