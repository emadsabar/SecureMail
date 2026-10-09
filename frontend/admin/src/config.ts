export const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? ''

export const authMode = (import.meta.env.VITE_AUTH_MODE ?? 'Development') as 'Development' | 'Windows'
