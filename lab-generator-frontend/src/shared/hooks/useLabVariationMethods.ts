import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { http } from '@/shared/api/http';
import {
  LabVariationMethod,
  LabVariationMethodSchema,
  UpsertLabVariationMethodsRequest,
  UpsertLabVariationMethodsRequestSchema,
} from '@/shared/contracts/labVariationMethods';
import { z } from 'zod';

const LabVariationMethodsListSchema = z.array(LabVariationMethodSchema);

export function useLabVariationMethods(labId: number) {
  return useQuery({
    queryKey: ['lab-variation-methods', labId],
    queryFn: async (): Promise<LabVariationMethod[]> => {
      const res = await http.get(`/labs/${labId}/variation-methods`);
      return LabVariationMethodsListSchema.parse(res.data);
    },
    enabled: Number.isFinite(labId) && labId > 0,
  });
}

export function useUpsertLabVariationMethods(labId: number) {
  const qc = useQueryClient();

  return useMutation({
    mutationFn: async (input: UpsertLabVariationMethodsRequest): Promise<LabVariationMethod[]> => {
      const payload = UpsertLabVariationMethodsRequestSchema.parse(input);
      const res = await http.put(`/labs/${labId}/variation-methods`, payload);
      return LabVariationMethodsListSchema.parse(res.data);
    },
    onSuccess: async () => {
      await qc.invalidateQueries({ queryKey: ['lab-variation-methods', labId] });
    },
  });
}
