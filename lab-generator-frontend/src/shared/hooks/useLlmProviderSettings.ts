'use client';

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { http } from '@/shared/api/http';
import type { LlmProviderSettings, UpsertLlmProviderSettingsRequest } from '@/shared/contracts/llm-provider-settings';

export function useLlmProviderSettings(provider: string) {
  return useQuery({
    queryKey: ['llm-provider-settings', provider],
    enabled: !!provider,
    queryFn: async () => {
      const r = await http.get<LlmProviderSettings>(`/admin/llm-provider-settings/${encodeURIComponent(provider)}`);
      return r.data;
    },
  });
}

export function useUpsertLlmProviderSettings(provider: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (input: UpsertLlmProviderSettingsRequest) => {
      const r = await http.put<LlmProviderSettings>(
        `/admin/llm-provider-settings/${encodeURIComponent(provider)}`,
        input,
      );
      return r.data;
    },
    onSuccess: async () => {
      await qc.invalidateQueries({ queryKey: ['llm-provider-settings', provider] });
    },
  });
}
