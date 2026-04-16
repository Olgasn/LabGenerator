'use client';

import { AlertTriangle, Trash2 } from 'lucide-react';

import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';

type ConfirmDialogProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  title: string;
  description: string;
  confirmLabel?: string;
  cancelLabel?: string;
  isPending?: boolean;
  onConfirm: () => void | Promise<void>;
  tone?: 'default' | 'destructive';
};

export function ConfirmDialog({
  open,
  onOpenChange,
  title,
  description,
  confirmLabel = 'Подтвердить',
  cancelLabel = 'Отмена',
  isPending = false,
  onConfirm,
  tone = 'destructive',
}: ConfirmDialogProps) {
  const isDestructive = tone === 'destructive';
  const Icon = isDestructive ? Trash2 : AlertTriangle;

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="overflow-hidden border-[hsl(var(--border-default))] bg-surface-card p-0 shadow-[0_24px_80px_hsl(0_0%_0%_/_0.35)] sm:max-w-md">
        <div className="relative">
          <div className="absolute inset-0 bg-[radial-gradient(circle_at_top,_hsl(var(--accent-primary)/0.18),_transparent_58%)]" />
          <div className="relative border-b border-[hsl(var(--border-subtle))] px-6 pb-5 pt-6">
            <div className="mb-4 flex h-12 w-12 items-center justify-center rounded-2xl border border-[hsl(var(--border-default))] bg-destructive/10 text-destructive shadow-[0_0_30px_hsl(var(--accent-error)/0.12)]">
              <Icon className="h-5 w-5" />
            </div>
            <DialogHeader className="space-y-2 text-left">
              <DialogTitle className="text-xl font-semibold text-fg-primary">{title}</DialogTitle>
              <DialogDescription className="text-sm leading-6 text-fg-secondary">
                {description}
              </DialogDescription>
            </DialogHeader>
          </div>
        </div>

        <DialogFooter className="flex-row items-center justify-end gap-3 px-6 py-5 sm:space-x-0">
          <Button
            type="button"
            variant="outline"
            disabled={isPending}
            className="border-[hsl(var(--border-default))] bg-surface-card text-fg-primary hover:bg-surface-hover"
            onClick={() => onOpenChange(false)}
          >
            {cancelLabel}
          </Button>
          <Button
            type="button"
            variant={isDestructive ? 'destructive' : 'default'}
            disabled={isPending}
            className={isDestructive ? 'shadow-[0_10px_30px_hsl(var(--accent-error)/0.2)]' : 'bg-purple text-fg-inverse hover:bg-purple-hover'}
            onClick={() => {
              void onConfirm();
            }}
          >
            {isPending ? 'Удаление...' : confirmLabel}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
