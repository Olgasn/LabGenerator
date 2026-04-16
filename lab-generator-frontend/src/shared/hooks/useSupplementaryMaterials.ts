import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { http } from '@/shared/api/http';
import { GenerationJob, GenerationJobSchema } from '@/shared/contracts/jobs';
import {
  GenerateSupplementaryMaterialRequest,
  GenerateSupplementaryMaterialRequestSchema,
  LabSupplementaryMaterial,
  LabSupplementaryMaterialSchema,
} from '@/shared/contracts/supplementaryMaterials';

export function useSupplementaryMaterial(labId: number) {
  return useQuery({
    queryKey: ['labs', labId, 'supplementary-material'],
    queryFn: async (): Promise<LabSupplementaryMaterial | null> => {
      try {
        const res = await http.get(`/labs/${labId}/supplementary-material`);
        return LabSupplementaryMaterialSchema.parse(res.data);
      } catch (e: unknown) {
        const err = e as { response?: { status?: number } };
        if (err?.response?.status === 404) return null;
        throw e;
      }
    },
  });
}

export function useGenerateSupplementaryMaterialJob(labId: number) {
  const qc = useQueryClient();

  return useMutation({
    mutationFn: async (input: GenerateSupplementaryMaterialRequest): Promise<GenerationJob> => {
      const payload = GenerateSupplementaryMaterialRequestSchema.parse(input);
      const res = await http.post(`/labs/${labId}/supplementary-material/generate`, payload);
      return GenerationJobSchema.parse(res.data);
    },
    onSuccess: async () => {
      await qc.invalidateQueries({ queryKey: ['labs', labId, 'supplementary-material'] });
    },
  });
}
