'use client'

import { MoreHorizontal, Trash2, ShieldCheck } from 'lucide-react'
import { Button } from '@/components/ui/button'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import { ApiKey } from '@/types/api-key'

interface KeyActionsMenuProps {
  apiKey: ApiKey
  onDelete: (apiKey: ApiKey) => void
  onManageWhitelist: (apiKey: ApiKey) => void
}

export function KeyActionsMenu({
  apiKey,
  onDelete,
  onManageWhitelist,
}: KeyActionsMenuProps) {
  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button variant="ghost" size="icon-sm" className="cursor-pointer">
          <MoreHorizontal className="h-4 w-4" />
          <span className="sr-only">Open menu</span>
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end" className="p-2 pr-3">
        <DropdownMenuItem
          onClick={() => onManageWhitelist(apiKey)}
          className="cursor-pointer"
        >
          <ShieldCheck />
          Manage Whitelist
        </DropdownMenuItem>
        <DropdownMenuItem
          variant="destructive"
          onClick={() => onDelete(apiKey)}
          className="cursor-pointer"
        >
          <Trash2 />
          Delete
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  )
}
