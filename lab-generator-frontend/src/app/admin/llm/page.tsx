'use client';

import { useState } from 'react';
import { motion } from 'framer-motion';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { useLlmSettings, useUpdateLlmSettings } from '@/shared/hooks/useLlmSettings';
import { useLlmProviderSettings, useUpsertLlmProviderSettings } from '@/shared/hooks/useLlmProviderSettings';
import { PageWrapper } from '@/components/page-wrapper';
import { Save, Server, Globe, Check } from 'lucide-react';

const providers = [
  { id: 'Ollama', label: 'Ollama', icon: Server, description: 'Локальный провайдер' },
  { id: 'OpenRouter', label: 'OpenRouter', icon: Globe, description: 'Облачный провайдер' },
];

const knownModels: Record<string, string[]> = {
  Ollama: ['cogito-2.1:671b-cloud'],
  OpenRouter: ['openai/gpt-4o-mini'],
};

const modelPlaceholders: Record<string, string> = {
  Ollama: 'cogito-2.1:671b-cloud',
  OpenRouter: 'openai/gpt-4o-mini',
};

export default function AdminLlmSettingsPage() {
  const q = useLlmSettings();
  const save = useUpdateLlmSettings();

  const [draftProvider, setDraftProvider] = useState<string | null>(null);
  const [draftProviderModel, setDraftProviderModel] = useState<string | null>(null);
  const [draftApiKey, setDraftApiKey] = useState<string>('');
  const [clearApiKey, setClearApiKey] = useState(false);
  const [draftTemperature, setDraftTemperature] = useState<string | null>(null);
  const [draftMaxTokens, setDraftMaxTokens] = useState<string | null>(null);

  const currentProvider = q.data?.provider ?? '';
  const effectiveProvider = draftProvider ?? currentProvider;

  const providerQ = useLlmProviderSettings(effectiveProvider);
  const providerSave = useUpsertLlmProviderSettings(effectiveProvider);
  const activeProviderSettings = useLlmProviderSettings(currentProvider);

  if (q.isLoading) {
    return (
      <PageWrapper>
        <div className="text-sm text-fg-muted">Загрузка...</div>
      </PageWrapper>
    );
  }

  if (q.error) {
    return (
      <PageWrapper>
        <div className="text-sm text-destructive">{String(q.error)}</div>
      </PageWrapper>
    );
  }

  const current = q.data!;
  const displayProvider = draftProvider ?? current.provider;

  const providerSettings = providerQ.data;
  const effectiveProviderModel = draftProviderModel ?? providerSettings?.model ?? '';
  const effectiveApiKey = draftApiKey;
  const effectiveTemperature = draftTemperature ?? (providerSettings?.temperature?.toString() ?? '');
  const effectiveMaxTokens = draftMaxTokens ?? (providerSettings?.maxOutputTokens?.toString() ?? '');

  const activeData = activeProviderSettings.data;
  const activeModel = (current.model || '').trim() || (activeData?.model ?? '') || '(по умолчанию)';
  const activeTemp = activeData?.temperature ?? null;
  const activeMax = activeData?.maxOutputTokens ?? null;
  const activeHasApiKey = activeData?.hasApiKey ?? false;

  const models = knownModels[displayProvider] ?? [];

  return (
    <PageWrapper>
      <div className="flex items-start justify-between">
        <div>
          <h1 className="text-[28px] font-bold text-fg-primary">Настройки</h1>
          <p className="mt-1 text-sm text-fg-muted">Конфигурация языковых моделей для генерации заданий</p>
        </div>
        <Button
          className="gap-2 bg-purple text-fg-inverse hover:bg-purple-hover"
          disabled={save.isPending || providerSave.isPending}
          onClick={async () => {
            const t = effectiveTemperature.trim();
            const mt = effectiveMaxTokens.trim();
            await save.mutateAsync({ provider: displayProvider, model: '' });
            await providerSave.mutateAsync({
              provider: displayProvider,
              model: effectiveProviderModel.trim() || null,
              apiKey: clearApiKey ? null : (effectiveApiKey.trim() || null),
              clearApiKey,
              temperature: t ? Number(t) : null,
              maxOutputTokens: mt ? Number(mt) : null,
            });
            setDraftApiKey('');
            setClearApiKey(false);
            void q.refetch();
            void providerQ.refetch();
          }}
        >
          <Save className="h-4 w-4" />
          {save.isPending || providerSave.isPending ? 'Сохранение...' : 'Сохранить'}
        </Button>
      </div>

      <div className="flex gap-7">
        <div className="flex flex-1 flex-col gap-6">
          <div className="overflow-hidden rounded-2xl border border-[hsl(var(--border-subtle))] bg-surface-card">
            <div className="border-b border-[hsl(var(--border-subtle))] px-5 py-3.5">
              <span className="text-[15px] font-semibold text-fg-primary">Провайдер LLM</span>
              <p className="mt-0.5 text-xs text-fg-muted">Выберите провайдера языковой модели</p>
            </div>
            <div className="flex gap-4 p-6">
              {providers.map((p) => {
                const isActive = displayProvider === p.id;
                return (
                  <motion.button
                    key={p.id}
                    whileHover={{ scale: 1.02 }}
                    whileTap={{ scale: 0.98 }}
                    onClick={() => {
                      setDraftProvider(p.id);
                      setDraftProviderModel(null);
                      setDraftApiKey('');
                      setClearApiKey(false);
                      setDraftTemperature(null);
                      setDraftMaxTokens(null);
                    }}
                    className={`relative flex flex-1 flex-col items-center gap-3 rounded-xl border-2 p-6 transition-all duration-200 ${
                      isActive
                        ? 'border-purple bg-purple-muted shadow-lg shadow-purple/10'
                        : 'border-[hsl(var(--border-default))] bg-surface-tertiary hover:border-[hsl(var(--border-default))]'
                    }`}
                  >
                    {isActive && (
                      <div className="absolute right-3 top-3">
                        <div className="flex h-5 w-5 items-center justify-center rounded-full bg-purple">
                          <Check className="h-3 w-3 text-fg-inverse" />
                        </div>
                      </div>
                    )}
                    <div className={`flex h-12 w-12 items-center justify-center rounded-xl ${
                      isActive ? 'bg-purple/20' : 'bg-surface-hover'
                    }`}>
                      <p.icon className={`h-6 w-6 ${isActive ? 'text-purple' : 'text-fg-muted'}`} />
                    </div>
                    <span className={`text-sm font-medium ${isActive ? 'text-fg-primary' : 'text-fg-secondary'}`}>
                      {p.label}
                    </span>
                  </motion.button>
                );
              })}
            </div>
          </div>

          <div className="overflow-hidden rounded-2xl border border-[hsl(var(--border-subtle))] bg-surface-card">
            <div className="border-b border-[hsl(var(--border-subtle))] px-5 py-3.5">
              <span className="text-[15px] font-semibold text-fg-primary">Параметры модели</span>
              <p className="mt-0.5 text-xs text-fg-muted">Настройка параметров генерации</p>
            </div>
            <div className="flex flex-col gap-5 p-6">
              {providerQ.isLoading ? (
                <div className="text-sm text-fg-muted">Загрузка...</div>
              ) : providerQ.error ? (
                <div className="text-sm text-destructive">{String(providerQ.error)}</div>
              ) : (
                <>
                  <div className="grid gap-1.5">
                    <div className="text-sm text-fg-muted">Модель</div>
                    <Input
                      value={effectiveProviderModel}
                      onChange={(e) => setDraftProviderModel(e.target.value)}
                      placeholder={modelPlaceholders[displayProvider] ?? ''}
                      className="border-[hsl(var(--border-default))] bg-surface-tertiary"
                    />
                  </div>
                  <div className="grid gap-1.5">
                    <div className="flex items-center justify-between gap-3">
                      <div className="text-sm text-fg-muted">API key</div>
                      {providerSettings?.hasApiKey && !clearApiKey ? (
                        <span className="font-mono text-xs text-fg-muted">
                          {providerSettings.apiKeyMasked ?? 'Сохранен'}
                        </span>
                      ) : null}
                    </div>
                    <Input
                      type="password"
                      value={effectiveApiKey}
                      onChange={(e) => {
                        setDraftApiKey(e.target.value);
                        setClearApiKey(false);
                      }}
                      placeholder={providerSettings?.hasApiKey ? 'Введите новый ключ, если нужно заменить текущий' : 'Введите API key'}
                      className="border-[hsl(var(--border-default))] bg-surface-tertiary"
                    />
                    <div className="flex items-center justify-between gap-3">
                      <span className="text-xs text-fg-muted">
                        Ключ хранится на сервере и не отображается в открытом виде.
                      </span>
                      {providerSettings?.hasApiKey ? (
                        <Button
                          type="button"
                          variant="ghost"
                          size="sm"
                          className="h-auto px-2 py-1 text-xs text-destructive hover:text-destructive"
                          onClick={() => {
                            setDraftApiKey('');
                            setClearApiKey(true);
                          }}
                        >
                          Очистить
                        </Button>
                      ) : null}
                    </div>
                    {clearApiKey ? (
                      <div className="text-xs text-warning">После сохранения текущий ключ будет удален.</div>
                    ) : null}
                  </div>
                  <div className="grid grid-cols-2 gap-5">
                    <div className="grid gap-1.5">
                      <div className="text-sm text-fg-muted">Температура</div>
                      <Input
                        value={effectiveTemperature}
                        onChange={(e) => setDraftTemperature(e.target.value)}
                        placeholder="0.7"
                        className="border-[hsl(var(--border-default))] bg-surface-tertiary"
                      />
                    </div>
                    <div className="grid gap-1.5">
                      <div className="text-sm text-fg-muted">Макс. Токенов</div>
                      <Input
                        value={effectiveMaxTokens}
                        onChange={(e) => setDraftMaxTokens(e.target.value)}
                        placeholder="4096"
                        className="border-[hsl(var(--border-default))] bg-surface-tertiary"
                      />
                    </div>
                  </div>
                  {effectiveTemperature && (
                    <div className="grid gap-2">
                      <div className="text-xs text-fg-muted">Температура</div>
                      <div className="relative h-2 rounded-full bg-surface-hover">
                        <motion.div
                          className="absolute left-0 top-0 h-2 rounded-full bg-purple"
                          initial={{ width: 0 }}
                          animate={{ width: `${Math.min(100, (Number(effectiveTemperature) || 0) * 50)}%` }}
                          transition={{ duration: 0.3 }}
                        />
                      </div>
                      <div className="flex justify-between text-[11px] text-fg-muted">
                        <span>0</span>
                        <span>{effectiveTemperature}</span>
                      </div>
                    </div>
                  )}
                </>
              )}

              {save.error && <span className="text-sm text-destructive">{String(save.error)}</span>}
              {providerSave.error && <span className="text-sm text-destructive">{String(providerSave.error)}</span>}
            </div>
          </div>
        </div>

        <div className="flex w-[360px] shrink-0 flex-col gap-5">
          <div className="overflow-hidden rounded-2xl border border-[hsl(var(--border-subtle))] bg-surface-card">
            <div className="border-b border-[hsl(var(--border-subtle))] px-5 py-3.5">
              <span className="text-[15px] font-semibold text-fg-primary">Текущая конфигурация</span>
            </div>
            <div className="flex flex-col gap-4 px-5 py-4">
              <div className="flex items-center justify-between">
                <span className="text-[13px] text-fg-muted">Провайдер</span>
                <span className="text-[13px] font-medium text-fg-primary">{current.provider}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-[13px] text-fg-muted">Модель</span>
                <span className="font-mono text-[13px] font-medium text-purple">{activeModel}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-[13px] text-fg-muted">Температура</span>
                <span className="font-mono text-[13px] font-medium text-fg-primary">
                  {activeTemp == null ? '—' : String(activeTemp)}
                </span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-[13px] text-fg-muted">Макс. токенов</span>
                <span className="font-mono text-[13px] font-medium text-fg-primary">
                  {activeMax == null ? '—' : String(activeMax)}
                </span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-[13px] text-fg-muted">API key</span>
                <span className="font-mono text-[13px] font-medium text-fg-primary">
                  {activeData?.apiKeyMasked ?? '—'}
                </span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-[13px] text-fg-muted">Статус</span>
                <Badge className={activeHasApiKey ? 'border-success/30 bg-success/10 text-success text-xs' : 'border-destructive/30 bg-destructive/10 text-destructive text-xs'}>
                  {activeHasApiKey ? 'Ключ задан' : 'Ключ не задан'}
                </Badge>
              </div>
            </div>
          </div>

          <div className="overflow-hidden rounded-2xl border border-[hsl(var(--border-subtle))] bg-surface-card">
            <div className="border-b border-[hsl(var(--border-subtle))] px-5 py-3.5">
              <span className="text-[15px] font-semibold text-fg-primary">Доступные модели</span>
            </div>
            <div className="flex flex-col gap-1.5 p-3">
              {models.map((model) => {
                const isSelected = effectiveProviderModel === model;
                return (
                  <motion.button
                    key={model}
                    whileHover={{ x: 2 }}
                    onClick={() => setDraftProviderModel(model)}
                    className={`flex items-center gap-2 rounded-lg px-3 py-2.5 text-left text-sm transition-colors ${
                      isSelected
                        ? 'bg-purple-muted text-purple font-medium'
                        : 'text-fg-secondary hover:bg-surface-hover/50'
                    }`}
                  >
                    <span className="font-mono text-xs">•</span>
                    {model}
                  </motion.button>
                );
              })}
            </div>
          </div>
        </div>
      </div>
    </PageWrapper>
  );
}
