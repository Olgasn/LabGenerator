'use client';

import Link from 'next/link';
import {
  BookOpen, FlaskConical, CheckCircle2, Loader2,
  CirclePlus, FilePlus, Sparkles, SlidersHorizontal,
  ArrowRight,
} from 'lucide-react';
import { useLabs } from '@/shared/hooks/useLabs';
import { useDisciplines } from '@/shared/hooks/useDisciplines';
import { PageWrapper, StaggerContainer, StaggerItem, FadeIn } from '@/components/page-wrapper';
import { Badge } from '@/components/ui/badge';
import { motion } from 'framer-motion';

const statsConfig = [
  {
    label: 'Дисциплины',
    icon: BookOpen,
    color: 'text-purple',
    iconBg: 'bg-purple/10',
    gradient: 'from-purple/10 to-transparent',
    glow: 'group-hover:shadow-[0_0_20px_hsl(var(--accent-primary)/0.15)]',
    border: 'group-hover:border-purple/30',
  },
  {
    label: 'Лабораторные',
    icon: FlaskConical,
    color: 'text-indigo',
    iconBg: 'bg-indigo/10',
    gradient: 'from-indigo/10 to-transparent',
    glow: 'group-hover:shadow-[0_0_20px_hsl(var(--accent-secondary)/0.15)]',
    border: 'group-hover:border-indigo/30',
  },
  {
    label: 'Одобрено',
    icon: CheckCircle2,
    color: 'text-success',
    iconBg: 'bg-success/10',
    gradient: 'from-success/10 to-transparent',
    glow: 'group-hover:shadow-[0_0_20px_hsl(var(--accent-success)/0.15)]',
    border: 'group-hover:border-success/30',
  },
  {
    label: 'В процессе',
    icon: Loader2,
    color: 'text-warning',
    iconBg: 'bg-warning/10',
    gradient: 'from-warning/10 to-transparent',
    glow: 'group-hover:shadow-[0_0_20px_hsl(var(--accent-warning)/0.15)]',
    border: 'group-hover:border-warning/30',
  },
];

const quickActions = [
  {
    href: '/disciplines',
    title: 'Создать дисциплину',
    subtitle: 'Добавить новый предмет',
    icon: CirclePlus,
    color: 'text-purple',
    bg: 'bg-purple/10',
    hoverBg: 'hover:bg-purple/15',
  },
  {
    href: '/labs',
    title: 'Создать лабораторную',
    subtitle: 'Сгенерировать задание',
    icon: FilePlus,
    color: 'text-indigo',
    bg: 'bg-indigo/10',
    hoverBg: 'hover:bg-indigo/15',
  },
  {
    href: '/labs',
    title: 'Генерация вариантов',
    subtitle: 'ИИ-генерация заданий',
    icon: Sparkles,
    color: 'text-success',
    bg: 'bg-success/10',
    hoverBg: 'hover:bg-success/15',
  },
  {
    href: '/admin/llm',
    title: 'Настройки LLM',
    subtitle: 'Настроить модели ИИ',
    icon: SlidersHorizontal,
    color: 'text-warning',
    bg: 'bg-warning/10',
    hoverBg: 'hover:bg-warning/15',
  },
];

export default function DashboardPage() {
  const labs = useLabs();
  const disciplines = useDisciplines();

  const labsCount = labs.data?.length ?? 0;
  const disciplinesCount = disciplines.data?.length ?? 0;
  const statsValues = [disciplinesCount, labsCount, 0, 0];

  return (
    <PageWrapper>
      {/* Hero header */}
      <FadeIn className="relative overflow-hidden rounded-2xl border border-[hsl(var(--border-subtle))] bg-surface-card p-7">
        {/* Background gradient */}
        <div className="pointer-events-none absolute inset-0 bg-gradient-to-br from-purple/8 via-transparent to-indigo/5" />
        {/* Decorative orb */}
        <div className="pointer-events-none absolute -top-12 -right-12 h-48 w-48 rounded-full bg-purple/10 blur-3xl" />

        <div className="relative flex items-center gap-3">
          <div className="flex h-9 w-9 items-center justify-center rounded-xl bg-purple/15 flex-shrink-0">
            <Sparkles className="h-4 w-4 text-purple" />
          </div>
          <div>
            <h1 className="text-[28px] font-bold leading-tight">
              <span className="gradient-text-white">Lab Generator</span>
            </h1>
            <p className="text-sm text-fg-muted">
              Автоматическая генерация вариантов лабораторных работ с помощью ИИ
            </p>
          </div>
        </div>
      </FadeIn>

      {/* Stat cards */}
      <StaggerContainer className="grid grid-cols-4 gap-4">
        {statsConfig.map((stat, i) => (
          <StaggerItem key={stat.label}>
            <div
              className={`group relative overflow-hidden rounded-2xl border border-[hsl(var(--border-subtle))] bg-surface-card p-5 transition-all duration-300 ${stat.border} ${stat.glow}`}
            >
              {/* Card gradient overlay */}
              <div className={`pointer-events-none absolute inset-0 bg-gradient-to-br ${stat.gradient} opacity-0 group-hover:opacity-100 transition-opacity duration-300`} />

              <div className="relative">
                <div className={`flex h-10 w-10 items-center justify-center rounded-xl ${stat.iconBg} transition-transform duration-300 group-hover:scale-110`}>
                  <stat.icon className={`h-5 w-5 ${stat.color}`} />
                </div>
              </div>

              <motion.div
                key={statsValues[i]}
                initial={{ opacity: 0, y: 6 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ duration: 0.4, delay: i * 0.05 }}
                className="relative mt-4 text-3xl font-bold text-fg-primary tabular-nums"
              >
                {statsValues[i]}
              </motion.div>
              <div className="relative mt-1 text-[13px] text-fg-muted">{stat.label}</div>
            </div>
          </StaggerItem>
        ))}
      </StaggerContainer>

      <div className="flex gap-5">
        {/* Recent labs */}
        <FadeIn delay={0.15} className="flex-1 overflow-hidden rounded-2xl border border-[hsl(var(--border-subtle))] bg-surface-card">
          <div className="flex items-center justify-between border-b border-[hsl(var(--border-subtle))] px-5 py-4">
            <h2 className="text-sm font-semibold text-fg-primary">Последние лабораторные</h2>
            <Link
              href="/labs"
              className="group flex items-center gap-1 text-xs font-medium text-purple hover:text-purple-hover transition-colors"
            >
              Все
              <ArrowRight className="h-3 w-3 transition-transform group-hover:translate-x-0.5" />
            </Link>
          </div>

          <div className="flex items-center bg-surface-tertiary/60 px-5 py-2.5 text-[11px] font-medium uppercase tracking-wider text-fg-muted">
            <div className="w-[300px]">Название</div>
            <div className="flex-1">Дисциплина</div>
            <div className="w-[100px] text-right">Статус</div>
          </div>

          <div className="divide-y divide-[hsl(var(--border-subtle))]">
            {labs.isLoading ? (
              <div className="space-y-1 p-3">
                {[...Array(3)].map((_, k) => (
                  <div key={k} className="h-10 rounded-lg skeleton" />
                ))}
              </div>
            ) : (labs.data ?? []).length === 0 ? (
              <div className="px-5 py-10 text-center">
                <FlaskConical className="mx-auto h-8 w-8 text-fg-muted/40 mb-2" />
                <p className="text-sm text-fg-muted">Нет лабораторных работ</p>
              </div>
            ) : (
              (labs.data ?? []).slice(0, 5).map((lab, idx) => {
                const disc = (disciplines.data ?? []).find((d) => d.id === lab.disciplineId);
                return (
                  <motion.div
                    key={lab.id}
                    initial={{ opacity: 0, x: -8 }}
                    animate={{ opacity: 1, x: 0 }}
                    transition={{ delay: 0.1 + idx * 0.04, duration: 0.3 }}
                  >
                    <Link
                      href={`/labs/${lab.id}`}
                      className="group flex items-center px-5 py-3.5 transition-all duration-200 hover:bg-surface-hover/60"
                    >
                      <div className="w-[300px] truncate text-sm font-medium text-fg-primary group-hover:text-purple transition-colors duration-200">
                        {lab.title}
                      </div>
                      <div className="flex-1 truncate text-sm text-fg-secondary">{disc?.name ?? '—'}</div>
                      <div className="w-[100px] flex justify-end">
                        <Badge
                          variant="outline"
                          className="border-purple/25 bg-purple/5 text-purple text-[11px] font-medium"
                        >
                          Готово
                        </Badge>
                      </div>
                    </Link>
                  </motion.div>
                );
              })
            )}
          </div>
        </FadeIn>

        {/* Quick actions */}
        <FadeIn delay={0.2} className="w-[280px] shrink-0 overflow-hidden rounded-2xl border border-[hsl(var(--border-subtle))] bg-surface-card">
          <div className="border-b border-[hsl(var(--border-subtle))] px-5 py-4">
            <h2 className="text-sm font-semibold text-fg-primary">Быстрые действия</h2>
          </div>
          <div className="flex flex-col gap-0.5 p-2">
            {quickActions.map((action, idx) => (
              <motion.div
                key={action.title}
                initial={{ opacity: 0, x: 8 }}
                animate={{ opacity: 1, x: 0 }}
                transition={{ delay: 0.25 + idx * 0.06, duration: 0.3 }}
              >
                <Link
                  href={action.href}
                  className={`group flex items-center gap-3 rounded-xl p-3 transition-all duration-200 ${action.hoverBg}`}
                >
                  <div
                    className={`flex h-9 w-9 shrink-0 items-center justify-center rounded-lg ${action.bg} transition-transform duration-200 group-hover:scale-110`}
                  >
                    <action.icon className={`h-[17px] w-[17px] ${action.color}`} />
                  </div>
                  <div className="min-w-0 flex-1">
                    <div className="text-[13px] font-medium text-fg-primary leading-tight">
                      {action.title}
                    </div>
                    <div className="text-[11px] text-fg-muted leading-tight mt-0.5">
                      {action.subtitle}
                    </div>
                  </div>
                  <ArrowRight className="h-3.5 w-3.5 text-fg-muted opacity-0 group-hover:opacity-100 transition-all duration-200 group-hover:translate-x-0.5" />
                </Link>
              </motion.div>
            ))}
          </div>
        </FadeIn>
      </div>
    </PageWrapper>
  );
}
