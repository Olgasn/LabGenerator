'use client';

import Link from 'next/link';
import { Suspense, useEffect, useMemo, useState } from 'react';
import { motion } from 'framer-motion';
import { ArrowRight, ChevronLeft, ChevronRight, FlaskConical, Plus, Search, Trash2 } from 'lucide-react';
import { usePathname, useRouter, useSearchParams } from 'next/navigation';

import { ConfirmDialog } from '@/components/confirm-dialog';
import { PageWrapper } from '@/components/page-wrapper';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Textarea } from '@/components/ui/textarea';
import { useDisciplines } from '@/shared/hooks/useDisciplines';
import { type LabsSortOption, useCreateLab, useDeleteLab, usePagedLabs } from '@/shared/hooks/useLabs';
import { formatAxiosError } from '@/shared/utils/formatAxiosError';

const PAGE_SIZE_OPTIONS = ['5', '10', '25'] as const;
const ALL_DISCIPLINES_VALUE = 'all';

type DeleteTarget = {
  id: number;
  title: string;
} | null;

function readDisciplineFilter(searchParams: Pick<URLSearchParams, 'get'>): string {
  const rawValue = searchParams.get('disciplineId');
  if (!rawValue) {
    return ALL_DISCIPLINES_VALUE;
  }

  const parsed = Number(rawValue);
  return Number.isInteger(parsed) && parsed > 0 ? String(parsed) : ALL_DISCIPLINES_VALUE;
}

function LabsPageContent() {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();

  const disciplines = useDisciplines();
  const create = useCreateLab();
  const deleteLab = useDeleteLab();

  const [disciplineId, setDisciplineId] = useState<number | ''>('');
  const [orderIndex, setOrderIndex] = useState('1');
  const [title, setTitle] = useState('');
  const [initialDescription, setInitialDescription] = useState('');
  const [search, setSearch] = useState('');
  const [sortBy, setSortBy] = useState<LabsSortOption>('desc');
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState<(typeof PAGE_SIZE_OPTIONS)[number]>('10');
  const [open, setOpen] = useState(false);
  const [disciplineFilter, setDisciplineFilter] = useState(() => readDisciplineFilter(searchParams));
  const [deleteError, setDeleteError] = useState<string | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<DeleteTarget>(null);

  useEffect(() => {
    setDisciplineFilter(readDisciplineFilter(searchParams));
  }, [searchParams]);

  const labs = usePagedLabs({
    disciplineId: disciplineFilter === ALL_DISCIPLINES_VALUE ? undefined : Number(disciplineFilter),
    search: search.trim() || undefined,
    sort: sortBy,
    page,
    pageSize: Number(pageSize),
  });

  const createDisabled = useMemo(() => {
    const parsedOrderIndex = Number(orderIndex);
    return (
      create.isPending ||
      disciplineId === '' ||
      !Number.isFinite(parsedOrderIndex) ||
      parsedOrderIndex < 0 ||
      title.trim().length === 0 ||
      initialDescription.trim().length === 0
    );
  }, [create.isPending, disciplineId, initialDescription, orderIndex, title]);

  const items = labs.data?.items ?? [];
  const totalCount = labs.data?.totalCount ?? 0;
  const currentPage = labs.data?.page ?? page;
  const totalPages = labs.data?.totalPages ?? 1;
  const paginationStart = totalCount === 0 ? 0 : (currentPage - 1) * Number(pageSize) + 1;
  const paginationEnd = totalCount === 0 ? 0 : Math.min(currentPage * Number(pageSize), totalCount);

  const pageNumbers = useMemo(() => {
    if (totalPages <= 5) {
      return Array.from({ length: totalPages }, (_, index) => index + 1);
    }

    if (currentPage <= 3) {
      return [1, 2, 3, 4, totalPages];
    }

    if (currentPage >= totalPages - 2) {
      return [1, totalPages - 3, totalPages - 2, totalPages - 1, totalPages];
    }

    return [1, currentPage - 1, currentPage, currentPage + 1, totalPages];
  }, [currentPage, totalPages]);

  const getDisciplineName = (id: number) =>
    (disciplines.data ?? []).find((discipline) => discipline.id === id)?.name ?? '—';

  const selectedDisciplineName =
    disciplineFilter === ALL_DISCIPLINES_VALUE
      ? 'Все дисциплины'
      : getDisciplineName(Number(disciplineFilter));

  function updateDisciplineFilter(value: string) {
    setDisciplineFilter(value);
    setPage(1);

    const params = new URLSearchParams(searchParams.toString());
    if (value === ALL_DISCIPLINES_VALUE) {
      params.delete('disciplineId');
    } else {
      params.set('disciplineId', value);
    }

    const nextUrl = params.toString() ? `${pathname}?${params.toString()}` : pathname;
    router.replace(nextUrl);
  }

  async function confirmDeleteLab() {
    if (!deleteTarget) {
      return;
    }

    setDeleteError(null);

    try {
      await deleteLab.mutateAsync(deleteTarget.id);
      setDeleteTarget(null);
    } catch (error) {
      setDeleteError(formatAxiosError(error).message);
    }
  }

  return (
    <PageWrapper>
      <div className="flex items-start justify-between">
        <div>
          <h1 className="text-[28px] font-bold text-fg-primary">Лабораторные работы</h1>
          <p className="mt-1 text-sm text-fg-muted">
            Создание, фильтрация и удаление лабораторных работ по дисциплинам.
          </p>
        </div>

        <Dialog open={open} onOpenChange={setOpen}>
          <DialogTrigger asChild>
            <Button className="gap-2 bg-purple text-fg-inverse shadow-[0_4px_14px_hsl(var(--accent-primary)/0.3)] transition-all duration-200 hover:bg-purple-hover hover:shadow-[0_4px_20px_hsl(var(--accent-primary)/0.4)]">
              <Plus className="h-4 w-4" />
              Создать лабораторную
            </Button>
          </DialogTrigger>
          <DialogContent className="border-[hsl(var(--border-default))] bg-surface-card sm:max-w-lg">
            <DialogHeader>
              <DialogTitle>Новая лабораторная</DialogTitle>
            </DialogHeader>

            <div className="grid gap-4 pt-2">
              <Select
                value={disciplineId === '' ? '' : String(disciplineId)}
                onValueChange={(value) => setDisciplineId(value ? Number(value) : '')}
              >
                <SelectTrigger className="border-[hsl(var(--border-default))] bg-surface-tertiary">
                  <SelectValue placeholder="Выберите дисциплину..." />
                </SelectTrigger>
                <SelectContent>
                  {(disciplines.data ?? []).map((discipline) => (
                    <SelectItem key={discipline.id} value={String(discipline.id)}>
                      {discipline.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              <Input
                placeholder="Порядковый номер"
                type="number"
                value={orderIndex}
                onChange={(event) => setOrderIndex(event.target.value)}
                className="border-[hsl(var(--border-default))] bg-surface-tertiary transition-colors focus:border-purple/50"
              />

              <Input
                placeholder="Название лабораторной"
                value={title}
                onChange={(event) => setTitle(event.target.value)}
                className="border-[hsl(var(--border-default))] bg-surface-tertiary transition-colors focus:border-purple/50"
              />

              <Textarea
                placeholder="Краткое описание из программы"
                value={initialDescription}
                onChange={(event) => setInitialDescription(event.target.value)}
                rows={4}
                className="resize-none border-[hsl(var(--border-default))] bg-surface-tertiary transition-colors focus:border-purple/50"
              />

              <Button
                disabled={createDisabled}
                className="bg-purple text-fg-inverse hover:bg-purple-hover"
                onClick={async () => {
                  await create.mutateAsync({
                    disciplineId: Number(disciplineId),
                    orderIndex: Number(orderIndex),
                    title: title.trim(),
                    initialDescription: initialDescription.trim(),
                  });

                  setDisciplineId('');
                  setOrderIndex('1');
                  setTitle('');
                  setInitialDescription('');
                  setPage(1);
                  setOpen(false);
                }}
              >
                {create.isPending ? 'Создание...' : 'Создать'}
              </Button>

              {create.error ? (
                <p className="text-sm text-destructive">{formatAxiosError(create.error).message}</p>
              ) : null}
            </div>
          </DialogContent>
        </Dialog>
      </div>

      <div className="flex flex-col gap-3 lg:flex-row lg:items-center">
        <div className="relative flex-1">
          <Search className="absolute left-3.5 top-1/2 h-4 w-4 -translate-y-1/2 text-fg-muted" />
          <Input
            placeholder="Поиск лабораторных..."
            value={search}
            onChange={(event) => {
              setSearch(event.target.value);
              setPage(1);
            }}
            className="border-[hsl(var(--border-default))] bg-surface-tertiary pl-10 transition-colors focus:border-purple/40"
          />
        </div>

        <div className="grid gap-3 sm:grid-cols-3 lg:w-[720px]">
          <Select value={disciplineFilter} onValueChange={(value) => updateDisciplineFilter(value)}>
            <SelectTrigger className="border-[hsl(var(--border-default))] bg-surface-tertiary">
              <SelectValue placeholder="Фильтр по дисциплине" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value={ALL_DISCIPLINES_VALUE}>Все дисциплины</SelectItem>
              {(disciplines.data ?? []).map((discipline) => (
                <SelectItem key={discipline.id} value={String(discipline.id)}>
                  {discipline.name}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>

          <Select
            value={sortBy}
            onValueChange={(value) => {
              setSortBy(value as LabsSortOption);
              setPage(1);
            }}
          >
            <SelectTrigger className="border-[hsl(var(--border-default))] bg-surface-tertiary">
              <SelectValue placeholder="Сортировка" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="desc">По убыванию</SelectItem>
              <SelectItem value="asc">По возрастанию</SelectItem>
            </SelectContent>
          </Select>

          <Select
            value={pageSize}
            onValueChange={(value) => {
              setPageSize(value as (typeof PAGE_SIZE_OPTIONS)[number]);
              setPage(1);
            }}
          >
            <SelectTrigger className="border-[hsl(var(--border-default))] bg-surface-tertiary">
              <SelectValue placeholder="На странице" />
            </SelectTrigger>
            <SelectContent>
              {PAGE_SIZE_OPTIONS.map((size) => (
                <SelectItem key={size} value={size}>
                  {size} на странице
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      </div>

      <div className="flex flex-col gap-2 text-sm text-fg-muted sm:flex-row sm:items-center sm:justify-between">
        <p>
          {totalCount === 0
            ? 'Список пуст'
            : `Показаны ${paginationStart}-${paginationEnd} из ${totalCount}`}
        </p>
        <p>Текущий фильтр: {selectedDisciplineName}.</p>
      </div>

      {deleteError ? (
        <div className="rounded-xl border border-destructive/20 bg-destructive/5 px-4 py-3 text-sm text-destructive">
          {deleteError}
        </div>
      ) : null}

      <div className="overflow-hidden rounded-2xl border border-[hsl(var(--border-subtle))] bg-surface-card">
        <div className="flex items-center bg-surface-tertiary/60 px-5 py-3 text-[11px] font-medium uppercase tracking-wider text-fg-muted">
          <div className="w-[48px]">#</div>
          <div className="w-[320px]">Название</div>
          <div className="flex-1">Дисциплина</div>
          <div className="w-[110px]">Вариантов</div>
          <div className="w-[180px] text-right">Действия</div>
        </div>

        {labs.isLoading ? (
          <div className="space-y-px p-3">
            {[...Array(5)].map((_, index) => (
              <div key={index} className="skeleton h-12 rounded-lg" />
            ))}
          </div>
        ) : totalCount === 0 ? (
          <motion.div
            initial={{ opacity: 0, y: 8 }}
            animate={{ opacity: 1, y: 0 }}
            className="flex flex-col items-center justify-center gap-3 py-16"
          >
            <div className="flex h-14 w-14 items-center justify-center rounded-2xl bg-surface-tertiary">
              <FlaskConical className="h-6 w-6 text-fg-muted/50" />
            </div>
            <p className="text-sm text-fg-muted">
              {search.trim() || disciplineFilter !== ALL_DISCIPLINES_VALUE
                ? 'По текущему фильтру ничего не найдено'
                : 'Нет лабораторных работ'}
            </p>
          </motion.div>
        ) : (
          <div className="divide-y divide-[hsl(var(--border-subtle))]">
            {items.map((lab, index) => {
              const isDeleting = deleteLab.isPending && deleteTarget?.id === lab.id;

              return (
                <motion.div
                  key={lab.id}
                  initial={{ opacity: 0, y: 6 }}
                  animate={{ opacity: 1, y: 0 }}
                  transition={{ delay: index * 0.035, duration: 0.28 }}
                  className="group flex items-center px-5 py-3.5 transition-all duration-200 hover:bg-surface-hover/60"
                >
                  <div className="w-[48px] text-sm font-mono text-fg-muted">{lab.orderIndex ?? lab.id}</div>
                  <div className="w-[320px] min-w-0">
                    <Link
                      href={`/labs/${lab.id}`}
                      className="block truncate text-sm font-medium text-fg-primary transition-colors duration-200 hover:text-purple"
                    >
                      {lab.title}
                    </Link>
                  </div>
                  <div className="flex-1 truncate text-sm text-fg-secondary">
                    {getDisciplineName(lab.disciplineId)}
                  </div>
                  <div className="w-[110px] text-sm font-mono text-fg-muted">—</div>
                  <div className="flex w-[180px] items-center justify-end gap-2">
                    <Badge
                      variant="outline"
                      className="border-purple/25 bg-purple/5 text-[11px] font-medium text-purple"
                    >
                      Готово
                    </Badge>
                    <Button asChild variant="ghost" size="icon" className="text-fg-muted hover:text-purple">
                      <Link href={`/labs/${lab.id}`} aria-label={`Открыть лабораторную ${lab.title}`}>
                        <ArrowRight className="h-3.5 w-3.5 transition-transform duration-200 group-hover:translate-x-0.5" />
                      </Link>
                    </Button>
                    <Button
                      type="button"
                      variant="ghost"
                      size="icon"
                      disabled={isDeleting}
                      className="text-fg-muted hover:bg-destructive/10 hover:text-destructive"
                      onClick={() => setDeleteTarget({ id: lab.id, title: lab.title })}
                    >
                      <Trash2 className="h-4 w-4" />
                    </Button>
                  </div>
                </motion.div>
              );
            })}
          </div>
        )}
      </div>

      {totalCount > 0 ? (
        <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
          <div className="text-sm text-fg-muted">
            Страница {currentPage} из {totalPages}
          </div>

          <div className="flex flex-wrap items-center justify-end gap-2">
            <Button
              variant="outline"
              size="sm"
              disabled={currentPage <= 1}
              onClick={() => setPage((previous) => Math.max(1, previous - 1))}
              className="border-[hsl(var(--border-default))] bg-surface-card"
            >
              <ChevronLeft className="h-4 w-4" />
              Назад
            </Button>

            {pageNumbers.map((pageNumber, index) => {
              const showGap = index > 0 && pageNumbers[index - 1] !== pageNumber - 1;

              return (
                <div key={pageNumber} className="flex items-center gap-2">
                  {showGap ? <span className="px-1 text-sm text-fg-muted">…</span> : null}
                  <Button
                    variant={pageNumber === currentPage ? 'default' : 'outline'}
                    size="sm"
                    onClick={() => setPage(pageNumber)}
                    className={
                      pageNumber === currentPage
                        ? 'bg-purple text-fg-inverse hover:bg-purple-hover'
                        : 'border-[hsl(var(--border-default))] bg-surface-card'
                    }
                  >
                    {pageNumber}
                  </Button>
                </div>
              );
            })}

            <Button
              variant="outline"
              size="sm"
              disabled={currentPage >= totalPages}
              onClick={() => setPage((previous) => Math.min(totalPages, previous + 1))}
              className="border-[hsl(var(--border-default))] bg-surface-card"
            >
              Вперёд
              <ChevronRight className="h-4 w-4" />
            </Button>
          </div>
        </div>
      ) : null}

      <ConfirmDialog
        open={deleteTarget !== null}
        onOpenChange={(nextOpen) => {
          if (!nextOpen && !deleteLab.isPending) {
            setDeleteTarget(null);
          }
        }}
        title={deleteTarget ? `Удалить лабораторную «${deleteTarget.title}»?` : 'Удалить лабораторную?'}
        description="Будут удалены сама лабораторная, варианты, материалы, профили вариативности и связанные записи генерации. Это действие нельзя отменить."
        confirmLabel="Удалить лабораторную"
        isPending={deleteLab.isPending}
        onConfirm={confirmDeleteLab}
      />
    </PageWrapper>
  );
}

export default function LabsPage() {
  return (
    <Suspense fallback={<PageWrapper><div className="h-32 rounded-2xl skeleton" /></PageWrapper>}>
      <LabsPageContent />
    </Suspense>
  );
}
