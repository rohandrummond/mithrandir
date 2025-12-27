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

interface WhitelistDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  apiKey: ApiKey | null
  onAddIp: (ip: string) => void
  onRemoveIp: (ip: string) => void
}

export function WhitelistDialog({
  open,
  onOpenChange,
  apiKey,
  onAddIp,
  onRemoveIp,
}: WhitelistDialogProps) {
  const [newIp, setNewIp] = useState('')

  const handleAddIp = () => {
    if (newIp.trim()) {
      console.log('Add IP:', newIp.trim(), 'to key:', apiKey?.id)
      onAddIp(newIp.trim())
      setNewIp('')
    }
  }

  const handleRemoveIp = (ip: string) => {
    console.log('Remove IP:', ip, 'from key:', apiKey?.id)
    onRemoveIp(ip)
  }

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      e.preventDefault()
      handleAddIp()
    }
  }

  const whitelist = apiKey?.ipWhitelist ?? []

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="py-7 px-8">
        <DialogHeader>
          <DialogTitle>Manage IP Whitelist</DialogTitle>
          <DialogDescription>
            Manage whitelisted IP addresses for &quot;{apiKey?.name}&quot;. Only
            these IPs will be allowed to use this API key.
          </DialogDescription>
        </DialogHeader>
        <div className="space-y-4">
          <div className="flex gap-2">
            <Input
              placeholder="Enter IP address..."
              value={newIp}
              onChange={(e) => setNewIp(e.target.value)}
              onKeyDown={handleKeyDown}
            />
            <Button onClick={handleAddIp} disabled={!newIp.trim()}>
              Add IP
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
