import Link from "next/link";

import { Button } from "@/components/ui/button";

export function AppHeader() {
  return (
    <header className="border-b bg-background">
      <div className="mx-auto flex h-14 max-w-5xl items-center justify-between px-6">
        <div className="flex items-center gap-3">
          <Link href="/" className="text-sm font-semibold">
            LLM Lab Generator
          </Link>
          <nav className="hidden items-center gap-2 sm:flex">
            <Button asChild variant="ghost" size="sm">
              <Link href="/disciplines">Disciplines</Link>
            </Button>
            <Button asChild variant="ghost" size="sm">
              <Link href="/labs">Labs</Link>
            </Button>
            <Button asChild variant="ghost" size="sm">
              <Link href="/admin/llm">Admin</Link>
            </Button>
          </nav>
        </div>

        <Button asChild variant="outline" size="sm">
          <Link href="/admin/llm">Settings</Link>
        </Button>
      </div>
    </header>
  );
}