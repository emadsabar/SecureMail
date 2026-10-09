export const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? ''

export const authMode = (import.meta.env.VITE_AUTH_MODE ?? 'Development') as
  | 'Development'
  | 'EntraId'

export const msalConfig = {
  auth: {
    clientId: import.meta.env.VITE_ENTRA_CLIENT_ID ?? '',
    authority: `https://login.microsoftonline.com/${import.meta.env.VITE_ENTRA_TENANT_ID ?? 'common'}`,
    redirectUri: window.location.origin,
  },
  cache: {
    cacheLocation: 'sessionStorage' as const,
  },
}

export const apiScopes = import.meta.env.VITE_API_SCOPE
  ? [import.meta.env.VITE_API_SCOPE]
  : []
