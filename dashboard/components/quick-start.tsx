export function QuickStart() {
  const apiUrl = process.env.NEXT_PUBLIC_DOTNET_API_URL

  return (
    <div className="mt-8 sm:mt-10 md:mt-12 text-xs sm:text-sm text-muted-foreground space-y-2">
      <p>Want to know your middle earth zodiac?</p>
      <ol className="list-decimal list-inside space-y-1">
        <li>Generate your key</li>
        <li>Add your IP to the whitelist</li>
        <li>Update the below command and ping the API</li>
      </ol>
      <pre className="font-mono mt-2 rounded-md bg-muted/50 p-3 overflow-x-auto whitespace-pre">
        {`curl -X POST ${apiUrl}/api/who-am-i \\
            -H "X-Api-Key: <your-api-key>" \\
            -d "dob=<yyyy-mm-dd>"`}
      </pre>
    </div>
  )
}
