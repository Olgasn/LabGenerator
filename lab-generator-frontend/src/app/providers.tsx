'use client';

import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useState } from 'react';
import { JobCenterProvider } from '@/shared/job-center/job-center-context';
import { SidebarProvider } from '@/shared/sidebar-context';
import { ThemeProvider } from '@/shared/theme-context';

export default function Providers({ children }: { children: React.ReactNode }) {
  const [queryClient] = useState(
    () =>
      new QueryClient({
        defaultOptions: {
          queries: {
            retry: 1,
            refetchOnWindowFocus: false,
          },
          mutations: {
            retry: 0,
          },
        },
      })
  );

  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider>
        <SidebarProvider>
          <JobCenterProvider>{children}</JobCenterProvider>
        </SidebarProvider>
      </ThemeProvider>
    </QueryClientProvider>
  );
}
