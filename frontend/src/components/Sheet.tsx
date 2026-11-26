import { useEffect, useRef, type ReactNode } from 'react'
import { useTranslation } from 'react-i18next'

type SheetProps = { open: boolean; title: string; onClose: () => void; children: ReactNode }

export function Sheet({ open, title, onClose, children }: SheetProps) {
  const { t } = useTranslation()
  const dialog = useRef<HTMLDialogElement>(null)

  useEffect(() => {
    const element = dialog.current
    if (!element) return
    if (open && !element.open) element.showModal?.()
    if (!open && element.open) element.close?.()
  }, [open])

  return (
    <dialog
      ref={dialog}
      onClose={onClose}
      aria-label={title}
      className="m-0 mt-auto w-full max-w-none rounded-t-3xl bg-paper p-6 shadow-sh-2 backdrop:bg-ink/40 sm:m-auto sm:max-w-md sm:rounded-3xl"
    >
      {open && (
        <div className="flex flex-col gap-4">
          <div className="flex items-start justify-between gap-4">
            <h2 className="font-display text-xl font-bold">{title}</h2>
            <button type="button" onClick={onClose} className="min-h-11 min-w-11 underline">
              {t('common.close')}
            </button>
          </div>
          {children}
        </div>
      )}
    </dialog>
  )
}
