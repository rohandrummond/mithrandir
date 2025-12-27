import Link from 'next/link'
import { GitHubLogoIcon } from '@radix-ui/react-icons'

export default function Nav() {
  return (
    <div className="flex flex-row items-center justify-between p-6">
      <Link href="/" className="font-bold text-xl">
        Mithrandir
      </Link>
      <div className="flex flex-row items-center gap-5">
        <a
          href={`${process.env.DOTNET_PUBLIC_URL}/swagger/index.html`}
          target="_blank"
          rel="noopener noreferrer"
        >
          Docs
        </a>
        <a
          href="https://github.com/rohandrummond/mithrandir"
          target="_blank"
          rel="noopener noreferrer"
        >
          <GitHubLogoIcon className="w-[22px] h-[22px]" />
        </a>
      </div>
    </div>
  )
}
