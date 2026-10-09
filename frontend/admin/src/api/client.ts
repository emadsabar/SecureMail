import { apiBaseUrl } from '../config'

export type DashboardSummary = {
  eligibleUsers: number
  activeCertificates: number
  expiringSoon: number
  failed: number
  pendingIssuance: number
  devicesSeen: number
  certificates: CertificateListItem[]
}

export type CertificateListItem = {
  id: string
  userPrincipalName: string
  thumbprint: string
  status: string
  notAfter: string
  hostname?: string | null
}

export type UserListItem = {
  id: string
  userPrincipalName: string
  mail: string
  displayName: string
  isEligible: boolean
  distinguishedName?: string | null
}

export type AuditLogItem = {
  id: string
  createdAt: string
  actorType: string
  actor: string
  action: string
  entityType: string
  entityId?: string | null
  details: string
}

let accessTokenProvider: (() => Promise<string | undefined>) | undefined

export function setAccessTokenProvider(provider: () => Promise<string | undefined>) {
  accessTokenProvider = provider
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const headers = new Headers(init?.headers)
  headers.set('Accept', 'application/json')
  if (init?.body && !headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json')
  }

  const token = accessTokenProvider ? await accessTokenProvider() : undefined
  if (token) {
    headers.set('Authorization', `Bearer ${token}`)
  }

  const response = await fetch(`${apiBaseUrl}${path}`, { ...init, headers })
  if (!response.ok) {
    const text = await response.text()
    throw new Error(text || `${response.status} ${response.statusText}`)
  }

  if (response.status === 204) {
    return undefined as T
  }

  return (await response.json()) as T
}

export const api = {
  getDashboard: () => request<DashboardSummary>('/api/dashboard'),
  getUsers: () => request<UserListItem[]>('/api/users'),
  getAudit: () => request<AuditLogItem[]>('/api/audit'),
  syncUser: (userPrincipalName: string) =>
    request<string>('/api/users/sync', {
      method: 'POST',
      body: JSON.stringify({ userPrincipalName }),
    }),
  startEnrollment: (userId: string, isRenewal = false) =>
    request<string>(`/api/users/${userId}/enrollments`, {
      method: 'POST',
      body: JSON.stringify({ deviceId: null, isRenewal }),
    }),
  confirmDirectorySync: (certificateId: string) =>
    request<void>('/api/certificates/confirm-directory-sync', {
      method: 'POST',
      body: JSON.stringify({ certificateId }),
    }),
  revoke: (certificateId: string, reason: string) =>
    request<void>('/api/certificates/revoke', {
      method: 'POST',
      body: JSON.stringify({ certificateId, reason }),
    }),
}
