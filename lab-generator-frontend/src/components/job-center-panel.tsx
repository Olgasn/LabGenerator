'use client';

import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from '@/components/ui/dialog'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { JobStatus } from '@/shared/contracts/jobs'
import { useJob } from '@/shared/hooks/useJobs'
import { useJobCenter } from '@/shared/job-center/job-center-context'
import { PanelRightClose, PanelRightOpen, Trash2 } from 'lucide-react'
import React, { useState } from 'react'

class JobCenterErrorBoundary extends React.Component<
  { children: React.ReactNode },
  { hasError: boolean }
> {
  constructor(props: { children: React.ReactNode }) {
    super(props);
    this.state = { hasError: false };
  }

  static getDerivedStateFromError() {
    return { hasError: true };
  }

  componentDidCatch(error: unknown, errorInfo: unknown) {
    console.error('JobCenterPanel crashed:', error, errorInfo);
  }

  render() {
    if (this.state.hasError) {
      return null;
    }
    return this.props.children;
  }
}

function statusLabel(status: number) {
  switch (status) {
    case JobStatus.Pending:
      return 'Pending';
    case JobStatus.InProgress:
      return 'In progress';
    case JobStatus.Succeeded:
      return 'Succeeded';
    case JobStatus.Failed:
      return 'Failed';
    case JobStatus.Canceled:
      return 'Canceled';
    default:
      return String(status);
  }
}

type LlmPrompt = {
  purpose?: string;
  attempt?: number;
  systemPrompt?: string;
  userPrompt?: string;
  temperature?: number;
  maxOutputTokens?: number;
};

function tryParseJsonObject(text: string): Record<string, unknown> | null {
  try {
    const value = JSON.parse(text);
    if (!value || typeof value !== 'object' || Array.isArray(value)) return null;
    return value as Record<string, unknown>;
  } catch {
    return null;
  }
}

function coerceLlmPrompts(payload: Record<string, unknown> | null): LlmPrompt[] {
  const raw = payload?.llmPrompts;
  if (!raw || !Array.isArray(raw)) return [];

  const prompts: LlmPrompt[] = [];
  for (const item of raw) {
    if (!item || typeof item !== 'object' || Array.isArray(item)) continue;
    const prompt = item as Record<string, unknown>;
    prompts.push({
      purpose: typeof prompt.purpose === 'string' ? prompt.purpose : undefined,
      attempt: typeof prompt.attempt === 'number' ? prompt.attempt : undefined,
      systemPrompt: typeof prompt.systemPrompt === 'string' ? prompt.systemPrompt : undefined,
      userPrompt: typeof prompt.userPrompt === 'string' ? prompt.userPrompt : undefined,
      temperature: typeof prompt.temperature === 'number' ? prompt.temperature : undefined,
      maxOutputTokens: typeof prompt.maxOutputTokens === 'number' ? prompt.maxOutputTokens : undefined,
    });
  }
  return prompts;
}

function statusVariant(status: number): 'default' | 'destructive' | 'secondary' | 'outline' {
  if (status === JobStatus.Succeeded) return 'default';
  if (status === JobStatus.Failed) return 'destructive';
  if (status === JobStatus.Canceled) return 'secondary';
  if (status === JobStatus.InProgress) return 'secondary';
  return 'outline';
}

function statusClassName(status: number): string {
  if (status === JobStatus.Succeeded) return 'border-emerald-600 bg-emerald-500 text-white';
  if (status === JobStatus.Failed) return 'border-rose-600 bg-rose-500 text-white';
  if (status === JobStatus.Canceled) return 'border-slate-500 bg-slate-500 text-white';
  if (status === JobStatus.InProgress) return 'border-sky-600 bg-sky-500 text-white';
  if (status === JobStatus.Pending) return 'border-amber-600 bg-amber-500 text-white';
  return '';
}

function formatTs(ms: number) {
  const date = new Date(ms);
  const now = new Date();

  const sameDay =
    date.getFullYear() === now.getFullYear() &&
    date.getMonth() === now.getMonth() &&
    date.getDate() === now.getDate();

  const timeFmt = new Intl.DateTimeFormat(undefined, { hour: '2-digit', minute: '2-digit', second: '2-digit' });
  if (sameDay) return timeFmt.format(date);

  const dateTimeFmt = new Intl.DateTimeFormat(undefined, {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
  });
  return dateTimeFmt.format(date);
}

type JobListFilter = 'all' | 'active' | 'succeeded' | 'failed';

function JobRow(props: {
  jobId: number;
  label?: string;
  request?: unknown;
  createdAt: number;
  onRemove: () => void;
  filter: JobListFilter;
}) {
  const query = useJob(props.jobId);
  const job = query.data;

  const payloadObj = job?.payloadJson ? tryParseJsonObject(job.payloadJson) : null;
  const llmPrompts = coerceLlmPrompts(payloadObj);

  const matchesFilter = (() => {
    if (props.filter === 'all') return true;
    if (!job) return true;

    if (props.filter === 'active') {
      return job.status === JobStatus.Pending || job.status === JobStatus.InProgress;
    }

    if (props.filter === 'succeeded') return job.status === JobStatus.Succeeded;
    if (props.filter === 'failed') return job.status === JobStatus.Failed || job.status === JobStatus.Canceled;
    return true;
  })();

  const badge = !job ? (
    <Badge variant="outline">Loading...</Badge>
  ) : (
    <Badge variant={statusVariant(job.status)} className={statusClassName(job.status)}>
      #{job.id} {statusLabel(job.status)} - {Math.round(job.progress)}%
    </Badge>
  );

  const hasError = !!job?.error;

  if (!matchesFilter) return null;

  return (
    <div className="flex items-start justify-between gap-3 rounded-md border bg-background p-3">
      <div className="min-w-0 flex-1">
        <div className="flex flex-wrap items-center gap-2">
          {badge}
          <div className="text-xs text-muted-foreground">started {formatTs(props.createdAt)}</div>
        </div>
        {props.label ? <div className="mt-1 text-sm font-medium whitespace-normal break-words">{props.label}</div> : null}
        {hasError ? <div className="mt-1 text-sm text-destructive whitespace-normal break-words">{job?.error}</div> : null}
      </div>

      <div className="flex shrink-0 flex-wrap items-center gap-2">
        <Dialog>
          <DialogTrigger asChild>
            <Button size="sm" variant="outline" disabled={!job}>
              Details
            </Button>
          </DialogTrigger>
          <DialogContent className="max-w-3xl max-h-[85vh] overflow-hidden">
            <DialogHeader>
              <DialogTitle>Job #{props.jobId}</DialogTitle>
            </DialogHeader>
            {job ? (
              <div className="grid max-h-[calc(85vh-5rem)] gap-3 overflow-y-auto pr-2 text-sm">
                {props.label ? (
                  <div>
                    <div className="text-muted-foreground">Label</div>
                    <div className="mt-1 break-words">{props.label}</div>
                  </div>
                ) : null}
                <div>
                  <span className="text-muted-foreground">Type:</span> <span className="font-mono">{job.type}</span>
                </div>
                <div>
                  <span className="text-muted-foreground">Status:</span> <span className="font-mono">{statusLabel(job.status)}</span>
                </div>
                <div>
                  <span className="text-muted-foreground">Progress:</span> <span className="font-mono">{job.progress}</span>
                </div>
                <div>
                  <span className="text-muted-foreground">CreatedAt:</span> <span className="font-mono">{job.createdAt}</span>
                </div>
                {job.startedAt ? (
                  <div>
                    <span className="text-muted-foreground">StartedAt:</span> <span className="font-mono">{job.startedAt}</span>
                  </div>
                ) : null}
                {job.finishedAt ? (
                  <div>
                    <span className="text-muted-foreground">FinishedAt:</span> <span className="font-mono">{job.finishedAt}</span>
                  </div>
                ) : null}

                {llmPrompts.length > 0 ? (
                  <div>
                    <div className="text-muted-foreground">LLM prompts</div>
                    <Tabs defaultValue="0" className="mt-1">
                      <div className="overflow-x-auto pb-1">
                        <TabsList className="inline-flex min-w-max flex-nowrap justify-start">
                          {llmPrompts.map((prompt, idx) => {
                            const label = `${prompt.purpose ?? 'prompt'}${prompt.attempt != null ? ` #${prompt.attempt}` : ''}`;
                            return (
                              <TabsTrigger key={idx} value={String(idx)} className="shrink-0">
                                {label}
                              </TabsTrigger>
                            );
                          })}
                        </TabsList>
                      </div>
                      {llmPrompts.map((prompt, idx) => (
                        <TabsContent key={idx} value={String(idx)} className="mt-2">
                          <div className="grid gap-2">
                            <div className="text-xs text-muted-foreground">
                              {prompt.temperature != null ? <span>temp={prompt.temperature}</span> : null}
                              {prompt.temperature != null && prompt.maxOutputTokens != null ? <span> - </span> : null}
                              {prompt.maxOutputTokens != null ? <span>maxTokens={prompt.maxOutputTokens}</span> : null}
                            </div>
                            {prompt.systemPrompt ? (
                              <div>
                                <div className="text-xs text-muted-foreground">System</div>
                                <pre className="mt-1 whitespace-pre-wrap break-words rounded-md border bg-muted/20 p-2 text-xs">
                                  {prompt.systemPrompt}
                                </pre>
                              </div>
                            ) : null}
                            {prompt.userPrompt ? (
                              <div>
                                <div className="text-xs text-muted-foreground">User</div>
                                <pre className="mt-1 whitespace-pre-wrap break-words rounded-md border bg-muted/20 p-2 text-xs">
                                  {prompt.userPrompt}
                                </pre>
                              </div>
                            ) : null}
                          </div>
                        </TabsContent>
                      ))}
                    </Tabs>
                  </div>
                ) : null}
                {props.request != null ? (
                  <div>
                    <div className="text-muted-foreground">Request</div>
                    <pre className="mt-1 max-h-48 overflow-auto whitespace-pre-wrap break-words rounded-md border bg-muted/20 p-2 text-xs">
                      {typeof props.request === 'string' ? props.request : JSON.stringify(props.request, null, 2)}
                    </pre>
                  </div>
                ) : null}
                {job.resultJson ? (
                  <div>
                    <div className="text-muted-foreground">Result</div>
                    <pre className="mt-1 max-h-48 overflow-auto whitespace-pre-wrap break-words rounded-md border bg-muted/20 p-2 text-xs">
                      {job.resultJson}
                    </pre>
                  </div>
                ) : null}
                {job.error ? (
                  <div>
                    <div className="text-muted-foreground">Error</div>
                    <pre className="mt-1 max-h-48 overflow-auto whitespace-pre-wrap break-words rounded-md border bg-muted/20 p-2 text-xs text-destructive">
                      {job.error}
                    </pre>
                  </div>
                ) : null}
              </div>
            ) : (
              <div className="text-sm text-muted-foreground">Loading...</div>
            )}
          </DialogContent>
        </Dialog>

        <Button size="sm" variant="secondary" onClick={props.onRemove}>
          Dismiss
        </Button>
      </div>
    </div>
  );
}

export function JobCenterPanel() {
  const { items, removeJob, clear } = useJobCenter();
  const [open, setOpen] = useState(true);
  const [filter, setFilter] = useState<JobListFilter>('all');

  const activeCount = items.length;
  const filteredItems = items;

  if (!open) {
    return (
      <div className="fixed bottom-4 right-4 z-50">
        <Button variant="outline" className="gap-2 shadow-lg" onClick={() => setOpen(true)}>
          <PanelRightOpen className="h-4 w-4" />
          Jobs ({activeCount})
        </Button>
      </div>
    );
  }

  return (
    <JobCenterErrorBoundary>
      <div className="fixed bottom-4 right-4 z-50 w-[560px] max-w-[calc(100vw-2rem)]">
        <Card className="shadow-lg">
          <CardHeader className="relative pb-3 pr-14">
            <Button
              size="icon"
              variant="ghost"
              className="absolute right-4 top-4 h-8 w-8 rounded-full text-muted-foreground hover:text-foreground"
              onClick={() => setOpen(false)}
              aria-label="Collapse jobs"
              title="Collapse jobs"
            >
              <PanelRightClose className="h-4 w-4" />
            </Button>
            <div className="flex items-start justify-between gap-3">
              <div className="min-w-0">
                <CardTitle>Jobs</CardTitle>
                <CardDescription>Queue and request history</CardDescription>
              </div>
            </div>
          </CardHeader>
          <CardContent className="grid max-h-[min(65vh,36rem)] gap-2 overflow-y-auto">
            <div className="flex items-start justify-between gap-3">
              <div className="flex flex-wrap gap-2">
                <Button
                  size="sm"
                  variant={filter === 'all' ? 'default' : 'outline'}
                  onClick={() => setFilter('all')}
                >
                  All
                </Button>
                <Button
                  size="sm"
                  variant={filter === 'active' ? 'default' : 'outline'}
                  onClick={() => setFilter('active')}
                >
                  Active
                </Button>
                <Button
                  size="sm"
                  variant={filter === 'succeeded' ? 'default' : 'outline'}
                  onClick={() => setFilter('succeeded')}
                >
                  Succeeded
                </Button>
                <Button
                  size="sm"
                  variant={filter === 'failed' ? 'default' : 'outline'}
                  onClick={() => setFilter('failed')}
                >
                  Failed
                </Button>
              </div>
              <Button
                size="icon"
                variant="ghost"
                className="h-9 w-9 shrink-0 rounded-full text-muted-foreground hover:text-destructive"
                onClick={clear}
                disabled={items.length === 0}
                aria-label="Clear job list"
                title="Clear job list"
              >
                <Trash2 className="h-4 w-4" />
              </Button>
            </div>

            {filteredItems.length === 0 ? (
              <div className="text-sm text-muted-foreground">No jobs yet.</div>
            ) : (
              filteredItems.map((item) => (
                <JobRow
                  key={item.jobId}
                  jobId={item.jobId}
                  label={item.label}
                  request={item.request}
                  createdAt={item.createdAt}
                  filter={filter}
                  onRemove={() => removeJob(item.jobId)}
                />
              ))
            )}
          </CardContent>
        </Card>
      </div>
    </JobCenterErrorBoundary>
  );
}
