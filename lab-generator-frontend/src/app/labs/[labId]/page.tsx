'use client';

import { PageWrapper } from '@/components/page-wrapper'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from '@/components/ui/dialog'
import { Input } from '@/components/ui/input'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { Textarea } from '@/components/ui/textarea'
import { JobStatus } from '@/shared/contracts/jobs'
import { useDifficultyTarget, useResetDifficultyTarget, useSetDifficultyTarget } from '@/shared/hooks/useDifficultyTarget'
import { useDisciplines } from '@/shared/hooks/useDisciplines'
import { useJob } from '@/shared/hooks/useJobs'
import { useLab } from '@/shared/hooks/useLabs'
import { useLabVariationMethods, useUpsertLabVariationMethods } from '@/shared/hooks/useLabVariationMethods'
import { useLlmProviderSettings } from '@/shared/hooks/useLlmProviderSettings'
import { useLlmSettings } from '@/shared/hooks/useLlmSettings'
import { useApproveMaster, useCurrentMaster, useGenerateMasterJob, useUpdateMaster } from '@/shared/hooks/useMaster'
import { usePromptSection, useResetPromptSection, useUpdatePromptSection } from '@/shared/hooks/usePromptSections'
import { useGenerateSupplementaryMaterialJob, useSupplementaryMaterial } from '@/shared/hooks/useSupplementaryMaterials'
import { useGenerateVariantsJob, useVariants } from '@/shared/hooks/useVariants'
import { useCreateVariationMethod, useUpdateVariationMethod, useVariationMethods } from '@/shared/hooks/useVariationMethods'
import { useLabVerificationReports, useVerifyLabJob } from '@/shared/hooks/useVerification'
import { useJobCenter } from '@/shared/job-center/job-center-context'
import { formatAxiosError } from '@/shared/utils/formatAxiosError'
import { AnimatePresence, motion } from 'framer-motion'
import {
  CheckCircle2,
  ChevronLeft,
  ChevronRight,
  CircleAlert, Download,
  Loader2,
  RotateCcw,
  Settings
} from 'lucide-react'
import Link from 'next/link'
import { useParams } from 'next/navigation'
import { useEffect, useMemo, useState } from 'react'
import ReactMarkdown from 'react-markdown'
import remarkGfm from 'remark-gfm'

function stripMarkdownFences(input: string): string {
  const s = input.trim();
  const m = s.match(/^```(?:markdown)?\s*\n([\s\S]*?)\n```\s*$/i);
  return m ? m[1].trim() : input;
}

function PromptSectionEditor({ sectionKey, label }: { sectionKey: string; label: string }) {
  const section = usePromptSection(sectionKey);
  const updateSection = useUpdatePromptSection(sectionKey);
  const resetSection = useResetPromptSection(sectionKey);
  const [open, setOpen] = useState(false);
  const [draft, setDraft] = useState('');

  const dirty = section.data ? draft !== section.data.content : false;
  const isCustomized = section.data?.isCustomized ?? false;

  return (
    <Dialog open={open} onOpenChange={(v) => { setOpen(v); if (v && section.data) setDraft(section.data.content); }}>
      <DialogTrigger asChild>
        <Button variant="outline" size="sm" className="gap-1.5 border-[hsl(var(--border-default))] text-fg-secondary">
          <Settings className="h-3.5 w-3.5" />
          {label}
          {isCustomized && <span className="ml-1 h-1.5 w-1.5 rounded-full bg-purple" />}
        </Button>
      </DialogTrigger>
      <DialogContent className="max-w-2xl max-h-[85vh] flex flex-col">
        <DialogHeader>
          <DialogTitle>{section.data?.displayName ?? label}</DialogTitle>
        </DialogHeader>
        <div className="flex flex-col gap-3 flex-1 min-h-0">
          <p className="text-sm text-fg-muted">
            Эти требования передаются в промпт при генерации. Измените текст под свои нужды или сбросьте к значению по умолчанию.
          </p>
          {section.isLoading && <p className="text-sm text-fg-muted">Загрузка...</p>}
          {section.data && (
            <>
              <Textarea
                value={draft}
                onChange={(e) => setDraft(e.target.value)}
                rows={14}
                className="border-[hsl(var(--border-default))] bg-surface-tertiary font-mono text-sm flex-1 min-h-[200px]"
              />
              <div className="flex items-center gap-2 flex-wrap">
                <Button
                  className="bg-purple text-fg-inverse hover:bg-purple-hover"
                  disabled={updateSection.isPending || !dirty}
                  onClick={async () => {
                    await updateSection.mutateAsync(draft);
                    setOpen(false);
                  }}
                >
                  {updateSection.isPending ? 'Сохранение...' : 'Сохранить'}
                </Button>
                <Button
                  variant="outline"
                  className="gap-1.5 border-[hsl(var(--border-default))]"
                  disabled={resetSection.isPending || !isCustomized}
                  onClick={async () => {
                    await resetSection.mutateAsync();
                    setDraft(section.data!.defaultContent);
                  }}
                >
                  <RotateCcw className="h-3.5 w-3.5" />
                  {resetSection.isPending ? 'Сброс...' : 'По умолчанию'}
                </Button>
                {dirty && <Badge className="border-destructive/40 bg-destructive/10 text-destructive">Не сохранено</Badge>}
                {isCustomized && !dirty && <Badge className="border-purple/40 bg-purple-muted text-purple">Изменено</Badge>}
              </div>
            </>
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
}

function ErrorMsg({ error }: { error: unknown }) {
  if (!error) return null;
  const e = formatAxiosError(error);
  return (
    <span className="text-sm text-destructive">
      {e.title}: {e.message}
      {e.status != null ? ` (status=${e.status})` : ''}
    </span>
  );
}

function MasterEditor(props: {
  masterId: number;
  version: number;
  status: number;
  content: string;
  onSave: (args: { masterAssignmentId: number; content: string }) => Promise<void>;
  onApprove: (masterAssignmentId: number) => Promise<void>;
  saving: boolean;
  approving: boolean;
  saveError: unknown;
  approveError: unknown;
}) {
  const original = stripMarkdownFences(props.content);
  const [draft, setDraft] = useState(original);
  const [preview, setPreview] = useState(true);
  const dirty = draft !== original;
  const isApproved = props.status === 1;

  return (
    <div className="flex flex-col gap-4">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <div className="text-sm text-fg-muted">
          Мастер #{props.masterId} · версия {props.version}
        </div>
        <div className="flex items-center gap-2">
          <Badge className={isApproved
            ? 'border-success/40 bg-success/10 text-success'
            : 'border-warning/40 bg-warning/10 text-warning'
          }>
            {isApproved ? 'Одобрено' : 'Черновик'}
          </Badge>
          {dirty && <Badge className="border-destructive/40 bg-destructive/10 text-destructive">Не сохранено</Badge>}
          <Button
            variant="ghost"
            size="sm"
            onClick={() => setPreview((v) => !v)}
            className="text-fg-secondary"
          >
            {preview ? 'Скрыть превью' : 'Показать превью'}
          </Button>
        </div>
      </div>

      <Textarea
        value={draft}
        onChange={(e) => setDraft(e.target.value)}
        rows={14}
        className="border-[hsl(var(--border-default))] bg-surface-tertiary font-mono text-sm"
      />

      <AnimatePresence>
        {preview && (
          <motion.div
            initial={{ opacity: 0, height: 0 }}
            animate={{ opacity: 1, height: 'auto' }}
            exit={{ opacity: 0, height: 0 }}
            className="overflow-hidden rounded-xl border border-[hsl(var(--border-subtle))] bg-surface-primary p-5"
          >
            <div className="mb-2 text-xs font-medium uppercase tracking-wider text-fg-muted">Предпросмотр</div>
            <div className="prose prose-sm max-w-none">
              <ReactMarkdown remarkPlugins={[remarkGfm]}>{draft}</ReactMarkdown>
            </div>
          </motion.div>
        )}
      </AnimatePresence>

      <div className="flex flex-wrap items-center gap-2">
        <Button
          variant="outline"
          className="border-[hsl(var(--border-default))]"
          disabled={props.saving || !dirty}
          onClick={async () => {
            await props.onSave({ masterAssignmentId: props.masterId, content: stripMarkdownFences(draft) });
          }}
        >
          {props.saving ? 'Сохранение...' : 'Сохранить'}
        </Button>
        <Button
          className="bg-purple text-fg-inverse hover:bg-purple-hover"
          disabled={props.approving || dirty || isApproved}
          onClick={async () => { await props.onApprove(props.masterId); }}
        >
          {props.approving ? 'Одобрение...' : 'Одобрить'}
        </Button>
        <ErrorMsg error={props.saveError} />
        <ErrorMsg error={props.approveError} />
      </div>

      {dirty && (
        <div className="text-sm text-fg-muted">Сначала сохраните, затем одобрите.</div>
      )}
    </div>
  );
}

const tabs = [
  { value: 'master', label: 'Мастер-задание' },
  { value: 'variation', label: 'Варианты' },
  { value: 'materials', label: 'Материалы' },
  { value: 'verification', label: 'Верификация' },
];

export default function LabDetailsPage() {
  const params = useParams<{ labId: string }>();
  const labId = Number(params.labId);
  const [activeTab, setActiveTab] = useState('master');

  const lab = useLab(labId);
  const allDisciplines = useDisciplines();
  const discipline = lab.data ? (allDisciplines.data ?? []).find((d) => d.id === lab.data!.disciplineId) : null;

  const master = useCurrentMaster(labId);
  const genMasterJob = useGenerateMasterJob(labId);
  const updateMaster = useUpdateMaster(labId);
  const approveMaster = useApproveMaster(labId);

  const llmSettings = useLlmSettings();
  const activeProvider = llmSettings.data?.provider ?? '';
  const activeProviderSettings = useLlmProviderSettings(activeProvider);

  const [variantsPage, setVariantsPage] = useState(1);
  const [variantsPageSize, setVariantsPageSize] = useState('10');
  const [variantsSort, setVariantsSort] = useState<'asc' | 'desc'>('asc');
  const variants = useVariants(labId, {
    page: variantsPage,
    pageSize: Number(variantsPageSize),
    sort: variantsSort,
  });
  const genVariantsJob = useGenerateVariantsJob(labId);
  const supplementaryMaterial = useSupplementaryMaterial(labId);
  const genSupplementaryMaterialJob = useGenerateSupplementaryMaterialJob(labId);
  const verifyJob = useVerifyLabJob(labId);
  const verificationReports = useLabVerificationReports(labId);

  const variationMethods = useVariationMethods();
  const createVariationMethod = useCreateVariationMethod();
  const updateVariationMethod = useUpdateVariationMethod();
  const labVariationMethods = useLabVariationMethods(labId);
  const upsertLabVariationMethods = useUpsertLabVariationMethods(labId);

  const difficultyTarget = useDifficultyTarget(labId);
  const setDifficultyTarget = useSetDifficultyTarget(labId);
  const resetDifficultyTarget = useResetDifficultyTarget(labId);

  const [difficultyDraft, setDifficultyDraft] = useState<{
    complexity: string;
    hoursMin: string;
    hoursMax: string;
  } | null>(null);

  const [newVmName, setNewVmName] = useState('');
  const [newVmDescription, setNewVmDescription] = useState('');
  const [vmDescriptionDraft, setVmDescriptionDraft] = useState<Record<number, string>>({});
  const [activeJobId, setActiveJobId] = useState<number | null>(null);
  const activeJob = useJob(activeJobId);
  const activeJobStatus = activeJob.data?.status;
  const jobCenter = useJobCenter();
  const hasActiveJob = activeJobStatus === JobStatus.Pending || activeJobStatus === JobStatus.InProgress;
  const [variantsCount, setVariantsCount] = useState('3');
  const [vmSelectedId, setVmSelectedId] = useState('');
  const [vmDraftOverride, setVmDraftOverride] = useState<Array<{ variationMethodId: number; preserveAcrossLabs: boolean }> | null>(null);

  const vmBase = useMemo<Array<{ variationMethodId: number; preserveAcrossLabs: boolean }>>(
    () => (labVariationMethods.data ?? []).map((x) => ({ variationMethodId: x.variationMethodId, preserveAcrossLabs: x.preserveAcrossLabs })),
    [labVariationMethods.data]
  );

  const vmDraft = vmDraftOverride ?? vmBase;
  const dtComplexity = difficultyDraft?.complexity ?? difficultyTarget.data?.complexity ?? 'medium';
  const dtHoursMin = difficultyDraft?.hoursMin ?? String(difficultyTarget.data?.estimatedHoursMin ?? 5);
  const dtHoursMax = difficultyDraft?.hoursMax ?? String(difficultyTarget.data?.estimatedHoursMax ?? 7);
  const getVmDescriptionValue = (variationMethodId: number, fallback?: string | null) =>
    Object.prototype.hasOwnProperty.call(vmDescriptionDraft, variationMethodId)
      ? vmDescriptionDraft[variationMethodId]
      : (fallback ?? '');
  const updateVmDraft = (updater: (current: typeof vmBase) => typeof vmBase) => {
    setVmDraftOverride((prev) => updater(prev ?? vmBase));
  };

  const masterApproved = (master.data?.status ?? 0) === 1;
  const refetchMaster = master.refetch;
  const refetchVariants = variants.refetch;
  const refetchSupplementaryMaterial = supplementaryMaterial.refetch;
  const refetchVerificationReports = verificationReports.refetch;

  useEffect(() => {
    if (activeJobStatus == null) return;
    if (activeJobStatus === JobStatus.Succeeded || activeJobStatus === JobStatus.Failed || activeJobStatus === JobStatus.Canceled) {
      void refetchMaster();
      void refetchVariants();
      void refetchSupplementaryMaterial();
      void refetchVerificationReports();
    }
  }, [activeJobStatus, refetchMaster, refetchVariants, refetchSupplementaryMaterial, refetchVerificationReports]);

  const verificationByVariantId = useMemo(() => {
    const map = new Map<number, {
      passed: boolean; overall: number | null; issuesCount: number; createdAt: string;
      issues: Array<{ code?: string; message?: string; severity?: string }>;
    }>();
    for (const r of verificationReports.data ?? []) {
      let overall: number | null = null;
      let issuesCount = 0;
      let issues: Array<{ code?: string; message?: string; severity?: string }> = [];
      try { const s = JSON.parse(r.judgeScoreJson ?? '{}'); if (typeof s?.overall === 'number') overall = s.overall; } catch { overall = null; }
      if (overall != null) {
        if (overall <= 1 && overall >= 0) overall = Math.round(overall * 10);
        overall = Math.max(0, Math.min(10, Math.round(overall)));
      }
      try { const p = JSON.parse(r.issuesJson ?? '[]'); if (Array.isArray(p)) { issues = p; issuesCount = p.length; } } catch { issuesCount = 0; issues = []; }
      map.set(r.assignmentVariantId, { passed: r.passed, overall, issuesCount, createdAt: r.createdAt, issues });
    }
    return map;
  }, [verificationReports.data]);

  const llmAccessLoading = llmSettings.isLoading || (!!activeProvider && activeProviderSettings.isLoading);
  const llmReady = !!activeProvider && !!activeProviderSettings.data?.hasApiKey;
  const llmAccessBlocked = llmAccessLoading || !llmReady;
  const llmAccessMessage = llmAccessLoading
    ? 'Проверяем настройки LLM...'
    : `Для провайдера ${activeProvider || 'LLM'} не задан API key. Перейдите в настройки и сохраните ключ.`;

  const variantsTotalCount = variants.data?.totalCount ?? 0;
  const variantItems = variants.data?.items ?? [];
  const currentVariantsPage = variants.data?.page ?? variantsPage;
  const totalVariantPages = variants.data?.totalPages ?? 1;
  const variantRangeStart = variantsTotalCount === 0 ? 0 : (currentVariantsPage - 1) * Number(variantsPageSize) + 1;
  const variantRangeEnd = variantsTotalCount === 0 ? 0 : Math.min(currentVariantsPage * Number(variantsPageSize), variantsTotalCount);
  const hasVariants = variantsTotalCount > 0;
  const supplementaryMaterialReady = supplementaryMaterial.data != null;

  const jobItems = useMemo(() => {
    const items: Array<{ label: string; status: 'done' | 'pending' | 'running'; detail: string }> = [];
    if (master.data) items.push({ label: 'Генерация мастер-задания', status: masterApproved ? 'done' : 'pending', detail: masterApproved ? 'Завершено' : 'Ожидание' });
    if (hasVariants) items.push({ label: 'Генерация вариантов', status: 'done', detail: `Завершено · ${variantsTotalCount} вариантов` });
    if (supplementaryMaterialReady) items.push({ label: 'Доп. материалы', status: 'done', detail: 'Завершено' });
    if (hasActiveJob) {
      const last = items[items.length - 1];
      if (last) { last.status = 'running'; last.detail = 'В процессе...'; }
    }
    return items;
  }, [master.data, masterApproved, hasVariants, variantsTotalCount, supplementaryMaterialReady, hasActiveJob]);

  return (
    <PageWrapper>
      <nav className="flex items-center gap-2 text-[13px]">
        <Link href="/labs" className="text-purple hover:text-purple-hover transition-colors">Лабораторные</Link>
        <ChevronRight className="h-3.5 w-3.5 text-fg-muted" />
        <Link href="/disciplines" className="text-purple hover:text-purple-hover transition-colors">{discipline?.name ?? 'Дисциплина'}</Link>
        <ChevronRight className="h-3.5 w-3.5 text-fg-muted" />
        <span className="text-fg-secondary">{lab.data?.title ?? `Лабораторная #${labId}`}</span>
      </nav>

      <div className="flex items-center">
        <div className="flex items-center gap-4">
          <h1 className="text-2xl font-bold text-fg-primary">{lab.data?.title ?? `Лабораторная #${labId}`}</h1>
          {masterApproved && (
            <Badge className="border-purple/30 bg-purple-muted text-purple">Одобрено</Badge>
          )}
        </div>
      </div>

      {llmAccessBlocked && (
        <div className="rounded-2xl border border-destructive/30 bg-destructive/5 px-4 py-3 text-sm text-fg-primary">
          <div className="font-medium text-destructive">LLM недоступен</div>
          <div className="mt-1">
            {llmAccessMessage}{' '}
            {!llmAccessLoading && (
              <Link href="/admin/llm" className="text-purple hover:text-purple-hover">
                Открыть настройки
              </Link>
            )}
          </div>
        </div>
      )}

      <div className="flex items-center gap-1 rounded-lg bg-surface-tertiary p-1">
        {tabs.map((tab) => (
          <button
            key={tab.value}
            onClick={() => setActiveTab(tab.value)}
            className={`rounded-md px-4 py-2 text-[13px] font-medium transition-all duration-200 ${
              activeTab === tab.value
                ? 'bg-purple text-fg-inverse shadow-sm'
                : 'text-fg-muted hover:text-fg-secondary'
            }`}
          >
            {tab.label}
          </button>
        ))}
      </div>

      <div className="flex gap-6">
        <div className="flex-1 min-w-0">
          <AnimatePresence mode="wait">
            <motion.div
              key={activeTab}
              initial={{ opacity: 0, y: 8 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: -8 }}
              transition={{ duration: 0.25 }}
            >
              {activeTab === 'master' && (
                <div className="overflow-hidden rounded-2xl border border-[hsl(var(--border-subtle))] bg-surface-card">
                  <div className="flex items-center justify-between border-b border-[hsl(var(--border-subtle))] px-5 py-3.5">
                    <span className="text-[15px] font-semibold text-fg-primary">Мастер-задание</span>
                  </div>
                  <div className="p-6">
                    <div className="mb-4 flex items-center gap-2">
                      <Button
                        className="bg-purple text-fg-inverse hover:bg-purple-hover"
                        disabled={genMasterJob.isPending || hasActiveJob || llmAccessBlocked}
                        onClick={async () => {
                          const j = await genMasterJob.mutateAsync();
                          jobCenter.addJob(j.id, `Lab #${labId}: Generate master assignment`, { endpoint: 'POST /labs/{labId}/master-assignments/generate-draft', labId });
                          setActiveJobId(j.id);
                        }}
                      >
                        {genMasterJob.isPending ? 'Запуск...' : 'Генерировать мастер-задание'}
                      </Button>
                      <PromptSectionEditor sectionKey="master_requirements" label="Настройки промпта" />
                      <ErrorMsg error={genMasterJob.error} />
                    </div>

                    {master.isLoading && <p className="text-sm text-fg-muted">Загрузка...</p>}
                    <ErrorMsg error={master.error} />

                    {master.data ? (
                      <MasterEditor
                        key={master.data.id}
                        masterId={master.data.id}
                        version={master.data.version}
                        status={master.data.status}
                        content={master.data.content}
                        onSave={async ({ masterAssignmentId, content }) => {
                          await updateMaster.mutateAsync({ masterAssignmentId, input: { content } });
                        }}
                        onApprove={async (masterAssignmentId) => {
                          await approveMaster.mutateAsync(masterAssignmentId);
                        }}
                        saving={updateMaster.isPending}
                        approving={approveMaster.isPending}
                        saveError={updateMaster.error}
                        approveError={approveMaster.error}
                      />
                    ) : !master.isLoading ? (
                      <div className="rounded-xl border border-dashed border-[hsl(var(--border-default))] p-8 text-center text-sm text-fg-muted">
                        Мастер-задание ещё не сгенерировано. Нажмите кнопку выше.
                      </div>
                    ) : null}
                  </div>
                </div>
              )}

              {activeTab === 'variation' && (
                <div className="flex flex-col gap-6">
                  <div className="overflow-hidden rounded-2xl border border-[hsl(var(--border-subtle))] bg-surface-card">
                    <div className="border-b border-[hsl(var(--border-subtle))] px-5 py-3.5">
                      <span className="text-[15px] font-semibold text-fg-primary">Генерация вариантов</span>
                    </div>
                    <div className="p-6">
                      <div className="flex flex-wrap items-end gap-3">
                        <div className="w-32">
                          <div className="mb-1.5 text-sm text-fg-muted">Кол-во</div>
                          <Input
                            value={variantsCount}
                            onChange={(e) => setVariantsCount(e.target.value)}
                            className="border-[hsl(var(--border-default))] bg-surface-tertiary"
                          />
                        </div>
                        <Button
                          className="bg-purple text-fg-inverse hover:bg-purple-hover"
                          onClick={async () => {
                            const count = Number(variantsCount);
                            const j = await genVariantsJob.mutateAsync({ count });
                            jobCenter.addJob(j.id, `Lab #${labId}: Generate variants (count=${count})`, { endpoint: 'POST /labs/{labId}/variants/generate', labId, body: { count } });
                            setActiveJobId(j.id);
                          }}
                          disabled={genVariantsJob.isPending || !masterApproved || hasActiveJob || llmAccessBlocked}
                        >
                          {genVariantsJob.isPending ? 'Запуск...' : 'Генерировать варианты'}
                        </Button>
                        <ErrorMsg error={genVariantsJob.error} />
                      </div>

                      {!masterApproved && (
                        <div className="mt-4 rounded-xl border border-dashed border-[hsl(var(--border-default))] p-4 text-sm text-fg-muted">
                          Генерация вариантов недоступна: мастер-задание не одобрено.
                        </div>
                      )}

                      {variants.isLoading && <p className="mt-4 text-sm text-fg-muted">Загрузка...</p>}
                      <ErrorMsg error={variants.error} />

                      {variantsTotalCount > 0 && (
                        <div className="mt-4 flex flex-col gap-3 rounded-xl border border-[hsl(var(--border-subtle))] bg-surface-tertiary/40 p-4">
                          <div className="flex flex-col gap-2 text-sm text-fg-muted md:flex-row md:items-center md:justify-between">
                            <span>Показаны {variantRangeStart}-{variantRangeEnd} из {variantsTotalCount}</span>
                          </div>

                          <div className="grid gap-3 md:grid-cols-[minmax(0,1fr)_180px_180px]">
                            <div className="grid gap-1.5">
                              <div className="text-sm text-fg-muted">Сортировка</div>
                              <select
                                className="h-10 w-full rounded-lg border border-[hsl(var(--border-default))] bg-surface-card px-3 text-sm text-fg-primary"
                                value={variantsSort}
                                onChange={(e) => {
                                  setVariantsSort(e.target.value as typeof variantsSort);
                                  setVariantsPage(1);
                                }}
                              >
                                <option value="asc">По возрастанию</option>
                                <option value="desc">По убыванию</option>
                              </select>
                            </div>

                            <div className="grid gap-1.5">
                              <div className="text-sm text-fg-muted">На странице</div>
                              <select
                                className="h-10 w-full rounded-lg border border-[hsl(var(--border-default))] bg-surface-card px-3 text-sm text-fg-primary"
                                value={variantsPageSize}
                                onChange={(e) => {
                                  setVariantsPageSize(e.target.value);
                                  setVariantsPage(1);
                                }}
                              >
                                <option value="5">5</option>
                                <option value="10">10</option>
                                <option value="25">25</option>
                              </select>
                            </div>

                            <div className="grid gap-1.5">
                              <div className="text-sm text-fg-muted">Страница</div>
                              <div className="flex items-center gap-2">
                                <Button
                                  variant="outline"
                                  size="sm"
                                  className="border-[hsl(var(--border-default))]"
                                  disabled={currentVariantsPage <= 1}
                                  onClick={() => setVariantsPage((prev) => Math.max(1, prev - 1))}
                                >
                                  <ChevronLeft className="h-4 w-4" />
                                </Button>
                                <div className="min-w-[96px] text-center text-sm text-fg-primary">
                                  {currentVariantsPage} / {totalVariantPages}
                                </div>
                                <Button
                                  variant="outline"
                                  size="sm"
                                  className="border-[hsl(var(--border-default))]"
                                  disabled={currentVariantsPage >= totalVariantPages}
                                  onClick={() => setVariantsPage((prev) => Math.min(totalVariantPages, prev + 1))}
                                >
                                  <ChevronRight className="h-4 w-4" />
                                </Button>
                              </div>
                            </div>
                          </div>
                        </div>
                      )}

                      {variantsTotalCount > 0 && (
                        <div className="mt-4 overflow-hidden rounded-xl border border-[hsl(var(--border-subtle))]">
                          <Table>
                            <TableHeader>
                              <TableRow className="border-[hsl(var(--border-subtle))] bg-surface-tertiary hover:bg-surface-tertiary">
                                <TableHead className="w-[48px] text-fg-muted">#</TableHead>
                                <TableHead className="w-[320px] text-fg-muted">Название</TableHead>
                                <TableHead className="text-fg-muted">Fingerprint</TableHead>
                                <TableHead className="text-fg-muted">Верификация</TableHead>
                              </TableRow>
                            </TableHeader>
                            <TableBody>
                              {variantItems.map((v) => {
                                const rep = verificationByVariantId.get(v.id);
                                const hasWarnings = rep != null && rep.passed && rep.issuesCount > 0;
                                const badgeClass = rep == null
                                  ? 'border-fg-muted/30 text-fg-muted'
                                  : rep.passed
                                    ? hasWarnings ? 'border-warning/30 bg-warning/10 text-warning' : 'border-success/30 bg-success/10 text-success'
                                    : 'border-destructive/30 bg-destructive/10 text-destructive';
                                const badgeText = rep == null ? 'Не проверен'
                                  : rep.passed ? `${hasWarnings ? 'С замечаниями' : 'Пройден'}${rep.overall != null ? ` (${rep.overall}/10)` : ''}`
                                  : `Не пройден${rep.overall != null ? ` (${rep.overall}/10)` : ''}`;

                                return (
                                  <TableRow key={v.id} className="border-[hsl(var(--border-subtle))] hover:bg-surface-hover/50">
                                    <TableCell className="text-fg-muted">{v.variantIndex}</TableCell>
                                    <TableCell>
                                      <Dialog>
                                        <DialogTrigger asChild>
                                          <button className="text-left text-sm font-medium text-purple hover:text-purple-hover transition-colors">
                                            {v.title}
                                          </button>
                                        </DialogTrigger>
                                        <DialogContent className="max-w-3xl border-[hsl(var(--border-default))] bg-surface-card">
                                          <DialogHeader>
                                            <DialogTitle>Вариант #{v.variantIndex}: {v.title}</DialogTitle>
                                          </DialogHeader>
                                          <div className="max-h-[70vh] overflow-y-auto pr-2">
                                            <div className="mb-2 text-sm text-fg-muted">
                                              Fingerprint: <span className="font-mono text-xs">{v.fingerprint}</span>
                                            </div>
                                            <div className="prose prose-sm max-w-none">
                                              <ReactMarkdown remarkPlugins={[remarkGfm]}>{v.content}</ReactMarkdown>
                                            </div>
                                          </div>
                                        </DialogContent>
                                      </Dialog>
                                    </TableCell>
                                    <TableCell className="font-mono text-xs text-fg-muted">{v.fingerprint}</TableCell>
                                    <TableCell>
                                      <div className="flex items-center gap-2">
                                        <Badge variant="outline" className={badgeClass}>{badgeText}</Badge>
                                        <Button
                                          size="sm"
                                          variant="ghost"
                                          className="h-7 text-xs text-fg-secondary"
                                          onClick={async () => {
                                            const j = await verifyJob.mutateAsync({ variantId: v.id });
                                            jobCenter.addJob(j.id, `Lab #${labId}: Verify variant #${v.variantIndex}`, { endpoint: 'POST /labs/{labId}/verification/verify', labId, body: { variantId: v.id } });
                                            setActiveJobId(j.id);
                                          }}
                                          disabled={verifyJob.isPending || hasActiveJob || llmAccessBlocked}
                                        >
                                          Проверить
                                        </Button>
                                      </div>
                                    </TableCell>
                                  </TableRow>
                                );
                              })}
                            </TableBody>
                          </Table>
                        </div>
                      )}
                      {variants.data && variantsTotalCount === 0 && (
                        <p className="mt-4 text-sm text-fg-muted">Вариантов пока нет.</p>
                      )}
                    </div>
                  </div>

                  <div className="overflow-hidden rounded-2xl border border-[hsl(var(--border-subtle))] bg-surface-card">
                    <div className="border-b border-[hsl(var(--border-subtle))] px-5 py-3.5">
                      <span className="text-[15px] font-semibold text-fg-primary">Матрица варьирования</span>
                    </div>
                    <div className="flex flex-col gap-4 p-6">
                      <div className="grid gap-3">
                        <div className="text-sm text-fg-muted">Создать параметр</div>
                        <div className="grid gap-3 sm:grid-cols-2">
                          <Input placeholder="Название" value={newVmName} onChange={(e) => setNewVmName(e.target.value)} className="border-[hsl(var(--border-default))] bg-surface-tertiary" />
                          <Input placeholder="Описание" value={newVmDescription} onChange={(e) => setNewVmDescription(e.target.value)} className="border-[hsl(var(--border-default))] bg-surface-tertiary" />
                        </div>
                        <div className="flex items-center gap-2">
                          <Button
                            variant="outline"
                            className="border-[hsl(var(--border-default))]"
                            disabled={createVariationMethod.isPending || hasActiveJob || !newVmName.trim() || (variationMethods.data ?? []).some((m) => m.name.toLowerCase() === newVmName.trim().toLowerCase())}
                            onClick={async () => {
                              await createVariationMethod.mutateAsync({ name: newVmName.trim(), description: newVmDescription.trim() || null });
                              setNewVmName('');
                              setNewVmDescription('');
                            }}
                          >
                            {createVariationMethod.isPending ? 'Создание...' : 'Создать'}
                          </Button>
                          <ErrorMsg error={createVariationMethod.error} />
                        </div>
                      </div>

                      <div className="grid gap-3">
                        <div className="text-sm text-fg-muted">Добавить метод варьирования</div>
                        <div className="flex flex-col gap-2">
                          <select
                            className="h-10 w-full rounded-lg border border-[hsl(var(--border-default))] bg-surface-tertiary px-3 text-sm text-fg-primary"
                            value={vmSelectedId}
                            onChange={(e) => setVmSelectedId(e.target.value)}
                            disabled={upsertLabVariationMethods.isPending || hasActiveJob}
                          >
                            <option value="">Выберите...</option>
                            {(variationMethods.data ?? []).map((m) => (
                              <option key={m.id} value={String(m.id)}>
                                {m.name} ({m.code}){m.description ? ` — ${m.description}` : ''}
                              </option>
                            ))}
                          </select>
                          <Button
                            variant="outline"
                            className="w-fit border-[hsl(var(--border-default))]"
                            disabled={!vmSelectedId || upsertLabVariationMethods.isPending || hasActiveJob}
                            onClick={() => {
                              const id = Number(vmSelectedId);
                              if (!Number.isFinite(id) || id <= 0 || vmDraft.some((x) => x.variationMethodId === id)) return;
                              updateVmDraft((prev) => prev.concat({ variationMethodId: id, preserveAcrossLabs: false }));
                              setVmSelectedId('');
                            }}
                          >
                            Добавить
                          </Button>
                        </div>
                      </div>

                      {vmDraft.length > 0 && (
                        <div className="overflow-hidden rounded-xl border border-[hsl(var(--border-subtle))]">
                          <Table>
                            <TableHeader>
                              <TableRow className="border-[hsl(var(--border-subtle))] bg-surface-tertiary hover:bg-surface-tertiary">
                                <TableHead className="text-fg-muted">Метод</TableHead>
                                <TableHead className="text-fg-muted">Перенос</TableHead>
                                <TableHead className="text-fg-muted">Description</TableHead>
                                <TableHead className="text-fg-muted"></TableHead>
                              </TableRow>
                            </TableHeader>
                            <TableBody>
                              {vmDraft.map((row) => {
                                const method = (variationMethods.data ?? []).find((m) => m.id === row.variationMethodId);
                                const descriptionValue = getVmDescriptionValue(row.variationMethodId, method?.description);
                                const descriptionChanged = (descriptionValue.trim() || '') !== ((method?.description ?? '').trim());
                                return (
                                  <TableRow key={row.variationMethodId} className="border-[hsl(var(--border-subtle))] hover:bg-surface-hover/50">
                                    <TableCell>
                                      <div className="font-medium text-fg-primary">{method?.name ?? `#${row.variationMethodId}`}</div>
                                      <div className="text-xs text-fg-muted">{method?.code ?? ''}</div>
                                    </TableCell>
                                    <TableCell>
                                      <label className="flex items-center gap-2 text-sm text-fg-secondary">
                                        <input
                                          type="checkbox"
                                          checked={row.preserveAcrossLabs}
                                          className="accent-purple"
                                          disabled={upsertLabVariationMethods.isPending || hasActiveJob}
                                          onChange={(e) => updateVmDraft((prev) => prev.map((x) => x.variationMethodId === row.variationMethodId ? { ...x, preserveAcrossLabs: e.target.checked } : x))}
                                        />
                                        <span>Keep value</span>
                                      </label>
                                    </TableCell>
                                    <TableCell>
                                      <div className="grid gap-2">
                                        <Textarea
                                          value={descriptionValue}
                                          onChange={(e) => setVmDescriptionDraft((prev) => ({ ...prev, [row.variationMethodId]: e.target.value }))}
                                          rows={3}
                                          disabled={!method || updateVariationMethod.isPending || hasActiveJob}
                                          className="min-w-[18rem] border-[hsl(var(--border-default))] bg-surface-tertiary text-sm"
                                        />
                                        <div className="flex items-center gap-2">
                                          <Button
                                            variant="outline"
                                            size="sm"
                                            className="h-8 border-[hsl(var(--border-default))]"
                                            disabled={!method || updateVariationMethod.isPending || hasActiveJob || !descriptionChanged}
                                            onClick={async () => {
                                              if (!method) return;
                                              await updateVariationMethod.mutateAsync({
                                                id: method.id,
                                                data: {
                                                  code: method.code,
                                                  name: method.name,
                                                  description: descriptionValue.trim() || null,
                                                },
                                              });
                                              setVmDescriptionDraft((prev) => {
                                                const next = { ...prev };
                                                delete next[row.variationMethodId];
                                                return next;
                                              });
                                            }}
                                          >
                                            {updateVariationMethod.isPending ? 'Saving...' : 'Save description'}
                                          </Button>
                                          {method?.isSystem && (
                                            <Badge variant="outline" className="border-[hsl(var(--border-default))] text-fg-muted">
                                              System
                                            </Badge>
                                          )}
                                        </div>
                                      </div>
                                    </TableCell>
                                    <TableCell>
                                      <Button
                                        variant="ghost"
                                        size="sm"
                                        className="h-7 text-xs text-fg-muted hover:text-destructive"
                                        disabled={upsertLabVariationMethods.isPending || hasActiveJob}
                                        onClick={() => updateVmDraft((prev) => prev.filter((x) => x.variationMethodId !== row.variationMethodId))}
                                      >
                                        Убрать
                                      </Button>
                                    </TableCell>
                                  </TableRow>
                                );
                              })}
                            </TableBody>
                          </Table>
                        </div>
                      )}
                      <ErrorMsg error={updateVariationMethod.error} />

                      <div className="flex items-center gap-2">
                        <Button
                          className="bg-purple text-fg-inverse hover:bg-purple-hover"
                          disabled={upsertLabVariationMethods.isPending || hasActiveJob}
                          onClick={async () => {
                            await upsertLabVariationMethods.mutateAsync({ items: vmDraft.map((x) => ({ variationMethodId: x.variationMethodId, preserveAcrossLabs: x.preserveAcrossLabs })) });
                            setVmDraftOverride(null);
                          }}
                        >
                          {upsertLabVariationMethods.isPending ? 'Сохранение...' : 'Сохранить матрицу'}
                        </Button>
                        <ErrorMsg error={upsertLabVariationMethods.error} />
                      </div>
                    </div>
                  </div>

                  <div className="overflow-hidden rounded-2xl border border-[hsl(var(--border-subtle))] bg-surface-card">
                    <div className="border-b border-[hsl(var(--border-subtle))] px-5 py-3.5">
                      <span className="text-[15px] font-semibold text-fg-primary">Целевая сложность</span>
                    </div>
                    <div className="flex flex-col gap-4 p-6">
                      {difficultyTarget.isLoading ? (
                        <div className="text-sm text-fg-muted">Загрузка...</div>
                      ) : (
                        <>
                          <div className="grid gap-4 sm:grid-cols-3">
                            <div className="grid gap-1.5">
                              <div className="text-sm text-fg-muted">Сложность</div>
                              <select
                                className="h-10 w-full rounded-lg border border-[hsl(var(--border-default))] bg-surface-tertiary px-3 text-sm text-fg-primary"
                                value={dtComplexity}
                                onChange={(e) => setDifficultyDraft((prev) => ({
                                  complexity: e.target.value,
                                  hoursMin: prev?.hoursMin ?? dtHoursMin,
                                  hoursMax: prev?.hoursMax ?? dtHoursMax,
                                }))}
                              >
                                <option value="low">Низкая</option>
                                <option value="medium">Средняя</option>
                                <option value="high">Высокая</option>
                              </select>
                            </div>
                            <div className="grid gap-1.5">
                              <div className="text-sm text-fg-muted">Мин. часов</div>
                              <Input
                                type="number"
                                min={0}
                                value={dtHoursMin}
                                onChange={(e) => setDifficultyDraft((prev) => ({
                                  complexity: prev?.complexity ?? dtComplexity,
                                  hoursMin: e.target.value,
                                  hoursMax: prev?.hoursMax ?? dtHoursMax,
                                }))}
                                className="border-[hsl(var(--border-default))] bg-surface-tertiary"
                              />
                            </div>
                            <div className="grid gap-1.5">
                              <div className="text-sm text-fg-muted">Макс. часов</div>
                              <Input
                                type="number"
                                min={0}
                                value={dtHoursMax}
                                onChange={(e) => setDifficultyDraft((prev) => ({
                                  complexity: prev?.complexity ?? dtComplexity,
                                  hoursMin: prev?.hoursMin ?? dtHoursMin,
                                  hoursMax: e.target.value,
                                }))}
                                className="border-[hsl(var(--border-default))] bg-surface-tertiary"
                              />
                            </div>
                          </div>
                          <div className="flex items-center gap-2">
                            <Button
                              className="bg-purple text-fg-inverse hover:bg-purple-hover"
                              disabled={setDifficultyTarget.isPending || hasActiveJob || !dtComplexity || Number(dtHoursMin) < 0 || Number(dtHoursMax) < Number(dtHoursMin)}
                              onClick={async () => {
                                await setDifficultyTarget.mutateAsync({ complexity: dtComplexity, estimatedHoursMin: Number(dtHoursMin), estimatedHoursMax: Number(dtHoursMax) });
                                setDifficultyDraft(null);
                              }}
                            >
                              {setDifficultyTarget.isPending ? 'Сохранение...' : 'Сохранить'}
                            </Button>
                            {difficultyTarget.data?.isOverridden && (
                              <Button variant="outline" className="border-[hsl(var(--border-default))]" disabled={resetDifficultyTarget.isPending || hasActiveJob} onClick={async () => { await resetDifficultyTarget.mutateAsync(); setDifficultyDraft(null); }}>
                                {resetDifficultyTarget.isPending ? 'Сброс...' : 'Сбросить'}
                              </Button>
                            )}
                            <ErrorMsg error={setDifficultyTarget.error} />
                            <ErrorMsg error={resetDifficultyTarget.error} />
                          </div>
                        </>
                      )}
                    </div>
                  </div>
                </div>
              )}

              {activeTab === 'materials' && (
                <div className="overflow-hidden rounded-2xl border border-[hsl(var(--border-subtle))] bg-surface-card">
                  <div className="border-b border-[hsl(var(--border-subtle))] px-5 py-3.5">
                    <span className="text-[15px] font-semibold text-fg-primary">Теория и контрольные вопросы</span>
                  </div>
                  <div className="flex flex-col gap-4 p-6">
                    <div className="flex flex-wrap items-center gap-3">
                      <Badge variant="outline" className={supplementaryMaterialReady ? 'border-success/40 bg-success/10 text-success' : 'border-fg-muted/30 text-fg-muted'}>
                        {supplementaryMaterialReady ? 'Готово' : 'Не создано'}
                      </Badge>
                      <Button
                        className="bg-purple text-fg-inverse hover:bg-purple-hover"
                        onClick={async () => {
                          const j = await genSupplementaryMaterialJob.mutateAsync({ force: false });
                          jobCenter.addJob(j.id, `Lab #${labId}: Generate materials`, { endpoint: 'POST /labs/{labId}/supplementary-material/generate', labId });
                          setActiveJobId(j.id);
                        }}
                        disabled={genSupplementaryMaterialJob.isPending || hasActiveJob || !masterApproved || !hasVariants || llmAccessBlocked}
                      >
                        {genSupplementaryMaterialJob.isPending ? 'Запуск...' : 'Генерировать материалы'}
                      </Button>
                      {supplementaryMaterialReady && (
                        <Button
                          variant="outline"
                          className="border-[hsl(var(--border-default))]"
                          onClick={async () => {
                            const j = await genSupplementaryMaterialJob.mutateAsync({ force: true });
                            jobCenter.addJob(j.id, `Lab #${labId}: Regenerate materials`, { endpoint: 'POST /labs/{labId}/supplementary-material/generate', labId });
                            setActiveJobId(j.id);
                          }}
                          disabled={genSupplementaryMaterialJob.isPending || hasActiveJob || !masterApproved || !hasVariants || llmAccessBlocked}
                        >
                          Перегенерировать
                        </Button>
                      )}
                      <PromptSectionEditor sectionKey="material_requirements" label="Настройки промпта" />
                      <ErrorMsg error={genSupplementaryMaterialJob.error} />
                    </div>

                    {!masterApproved && <div className="rounded-xl border border-dashed border-[hsl(var(--border-default))] p-4 text-sm text-fg-muted">Сначала утвердите мастер-задание.</div>}
                    {masterApproved && !hasVariants && <div className="rounded-xl border border-dashed border-[hsl(var(--border-default))] p-4 text-sm text-fg-muted">Сначала сгенерируйте варианты.</div>}

                    {supplementaryMaterial.isLoading && <p className="text-sm text-fg-muted">Загрузка...</p>}
                    <ErrorMsg error={supplementaryMaterial.error} />

                    {supplementaryMaterial.data && (
                      <div className="grid gap-5 lg:grid-cols-[1.3fr_1fr]">
                        <div className="rounded-xl border border-[hsl(var(--border-subtle))] bg-surface-primary p-5">
                          <div className="mb-3 text-base font-semibold text-fg-primary">Теоретические сведения</div>
                          <div className="prose prose-sm max-w-none">
                            <ReactMarkdown remarkPlugins={[remarkGfm]}>{supplementaryMaterial.data.theoryMarkdown}</ReactMarkdown>
                          </div>
                        </div>
                        <div className="rounded-xl border border-[hsl(var(--border-subtle))] bg-surface-tertiary p-5">
                          <div className="mb-3 text-base font-semibold text-fg-primary">Контрольные вопросы</div>
                          <div className="flex flex-col gap-2">
                            {supplementaryMaterial.data.controlQuestions.map((q, i) => (
                              <div key={`${i}-${q}`} className="rounded-lg border border-[hsl(var(--border-subtle))] bg-surface-card px-3 py-2 text-sm text-fg-secondary">
                                <span className="mr-2 font-semibold text-fg-muted">{i + 1}.</span>
                                {q}
                              </div>
                            ))}
                          </div>
                        </div>
                      </div>
                    )}
                  </div>
                </div>
              )}

              {activeTab === 'verification' && (
                <div className="overflow-hidden rounded-2xl border border-[hsl(var(--border-subtle))] bg-surface-card">
                  <div className="border-b border-[hsl(var(--border-subtle))] px-5 py-3.5">
                    <span className="text-[15px] font-semibold text-fg-primary">Верификация</span>
                  </div>
                  <div className="flex flex-col gap-4 p-6">
                    <div className="flex items-center gap-3">
                      <Button
                        className="bg-purple text-fg-inverse hover:bg-purple-hover"
                        onClick={async () => {
                          const j = await verifyJob.mutateAsync({});
                          jobCenter.addJob(j.id, `Lab #${labId}: Verify variants`, { endpoint: 'POST /labs/{labId}/verification/verify', labId });
                          setActiveJobId(j.id);
                        }}
                        disabled={verifyJob.isPending || hasActiveJob || llmAccessBlocked}
                      >
                        {verifyJob.isPending ? 'Запуск...' : 'Верифицировать варианты'}
                      </Button>
                      <Button
                        variant="outline"
                        className="gap-2 border-[hsl(var(--border-default))]"
                        onClick={() => { window.location.href = `/api/labs/${labId}/export/docx`; }}
                        disabled={hasActiveJob}
                      >
                        <Download className="h-4 w-4" />
                        Скачать DOCX
                      </Button>
                      <ErrorMsg error={verifyJob.error} />
                    </div>
                    <p className="text-sm text-fg-muted">
                      Запускает верификацию всех вариантов. Статус и отчёт по каждому варианту отображаются во вкладке «Варианты».
                    </p>
                  </div>
                </div>
              )}
            </motion.div>
          </AnimatePresence>
        </div>

        <div className="w-[320px] shrink-0 flex flex-col gap-5">
          <div className="overflow-hidden rounded-2xl border border-[hsl(var(--border-subtle))] bg-surface-card">
            <div className="border-b border-[hsl(var(--border-subtle))] px-5 py-3.5">
              <span className="text-[15px] font-semibold text-fg-primary">Информация</span>
            </div>
            <div className="flex flex-col gap-4 px-5 py-4">
              <div className="flex items-center justify-between">
                <span className="text-[13px] text-fg-muted">Дисциплина</span>
                <span className="text-[13px] font-medium text-fg-primary">{discipline?.name ?? '—'}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-[13px] text-fg-muted">Порядковый №</span>
                <span className="text-[13px] font-medium text-fg-primary">{lab.data?.orderIndex ?? '—'}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-[13px] text-fg-muted">Статус</span>
                {masterApproved ? (
                  <Badge className="border-purple/30 bg-purple-muted text-purple text-xs">Одобрено</Badge>
                ) : (
                  <Badge variant="outline" className="border-warning/30 text-warning text-xs">Черновик</Badge>
                )}
              </div>
              <div className="flex items-center justify-between">
                <span className="text-[13px] text-fg-muted">Вариантов</span>
                <span className="text-[13px] font-medium text-fg-primary">{variantsTotalCount}</span>
              </div>
            </div>
          </div>

          <div className="overflow-hidden rounded-2xl border border-[hsl(var(--border-subtle))] bg-surface-card">
            <div className="border-b border-[hsl(var(--border-subtle))] px-5 py-3.5">
              <span className="text-[15px] font-semibold text-fg-primary">Задачи генерации</span>
            </div>
            <div className="flex flex-col gap-2.5 p-4">
              {jobItems.length === 0 ? (
                <div className="px-2 py-4 text-center text-sm text-fg-muted">Нет задач</div>
              ) : (
                jobItems.map((item, i) => (
                  <motion.div
                    key={i}
                    initial={{ opacity: 0, x: -8 }}
                    animate={{ opacity: 1, x: 0 }}
                    transition={{ delay: i * 0.08 }}
                    className="flex items-center gap-2.5 rounded-lg bg-surface-tertiary px-3 py-2.5"
                  >
                    {item.status === 'done' && <CheckCircle2 className="h-4 w-4 shrink-0 text-success" />}
                    {item.status === 'running' && <Loader2 className="h-4 w-4 shrink-0 text-warning animate-spin" />}
                    {item.status === 'pending' && <CircleAlert className="h-4 w-4 shrink-0 text-fg-muted" />}
                    <div className="min-w-0 flex-1">
                      <div className="text-[13px] font-medium text-fg-primary">{item.label}</div>
                      <div className={`font-mono text-[11px] ${
                        item.status === 'done' ? 'text-success'
                        : item.status === 'running' ? 'text-warning'
                        : 'text-fg-muted'
                      }`}>
                        {item.detail}
                      </div>
                    </div>
                  </motion.div>
                ))
              )}
            </div>
          </div>
        </div>
      </div>
    </PageWrapper>
  );
}
