"use client";

import AppShell from "@/components/AppShell";
import { api } from "@/lib/api";
import { Analysis, Project } from "@/lib/types";
import { AutoAwesome, CheckCircleOutline, EmailOutlined, LightbulbOutlined } from "@mui/icons-material";
import { Alert, Box, Button, Chip, CircularProgress, Divider, Fade, Grid2 as Grid, LinearProgress, Paper, Skeleton, Stack, Typography } from "@mui/material";
import { useParams } from "next/navigation";
import { ReactNode, useCallback, useEffect, useMemo, useState } from "react";

const wait = (milliseconds: number) => new Promise(resolve => setTimeout(resolve, milliseconds));
const loadingMessages = [
  "Reading the project brief…",
  "Matching employee skills and availability…",
  "Estimating complexity, timeline, and cost…",
  "Preparing risks and the client reply…"
];

function technologyRecommendation(analysis: Analysis) {
  const stack = analysis.suggestedTechStack.toLowerCase();
  if (stack.includes("sqlite")) return "Keep SQLite for the thesis demo because it is portable and reliable. Before a multi-user production launch, migrate to PostgreSQL or Azure SQL and add automated backups, monitoring, and CI/CD.";
  if (stack.includes("azure")) return "Standardize deployment on Azure using a managed database, application monitoring, and an automated CI/CD pipeline. Keep infrastructure intentionally small for the MVP.";
  return "Use the proposed stack for the MVP because it aligns with the available team. Before production, add a managed database, centralized monitoring, automated tests, and a simple CI/CD pipeline.";
}

function teamRecommendation(analysis: Analysis) {
  const team = analysis.recommendedTeam.toLowerCase();
  const recommendations: string[] = [];
  if (!team.includes("qa") && !team.includes("test")) recommendations.push("hire or assign a part-time QA Automation Engineer before the stabilization phase");
  if (!team.includes("ux") && !team.includes("design")) recommendations.push("contract a UX/UI Designer for discovery, wireframes, and usability review");
  if (!team.includes("devops") && !team.includes("cloud")) recommendations.push("add a part-time DevOps/Cloud Engineer before production deployment");
  if (!recommendations.length) return "The proposed team covers the main delivery disciplines. Reserve 10–15% of team capacity for technical leadership, review, and release support.";
  return `Keep the proposed employees as the core MVP team. To close capability gaps, ${recommendations.slice(0, 2).join(" and ")}.`;
}

function AnalysisCard({ title, children, wide = false, recommendation }: { title: string; children: ReactNode; wide?: boolean; recommendation?: string }) {
  return <Grid size={{ xs: 12, md: wide ? 12 : 6 }}><Fade in timeout={550}><Paper elevation={0} sx={{ p: 3, border: "1px solid #e5e9f2", height: "100%", boxShadow: "0 8px 24px rgba(36, 57, 96, 0.05)" }}><Stack direction="row" alignItems="center" gap={1}><CheckCircleOutline color="success" fontSize="small" /><Typography variant="overline" color="primary" fontWeight={800}>{title}</Typography></Stack><Typography component="div" sx={{ whiteSpace: "pre-line", mt: 1, lineHeight: 1.75 }}>{children}</Typography>{recommendation && <><Divider sx={{ my: 2.5 }} /><Box sx={{ p: 2, borderRadius: 2, bgcolor: "rgba(49, 87, 213, 0.06)", border: "1px solid rgba(49, 87, 213, 0.14)" }}><Stack direction="row" gap={1} alignItems="center" mb={0.75}><LightbulbOutlined color="primary" fontSize="small" /><Typography variant="subtitle2" color="primary" fontWeight={800}>Recommendation</Typography></Stack><Typography variant="body2" sx={{ lineHeight: 1.7 }}>{recommendation}</Typography></Box></>}</Paper></Fade></Grid>;
}

export default function RequestDetailsPage() {
  const { id } = useParams<{ id: string }>();
  const [item, setItem] = useState<Project>();
  const [displayedAnalysis, setDisplayedAnalysis] = useState<Analysis>();
  const [revealedSections, setRevealedSections] = useState(0);
  const [loadingPhase, setLoadingPhase] = useState(0);
  const [error, setError] = useState("");
  const [notice, setNotice] = useState("");
  const [busy, setBusy] = useState<"analyze" | "send" | "">("");

  const load = useCallback(() => {
    setError("");
    api<Project>(`/project-requests/${id}`).then(result => {
      setItem(result);
      setDisplayedAnalysis(result.analysis);
      setRevealedSections(result.analysis ? 9 : 0);
    }).catch(x => setError(x.message));
  }, [id]);

  useEffect(load, [load]);

  useEffect(() => {
    if (busy !== "analyze" || displayedAnalysis) return;
    setLoadingPhase(0);
    const timer = window.setInterval(() => setLoadingPhase(current => Math.min(current + 1, loadingMessages.length - 1)), 850);
    return () => window.clearInterval(timer);
  }, [busy, displayedAnalysis]);

  const sections = useMemo<Array<{ title: string; value: string; wide?: boolean; recommendation?: string }>>(() => displayedAnalysis ? [
    { title: "Project summary", value: displayedAnalysis.projectSummary, wide: true },
    { title: "Functional requirements", value: displayedAnalysis.functionalRequirements },
    { title: "Suggested modules", value: displayedAnalysis.suggestedModules },
    { title: "Technology", value: displayedAnalysis.suggestedTechStack, recommendation: technologyRecommendation(displayedAnalysis) },
    { title: "Estimated cost", value: displayedAnalysis.estimatedCostRange },
    { title: "Recommended team", value: displayedAnalysis.recommendedTeam, wide: true, recommendation: teamRecommendation(displayedAnalysis) },
    { title: "Clarification questions", value: displayedAnalysis.clarificationQuestions },
    { title: "Risks and assumptions", value: displayedAnalysis.risksAndAssumptions },
    { title: "Client reply draft", value: displayedAnalysis.clientReplyDraft, wide: true }
  ] : [], [displayedAnalysis]);

  async function analyze() {
    setBusy("analyze"); setError(""); setNotice("");
    setDisplayedAnalysis(undefined); setRevealedSections(0); setLoadingPhase(0);
    try {
      const [analysis] = await Promise.all([
        api<Analysis>(`/project-requests/${id}/analyze`, { method: "POST" }),
        wait(3200)
      ]);
      setItem(current => current ? { ...current, status: "Analyzed", analysis } : current);
      setDisplayedAnalysis(analysis);
      for (let section = 1; section <= 9; section++) {
        await wait(420);
        setRevealedSections(section);
      }
      setNotice("Local Ollama analysis completed successfully.");
    } catch (x) {
      setError(x instanceof Error ? x.message : "Analysis failed.");
      load();
    } finally {
      setBusy("");
    }
  }

  async function send() {
    setBusy("send"); setError(""); setNotice("");
    try {
      await api(`/project-requests/${id}/send-reply`, { method: "POST" });
      setNotice("Email reply simulated successfully.");
      load();
    } catch (x) {
      setError(x instanceof Error ? x.message : "Could not simulate email.");
    } finally {
      setBusy("");
    }
  }

  return <AppShell role="Admin">
    {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
    {notice && <Alert severity="success" sx={{ mb: 2 }}>{notice}</Alert>}
    {!item ? <Skeleton variant="rounded" height={360} /> : <>
      <Stack direction={{ xs: "column", md: "row" }} justifyContent="space-between" alignItems={{ md: "center" }} gap={2} mb={3}>
        <div><Stack direction="row" gap={1} alignItems="center" flexWrap="wrap"><Typography variant="h4">{item.projectTitle}</Typography><Chip label={item.status} color={item.analysis ? "success" : "default"} /></Stack><Typography color="text.secondary">Submitted by {item.clientName} · {item.clientEmail}</Typography></div>
        <Stack direction={{ xs: "column", sm: "row" }} gap={1}>
          <Button variant="contained" startIcon={busy === "analyze" ? <CircularProgress size={17} color="inherit" /> : <AutoAwesome />} onClick={analyze} disabled={!!busy}>{busy === "analyze" ? "Analyzing project…" : item.analysis ? "Analyze again" : "Analyze with AI"}</Button>
          <Button variant="outlined" startIcon={<EmailOutlined />} onClick={send} disabled={!displayedAnalysis || revealedSections < sections.length || !!busy}>{busy === "send" ? "Sending…" : "Send / simulate email"}</Button>
        </Stack>
      </Stack>

      <Paper elevation={0} sx={{ p: 3, border: "1px solid #e5e9f2", mb: 3 }}><Grid container spacing={2}><Grid size={{ xs: 12, md: 8 }}><Typography variant="overline" color="text.secondary">Project brief</Typography><Typography sx={{ whiteSpace: "pre-line", lineHeight: 1.7 }}>{item.projectDescription}</Typography></Grid><Grid size={{ xs: 12, md: 4 }}><Stack spacing={1}><Typography><b>Industry:</b> {item.industry}</Typography><Typography><b>Budget:</b> {item.budgetRange}</Typography><Typography><b>Deadline:</b> {item.desiredDeadline ? new Date(item.desiredDeadline).toLocaleDateString() : "Flexible"}</Typography><Typography><b>Company:</b> {item.companyName || "—"}</Typography></Stack></Grid></Grid></Paper>

      {busy === "analyze" && !displayedAnalysis ? <Paper elevation={0} sx={{ p: { xs: 3, md: 5 }, textAlign: "center", border: "1px solid #dbe2f1" }}><CircularProgress size={42} thickness={4} /><Typography variant="h6" mt={2}>AI project analyst is working</Typography><Typography color="text.secondary" mt={0.5} mb={3}>{loadingMessages[loadingPhase]}</Typography><LinearProgress variant="determinate" value={(loadingPhase + 1) * 22} sx={{ maxWidth: 520, mx: "auto", height: 7, borderRadius: 4 }} /><Typography variant="caption" color="text.secondary" display="block" mt={1.5}>This normally takes a few seconds.</Typography></Paper>
      : displayedAnalysis ? <>
        <Stack direction={{ xs: "column", sm: "row" }} justifyContent="space-between" gap={2} mb={2}><div><Typography variant="h5">AI analysis</Typography>{busy === "analyze" && <Typography color="text.secondary" variant="body2">Building the response section by section…</Typography>}</div><Stack direction="row" gap={1} flexWrap="wrap"><Chip label={`${displayedAnalysis.complexityLevel} complexity`} color="primary" variant="outlined" /><Chip label={displayedAnalysis.estimatedTimeline} /></Stack></Stack>
        {busy === "analyze" && <LinearProgress sx={{ mb: 2, height: 4, borderRadius: 2 }} />}
        <Grid container spacing={2}>{sections.slice(0, revealedSections).map(section => <AnalysisCard key={section.title} title={section.title} wide={section.wide} recommendation={section.recommendation}>{section.value}</AnalysisCard>)}</Grid>
      </> : <Paper elevation={0} sx={{ p: 5, textAlign: "center", border: "1px dashed #b8c2d8" }}><AutoAwesome color="primary" sx={{ fontSize: 42 }} /><Typography variant="h6" mt={1}>Ready for analysis</Typography><Typography color="text.secondary">Generate requirements, estimates, risks, and a suggested delivery team.</Typography></Paper>}
    </>}
  </AppShell>;
}
