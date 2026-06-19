"use client";

import { getSession, Session } from "@/lib/api";
import { Box, CircularProgress } from "@mui/material";
import { useRouter } from "next/navigation";
import { ReactNode, useEffect, useState } from "react";

export default function ProtectedRoute({ children, role }: { children: ReactNode; role: "Admin" | "Client" }) {
  const router = useRouter();
  const [session, setSession] = useState<Session | null>();

  useEffect(() => {
    const current = getSession();
    if (!current) { router.replace("/login"); return; }
    if (!current.roles.includes(role)) {
      router.replace(current.roles.includes("Admin") ? "/admin/dashboard" : "/client/my-requests");
      return;
    }
    setSession(current);
  }, [role, router]);

  if (!session) return <Box sx={{ minHeight: "70vh", display: "grid", placeItems: "center" }}><CircularProgress /></Box>;
  return children;
}
