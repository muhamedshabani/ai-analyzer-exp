"use client";

import AppShell from "@/components/AppShell";
import RequestTable from "@/components/RequestTable";
import { api } from "@/lib/api";
import { Project } from "@/lib/types";
import { Add } from "@mui/icons-material";
import { Alert, Button, Paper, Skeleton, Stack, Typography } from "@mui/material";
import Link from "next/link";
import { useEffect, useState } from "react";

export default function MyRequestsPage() {
  const [items, setItems] = useState<Project[]>(); const [error, setError] = useState("");
  useEffect(() => { api<Project[]>("/project-requests").then(setItems).catch(x => setError(x.message)); }, []);
  return <AppShell role="Client"><Stack direction={{ xs: "column", sm: "row" }} justifyContent="space-between" alignItems={{ sm: "center" }} mb={3} gap={2}><div><Typography variant="h4">My requests</Typography><Typography color="text.secondary">Track your submitted project ideas.</Typography></div><Button component={Link} href="/client/submit-request" variant="contained" startIcon={<Add />}>New request</Button></Stack>{error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}{!items ? <Skeleton variant="rounded" height={260} /> : items.length ? <RequestTable requests={items} /> : <Paper elevation={0} sx={{ p: 5, textAlign: "center", border: "1px solid #e5e9f2" }}><Typography variant="h6">No project requests yet</Typography><Typography color="text.secondary" mb={2}>Submit your first idea to get started.</Typography><Button component={Link} href="/client/submit-request" variant="outlined">Submit a request</Button></Paper>}</AppShell>;
}
