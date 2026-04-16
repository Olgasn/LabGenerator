import { AppSidebar } from "@/components/app-sidebar"
import { JobCenterPanel } from "@/components/job-center-panel"
import { MainContent } from "@/components/main-content"
import { NeuralBackground } from "@/components/neural-background"
import type { Metadata } from "next"
import { Inter } from "next/font/google"
import React from "react"
import "./globals.css"
import Providers from "./providers"

const inter = Inter({
  variable: "--font-inter",
  subsets: ["latin", "cyrillic"],
});

export const metadata: Metadata = {
  title: "Lab Generator",
  description: "Генератор лабораторных заданий",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="ru" suppressHydrationWarning>
      <body className={`${inter.variable} font-sans antialiased`}>
        <Providers>
          <div className="relative flex min-h-dvh bg-background text-foreground">
            <NeuralBackground />
            <AppSidebar />
            <MainContent>{children}</MainContent>
            <JobCenterPanel />
          </div>
        </Providers>
      </body>
    </html>
  );
}
