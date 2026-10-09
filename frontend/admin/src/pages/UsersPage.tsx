import {
  Button,
  DataGrid,
  DataGridBody,
  DataGridCell,
  DataGridHeader,
  DataGridHeaderCell,
  DataGridRow,
  Input,
  Spinner,
  Text,
  Title3,
  createTableColumn,
  makeStyles,
  tokens,
} from '@fluentui/react-components'
import { useEffect, useState } from 'react'
import { api, type UserListItem } from '../api/client'

const useStyles = makeStyles({
  toolbar: {
    display: 'flex',
    gap: tokens.spacingHorizontalS,
    marginBottom: tokens.spacingVerticalL,
    alignItems: 'center',
    maxWidth: '640px',
  },
})

export function UsersPage() {
  const styles = useStyles()
  const [users, setUsers] = useState<UserListItem[]>([])
  const [upn, setUpn] = useState('alex.rivera@contoso.com')
  const [busy, setBusy] = useState(false)
  const [message, setMessage] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)

  const refresh = () =>
    api
      .getUsers()
      .then(setUsers)
      .catch((e: Error) => setError(e.message))

  useEffect(() => {
    void refresh()
  }, [])

  const sync = async () => {
    setBusy(true)
    setMessage(null)
    setError(null)
    try {
      const id = await api.syncUser(upn)
      setMessage(`Synced eligible user ${upn} (${id}).`)
      await refresh()
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Sync failed')
    } finally {
      setBusy(false)
    }
  }

  const enroll = async (userId: string) => {
    setBusy(true)
    setMessage(null)
    setError(null)
    try {
      const enrollmentId = await api.startEnrollment(userId)
      setMessage(`Enrollment ${enrollmentId} started. Endpoint agent will generate CSR.`)
      await refresh()
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Enrollment failed')
    } finally {
      setBusy(false)
    }
  }

  const columns = [
    createTableColumn<UserListItem>({
      columnId: 'displayName',
      renderHeaderCell: () => 'Name',
      renderCell: (item) => item.displayName,
    }),
    createTableColumn<UserListItem>({
      columnId: 'upn',
      renderHeaderCell: () => 'UPN',
      renderCell: (item) => item.userPrincipalName,
    }),
    createTableColumn<UserListItem>({
      columnId: 'eligible',
      renderHeaderCell: () => 'Eligible',
      renderCell: (item) => (item.isEligible ? 'Yes' : 'No'),
    }),
    createTableColumn<UserListItem>({
      columnId: 'actions',
      renderHeaderCell: () => 'Actions',
      renderCell: (item) => (
        <Button size="small" disabled={busy || !item.isEligible} onClick={() => void enroll(item.id)}>
          Start enrollment
        </Button>
      ),
    }),
  ]

  if (error && users.length === 0) {
    return <Text>{error}</Text>
  }

  if (!users && !error) {
    return <Spinner label="Loading users…" />
  }

  return (
    <>
      <Title3>Eligible users</Title3>
      <Text>Sync AD-eligible identities, then start endpoint enrollment via the CMS (no direct CA calls).</Text>
      <div className={styles.toolbar}>
        <Input
          value={upn}
          onChange={(_, d) => setUpn(d.value)}
          placeholder="user@contoso.com"
          style={{ flex: 1 }}
        />
        <Button appearance="primary" disabled={busy} onClick={() => void sync()}>
          Sync user
        </Button>
      </div>
      {message ? <Text>{message}</Text> : null}
      {error ? <Text>{error}</Text> : null}
      <DataGrid items={users} columns={columns} getRowId={(item) => item.id}>
        <DataGridHeader>
          <DataGridRow>
            {({ renderHeaderCell }) => (
              <DataGridHeaderCell>{renderHeaderCell()}</DataGridHeaderCell>
            )}
          </DataGridRow>
        </DataGridHeader>
        <DataGridBody<UserListItem>>
          {({ item, rowId }) => (
            <DataGridRow<UserListItem> key={rowId}>
              {({ renderCell }) => <DataGridCell>{renderCell(item)}</DataGridCell>}
            </DataGridRow>
          )}
        </DataGridBody>
      </DataGrid>
    </>
  )
}
