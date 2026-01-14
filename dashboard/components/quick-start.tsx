export function QuickStart() {
  const apiUrl = process.env.NEXT_PUBLIC_DOTNET_API_URL

  return (
    <div className="mt-10 sm:mt-0 text-xs text-muted-foreground space-y-2">
      <p>Uncover your middle earth star sign 🔮</p>
      <pre className="font-mono rounded-md bg-muted/50 pl-0 sm:pl-3 overflow-x-auto whitespace-pre-wrap">{`curl "${apiUrl}/api/zodiac?dob=<yyyy-mm-dd>" -H "X-Api-Key: <your-api-key>"`}</pre>
    </div>
  )
}
