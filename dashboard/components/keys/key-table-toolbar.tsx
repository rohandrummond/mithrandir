'use client'

import { Table } from '@tanstack/react-table'
import { Plus, Search } from 'lucide-react'
import { Input } from '@/components/ui/input'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { Button } from '@/components/ui/button'
import { Tier, Status } from '@/types/enums'
import { ApiKey } from '@/types/api-key'

interface KeyTableToolbarProps {
  table: Table<ApiKey>
  onGenerateKey: () => void
}

export function KeyTableToolbar({ table, onGenerateKey }: KeyTableToolbarProps) {
  const nameFilter = (table.getColumn('name')?.getFilterValue() as string) ?? ''
  const tierFilter =
    (table.getColumn('tier')?.getFilterValue() as string) ?? 'all'
  const statusFilter =
    (table.getColumn('status')?.getFilterValue() as string) ?? 'all'

  return (
    <>
      <div className="flex flex-row items-center justify-between">
        <h1 className="text-3xl font-bold">API Keys</h1>
        <Button onClick={onGenerateKey}>
          <Plus className="w-4 h-4" />
          Generate new key
        </Button>
      </div>
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div className="relative flex-1 sm:max-w-lg">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder="Search by name..."
            value={nameFilter}
            onChange={(e) =>
              table.getColumn('name')?.setFilterValue(e.target.value)
            }
            className="pl-9"
          />
        </div>
        <div className="flex gap-2">
          <Select
            value={tierFilter}
            onValueChange={(value) =>
              table
                .getColumn('tier')
                ?.setFilterValue(value === 'all' ? '' : value)
            }
          >
            <SelectTrigger>
              <SelectValue placeholder="Tier" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Tiers</SelectItem>
              <SelectItem value={Tier.Free}>Free</SelectItem>
              <SelectItem value={Tier.Pro}>Pro</SelectItem>
            </SelectContent>
          </Select>
          <Select
            value={statusFilter}
            onValueChange={(value) =>
              table
                .getColumn('status')
                ?.setFilterValue(value === 'all' ? '' : value)
            }
          >
            <SelectTrigger>
              <SelectValue placeholder="Status" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Statuses</SelectItem>
              <SelectItem value={Status.Active}>Active</SelectItem>
              <SelectItem value={Status.Revoked}>Revoked</SelectItem>
            </SelectContent>
          </Select>
        </div>
      </div>
    </>
  )
}
