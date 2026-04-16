'use client';

import { cn } from '@/lib/utils'
import { useSidebar } from '@/shared/sidebar-context'
import { useTheme } from '@/shared/theme-context'
import { AnimatePresence, motion } from 'framer-motion'
import {
  BookOpen,
  ChevronLeft, ChevronRight,
  FlaskConical,
  LayoutDashboard,
  Moon,
  Settings, Sparkles,
  Sun,
} from 'lucide-react'
import Link from 'next/link'
import { usePathname } from 'next/navigation'

const navItems = [
  { href: '/', label: 'Главная', icon: LayoutDashboard },
  { href: '/disciplines', label: 'Дисциплины', icon: BookOpen },
  { href: '/labs', label: 'Лабораторные', icon: FlaskConical },
  { href: '/admin/llm', label: 'Настройки', icon: Settings },
];

export function AppSidebar() {
  const pathname = usePathname();
  const { isCollapsed, toggle } = useSidebar();
  const { theme, toggle: toggleTheme } = useTheme();

  return (
    <motion.aside
      animate={{ width: isCollapsed ? 56 : 240 }}
      transition={{ duration: 0.25, ease: [0.25, 0.46, 0.45, 0.94] }}
      className="fixed left-0 top-0 z-30 flex h-dvh flex-col border-r border-[hsl(var(--border-subtle))] bg-surface-secondary overflow-hidden"
    >
      <div className="pointer-events-none absolute -top-20 -left-10 h-40 w-40 rounded-full bg-purple/10 blur-3xl" />
      <div className="pointer-events-none absolute bottom-20 -right-10 h-32 w-32 rounded-full bg-indigo/[0.08] blur-3xl" />

      <div className="relative flex flex-col gap-2 px-3 pb-4 pt-4 flex-1 overflow-hidden">
        <div className={cn(
          'flex items-center pb-5 transition-all duration-200',
          isCollapsed ? 'justify-center px-0' : 'gap-2.5 px-1',
        )}>
          <div className="relative flex h-8 w-8 flex-shrink-0 items-center justify-center rounded-lg overflow-hidden">
            <div className="absolute inset-0 bg-gradient-to-br from-purple to-indigo opacity-90" />
            <div className="absolute inset-0 bg-gradient-to-r from-transparent via-white/20 to-transparent animate-shimmer" />
            <Sparkles className="relative z-10 h-4 w-4 text-white drop-shadow" />
          </div>
          <AnimatePresence>
            {!isCollapsed && (
              <motion.span
                initial={{ opacity: 0, width: 0 }}
                animate={{ opacity: 1, width: 'auto' }}
                exit={{ opacity: 0, width: 0 }}
                transition={{ duration: 0.2 }}
                className="text-sm font-bold text-fg-primary leading-tight tracking-tight whitespace-nowrap overflow-hidden"
              >
                Lab Generator
              </motion.span>
            )}
          </AnimatePresence>
        </div>

        <AnimatePresence>
          {!isCollapsed && (
            <motion.div
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
              transition={{ duration: 0.15 }}
              className="px-2 pb-1"
            >
              <span className="font-mono text-[10px] font-medium uppercase tracking-wider text-fg-muted">
                Навигация
              </span>
            </motion.div>
          )}
        </AnimatePresence>

        <nav className="flex flex-col gap-0.5">
          {navItems.map((item) => {
            const isActive =
              item.href === '/'
                ? pathname === '/'
                : pathname.startsWith(item.href);

            return (
              <div key={item.href} className="relative group/tooltip">
                <Link
                  href={item.href}
                  className={cn(
                    'group relative flex items-center rounded-lg text-sm font-medium transition-all duration-200',
                    isCollapsed ? 'h-10 w-10 justify-center mx-auto' : 'gap-2.5 px-3 py-2.5',
                    isActive ? 'text-fg-primary' : 'text-fg-secondary hover:text-fg-primary',
                  )}
                >
                  {isActive && (
                    <motion.div
                      layoutId="sidebar-active-bg"
                      className="absolute inset-0 rounded-lg bg-surface-hover"
                      initial={false}
                      transition={{ type: 'spring', stiffness: 400, damping: 35 }}
                    />
                  )}
                  {!isActive && (
                    <div className="absolute inset-0 rounded-lg bg-surface-hover/0 group-hover:bg-surface-hover/50 transition-colors duration-200" />
                  )}
                  <AnimatePresence>
                    {isActive && !isCollapsed && (
                      <motion.div
                        layoutId="sidebar-active-bar"
                        className="absolute left-0 top-1/2 h-4 w-0.5 -translate-y-1/2 rounded-full bg-purple shadow-[0_0_8px_hsl(var(--accent-primary)/0.8)]"
                        initial={{ opacity: 0, scaleY: 0 }}
                        animate={{ opacity: 1, scaleY: 1 }}
                        exit={{ opacity: 0, scaleY: 0 }}
                        transition={{ duration: 0.2 }}
                      />
                    )}
                  </AnimatePresence>

                  <item.icon
                    className={cn(
                      'relative z-10 h-[18px] w-[18px] flex-shrink-0 transition-colors duration-200',
                      isActive
                        ? 'text-purple drop-shadow-[0_0_6px_hsl(var(--accent-primary)/0.6)]'
                        : 'text-fg-muted group-hover:text-fg-secondary',
                    )}
                  />
                  <AnimatePresence>
                    {!isCollapsed && (
                      <motion.span
                        initial={{ opacity: 0 }}
                        animate={{ opacity: 1 }}
                        exit={{ opacity: 0 }}
                        transition={{ duration: 0.15 }}
                        className="relative z-10 whitespace-nowrap"
                      >
                        {item.label}
                      </motion.span>
                    )}
                  </AnimatePresence>
                </Link>

                {isCollapsed && (
                  <div className="pointer-events-none absolute left-full top-1/2 -translate-y-1/2 ml-3 z-50 opacity-0 group-hover/tooltip:opacity-100 transition-opacity duration-150">
                    <div className="rounded-md border border-[hsl(var(--border-default))] bg-surface-card px-2.5 py-1.5 shadow-lg">
                      <span className="text-xs font-medium text-fg-primary whitespace-nowrap">{item.label}</span>
                    </div>
                  </div>
                )}
              </div>
            );
          })}
        </nav>
      </div>

      <div className={cn(
        'relative shrink-0 border-t border-[hsl(var(--border-subtle))] py-3',
        isCollapsed ? 'flex flex-col items-center gap-1 px-0' : 'flex items-center justify-between px-3',
      )}>
        <button
          onClick={toggleTheme}
          className="flex h-8 w-8 items-center justify-center rounded-lg text-fg-muted hover:bg-surface-hover hover:text-fg-secondary transition-all duration-200"
          title={theme === 'dark' ? 'Светлая тема' : 'Тёмная тема'}
        >
          {theme === 'dark'
            ? <Sun className="h-4 w-4" />
            : <Moon className="h-4 w-4" />
          }
        </button>

        <button
          onClick={toggle}
          className="flex h-8 w-8 items-center justify-center rounded-lg text-fg-muted hover:bg-surface-hover hover:text-fg-secondary transition-all duration-200"
          title={isCollapsed ? 'Развернуть' : 'Свернуть'}
        >
          {isCollapsed
            ? <ChevronRight className="h-4 w-4" />
            : <ChevronLeft className="h-4 w-4" />
          }
        </button>
      </div>
    </motion.aside>
  );
}
