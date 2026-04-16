import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { http } from '@/shared/api/http';
import { GenerationJob, GenerationJobSchema } from '@/shared/contracts/jobs';
import { createPagedResponseSchema } from '@/shared/contracts/pagination';
import {
  AssignmentVariantSchema,
  GenerateVariantsRequest,
  GenerateVariantsRequestSchema,
} from '@/shared/contracts/variants';
const PagedVariantsSchema = createPagedResponseSchema(AssignmentVariantSchema);

export type VariantsSortOption = 'asc' | 'desc';

export type GetVariantsParams = {
  page?: number;
  pageSize?: number;
  sort?: VariantsSortOption;
};

export function useVariants(labId: number, params: GetVariantsParams) {
  return useQuery({
    queryKey: ['labs', labId, 'variants', params],
    enabled: Number.isFinite(labId) && labId > 0,
    queryFn: async () => {
      const res = await http.get(`/labs/${labId}/variants`, { params });
      return PagedVariantsSchema.parse(res.data);
    },
    placeholderData: keepPreviousData,
  });
}

export function useGenerateVariantsJob(labId: number) {
  const qc = useQueryClient();

  return useMutation({
    mutationFn: async (input: GenerateVariantsRequest): Promise<GenerationJob> => {
      const payload = GenerateVariantsRequestSchema.parse(input);
      const res = await http.post(`/labs/${labId}/variants/generate`, payload);
      return GenerationJobSchema.parse(res.data);
    },
    onSuccess: async () => {
      await qc.invalidateQueries({ queryKey: ['labs', labId, 'variants'] });
    },
  });
}
