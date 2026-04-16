import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { http } from '@/shared/api/http';
import {
  CreateDisciplineRequest,
  CreateDisciplineRequestSchema,
  Discipline,
  DisciplineSchema,
} from '@/shared/contracts/disciplines';
import { formatAxiosError } from '@/shared/utils/formatAxiosError';
import { z } from 'zod';

const DisciplinesListSchema = z.array(DisciplineSchema);

export function useDisciplines() {
  return useQuery({
    queryKey: ['disciplines'],
    queryFn: async (): Promise<Discipline[]> => {
      const res = await http.get('/disciplines');
      return DisciplinesListSchema.parse(res.data);
    },
  });
}

export function useCreateDiscipline() {
  const qc = useQueryClient();

  return useMutation({
    mutationFn: async (input: CreateDisciplineRequest): Promise<Discipline> => {
      const payload = CreateDisciplineRequestSchema.parse(input);
      const res = await http.post('/disciplines', payload);
      return DisciplineSchema.parse(res.data);
    },
    onSuccess: async () => {
      await qc.invalidateQueries({ queryKey: ['disciplines'] });
    },
  });
}

export function useDeleteDiscipline() {
  const qc = useQueryClient();

  return useMutation({
    mutationFn: async (disciplineId: number): Promise<void> => {
      await http.delete(`/disciplines/${disciplineId}`);
    },
    onSuccess: async () => {
      await Promise.all([
        qc.invalidateQueries({ queryKey: ['disciplines'] }),
        qc.invalidateQueries({ queryKey: ['labs'] }),
      ]);
    },
    throwOnError: false,
    meta: {
      formatError: formatAxiosError,
    },
  });
}
