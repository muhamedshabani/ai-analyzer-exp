"use client";

import AppShell from "@/components/AppShell";
import RequestTable from "@/components/RequestTable";
import { api } from "@/lib/api";
import { Project } from "@/lib/types";
import { Alert, Paper, Skeleton, Typography } from "@mui/material";
import { useEffect, useState } from "react";

export default function ProjectRequestsPage() {
  const [items, setItems] = useState<Project[]>(); const [error, setError] = useState("");
  useEffect(() => { api<Project[]>("/project-requests").then(setItems).catch(x => setError(x.message)); }, []);
  return <AppShell role="Admin"><Typography variant="h4">Project requests</Typography><Typography color="text.secondary" mb={3}>Review incoming briefs and generate an initial estimate.</Typography>{error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}{!items ? <Skeleton variant="rounded" height={280} /> : items.length ? <RequestTable requests={items} admin /> : <Paper elevation={0} sx={{ p: 5, textAlign: "center", border: "1px solid #e5e9f2" }}><Typography variant="h6">No requests yet</Typography><Typography color="text.secondary">Client submissions will appear here.</Typography></Paper>}</AppShell>;
}
