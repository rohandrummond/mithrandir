'use client'

import { useMemo, useState } from 'react'
import {
  ColumnFiltersState,
  flexRender,
  getCoreRowModel,
  getFilteredRowModel,
  getPaginationRowModel,
  useReactTable,
} from '@tanstack/react-table'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { useApiKeys } from '@/hooks/use-api-keys'
import { ApiKey } from '@/types/api-key'
import { getColumns } from './columns'
import { KeyTableToolbar } from './key-table-toolbar'
import { KeyTablePagination } from './key-table-pagination'
import { DeleteKeyDialog } from './delete-key-dialog'
import { WhitelistDialog } from './whitelist-dialog'
import { GenerateKeyDialog } from './generate-key-dialog'

export function KeyTable() {
  const { data, error, isLoading, mutate } = useApiKeys()

  const [columnFilters, setColumnFilters] = useState<ColumnFiltersState>([])
  const [selectedKey, setSelectedKey] = useState<ApiKey | null>(null)
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)
  const [whitelistDialogOpen, setWhitelistDialogOpen] = useState(false)
  const [generateDialogOpen, setGenerateDialogOpen] = useState(false)

  const handleDelete = (apiKey: ApiKey) => {
    setSelectedKey(apiKey)
    setDeleteDialogOpen(true)
  }

  const handleManageWhitelist = (apiKey: ApiKey) => {
    setSelectedKey(apiKey)
    setWhitelistDialogOpen(true)
  }

  const handleAddIp = (ip: string) => {
    // TO DO
    console.log('Add IP:', ip, 'to key:', selectedKey?.id)
  }

  const handleRemoveIp = (ip: string) => {
    // TO DO
    console.log('Remove IP:', ip, 'from key:', selectedKey?.id)
  }

  const columns = useMemo(
    () =>
      getColumns({
        onDelete: handleDelete,
        onManageWhitelist: handleManageWhitelist,
      }),
    []
  )

  const tableData = useMemo(() => data?.keys ?? [], [data?.keys])

  const table = useReactTable({
    data: tableData,
    columns,
    state: {
      columnFilters,
    },
    onColumnFiltersChange: setColumnFilters,
    getCoreRowModel: getCoreRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
    initialState: {
      pagination: {
        pageSize: 5,
      },
    },
  })

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-10">
        <p className="text-muted-foreground">Loading API keys...</p>
      </div>
    )
  }

  if (error) {
    return (
      <div className="flex items-center justify-center py-10">
        <p className="text-destructive">
          Failed to load API keys. Please try again.
        </p>
      </div>
    )
  }

  const hasFilters = columnFilters.length > 0
  const isEmpty = table.getFilteredRowModel().rows.length === 0

  return (
    <div className="space-y-4">
      <KeyTableToolbar
        table={table}
        onGenerateKey={() => setGenerateDialogOpen(true)}
      />
      <div className="rounded-md border">
        <Table>
          <TableHeader>
            {table.getHeaderGroups().map((headerGroup) => (
              <TableRow key={headerGroup.id}>
                {headerGroup.headers.map((header) => (
                  <TableHead key={header.id}>
                    {header.isPlaceholder
                      ? null
                      : flexRender(
                          header.column.columnDef.header,
                          header.getContext()
                        )}
                  </TableHead>
                ))}
              </TableRow>
            ))}
          </TableHeader>
          <TableBody>
            {isEmpty ? (
              <TableRow>
                <TableCell
                  colSpan={columns.length}
                  className="h-24 text-center"
                >
                  {hasFilters
                    ? 'No keys match your filters.'
                    : 'No API keys found.'}
                </TableCell>
              </TableRow>
            ) : (
              table.getRowModel().rows.map((row) => (
                <TableRow key={row.id}>
                  {row.getVisibleCells().map((cell) => (
                    <TableCell key={cell.id}>
                      {flexRender(
                        cell.column.columnDef.cell,
                        cell.getContext()
                      )}
                    </TableCell>
                  ))}
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </div>

      <KeyTablePagination table={table} />

      <DeleteKeyDialog
        open={deleteDialogOpen}
        onOpenChange={setDeleteDialogOpen}
        apiKey={selectedKey}
        onSuccess={() => mutate()}
      />

      <WhitelistDialog
        open={whitelistDialogOpen}
        onOpenChange={setWhitelistDialogOpen}
        apiKey={selectedKey}
        onAddIp={handleAddIp}
        onRemoveIp={handleRemoveIp}
      />

      <GenerateKeyDialog
        open={generateDialogOpen}
        onOpenChange={setGenerateDialogOpen}
        onSuccess={() => mutate()}
      />
    </div>
  )
}
