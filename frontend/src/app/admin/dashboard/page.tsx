"use client";

import AppShell from "@/components/AppShell";
import { api } from "@/lib/api";
import { AssessmentOutlined, GroupsOutlined, HourglassTopOutlined, TopicOutlined } from "@mui/icons-material";
import { Alert, Grid2 as Grid, Paper, Skeleton, Stack, Typography } from "@mui/material";
import { useEffect, useState } from "react";

type Stats = { totalRequests: number; analyzedRequests: number; pendingRequests: number; availableEmployees: number };

export default function DashboardPage() {
  const [stats, setStats] = useState<Stats>();
  const [error, setError] = useState("");
  useEffect(() => { api<Stats>("/dashboard").then(setStats).catch(x => setError(x.message)); }, []);
  const cards = [
    ["Total requests", stats?.totalRequests, TopicOutlined, "#3157d5"],
    ["Analyzed", stats?.analyzedRequests, AssessmentOutlined, "#158f7a"],
    ["Pending review", stats?.pendingRequests, HourglassTopOutlined, "#c47b16"],
    ["Available employees", stats?.availableEmployees, GroupsOutlined, "#7857c8"]
  ] as const;
  return <AppShell role="Admin"><Typography variant="h4">Dashboard</Typography><Typography color="text.secondary" mb={4}>Project intake activity and current team capacity.</Typography>{error && <Alert severity="error" sx={{ mb: 3 }}>{error}</Alert>}<Grid container spacing={2}>{cards.map(([label, value, Icon, color]) => <Grid size={{ xs: 12, sm: 6, md: 3 }} key={label}><Paper elevation={0} sx={{ p: 3, border: "1px solid #e5e9f2", height: "100%" }}><Stack direction="row" justifyContent="space-between" alignItems="flex-start"><div><Typography color="text.secondary" variant="body2">{label}</Typography>{value === undefined ? <Skeleton width={70} height={58} /> : <Typography variant="h3" fontWeight={800} mt={1}>{value}</Typography>}</div><Icon sx={{ color, bgcolor: `${color}14`, p: 1, borderRadius: 2, fontSize: 42 }} /></Stack></Paper></Grid>)}</Grid></AppShell>;
}
