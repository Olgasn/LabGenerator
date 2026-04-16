import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { http } from '@/shared/api/http';
import {
  CreateVariationMethodRequest,
  CreateVariationMethodRequestSchema,
  UpdateVariationMethodRequest,
  UpdateVariationMethodRequestSchema,
  VariationMethod,
  VariationMethodSchema,
} from '@/shared/contracts/variationMethods';
import { z } from 'zod';

const VariationMethodsListSchema = z.array(VariationMethodSchema);

export function useVariationMethods() {
  return useQuery({
    queryKey: ['variation-methods'],
    queryFn: async (): Promise<VariationMethod[]> => {
      const res = await http.get('/variation-methods');
      return VariationMethodsListSchema.parse(res.data);
    },
  });
}

export function useUpdateVariationMethod() {
  const qc = useQueryClient();

  return useMutation({
    mutationFn: async (input: { id: number; data: UpdateVariationMethodRequest }): Promise<VariationMethod> => {
      const payload = UpdateVariationMethodRequestSchema.parse(input.data);
      const res = await http.put(`/variation-methods/${input.id}`, payload);
      return VariationMethodSchema.parse(res.data);
    },
    onSuccess: async () => {
      await qc.invalidateQueries({ queryKey: ['variation-methods'] });
    },
  });
}

export function useCreateVariationMethod() {
  const qc = useQueryClient();

  return useMutation({
    mutationFn: async (input: CreateVariationMethodRequest): Promise<VariationMethod> => {
      const payload = CreateVariationMethodRequestSchema.parse(input);
      const res = await http.post('/variation-methods', payload);
      return VariationMethodSchema.parse(res.data);
    },
    onSuccess: async () => {
      await qc.invalidateQueries({ queryKey: ['variation-methods'] });
    },
  });
}
