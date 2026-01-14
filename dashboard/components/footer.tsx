export default function Footer() {
  return (
    <footer className="flex flex-col items-center gap-4 p-4 md:flex-row md:justify-between md:gap-4 md:p-6 font-mono text-xs text-muted-foreground">
      <p className="text-center">
        Built with .NET, Next.js, PostgreSQL & Redis
      </p>
      <p className="text-center">Running on Amazon EC2, Docker & Vercel</p>
    </footer>
  )
}
