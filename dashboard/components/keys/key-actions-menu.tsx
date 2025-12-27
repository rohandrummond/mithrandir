'use client';

import { MoreHorizontal, Trash2, Shield } from 'lucide-react';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { ApiKey } from '@/types/api-key';

interface KeyActionsMenuProps {
  apiKey: ApiKey;
  onDelete: (apiKey: ApiKey) => void;
  onManageWhitelist: (apiKey: ApiKey) => void;
}

export function KeyActionsMenu({
  apiKey,
  onDelete,
  onManageWhitelist,
}: KeyActionsMenuProps) {
  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button variant="ghost" size="icon-sm">
          <MoreHorizontal className="h-4 w-4" />
          <span className="sr-only">Open menu</span>
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end">
        <DropdownMenuItem onClick={() => onManageWhitelist(apiKey)}>
          <Shield />
          Manage Whitelist
        </DropdownMenuItem>
        <DropdownMenuItem variant="destructive" onClick={() => onDelete(apiKey)}>
          <Trash2 />
          Delete
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
