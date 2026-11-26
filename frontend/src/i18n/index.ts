import i18n from 'i18next'
import { initReactI18next } from 'react-i18next'
import en from './locales/en.json'
import sq from './locales/sq.json'
import bookingsEn from './locales/bookings/en.json'
import bookingsSq from './locales/bookings/sq.json'

// Albanian is the default language, English the fallback (see the plan: sq first, then en).
void i18n.use(initReactI18next).init({
  resources: { sq: { translation: sq, bookings: bookingsSq }, en: { translation: en, bookings: bookingsEn } },
  lng: 'sq',
  fallbackLng: 'en',
  interpolation: { escapeValue: false },
})

export default i18n
