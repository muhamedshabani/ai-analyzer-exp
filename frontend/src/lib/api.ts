export const API_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5080/api";

export type Session = { token: string; fullName: string; email: string; roles: string[] };
export function getSession(): Session | null { if (typeof window === "undefined") return null; const value = localStorage.getItem("session"); return value ? JSON.parse(value) : null; }
export async function api<T>(path: string, options: RequestInit = {}): Promise<T> {
  const token = getSession()?.token;
  const response = await fetch(`${API_URL}${path}`, { ...options, headers: { "Content-Type": "application/json", ...(token ? { Authorization: `Bearer ${token}` } : {}), ...options.headers } });
  if (!response.ok) { const detail = await response.text(); throw new Error(detail || `Request failed (${response.status})`); }
  return response.status === 204 ? undefined as T : response.json();
}
