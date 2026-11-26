import '@testing-library/jest-dom/vitest'
import '@/i18n'

HTMLDialogElement.prototype.showModal ??= function showModal(this: HTMLDialogElement) {
  this.open = true
}

HTMLDialogElement.prototype.close ??= function close(this: HTMLDialogElement) {
  this.open = false
  this.dispatchEvent(new Event('close'))
}
