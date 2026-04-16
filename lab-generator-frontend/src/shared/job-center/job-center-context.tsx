'use client';

import React, { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react'

export type JobCenterItem = {
  jobId: number;
  createdAt: number;
  label?: string;
  request?: unknown;
};

type JobCenterContextValue = {
  items: JobCenterItem[];
  addJob: (jobId: number, label?: string, request?: unknown) => void;
  removeJob: (jobId: number) => void;
  clear: () => void;
};

const JobCenterContext = createContext<JobCenterContextValue | null>(null);

const STORAGE_KEY = 'labgen.jobCenter.items.v1';

function restoreItems(maxItems: number): JobCenterItem[] {
  if (typeof window === 'undefined') {
    return [];
  }

  try {
    const raw = window.localStorage.getItem(STORAGE_KEY);
    if (!raw) return [];

    const parsed: unknown = JSON.parse(raw);
    if (!Array.isArray(parsed)) return [];

    const restored: JobCenterItem[] = [];
    for (const item of parsed) {
      if (!item || typeof item !== 'object') continue;

      const candidate = item as Record<string, unknown>;
      if (typeof candidate.jobId !== 'number' || typeof candidate.createdAt !== 'number') continue;

      restored.push({
        jobId: candidate.jobId,
        createdAt: candidate.createdAt,
        label: typeof candidate.label === 'string' ? candidate.label : undefined,
        request: candidate.request,
      });
    }

    return restored.slice(0, maxItems);
  } catch {
    return [];
  }
}

export function JobCenterProvider(props: { children: React.ReactNode; maxItems?: number }) {
  const maxItems = props.maxItems ?? 1000;
  const [items, setItems] = useState<JobCenterItem[]>(() => restoreItems(maxItems));

  useEffect(() => {
    try {
      window.localStorage.setItem(STORAGE_KEY, JSON.stringify(items));
    } catch {
      // ignore
    }
  }, [items]);

  const addJob = useCallback(
    (jobId: number, label?: string, request?: unknown) => {
      setItems((prev) => {
        const filtered = prev.filter((x) => x.jobId !== jobId);
        const next = [{ jobId, label, request, createdAt: Date.now() }, ...filtered];
        return next.slice(0, maxItems);
      });
    },
    [maxItems]
  );

  const removeJob = useCallback((jobId: number) => {
    setItems((prev) => prev.filter((x) => x.jobId !== jobId));
  }, []);

  const clear = useCallback(() => setItems([]), []);

  const value = useMemo<JobCenterContextValue>(
    () => ({ items, addJob, removeJob, clear }),
    [items, addJob, removeJob, clear]
  );

  return <JobCenterContext.Provider value={value}>{props.children}</JobCenterContext.Provider>;
}

export function useJobCenter() {
  const ctx = useContext(JobCenterContext);
  if (!ctx) throw new Error('useJobCenter must be used within JobCenterProvider');
  return ctx;
}
