'use client'

import { ColumnDef } from '@tanstack/react-table'
import { Badge } from '@/components/ui/badge'
import { ApiKey } from '@/types/api-key'
import { Status } from '@/types/enums'
import { KeyActionsMenu } from './key-actions-menu'

function formatDate(dateString: string | null): string {
  if (!dateString) return 'Never'
  const date = new Date(dateString)
  return date.toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  })
}

interface ColumnActions {
  onDelete: (apiKey: ApiKey) => void
  onManageWhitelist: (apiKey: ApiKey) => void
}

// Responsive class for columns to be hidden on mobile
const hiddenOnMobile = 'hidden md:table-cell'
const hiddenOnTablet = 'hidden lg:table-cell'

export function getColumns(actions: ColumnActions): ColumnDef<ApiKey>[] {
  return [
    {
      accessorKey: 'name',
      header: 'Name',
      filterFn: 'includesString',
      meta: { className: '' }, // Always visible
    },
    {
      accessorKey: 'tier',
      header: 'Tier',
      filterFn: 'includesString',
      meta: { className: '' }, // Always visible
    },
    {
      accessorKey: 'status',
      header: 'Status',
      cell: ({ row }) => {
        const status = row.getValue('status') as Status
        return (
          <Badge
            variant={status === Status.Revoked ? 'destructive' : 'default'}
          >
            {status}
          </Badge>
        )
      },
      filterFn: 'equals',
      meta: { className: '' }, // Always visible
    },
    {
      accessorKey: 'createdAt',
      header: 'Created',
      cell: ({ row }) => formatDate(row.getValue('createdAt')),
      meta: { className: hiddenOnMobile }, // Hidden on mobile
    },
    {
      accessorKey: 'expiresAt',
      header: 'Expires',
      cell: ({ row }) => formatDate(row.getValue('expiresAt')),
      meta: { className: hiddenOnMobile }, // Hidden on mobile
    },
    {
      accessorKey: 'lastUsedAt',
      header: 'Last Used',
      cell: ({ row }) => formatDate(row.getValue('lastUsedAt')),
      meta: { className: hiddenOnTablet }, // Hidden on mobile and tablet
    },
    {
      id: 'actions',
      header: 'Actions',
      cell: ({ row }) => (
        <KeyActionsMenu
          apiKey={row.original}
          onDelete={actions.onDelete}
          onManageWhitelist={actions.onManageWhitelist}
        />
      ),
      meta: { className: '' }, // Always visible
    },
  ]
}
