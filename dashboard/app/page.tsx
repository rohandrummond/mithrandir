import { KeyTable } from '@/components/keys/key-table'

export default function Home() {
  return (
    <div className="container py-6">
      <div className="mb-6">
        <h1 className="text-2xl font-bold">API Keys</h1>
      </div>
      <KeyTable />
    </div>
  )
}
