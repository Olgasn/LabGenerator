import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { http } from '@/shared/api/http';
import { PromptCustomSection, PromptCustomSectionSchema } from '@/shared/contracts/promptSections';

export function usePromptSection(sectionKey: string) {
  return useQuery({
    queryKey: ['prompt-sections', sectionKey],
    queryFn: async (): Promise<PromptCustomSection> => {
      const res = await http.get(`/prompt-sections/${sectionKey}`);
      return PromptCustomSectionSchema.parse(res.data);
    },
  });
}

export function useUpdatePromptSection(sectionKey: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (content: string): Promise<PromptCustomSection> => {
      const res = await http.put(`/prompt-sections/${sectionKey}`, { content });
      return PromptCustomSectionSchema.parse(res.data);
    },
    onSuccess: async () => {
      await qc.invalidateQueries({ queryKey: ['prompt-sections', sectionKey] });
    },
  });
}

export function useResetPromptSection(sectionKey: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (): Promise<PromptCustomSection> => {
      const res = await http.delete(`/prompt-sections/${sectionKey}`);
      return PromptCustomSectionSchema.parse(res.data);
    },
    onSuccess: async () => {
      await qc.invalidateQueries({ queryKey: ['prompt-sections', sectionKey] });
    },
  });
}
