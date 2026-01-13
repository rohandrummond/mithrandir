export default function Footer() {
  return (
    <footer className="flex flex-col items-center gap-4 p-4 min-[655px]:flex-row min-[655px]:justify-between min-[655px]:gap-4 min-[655px]:p-6 font-mono text-xs text-muted-foreground">
      <p className="text-center">
        Built with .NET, Next.js, PostgreSQL & Redis
      </p>
      <p className="text-center">Running on Amazon EC2, Docker & Vercel</p>
    </footer>
  )
}
