import { useQuery } from '@tanstack/react-query'
import { getHealth } from '@/api/client'

export function HealthBadge() {
  const { data, isError, isPending } = useQuery({ queryKey: ['health'], queryFn: getHealth })
  const label = isPending ? 'checking…' : isError ? 'API offline' : `API ${data}`
  const color = isError ? 'bg-red-100 text-red-800' : 'bg-emerald-100 text-emerald-800'
  return <span className={`rounded-full px-3 py-1 text-sm ${color}`}>{label}</span>
}
