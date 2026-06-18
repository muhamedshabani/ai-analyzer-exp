"use client";
import AppShell from "@/components/AppShell";
import RequestTable from "@/components/RequestTable";
import { api } from "@/lib/api";
import { Project } from "@/lib/types";
import { Alert, CircularProgress, Typography } from "@mui/material";
import { useEffect, useState } from "react";
export default function RequestsPage({ admin = false }: { admin?: boolean }) { const [items, setItems] = useState<Project[]>(); const [error, setError] = useState(""); useEffect(() => { api<Project[]>("/project-requests").then(setItems).catch(x => setError(String(x))); }, []); return <AppShell><Typography variant="h4">{admin ? "Project requests" : "My requests"}</Typography><Typography color="text.secondary" mb={3}>{admin ? "Review, analyze, and respond to incoming briefs." : "Track the status of your submitted ideas."}</Typography>{error ? <Alert severity="error">{error}</Alert> : !items ? <CircularProgress /> : <RequestTable requests={items} />}</AppShell>; }
