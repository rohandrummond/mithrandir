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

export function getColumns(actions: ColumnActions): ColumnDef<ApiKey>[] {
  return [
    {
      accessorKey: 'name',
      header: 'Name',
      filterFn: 'includesString',
    },
    {
      accessorKey: 'tier',
      header: 'Tier',
      filterFn: 'includesString',
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
    },
    {
      accessorKey: 'createdAt',
      header: 'Created',
      cell: ({ row }) => formatDate(row.getValue('createdAt')),
    },
    {
      accessorKey: 'expiresAt',
      header: 'Expires',
      cell: ({ row }) => formatDate(row.getValue('expiresAt')),
    },
    {
      accessorKey: 'lastUsedAt',
      header: 'Last Used',
      cell: ({ row }) => formatDate(row.getValue('lastUsedAt')),
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
    },
  ]
}
