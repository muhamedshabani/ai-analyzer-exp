export const API_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5000/api";

export type Session = { token: string; fullName: string; email: string; roles: string[] };
export function getSession(): Session | null {
  if (typeof window === "undefined") return null;
  try {
    const value = localStorage.getItem("session");
    return value ? JSON.parse(value) as Session : null;
  } catch {
    localStorage.removeItem("session");
    return null;
  }
}
export function saveSession(session: Session) { localStorage.setItem("session", JSON.stringify(session)); }
export function clearSession() { localStorage.removeItem("session"); }
export async function api<T>(path: string, options: RequestInit = {}): Promise<T> {
  const token = getSession()?.token;
  const response = await fetch(`${API_URL}${path}`, { ...options, headers: { "Content-Type": "application/json", ...(token ? { Authorization: `Bearer ${token}` } : {}), ...options.headers } });
  if (response.status === 401 && typeof window !== "undefined") clearSession();
  if (!response.ok) {
    const detail = await response.text();
    let message = detail;
    try {
      const parsed = JSON.parse(detail);
      message = parsed.message ?? parsed.title ?? (Array.isArray(parsed) ? parsed.join(" ") : detail);
    } catch { /* keep response text */ }
    throw new Error(message || `Request failed (${response.status})`);
  }
  return response.status === 204 ? undefined as T : response.json();
}
