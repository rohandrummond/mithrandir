import { clsx, type ClassValue } from 'clsx'
import { twMerge } from 'tailwind-merge'

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

export function getEnvironmentVariable(name: string): string {
  const val = process.env[name]
  if (!val) throw new Error(`${name} environment variable is not set`)
  return val
}
