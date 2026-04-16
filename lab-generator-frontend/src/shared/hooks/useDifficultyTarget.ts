import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { http } from '@/shared/api/http';

export interface DifficultyTarget {
  complexity: string;
  estimatedHoursMin: number;
  estimatedHoursMax: number;
  isOverridden: boolean;
}

function normalise(d: Record<string, unknown>): DifficultyTarget {
  return {
    complexity: (d.Complexity ?? d.complexity ?? 'medium') as string,
    estimatedHoursMin: (d.EstimatedHoursMin ?? d.estimatedHoursMin ?? 5) as number,
    estimatedHoursMax: (d.EstimatedHoursMax ?? d.estimatedHoursMax ?? 7) as number,
    isOverridden: (d.IsOverridden ?? d.isOverridden ?? false) as boolean,
  };
}

export function useDifficultyTarget(labId: number) {
  return useQuery<DifficultyTarget>({
    queryKey: ['difficulty-target', labId],
    queryFn: async () => {
      const res = await http.get(`/labs/${labId}/difficulty-target`);
      return normalise(res.data as Record<string, unknown>);
    },
    enabled: Number.isFinite(labId) && labId > 0,
  });
}

export function useSetDifficultyTarget(labId: number) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (data: {
      complexity: string;
      estimatedHoursMin: number;
      estimatedHoursMax: number;
    }) => {
      await http.put(`/labs/${labId}/difficulty-target`, {
        Complexity: data.complexity,
        EstimatedHoursMin: data.estimatedHoursMin,
        EstimatedHoursMax: data.estimatedHoursMax,
      });
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['difficulty-target', labId] }),
  });
}

export function useResetDifficultyTarget(labId: number) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async () => {
      await http.delete(`/labs/${labId}/difficulty-target`);
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['difficulty-target', labId] }),
  });
}
