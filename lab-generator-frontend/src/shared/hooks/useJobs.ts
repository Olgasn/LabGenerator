import { useQuery } from '@tanstack/react-query';
import { http } from '@/shared/api/http';
import { GenerationJob, GenerationJobSchema, JobStatus } from '@/shared/contracts/jobs';

export function useJob(jobId: number | null | undefined) {
  return useQuery({
    queryKey: ['jobs', jobId],
    enabled: typeof jobId === 'number' && Number.isFinite(jobId),
    queryFn: async (): Promise<GenerationJob> => {
      const res = await http.get(`/jobs/${jobId}`);
      return GenerationJobSchema.parse(res.data);
    },
    refetchInterval: (query) => {
      const data = query.state.data;
      if (!data) return 1000;
      if (data.status === JobStatus.Succeeded || data.status === JobStatus.Failed || data.status === JobStatus.Canceled)
        return false;
      return 1000;
    },
  });
}