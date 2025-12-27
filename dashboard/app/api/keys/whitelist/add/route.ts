import { api } from '@/lib/api'
import { AddToWhitelistRequest } from '@/types/requests/add-to-whitelist'
import { AddToWhitelistResponse } from '@/types/responses/add-to-whitelist'

export async function POST(request: Request) {
  try {
    const body: AddToWhitelistRequest = await request.json()
    const data = await api.post<AddToWhitelistResponse>(
      '/api/admin/keys/whitelist/add',
      body
    )
    return Response.json(data)
  } catch (error) {
    console.error('Failed to add IP to whitelist:', error)
    return Response.json(
      { success: false, message: 'Failed to add IP to whitelist' },
      { status: 500 }
    )
  }
}
