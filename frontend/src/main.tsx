import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import '@/i18n'
import App from '@/app/App'
import { Providers } from '@/app/providers'
import { startSessionSync } from '@/store/session'

startSessionSync()

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <Providers>
      <App />
    </Providers>
  </StrictMode>,
)
