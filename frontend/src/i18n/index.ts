import i18n from 'i18next'
import { initReactI18next } from 'react-i18next'
import en from '@/i18n/locales/en.json'
import sq from '@/i18n/locales/sq.json'

void i18n.use(initReactI18next).init({
  resources: { sq, en },
  lng: 'sq',
  fallbackLng: 'sq',
  ns: ['common', 'bookings'],
  defaultNS: 'common',
  interpolation: { escapeValue: false },
})

export const locales: Record<string, string> = { sq: 'sq-AL', en: 'en-GB' }

export default i18n
