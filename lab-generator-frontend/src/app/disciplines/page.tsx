'use client';

import Link from 'next/link';
import { useState } from 'react';
import { motion } from 'framer-motion';
import { BookOpen, FlaskConical, Plus, Search, Trash2 } from 'lucide-react';

import { ConfirmDialog } from '@/components/confirm-dialog';
import { PageWrapper, StaggerContainer, StaggerItem } from '@/components/page-wrapper';
import { Button } from '@/components/ui/button';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { useCreateDiscipline, useDeleteDiscipline, useDisciplines } from '@/shared/hooks/useDisciplines';
import { formatAxiosError } from '@/shared/utils/formatAxiosError';

const cardColors = [
  { bg: 'bg-purple/10', icon: '🧮', glow: 'group-hover:shadow-[0_0_24px_hsl(var(--accent-primary)/0.12)]', border: 'group-hover:border-purple/30' },
  { bg: 'bg-indigo/10', icon: '🗂️', glow: 'group-hover:shadow-[0_0_24px_hsl(var(--accent-secondary)/0.12)]', border: 'group-hover:border-indigo/30' },
  { bg: 'bg-success/10', icon: '🤖', glow: 'group-hover:shadow-[0_0_24px_hsl(var(--accent-success)/0.12)]', border: 'group-hover:border-success/30' },
  { bg: 'bg-warning/10', icon: '💻', glow: 'group-hover:shadow-[0_0_24px_hsl(var(--accent-warning)/0.12)]', border: 'group-hover:border-warning/30' },
  { bg: 'bg-purple/10', icon: '🌐', glow: 'group-hover:shadow-[0_0_24px_hsl(var(--accent-primary)/0.12)]', border: 'group-hover:border-purple/30' },
  { bg: 'bg-indigo/10', icon: '🔗', glow: 'group-hover:shadow-[0_0_24px_hsl(var(--accent-secondary)/0.12)]', border: 'group-hover:border-indigo/30' },
];

type DeleteTarget = {
  id: number;
  name: string;
} | null;

export default function DisciplinesPage() {
  const { data, isLoading, error } = useDisciplines();
  const create = useCreateDiscipline();
  const deleteDiscipline = useDeleteDiscipline();

  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [search, setSearch] = useState('');
  const [open, setOpen] = useState(false);
  const [deleteError, setDeleteError] = useState<string | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<DeleteTarget>(null);

  const filtered = (data ?? []).filter((discipline) =>
    discipline.name.toLowerCase().includes(search.toLowerCase()),
  );

  async function confirmDeleteDiscipline() {
    if (!deleteTarget) {
      return;
    }

    setDeleteError(null);

    try {
      await deleteDiscipline.mutateAsync(deleteTarget.id);
      setDeleteTarget(null);
    } catch (error) {
      setDeleteError(formatAxiosError(error).message);
    }
  }

  return (
    <PageWrapper>
      <div className="flex items-start justify-between">
        <div>
          <h1 className="text-[28px] font-bold text-fg-primary">Дисциплины</h1>
          <p className="mt-1 text-sm text-fg-muted">
            Управление учебными дисциплинами и быстрый переход к их лабораторным работам.
          </p>
        </div>

        <Dialog open={open} onOpenChange={setOpen}>
          <DialogTrigger asChild>
            <Button className="gap-2 bg-purple text-fg-inverse shadow-[0_4px_14px_hsl(var(--accent-primary)/0.3)] transition-all duration-200 hover:bg-purple-hover hover:shadow-[0_4px_20px_hsl(var(--accent-primary)/0.4)]">
              <Plus className="h-4 w-4" />
              Добавить дисциплину
            </Button>
          </DialogTrigger>
          <DialogContent className="border-[hsl(var(--border-default))] bg-surface-card">
            <DialogHeader>
              <DialogTitle>Новая дисциплина</DialogTitle>
            </DialogHeader>
            <div className="grid gap-4 pt-2">
              <Input
                placeholder="Название дисциплины"
                value={name}
                onChange={(event) => setName(event.target.value)}
                className="border-[hsl(var(--border-default))] bg-surface-tertiary transition-colors focus:border-purple/50"
              />
              <Textarea
                placeholder="Описание (необязательно)"
                value={description}
                onChange={(event) => setDescription(event.target.value)}
                rows={3}
                className="resize-none border-[hsl(var(--border-default))] bg-surface-tertiary transition-colors focus:border-purple/50"
              />
              <div className="flex items-center gap-3">
                <Button
                  disabled={create.isPending || !name.trim()}
                  className="bg-purple text-fg-inverse hover:bg-purple-hover"
                  onClick={async () => {
                    await create.mutateAsync({
                      name: name.trim(),
                      description: description.trim() || undefined,
                    });
                    setName('');
                    setDescription('');
                    setOpen(false);
                  }}
                >
                  {create.isPending ? 'Создание...' : 'Создать'}
                </Button>
                {create.error ? (
                  <p className="text-sm text-destructive">{formatAxiosError(create.error).message}</p>
                ) : null}
              </div>
            </div>
          </DialogContent>
        </Dialog>
      </div>

      <div className="relative">
        <Search className="absolute left-3.5 top-1/2 h-4 w-4 -translate-y-1/2 text-fg-muted" />
        <Input
          placeholder="Поиск дисциплин..."
          value={search}
          onChange={(event) => setSearch(event.target.value)}
          className="border-[hsl(var(--border-default))] bg-surface-tertiary pl-10 transition-colors focus:border-purple/40"
        />
      </div>

      {deleteError ? (
        <div className="rounded-xl border border-destructive/20 bg-destructive/5 px-4 py-3 text-sm text-destructive">
          {deleteError}
        </div>
      ) : null}

      {isLoading ? (
        <div className="grid grid-cols-3 gap-5">
          {[...Array(6)].map((_, index) => (
            <div key={index} className="skeleton h-36 rounded-2xl" />
          ))}
        </div>
      ) : error ? (
        <div className="py-12 text-center text-sm text-destructive">{String(error)}</div>
      ) : filtered.length === 0 ? (
        <motion.div
          initial={{ opacity: 0, y: 8 }}
          animate={{ opacity: 1, y: 0 }}
          className="flex flex-col items-center justify-center gap-3 py-16"
        >
          <div className="flex h-14 w-14 items-center justify-center rounded-2xl bg-surface-tertiary">
            <BookOpen className="h-6 w-6 text-fg-muted/50" />
          </div>
          <p className="text-sm text-fg-muted">
            {search ? 'Ничего не найдено' : 'Пока нет дисциплин. Создайте первую!'}
          </p>
        </motion.div>
      ) : (
        <StaggerContainer className="grid grid-cols-1 gap-5 xl:grid-cols-3">
          {filtered.map((discipline, index) => {
            const color = cardColors[index % cardColors.length];
            const labCount = discipline.labsCount ?? 0;
            const isDeleting = deleteDiscipline.isPending && deleteTarget?.id === discipline.id;

            return (
              <StaggerItem key={discipline.id}>
                <motion.div
                  whileHover={{ y: -3, transition: { duration: 0.2, ease: 'easeOut' } }}
                  className={`group relative flex flex-col gap-4 overflow-hidden rounded-2xl border border-[hsl(var(--border-subtle))] bg-surface-card p-6 transition-all duration-300 ${color.glow} ${color.border}`}
                >
                  <Link
                    href={`/labs?disciplineId=${discipline.id}`}
                    aria-label={`Открыть лабораторные работы дисциплины ${discipline.name}`}
                    className="absolute inset-0 z-10 rounded-2xl"
                  />

                  <div className={`pointer-events-none absolute inset-0 bg-gradient-to-br ${color.bg.replace('bg-', 'from-')} to-transparent opacity-0 transition-opacity duration-300 group-hover:opacity-30`} />

                  <Button
                    type="button"
                    variant="ghost"
                    size="icon"
                    disabled={isDeleting}
                    className="absolute right-4 top-4 z-20 text-fg-muted hover:bg-destructive/10 hover:text-destructive"
                    onClick={() => setDeleteTarget({ id: discipline.id, name: discipline.name })}
                  >
                    <Trash2 className="h-4 w-4" />
                  </Button>

                  <div className="flex items-start gap-3">
                    <motion.div
                      whileHover={{ rotate: [0, -5, 5, 0], transition: { duration: 0.4 } }}
                      className={`flex h-11 w-11 items-center justify-center rounded-xl text-xl ${color.bg} transition-transform duration-200`}
                    >
                      {color.icon}
                    </motion.div>
                    <div className="min-w-0 flex-1 pr-10">
                      <h3 className="font-semibold leading-snug text-fg-primary transition-colors group-hover:text-purple">
                        {discipline.name}
                      </h3>
                      {discipline.description ? (
                        <p className="mt-1 line-clamp-2 text-[13px] leading-snug text-fg-muted">
                          {discipline.description}
                        </p>
                      ) : null}
                    </div>
                  </div>

                  <div className="flex items-center justify-between gap-3">
                    <div className="flex items-center gap-1.5 text-xs text-fg-muted">
                      <FlaskConical className="h-3.5 w-3.5" />
                      <span>{labCount} лабораторных</span>
                    </div>
                    <span className="text-xs font-medium text-purple">Открыть</span>
                  </div>

                  <div className="pointer-events-none absolute inset-0 rounded-2xl ring-1 ring-transparent transition-all duration-300 group-hover:ring-purple/20" />
                </motion.div>
              </StaggerItem>
            );
          })}
        </StaggerContainer>
      )}

      <ConfirmDialog
        open={deleteTarget !== null}
        onOpenChange={(nextOpen) => {
          if (!nextOpen && !deleteDiscipline.isPending) {
            setDeleteTarget(null);
          }
        }}
        title={deleteTarget ? `Удалить дисциплину «${deleteTarget.name}»?` : 'Удалить дисциплину?'}
        description="Будут удалены сама дисциплина, все ее лабораторные работы и связанные данные генерации. Это действие нельзя отменить."
        confirmLabel="Удалить дисциплину"
        isPending={deleteDiscipline.isPending}
        onConfirm={confirmDeleteDiscipline}
      />
    </PageWrapper>
  );
}
