'use client'

import { useState } from 'react'
import { X } from 'lucide-react'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { ApiKey } from '@/types/api-key'
import { AddToWhitelistResponse } from '@/types/responses/add-to-whitelist'
import { RemoveFromWhitelistResponse } from '@/types/responses/remove-from-whitelist'

interface WhitelistDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  apiKey: ApiKey | null
  onSuccess: () => void
}

function isValidIpAddress(ip: string): boolean {
  // IPv4: e.g., 192.168.1.1
  const ipv4Regex = /^(\d{1,3}\.){3}\d{1,3}$/
  if (ipv4Regex.test(ip)) {
    const parts = ip.split('.')
    return parts.every((part) => {
      const num = parseInt(part, 10)
      return num >= 0 && num <= 255
    })
  }

  // IPv6: e.g., ::1, fe80::1, 2001:db8::1
  const ipv6Regex = /^([0-9a-fA-F]{0,4}:){1,7}[0-9a-fA-F]{0,4}$|^::1$|^::$/
  return ipv6Regex.test(ip)
}

export function WhitelistDialog({
  open,
  onOpenChange,
  apiKey,
  onSuccess,
}: WhitelistDialogProps) {
  const [newIp, setNewIp] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const handleOpenChange = (open: boolean) => {
    if (!open) {
      setError(null)
      setNewIp('')
    }
    onOpenChange(open)
  }

  const handleAddIp = async () => {
    const ip = newIp.trim()
    if (!ip || !apiKey) return

    if (!isValidIpAddress(ip)) {
      setError('Invalid IP address format')
      return
    }

    setIsLoading(true)
    setError(null)

    try {
      const response = await fetch('/api/keys/whitelist/add', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ id: apiKey.id, ipAddress: ip }),
      })

      const data: AddToWhitelistResponse = await response.json()

      if (!response.ok || !data.success) {
        throw new Error(data.message || 'Failed to add IP to whitelist')
      }

      setNewIp('')
      onSuccess()
    } catch (err) {
      console.error('Failed to add IP to whitelist:', err)
      setError(err instanceof Error ? err.message : 'Failed to add IP to whitelist')
    } finally {
      setIsLoading(false)
    }
  }

  const handleRemoveIp = async (ip: string) => {
    if (!apiKey) return

    setIsLoading(true)
    setError(null)

    try {
      const response = await fetch('/api/keys/whitelist/remove', {
        method: 'DELETE',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ id: apiKey.id, ipAddress: ip }),
      })

      const data: RemoveFromWhitelistResponse = await response.json()

      if (!response.ok || !data.success) {
        throw new Error(data.message || 'Failed to remove IP from whitelist')
      }

      onSuccess()
    } catch (err) {
      console.error('Failed to remove IP from whitelist:', err)
      setError(err instanceof Error ? err.message : 'Failed to remove IP from whitelist')
    } finally {
      setIsLoading(false)
    }
  }

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      e.preventDefault()
      handleAddIp()
    }
  }

  const whitelist = apiKey?.ipWhitelist ?? []

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent className="py-7 px-8">
        <DialogHeader>
          <DialogTitle>Manage IP Whitelist</DialogTitle>
          <DialogDescription>
            Manage whitelisted IP addresses for &quot;{apiKey?.name}&quot;. Only
            these IPs will be allowed to use this API key.
          </DialogDescription>
        </DialogHeader>
        <div className="space-y-4">
          {error && (
            <p className="text-sm text-destructive">{error}</p>
          )}
          <div className="flex gap-2">
            <Input
              placeholder="Enter IP address..."
              value={newIp}
              onChange={(e) => setNewIp(e.target.value)}
              onKeyDown={handleKeyDown}
              disabled={isLoading}
            />
            <Button onClick={handleAddIp} disabled={!newIp.trim() || isLoading}>
              {isLoading ? 'Adding...' : 'Add IP'}
            </Button>
          </div>
          {whitelist.length === 0 ? (
            <p className="text-sm text-muted-foreground py-4 text-center">
              No IP addresses whitelisted. All IPs are currently allowed.
            </p>
          ) : (
            <ul className="space-y-2">
              {whitelist.map((ip) => (
                <li
                  key={ip}
                  className="flex items-center justify-between rounded-md border px-3 py-2"
                >
                  <span className="text-sm font-mono">{ip}</span>
                  <Button
                    variant="ghost"
                    size="icon-sm"
                    onClick={() => handleRemoveIp(ip)}
                    disabled={isLoading}
                  >
                    <X className="h-4 w-4" />
                    <span className="sr-only">Remove {ip}</span>
                  </Button>
                </li>
              ))}
            </ul>
          )}
        </div>
      </DialogContent>
    </Dialog>
  )
}
