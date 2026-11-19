import { Logo } from '@/components/Logo'
import { HealthBadge } from '@/features/health/HealthBadge'

export default function App() {
  return (
    <main className="mx-auto flex min-h-screen max-w-xl flex-col items-center justify-center gap-4 p-4">
      <h1 aria-label="GjejVend">
        <Logo size="lg" />
      </h1>
      <p className="text-ink-2">Gjej parkingun tënd në Prishtinë.</p>
      <HealthBadge />
    </main>
  )
}
