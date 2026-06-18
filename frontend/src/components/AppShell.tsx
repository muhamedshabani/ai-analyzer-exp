"use client";
import { AppBar, Box, Button, Container, Toolbar, Typography } from "@mui/material";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { ReactNode, useEffect, useState } from "react";
import { getSession, Session } from "@/lib/api";

export default function AppShell({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<Session | null>(null); const router = useRouter();
  useEffect(() => setSession(getSession()), []);
  const admin = session?.roles.includes("Admin");
  return <><AppBar position="static" color="inherit" elevation={0} sx={{ borderBottom: "1px solid #e5e9f2" }}><Toolbar sx={{ gap: 1 }}><Typography variant="h6" fontWeight={800} color="primary" sx={{ flexGrow: 1 }}>IntakeAI</Typography>{admin ? <><Button component={Link} href="/admin">Dashboard</Button><Button component={Link} href="/admin/requests">Requests</Button><Button component={Link} href="/admin/employees">Employees</Button></> : <><Button component={Link} href="/client/submit">New request</Button><Button component={Link} href="/client/requests">My requests</Button></>}<Button color="inherit" onClick={() => { localStorage.removeItem("session"); router.push("/login"); }}>Sign out</Button></Toolbar></AppBar><Container maxWidth="lg" sx={{ py: 5 }}>{children}</Container></>;
}
