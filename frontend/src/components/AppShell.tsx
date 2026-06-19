"use client";
import { AppBar, Avatar, Box, Button, Container, Stack, Toolbar, Typography } from "@mui/material";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { ReactNode, useEffect, useState } from "react";
import { clearSession, getSession, Session } from "@/lib/api";
import ProtectedRoute from "@/components/ProtectedRoute";

export default function AppShell({ children, role }: { children: ReactNode; role: "Admin" | "Client" }) {
  const [session, setSession] = useState<Session | null>(null); const router = useRouter();
  useEffect(() => setSession(getSession()), []);
  const admin = role === "Admin";
  return <ProtectedRoute role={role}><AppBar position="static" color="inherit" elevation={0} sx={{ borderBottom: "1px solid #e5e9f2" }}><Toolbar sx={{ gap: 1, minHeight: 68 }}><Typography component={Link} href={admin ? "/admin/dashboard" : "/client/my-requests"} variant="h6" fontWeight={850} color="primary" sx={{ flexGrow: { xs: 1, md: 0 }, mr: { md: 4 } }}>IntakeAI</Typography><Stack direction="row" sx={{ display: { xs: "none", md: "flex" }, flexGrow: 1 }}>{admin ? <><Button component={Link} href="/admin/dashboard">Dashboard</Button><Button component={Link} href="/admin/project-requests">Project requests</Button><Button component={Link} href="/admin/employees">Employees</Button></> : <><Button component={Link} href="/client/submit-request">Submit request</Button><Button component={Link} href="/client/my-requests">My requests</Button></>}</Stack><Avatar sx={{ width: 32, height: 32, bgcolor: "primary.main", fontSize: 14 }}>{session?.fullName?.[0] ?? "U"}</Avatar><Box sx={{ display: { xs: "none", sm: "block" } }}><Typography variant="body2" fontWeight={700}>{session?.fullName}</Typography><Typography variant="caption" color="text.secondary">{role}</Typography></Box><Button color="inherit" onClick={() => { clearSession(); router.replace("/login"); }}>Sign out</Button></Toolbar></AppBar><Container maxWidth="lg" sx={{ py: { xs: 3, md: 5 } }}>{children}</Container></ProtectedRoute>;
}
