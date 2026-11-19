type LogoProps = {
  tone?: 'light' | 'dark'
  size?: 'md' | 'lg'
}

const sizes = {
  md: { text: 'text-[22px]', mark: 'size-[30px] rounded-[8px]', dot: 'size-[11px] right-[6px] bottom-[6px]' },
  lg: { text: 'text-[40px]', mark: 'size-[54px] rounded-[14px]', dot: 'size-[20px] right-[11px] bottom-[11px]' },
}

export function Logo({ tone = 'light', size = 'md' }: LogoProps) {
  const s = sizes[size]
  const dark = tone === 'dark'

  return (
    <span
      className={`inline-flex items-center gap-2.5 font-display leading-none font-extrabold tracking-[-0.045em] ${s.text} ${dark ? 'text-paper' : 'text-ink'}`}
    >
      <span aria-hidden="true" className={`relative flex-none ${s.mark} ${dark ? 'bg-paper' : 'bg-ink'}`}>
        <span className={`absolute rounded-full bg-volt ${s.dot}`} />
      </span>
      <span>
        gjej<b className="ml-px rounded-[5px] bg-volt px-1 pt-px pb-0.5 font-extrabold text-ink">vend</b>
      </span>
    </span>
  )
}
