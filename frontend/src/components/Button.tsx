import type { ButtonHTMLAttributes } from 'react'

type Variant = 'primary' | 'neutral' | 'danger'

const variants: Record<Variant, string> = {
  primary: 'bg-lime text-lime-ink hover:brightness-95 active:brightness-90',
  neutral: 'border border-paper-3 bg-paper text-ink hover:bg-paper-2 active:bg-paper-3',
  danger: 'bg-danger text-white hover:brightness-110 active:brightness-95',
}

type ButtonProps = ButtonHTMLAttributes<HTMLButtonElement> & { variant?: Variant; loading?: boolean }

export function Button({ variant = 'neutral', loading = false, disabled, className = '', children, ...props }: ButtonProps) {
  return (
    <button
      {...props}
      disabled={disabled || loading}
      aria-busy={loading}
      className={`inline-flex min-h-11 items-center justify-center gap-2 rounded-xl px-4 font-semibold transition focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-ink disabled:cursor-not-allowed disabled:bg-paper-3 disabled:text-ink-3 ${variants[variant]} ${className}`}
    >
      {children}
    </button>
  )
}
