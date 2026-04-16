import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { http } from '@/shared/api/http';
import { GenerationJob, GenerationJobSchema } from '@/shared/contracts/jobs';
import {
  MasterAssignment,
  MasterAssignmentSchema,
  UpdateMasterAssignmentRequest,
  UpdateMasterAssignmentRequestSchema,
} from '@/shared/contracts/master';

export function useCurrentMaster(labId: number) {
  return useQuery({
    queryKey: ['labs', labId, 'master', 'current'],
    queryFn: async (): Promise<MasterAssignment | null> => {
      try {
        const res = await http.get(`/labs/${labId}/master`);
        return MasterAssignmentSchema.parse(res.data);
      } catch (e: unknown) {
        const err = e as { response?: { status?: number } };
        if (err?.response?.status === 404) return null;
        throw e;
      }
    },
  });
}

export function useGenerateMasterJob(labId: number) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (): Promise<GenerationJob> => {
      const res = await http.post(`/labs/${labId}/master/generate`);
      return GenerationJobSchema.parse(res.data);
    },
    onSuccess: async () => {
      await qc.invalidateQueries({ queryKey: ['labs', labId, 'master', 'current'] });
    },
  });
}

export function useUpdateMaster(labId: number) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (args: { masterAssignmentId: number; input: UpdateMasterAssignmentRequest }): Promise<MasterAssignment> => {
      const payload = UpdateMasterAssignmentRequestSchema.parse(args.input);
      const res = await http.put(`/labs/${labId}/master/${args.masterAssignmentId}`, payload);
      return MasterAssignmentSchema.parse(res.data);
    },
    onSuccess: async () => {
      await qc.invalidateQueries({ queryKey: ['labs', labId, 'master', 'current'] });
    },
  });
}

export function useApproveMaster(labId: number) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (masterAssignmentId: number): Promise<MasterAssignment> => {
      const res = await http.post(`/labs/${labId}/master/${masterAssignmentId}/approve`);
      return MasterAssignmentSchema.parse(res.data);
    },
    onSuccess: async () => {
      await qc.invalidateQueries({ queryKey: ['labs', labId, 'master', 'current'] });
    },
  });
}