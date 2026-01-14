'use client'

import { useState } from 'react'
import Link from 'next/link'
import { Menu, X } from 'lucide-react'
import { Button } from '@/components/ui/button'
import {
  Sheet,
  SheetClose,
  SheetContent,
  SheetTitle,
  SheetTrigger,
} from '@/components/ui/sheet'

export default function Nav() {
  const [open, setOpen] = useState(false)

  const docsUrl = `${process.env.NEXT_PUBLIC_DOTNET_API_URL}/swagger/index.html`
  const githubUrl = 'https://github.com/rohandrummond/mithrandir'

  return (
    <nav className="flex flex-row items-center justify-between p-4 md:p-6">
      <Link href="/" className="font-bold">
        mithrandir
      </Link>

      {/* Desktop */}
      <div className="hidden md:flex flex-row items-center gap-8">
        <a
          href={docsUrl}
          target="_blank"
          rel="noopener noreferrer"
          className="hover:text-foreground/80 transition-colors"
        >
          Swagger Docs
        </a>
        <a
          href={githubUrl}
          target="_blank"
          rel="noopener noreferrer"
          className="hover:text-foreground/80 transition-colors"
        >
          GitHub
        </a>
      </div>

      {/* Mobile */}
      <Sheet open={open} onOpenChange={setOpen}>
        {/* Open button */}
        <SheetTrigger asChild className="md:hidden">
          <Button variant="ghost" size="icon" aria-label="Open menu">
            <Menu className="h-6 w-6" />
          </Button>
        </SheetTrigger>
        <SheetContent
          side="right"
          className="w-[280px] p-4"
          hideDefaultClose
          aria-describedby={undefined}
        >
          <SheetTitle className="sr-only">Navigation menu</SheetTitle>
          {/* Close button */}
          <SheetClose asChild>
            <Button
              variant="ghost"
              size="icon"
              className="absolute top-4 right-4"
              aria-label="Close menu"
            >
              <X className="h-6 w-6" />
            </Button>
          </SheetClose>

          {/* Menu items */}
          <div className="flex flex-col items-end justify-center h-full gap-6 pr-2">
            <a
              href={docsUrl}
              target="_blank"
              rel="noopener noreferrer"
              className="text-lg hover:text-foreground/80 transition-colors"
              onClick={() => setOpen(false)}
            >
              Swagger Docs
            </a>
            <a
              href={githubUrl}
              target="_blank"
              rel="noopener noreferrer"
              className="text-lg hover:text-foreground/80 transition-colors"
              onClick={() => setOpen(false)}
            >
              GitHub
            </a>
          </div>
        </SheetContent>
      </Sheet>
    </nav>
  )
}
