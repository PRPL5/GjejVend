import type { InputHTMLAttributes, SelectHTMLAttributes, TextareaHTMLAttributes } from 'react'

const control =
  'min-h-11 w-full rounded-xl border border-paper-3 bg-white px-3 text-base text-ink focus-visible:outline-2 focus-visible:outline-ink aria-invalid:border-danger'

type Labelled = { label: string }

export function TextField({
  label,
  className = '',
  ...props
}: Labelled & InputHTMLAttributes<HTMLInputElement>) {
  return (
    <label className="flex flex-col gap-1 font-medium">
      {label}
      <input {...props} className={`${control} ${className}`} />
    </label>
  )
}

export function SelectField({
  label,
  children,
  ...props
}: Labelled & SelectHTMLAttributes<HTMLSelectElement>) {
  return (
    <label className="flex flex-col gap-1 font-medium">
      {label}
      <select {...props} className={control}>
        {children}
      </select>
    </label>
  )
}

export function TextAreaField({
  label,
  ...props
}: Labelled & TextareaHTMLAttributes<HTMLTextAreaElement>) {
  return (
    <label className="flex flex-col gap-1 font-medium">
      {label}
      <textarea {...props} className={`${control} py-2`} rows={3} />
    </label>
  )
}
