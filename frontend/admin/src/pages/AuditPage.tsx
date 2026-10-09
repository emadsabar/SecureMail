import {
  DataGrid,
  DataGridBody,
  DataGridCell,
  DataGridHeader,
  DataGridHeaderCell,
  DataGridRow,
  Spinner,
  Text,
  Title3,
  createTableColumn,
} from '@fluentui/react-components'
import { useEffect, useState } from 'react'
import { api, type AuditLogItem } from '../api/client'

export function AuditPage() {
  const [items, setItems] = useState<AuditLogItem[] | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    api
      .getAudit()
      .then(setItems)
      .catch((e: Error) => setError(e.message))
  }, [])

  if (error) {
    return <Text>{error}</Text>
  }

  if (!items) {
    return <Spinner label="Loading audit log…" />
  }

  const columns = [
    createTableColumn<AuditLogItem>({
      columnId: 'when',
      renderHeaderCell: () => 'When',
      renderCell: (item) => new Date(item.createdAt).toLocaleString(),
    }),
    createTableColumn<AuditLogItem>({
      columnId: 'actor',
      renderHeaderCell: () => 'Actor',
      renderCell: (item) => `${item.actorType}: ${item.actor}`,
    }),
    createTableColumn<AuditLogItem>({
      columnId: 'action',
      renderHeaderCell: () => 'Action',
      renderCell: (item) => item.action,
    }),
    createTableColumn<AuditLogItem>({
      columnId: 'entity',
      renderHeaderCell: () => 'Entity',
      renderCell: (item) => item.entityType,
    }),
    createTableColumn<AuditLogItem>({
      columnId: 'details',
      renderHeaderCell: () => 'Details',
      renderCell: (item) => item.details,
    }),
  ]

  return (
    <>
      <Title3>Audit</Title3>
      <Text>Lifecycle actions for help-desk and administrator review.</Text>
      <DataGrid items={items} columns={columns} getRowId={(item) => item.id}>
        <DataGridHeader>
          <DataGridRow>
            {({ renderHeaderCell }) => (
              <DataGridHeaderCell>{renderHeaderCell()}</DataGridHeaderCell>
            )}
          </DataGridRow>
        </DataGridHeader>
        <DataGridBody<AuditLogItem>>
          {({ item, rowId }) => (
            <DataGridRow<AuditLogItem> key={rowId}>
              {({ renderCell }) => <DataGridCell>{renderCell(item)}</DataGridCell>}
            </DataGridRow>
          )}
        </DataGridBody>
      </DataGrid>
    </>
  )
}
