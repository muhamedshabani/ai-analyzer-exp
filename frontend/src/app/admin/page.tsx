"use client";
import AppShell from "@/components/AppShell";
import { api } from "@/lib/api";
import { Alert, Grid2 as Grid, Paper, Typography } from "@mui/material";
import { useEffect, useState } from "react";
type Stats = { totalRequests: number; analyzedRequests: number; pendingRequests: number; availableEmployees: number };
export default function DashboardPage() { const [stats, setStats] = useState<Stats>(); const [error, setError] = useState(""); useEffect(() => { api<Stats>("/dashboard").then(setStats).catch(x => setError(String(x))); }, []); return <AppShell><Typography variant="h4">Admin dashboard</Typography><Typography color="text.secondary" mb={3}>A quick view of intake activity and delivery capacity.</Typography>{error && <Alert severity="error">{error}</Alert>}<Grid container spacing={2}>{Object.entries({ "Total requests": stats?.totalRequests ?? "—", "Analyzed": stats?.analyzedRequests ?? "—", "Pending review": stats?.pendingRequests ?? "—", "Available employees": stats?.availableEmployees ?? "—" }).map(([label, value]) => <Grid size={{ xs: 12, sm: 6, md: 3 }} key={label}><Paper elevation={0} sx={{ p: 3, border: "1px solid #e5e9f2" }}><Typography color="text.secondary">{label}</Typography><Typography variant="h3" fontWeight={750} mt={1}>{value}</Typography></Paper></Grid>)}</Grid></AppShell>; }
