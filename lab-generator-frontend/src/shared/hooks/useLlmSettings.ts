'use client';

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { http } from '@/shared/api/http';
import type { LlmSettings, UpdateLlmSettingsRequest } from '@/shared/contracts/llm-settings';

export function useLlmSettings() {
  return useQuery({
    queryKey: ['llm-settings'],
    queryFn: async () => {
      const r = await http.get<LlmSettings>('/admin/llm-settings');
      return r.data;
    },
  });
}

export function useUpdateLlmSettings() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (input: UpdateLlmSettingsRequest) => {
      const r = await http.put<LlmSettings>('/admin/llm-settings', input);
      return r.data;
    },
    onSuccess: async () => {
      await qc.invalidateQueries({ queryKey: ['llm-settings'] });
    },
  });
}
