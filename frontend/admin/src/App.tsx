import {
  FluentProvider,
  Tab,
  TabList,
  Text,
  Title2,
  makeStyles,
  tokens,
  webLightTheme,
} from '@fluentui/react-components'
import { Navigate, Route, Routes, useLocation, useNavigate } from 'react-router-dom'
import { AuditPage } from './pages/AuditPage'
import { DashboardPage } from './pages/DashboardPage'
import { UsersPage } from './pages/UsersPage'

const useStyles = makeStyles({
  shell: {
    minHeight: '100vh',
    backgroundColor: tokens.colorNeutralBackground2,
  },
  header: {
    padding: `${tokens.spacingVerticalL} ${tokens.spacingHorizontalXXL}`,
    backgroundColor: tokens.colorNeutralBackground1,
    borderBottom: `1px solid ${tokens.colorNeutralStroke2}`,
    display: 'flex',
    flexDirection: 'column',
    gap: tokens.spacingVerticalS,
  },
  brand: {
    letterSpacing: '-0.02em',
  },
  content: {
    padding: tokens.spacingHorizontalXXL,
    display: 'flex',
    flexDirection: 'column',
    gap: tokens.spacingVerticalM,
  },
})

function Shell() {
  const styles = useStyles()
  const navigate = useNavigate()
  const location = useLocation()
  const selected = location.pathname.startsWith('/users')
    ? 'users'
    : location.pathname.startsWith('/audit')
      ? 'audit'
      : 'dashboard'

  return (
    <div className={styles.shell}>
      <header className={styles.header}>
        <Title2 className={styles.brand}>SmimeLifecycle</Title2>
        <Text>
          On-premises Certificate Management Server for S/MIME — clients never call the CA.
        </Text>
        <TabList
          selectedValue={selected}
          onTabSelect={(_, data) => {
            const value = String(data.value)
            navigate(value === 'dashboard' ? '/' : `/${value}`)
          }}
        >
          <Tab value="dashboard">Dashboard</Tab>
          <Tab value="users">Users</Tab>
          <Tab value="audit">Audit</Tab>
        </TabList>
      </header>
      <main className={styles.content}>
        <Routes>
          <Route path="/" element={<DashboardPage />} />
          <Route path="/users" element={<UsersPage />} />
          <Route path="/audit" element={<AuditPage />} />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </main>
    </div>
  )
}

export default function App() {
  return (
    <FluentProvider theme={webLightTheme}>
      <Shell />
    </FluentProvider>
  )
}
