'use client'

import { useState } from 'react'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { Button } from '@/components/ui/button'
import { ApiKey } from '@/types/api-key'
import { DeleteKeyResponse } from '@/types/responses/delete-key'

interface DeleteKeyDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  apiKey: ApiKey | null
  onSuccess: () => void
}

export function DeleteKeyDialog({
  open,
  onOpenChange,
  apiKey,
  onSuccess,
}: DeleteKeyDialogProps) {
  const [isDeleting, setIsDeleting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const handleOpenChange = (open: boolean) => {
    if (!open) {
      setError(null)
    }
    onOpenChange(open)
  }

  const handleConfirm = async () => {
    if (!apiKey) return

    setIsDeleting(true)
    setError(null)

    try {
      const response = await fetch('/api/keys/delete', {
        method: 'DELETE',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ id: apiKey.id }),
      })

      const data: DeleteKeyResponse = await response.json()

      if (!response.ok || !data.success) {
        throw new Error(data.message || 'Failed to delete key')
      }

      onSuccess()
      handleOpenChange(false)
    } catch (err) {
      console.error('Failed to delete key:', err)
      setError(err instanceof Error ? err.message : 'Failed to delete key')
    } finally {
      setIsDeleting(false)
    }
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent className="py-7 px-8">
        <DialogHeader>
          <DialogTitle>Delete API Key</DialogTitle>
          <DialogDescription>
            Are you sure you want to delete the API key &quot;{apiKey?.name}
            &quot;? This action cannot be undone.
          </DialogDescription>
        </DialogHeader>
        {error && (
          <p className="text-sm text-destructive">{error}</p>
        )}
        <DialogFooter>
          <Button
            variant="outline"
            onClick={() => handleOpenChange(false)}
            disabled={isDeleting}
          >
            Cancel
          </Button>
          <Button
            variant="destructive"
            onClick={handleConfirm}
            disabled={isDeleting}
          >
            {isDeleting ? 'Deleting...' : 'Delete'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
