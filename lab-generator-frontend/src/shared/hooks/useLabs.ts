import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { http } from '@/shared/api/http';
import { CreateLabRequest, CreateLabRequestSchema, Lab, LabSchema } from '@/shared/contracts/labs';
import { createPagedResponseSchema } from '@/shared/contracts/pagination';
import { formatAxiosError } from '@/shared/utils/formatAxiosError';
import { z } from 'zod';

const LabsListSchema = z.array(LabSchema);
const PagedLabsSchema = createPagedResponseSchema(LabSchema);

export type LabsSortOption = 'asc' | 'desc';

export type GetPagedLabsParams = {
  disciplineId?: number;
  search?: string;
  sort?: LabsSortOption;
  page?: number;
  pageSize?: number;
};

export function useLabs() {
  return useQuery({
    queryKey: ['labs'],
    queryFn: async (): Promise<Lab[]> => {
      const res = await http.get('/labs', { params: { all: true } });
      const parsed = PagedLabsSchema.parse(res.data);
      return LabsListSchema.parse(parsed.items);
    },
  });
}

export function usePagedLabs(params: GetPagedLabsParams) {
  return useQuery({
    queryKey: ['labs', 'paged', params],
    queryFn: async () => {
      const res = await http.get('/labs', { params });
      return PagedLabsSchema.parse(res.data);
    },
    placeholderData: keepPreviousData,
  });
}

export function useLab(labId: number) {
  return useQuery({
    queryKey: ['labs', labId],
    enabled: Number.isFinite(labId) && labId > 0,
    queryFn: async (): Promise<Lab> => {
      const res = await http.get(`/labs/${labId}`);
      return LabSchema.parse(res.data);
    },
  });
}

export function useCreateLab() {
  const qc = useQueryClient();

  return useMutation({
    mutationFn: async (input: CreateLabRequest): Promise<Lab> => {
      const payload = CreateLabRequestSchema.parse(input);
      const res = await http.post('/labs', payload);
      return LabSchema.parse(res.data);
    },
    onSuccess: async () => {
      await qc.invalidateQueries({ queryKey: ['labs'] });
    },
  });
}

export function useDeleteLab() {
  const qc = useQueryClient();

  return useMutation({
    mutationFn: async (labId: number): Promise<void> => {
      await http.delete(`/labs/${labId}`);
    },
    onSuccess: async () => {
      await Promise.all([
        qc.invalidateQueries({ queryKey: ['labs'] }),
        qc.invalidateQueries({ queryKey: ['disciplines'] }),
      ]);
    },
    throwOnError: false,
    meta: {
      formatError: formatAxiosError,
    },
  });
}
