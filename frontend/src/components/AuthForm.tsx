"use client";
import { Alert, Box, Button, Paper, Stack, TextField, Typography } from "@mui/material";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { FormEvent, useState } from "react";
import { api, saveSession, Session } from "@/lib/api";

export default function AuthForm({ register = false }: { register?: boolean }) {
  const [error, setError] = useState(""); const [busy, setBusy] = useState(false); const router = useRouter();
  async function submit(e: FormEvent<HTMLFormElement>) { e.preventDefault(); setBusy(true); setError(""); const data = new FormData(e.currentTarget); try { const body = Object.fromEntries(data); const session = await api<Session>(register ? "/auth/register" : "/auth/login", { method: "POST", body: JSON.stringify(body) }); saveSession(session); router.replace(session.roles.includes("Admin") ? "/admin/dashboard" : "/client/my-requests"); } catch (x) { setError(x instanceof Error ? x.message : "Something went wrong."); } finally { setBusy(false); } }
  return <Box sx={{ minHeight: "100vh", display: "grid", placeItems: "center", p: 2 }}><Paper elevation={0} sx={{ p: 4, width: "100%", maxWidth: 440, border: "1px solid #e5e9f2" }}><Typography variant="h4">{register ? "Create account" : "Welcome back"}</Typography><Typography color="text.secondary" mt={1} mb={3}>{register ? "Submit and track your project ideas." : "Sign in to continue to IntakeAI."}</Typography><Stack component="form" spacing={2} onSubmit={submit}>{error && <Alert severity="error">{error}</Alert>}{register && <TextField name="fullName" label="Full name" required />}<TextField name="email" label="Email" type="email" required defaultValue={register ? "" : "admin@demo.local"} /><TextField name="password" label="Password" type="password" required defaultValue={register ? "" : "Admin123!"} /><Button type="submit" size="large" variant="contained" disabled={busy}>{busy ? "Please wait…" : register ? "Register" : "Sign in"}</Button><Typography textAlign="center" variant="body2">{register ? "Already registered?" : "New client?"} <Link href={register ? "/login" : "/register"} style={{ color: "#3157d5" }}>{register ? "Sign in" : "Create an account"}</Link></Typography></Stack></Paper></Box>;
}
