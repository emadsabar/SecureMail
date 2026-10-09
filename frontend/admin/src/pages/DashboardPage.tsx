import {
  Badge,
  Card,
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
  makeStyles,
  tokens,
} from '@fluentui/react-components'
import { useEffect, useState } from 'react'
import { api, type CertificateListItem, type DashboardSummary } from '../api/client'

const useStyles = makeStyles({
  grid: {
    display: 'grid',
    gridTemplateColumns: 'repeat(auto-fit, minmax(160px, 1fr))',
    gap: tokens.spacingHorizontalM,
    marginBottom: tokens.spacingVerticalL,
  },
  metric: {
    padding: tokens.spacingVerticalM,
    display: 'flex',
    flexDirection: 'column',
    gap: tokens.spacingVerticalXS,
  },
  value: {
    fontSize: tokens.fontSizeHero800,
    fontWeight: tokens.fontWeightSemibold,
  },
})

export function DashboardPage() {
  const styles = useStyles()
  const [data, setData] = useState<DashboardSummary | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    api
      .getDashboard()
      .then(setData)
      .catch((e: Error) => setError(e.message))
  }, [])

  if (error) {
    return <Text>{error}</Text>
  }

  if (!data) {
    return <Spinner label="Loading lifecycle dashboard…" />
  }

  const columns = [
    createTableColumn<CertificateListItem>({
      columnId: 'user',
      renderHeaderCell: () => 'User',
      renderCell: (item) => item.userPrincipalName,
    }),
    createTableColumn<CertificateListItem>({
      columnId: 'thumbprint',
      renderHeaderCell: () => 'Thumbprint',
      renderCell: (item) => item.thumbprint.slice(0, 16) + '…',
    }),
    createTableColumn<CertificateListItem>({
      columnId: 'status',
      renderHeaderCell: () => 'Status',
      renderCell: (item) => <Badge appearance="outline">{item.status}</Badge>,
    }),
    createTableColumn<CertificateListItem>({
      columnId: 'notAfter',
      renderHeaderCell: () => 'Expires',
      renderCell: (item) => new Date(item.notAfter).toLocaleDateString(),
    }),
    createTableColumn<CertificateListItem>({
      columnId: 'host',
      renderHeaderCell: () => 'Device',
      renderCell: (item) => item.hostname ?? '—',
    }),
  ]

  const metrics = [
    ['Eligible users', data.eligibleUsers],
    ['Active', data.activeCertificates],
    ['Expiring soon', data.expiringSoon],
    ['Pending issuance', data.pendingIssuance],
    ['Failed', data.failed],
    ['Devices', data.devicesSeen],
  ] as const

  return (
    <>
      <Title3>Certificate lifecycle</Title3>
      <Text>
        Enrollment → issuance → CurrentUser\\My install → AD publish → Entra Connect → Outlook
        readiness.
      </Text>
      <div className={styles.grid}>
        {metrics.map(([label, value]) => (
          <Card key={label} className={styles.metric}>
            <Text size={200}>{label}</Text>
            <span className={styles.value}>{value}</span>
          </Card>
        ))}
      </div>
      <DataGrid items={data.certificates} columns={columns} getRowId={(item) => item.id}>
        <DataGridHeader>
          <DataGridRow>
            {({ renderHeaderCell }) => (
              <DataGridHeaderCell>{renderHeaderCell()}</DataGridHeaderCell>
            )}
          </DataGridRow>
        </DataGridHeader>
        <DataGridBody<CertificateListItem>>
          {({ item, rowId }) => (
            <DataGridRow<CertificateListItem> key={rowId}>
              {({ renderCell }) => <DataGridCell>{renderCell(item)}</DataGridCell>}
            </DataGridRow>
          )}
        </DataGridBody>
      </DataGrid>
    </>
  )
}
