import { Status, Tier } from "./enums";

export interface ApiKey {
  id: number;
  keyHash?: string;
  name: string;
  tier: Tier;
  status: Status;
  ipWhitelist: string[] | null;
  createdAt: string;
  expiresAt: string | null;
  lastUsedAt: string | null;
}
