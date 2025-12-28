export default function Footer() {
  return (
    <footer className="flex justify-between p-6 font-mono text-xs text-muted-foreground">
      <div>
        <p className="mb-1">.NET, Next.js, PostgreSQL, Redis and Docker</p>
        {/* <p className="mb-1">Built:</p>
        <ul className="pl-4 space-y-0.5">
          <li>Next.js</li>
          <li>.NET</li>
          <li>PostgreSQL</li>
          <li>Redis</li>
        </ul> */}
      </div>

      <div className="text-right">
        <p className="mb-1">Deployed on Vercel and AWS ECS</p>
        {/* <p className="mb-1">Deployed:</p>
        <ul className="pr-4 space-y-0.5">
          <li>Vercel</li>
          <li>AWS</li>
        </ul> */}
      </div>
    </footer>
  )
}
