import { API_URL } from '@/lib/env'

export async function apiGet<T>(path: string): Promise<T> {
  const res = await fetch(`${API_URL}${path}`)
  if (!res.ok) throw new Error(`API ${res.status}`)
  return (await res.json()) as T
}

export async function getHealth(): Promise<string> {
  const res = await fetch(`${API_URL}/health`)
  if (!res.ok) throw new Error(`API ${res.status}`)
  return res.text()
}
