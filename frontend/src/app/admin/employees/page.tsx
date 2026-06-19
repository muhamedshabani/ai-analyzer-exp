"use client";

import AppShell from "@/components/AppShell";
import { api } from "@/lib/api";
import { Employee } from "@/lib/types";
import { Add, CheckCircleOutline } from "@mui/icons-material";
import { Alert, Button, Chip, Paper, Skeleton, Stack, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Typography } from "@mui/material";
import Link from "next/link";
import { useEffect, useState } from "react";

export default function EmployeesPage() {
  const [items, setItems] = useState<Employee[]>(); const [error, setError] = useState("");
  useEffect(() => { api<Employee[]>("/employees").then(setItems).catch(x => setError(x.message)); }, []);
  return <AppShell role="Admin"><Stack direction={{ xs: "column", sm: "row" }} justifyContent="space-between" alignItems={{ sm: "center" }} mb={3} gap={2}><div><Typography variant="h4">Employees</Typography><Typography color="text.secondary">Manage skills, rates, and delivery capacity.</Typography></div><Button component={Link} href="/admin/employees/create" variant="contained" startIcon={<Add />}>Add employee</Button></Stack>{error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}{!items ? <Skeleton variant="rounded" height={280} /> : <TableContainer component={Paper} elevation={0} sx={{ border: "1px solid #e5e9f2" }}><Table><TableHead><TableRow><TableCell>Name</TableCell><TableCell>Position</TableCell><TableCell>Seniority</TableCell><TableCell>Tech stack</TableCell><TableCell>Rate</TableCell><TableCell>Availability</TableCell></TableRow></TableHead><TableBody>{items.map(x => <TableRow key={x.id} hover><TableCell><Typography fontWeight={700}>{x.fullName}</Typography></TableCell><TableCell>{x.position}</TableCell><TableCell><Chip size="small" label={x.seniorityLevel} /></TableCell><TableCell>{x.mainTechStack}</TableCell><TableCell>€{x.hourlyRate}/h</TableCell><TableCell>{x.isAvailable ? <Stack direction="row" gap={1} alignItems="center"><CheckCircleOutline color="success" fontSize="small" />{x.weeklyAvailableHours}h/week</Stack> : <Chip size="small" label="Unavailable" />}</TableCell></TableRow>)}</TableBody></Table></TableContainer>}</AppShell>;
}
