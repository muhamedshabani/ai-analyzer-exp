"use client";
import { CssBaseline, ThemeProvider, createTheme } from "@mui/material";
import { ReactNode } from "react";

const theme = createTheme({
  palette: {
    primary: { main: "#3157d5" },
    secondary: { main: "#15a18b" },
    background: { default: "#f5f7fb" }
  },
  shape: { borderRadius: 12 },
  typography: {
    fontFamily: "Inter, ui-sans-serif, system-ui, sans-serif",
    h4: { fontWeight: 750 },
    h5: { fontWeight: 700 }
  },
  components: {
    MuiButton: {
      defaultProps: { disableElevation: true },
      styleOverrides: { root: { textTransform: "none", fontWeight: 650 } }
    },
    MuiPaper: { styleOverrides: { root: { backgroundImage: "none" } } },
    MuiOutlinedInput: {
      styleOverrides: {
        root: {
          backgroundColor: "#fff",
          "&:hover .MuiOutlinedInput-notchedOutline": { borderColor: "#7f8ca3" },
          "&.Mui-focused .MuiOutlinedInput-notchedOutline": { borderWidth: 2 }
        },
        notchedOutline: { borderColor: "#c8d0df" }
      }
    },
    MuiInputLabel: {
      styleOverrides: {
        root: {
          maxWidth: "calc(100% - 28px)",
          "&.MuiInputLabel-shrink": {
            backgroundColor: "#fff",
            borderRadius: 4,
            marginLeft: -2,
            paddingLeft: 6,
            paddingRight: 6,
            zIndex: 1
          }
        }
      }
    },
    MuiFormHelperText: {
      styleOverrides: { root: { marginLeft: 2, marginRight: 2 } }
    }
  }
});
export default function Providers({ children }: { children: ReactNode }) { return <ThemeProvider theme={theme}><CssBaseline />{children}</ThemeProvider>; }
