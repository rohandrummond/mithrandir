export default function Footer() {
  return (
    <footer className="flex flex-col items-start gap-2 sm:gap-4 p-4 md:flex-row md:justify-between md:gap-4 md:p-6 font-mono text-xs text-muted-foreground">
      <p className="text-start">Built with .NET, Next.js, PostgreSQL & Redis</p>
      <p className="text-start">Running on Amazon EC2, Docker & Vercel</p>
    </footer>
  )
}
