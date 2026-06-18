"use client";
import { CssBaseline, ThemeProvider, createTheme } from "@mui/material";
import { ReactNode } from "react";

const theme = createTheme({ palette: { primary: { main: "#3157d5" }, secondary: { main: "#15a18b" }, background: { default: "#f5f7fb" } }, shape: { borderRadius: 12 }, typography: { fontFamily: "Inter, ui-sans-serif, system-ui, sans-serif", h4: { fontWeight: 750 }, h5: { fontWeight: 700 } }, components: { MuiButton: { defaultProps: { disableElevation: true }, styleOverrides: { root: { textTransform: "none", fontWeight: 650 } } }, MuiPaper: { styleOverrides: { root: { backgroundImage: "none" } } } } });
export default function Providers({ children }: { children: ReactNode }) { return <ThemeProvider theme={theme}><CssBaseline />{children}</ThemeProvider>; }
