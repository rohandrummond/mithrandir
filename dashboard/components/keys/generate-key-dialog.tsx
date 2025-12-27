'use client'

import { useState } from 'react'
import { AlertTriangle, Check, Copy, Info } from 'lucide-react'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { Tier } from '@/types/enums'
import { GenerateKeyRequest } from '@/types/requests/generate-key'
import { GenerateKeyResponse } from '@/types/responses/generate-key'

interface GenerateKeyDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  onSuccess: () => void
}

interface FormState {
  name: string
  tier: Tier
  expiresAt: string
}

interface FormErrors {
  name?: string
  expiresAt?: string
}

export function GenerateKeyDialog({
  open,
  onOpenChange,
  onSuccess,
}: GenerateKeyDialogProps) {
  const [view, setView] = useState<'form' | 'success'>('form')
  const [formState, setFormState] = useState<FormState>({
    name: '',
    tier: Tier.Free,
    expiresAt: '',
  })
  const [generatedKey, setGeneratedKey] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [errors, setErrors] = useState<FormErrors>({})
  const [hasCopied, setHasCopied] = useState(false)
  const [copySuccess, setCopySuccess] = useState(false)

  const resetForm = () => {
    setView('form')
    setFormState({
      name: '',
      tier: Tier.Free,
      expiresAt: '',
    })
    setGeneratedKey(null)
    setIsSubmitting(false)
    setErrors({})
    setHasCopied(false)
    setCopySuccess(false)
  }

  const handleOpenChange = (newOpen: boolean) => {
    if (!newOpen && view === 'success' && !hasCopied) {
      return
    }
    if (!newOpen) {
      resetForm()
    }
    onOpenChange(newOpen)
  }

  const validateForm = (): boolean => {
    const newErrors: FormErrors = {}

    if (!formState.name.trim()) {
      newErrors.name = 'Name is required'
    } else if (formState.name.length > 100) {
      newErrors.name = 'Name must be 100 characters or less'
    }

    if (formState.expiresAt) {
      const expiryDate = new Date(formState.expiresAt)
      if (expiryDate <= new Date()) {
        newErrors.expiresAt = 'Expiry date must be in the future'
      }
    }

    setErrors(newErrors)
    return Object.keys(newErrors).length === 0
  }

  const handleSubmit = async () => {
    if (!validateForm()) return

    setIsSubmitting(true)
    try {
      const request: GenerateKeyRequest = {
        name: formState.name.trim(),
        tier: formState.tier,
        expiresAt: formState.expiresAt || null,
      }

      const response = await fetch('/api/keys/generate', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(request),
      })

      if (!response.ok) {
        throw new Error('Failed to generate key')
      }

      const data: GenerateKeyResponse = await response.json()
      setGeneratedKey(data.key)
      setView('success')
    } catch (error) {
      console.error('Failed to generate key:', error)
    } finally {
      setIsSubmitting(false)
    }
  }

  const handleCopyKey = async () => {
    if (!generatedKey) return

    try {
      await navigator.clipboard.writeText(generatedKey)
      setCopySuccess(true)
      setTimeout(() => setCopySuccess(false), 2000)
    } catch (error) {
      console.error('Failed to copy key:', error)
    }
  }

  const handleDone = () => {
    onSuccess()
    onOpenChange(false)
    resetForm()
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent
        className="py-7 px-8"
        showCloseButton={view === 'form'}
        onInteractOutside={(e) => {
          if (view === 'success' && !hasCopied) {
            e.preventDefault()
          }
        }}
        onEscapeKeyDown={(e) => {
          if (view === 'success' && !hasCopied) {
            e.preventDefault()
          }
        }}
      >
        {/* Form view */}
        {view === 'form' ? (
          <>
            <DialogHeader>
              <DialogTitle>Generate API Key</DialogTitle>
              <DialogDescription>
                Create a new API key for your application.
              </DialogDescription>
            </DialogHeader>

            <div className="space-y-4 py-2">
              <div className="space-y-2">
                <Label htmlFor="name">Name</Label>
                <Input
                  id="name"
                  placeholder="My API Key"
                  value={formState.name}
                  onChange={(e) =>
                    setFormState((prev) => ({ ...prev, name: e.target.value }))
                  }
                  aria-invalid={!!errors.name}
                  maxLength={100}
                />
                {errors.name && (
                  <p className="text-sm text-destructive">{errors.name}</p>
                )}
              </div>

              <div className="space-y-2">
                <Label htmlFor="tier">Tier</Label>
                <Select
                  value={formState.tier}
                  onValueChange={(value: Tier) =>
                    setFormState((prev) => ({ ...prev, tier: value }))
                  }
                >
                  <SelectTrigger id="tier" className="w-full">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value={Tier.Free}>Free</SelectItem>
                    <SelectItem value={Tier.Pro}>Pro</SelectItem>
                  </SelectContent>
                </Select>
              </div>

              <div className="space-y-2">
                <Label htmlFor="expiresAt">Expiry Date (Optional)</Label>
                <Input
                  id="expiresAt"
                  type="date"
                  value={formState.expiresAt}
                  onChange={(e) =>
                    setFormState((prev) => ({
                      ...prev,
                      expiresAt: e.target.value,
                    }))
                  }
                  aria-invalid={!!errors.expiresAt}
                  min={new Date().toISOString().split('T')[0]}
                />
                {errors.expiresAt && (
                  <p className="text-sm text-destructive">{errors.expiresAt}</p>
                )}
              </div>

              <div className="flex items-start gap-2 rounded-md border border-blue-500/50 bg-blue-500/10 px-3 py-2 text-blue-600 dark:text-blue-400">
                <Info className="mt-0.5 h-3 w-3 shrink-0" />
                <p className="text-xs">
                  You must configure IP whitelisting for this key via the
                  dashboard table for requests to be allowed.
                </p>
              </div>
            </div>

            <DialogFooter>
              <Button variant="outline" onClick={() => onOpenChange(false)}>
                Cancel
              </Button>
              <Button onClick={handleSubmit} disabled={isSubmitting}>
                {isSubmitting ? 'Generating...' : 'Generate'}
              </Button>
            </DialogFooter>
          </>
        ) : (
          /* Success view */
          <>
            <DialogHeader>
              <DialogTitle>API Key Generated</DialogTitle>
              <DialogDescription>
                Your new API key has been created successfully.
              </DialogDescription>
            </DialogHeader>

            <div className="space-y-4 py-2">
              <div className="flex items-center gap-2 rounded-md border border-amber-500/50 bg-amber-500/10 px-3 py-2 text-amber-600 dark:text-amber-400">
                <AlertTriangle className="h-4 w-4 shrink-0" />
                <p className="text-sm">
                  This key will only be shown once. Make sure to copy and save
                  it securely.
                </p>
              </div>

              <div className="space-y-2">
                <Label>Your API Key</Label>
                <div className="flex items-center gap-2">
                  <code className="flex-1 rounded-md border bg-muted px-3 py-2 font-mono text-sm break-all">
                    {generatedKey}
                  </code>
                  <Button
                    variant="outline"
                    size="icon"
                    onClick={handleCopyKey}
                    className="shrink-0"
                  >
                    {copySuccess ? (
                      <Check className="h-4 w-4 text-green-500" />
                    ) : (
                      <Copy className="h-4 w-4" />
                    )}
                    <span className="sr-only">Copy key</span>
                  </Button>
                </div>
              </div>

              <label className="flex items-center gap-2 cursor-pointer">
                <input
                  type="checkbox"
                  checked={hasCopied}
                  onChange={(e) => setHasCopied(e.target.checked)}
                  className="h-4 w-4 rounded border-input"
                />
                <span className="text-sm">
                  I have securely saved my API key
                </span>
              </label>
            </div>

            <DialogFooter>
              <Button onClick={handleDone} disabled={!hasCopied}>
                Done
              </Button>
            </DialogFooter>
          </>
        )}
      </DialogContent>
    </Dialog>
  )
}
