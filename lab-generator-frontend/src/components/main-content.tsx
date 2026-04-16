'use client';

import { motion } from 'framer-motion';
import { ReactNode } from 'react';
import { useSidebar } from '@/shared/sidebar-context';

export function MainContent({ children }: { children: ReactNode }) {
  const { isCollapsed } = useSidebar();

  return (
    <motion.main
      animate={{ marginLeft: isCollapsed ? 56 : 240 }}
      transition={{ duration: 0.25, ease: [0.25, 0.46, 0.45, 0.94] }}
      className="relative z-10 flex-1 overflow-x-hidden"
    >
      {children}
    </motion.main>
  );
}
