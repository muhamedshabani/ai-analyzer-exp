"use client";

import AppShell from "@/components/AppShell";
import { api, getSession } from "@/lib/api";
import { Alert, Button, Grid2 as Grid, Paper, TextField, Typography } from "@mui/material";
import { FormEvent, useState } from "react";
import { useRouter } from "next/navigation";

export default function SubmitRequestPage() {
  const router = useRouter(); const [error, setError] = useState(""); const [busy, setBusy] = useState(false);
  async function submit(event: FormEvent<HTMLFormElement>) { event.preventDefault(); setError(""); setBusy(true); const body = Object.fromEntries(new FormData(event.currentTarget)); try { await api("/project-requests", { method: "POST", body: JSON.stringify(body) }); router.push("/client/my-requests"); } catch (x) { setError(x instanceof Error ? x.message : "Could not submit request."); setBusy(false); } }
  return <AppShell role="Client"><Typography variant="h4">Submit a project request</Typography><Typography color="text.secondary" mb={3}>Describe the outcome, users, and important constraints.</Typography><Paper elevation={0} sx={{ p: { xs: 2, md: 4 }, border: "1px solid #e5e9f2" }}><Grid container spacing={2.5} component="form" onSubmit={submit}>{error && <Grid size={12}><Alert severity="error">{error}</Alert></Grid>}<Grid size={{ xs: 12, md: 8 }}><TextField fullWidth name="projectTitle" label="Project title" required /></Grid><Grid size={{ xs: 12, md: 4 }}><TextField fullWidth name="industry" label="Industry" required /></Grid><Grid size={12}><TextField fullWidth multiline minRows={6} name="projectDescription" label="Project description" helperText="At least 20 characters. Include users, workflows, integrations, and expected outcome." required slotProps={{ htmlInput: { minLength: 20 } }} /></Grid><Grid size={{ xs: 12, md: 6 }}><TextField fullWidth name="budgetRange" label="Budget range" placeholder="€20,000–€40,000" required /></Grid><Grid size={{ xs: 12, md: 6 }}><TextField fullWidth type="date" name="desiredDeadline" label="Desired deadline" slotProps={{ inputLabel: { shrink: true } }} /></Grid><Grid size={{ xs: 12, md: 6 }}><TextField fullWidth name="contactEmail" type="email" label="Contact email" required defaultValue={getSession()?.email ?? ""} /></Grid><Grid size={{ xs: 12, md: 6 }}><TextField fullWidth name="companyName" label="Company name" /></Grid><Grid size={12}><Button type="submit" variant="contained" size="large" disabled={busy}>{busy ? "Submitting…" : "Submit project request"}</Button></Grid></Grid></Paper></AppShell>;
}
