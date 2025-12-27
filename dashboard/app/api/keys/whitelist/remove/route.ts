import { api } from '@/lib/api'
import { RemoveFromWhitelistRequest } from '@/types/requests/remove-from-whitelist'
import { RemoveFromWhitelistResponse } from '@/types/responses/remove-from-whitelist'

export async function DELETE(request: Request) {
  try {
    const body: RemoveFromWhitelistRequest = await request.json()
    const data = await api.delete<RemoveFromWhitelistResponse>(
      '/api/admin/keys/whitelist/remove',
      body
    )
    return Response.json(data)
  } catch (error) {
    console.error('Failed to remove IP from whitelist:', error)
    return Response.json(
      { success: false, message: 'Failed to remove IP from whitelist' },
      { status: 500 }
    )
  }
}
