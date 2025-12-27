import { api } from '@/lib/api'
import { DeleteKeyRequest } from '@/types/requests/delete-key'
import { DeleteKeyResponse } from '@/types/responses/delete-key'

export async function DELETE(request: Request) {
  try {
    const body: DeleteKeyRequest = await request.json()
    const data = await api.delete<DeleteKeyResponse>(
      '/api/admin/keys/delete',
      body
    )
    return Response.json(data)
  } catch (error) {
    console.error('Failed to delete key:', error)
    return Response.json(
      { success: false, message: 'Failed to delete key' },
      { status: 500 }
    )
  }
}
