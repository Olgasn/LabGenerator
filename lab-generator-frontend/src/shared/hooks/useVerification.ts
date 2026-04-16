import { http } from '@/shared/api/http'
import { GenerationJob, GenerationJobSchema } from '@/shared/contracts/jobs'
import { VerificationReport, VerificationReportSchema, VerifyVariantsRequest, VerifyVariantsRequestSchema } from '@/shared/contracts/verification'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { z } from 'zod'

const VerificationReportsSchema = z.array(VerificationReportSchema);

export function useVerifyLabJob(labId: number) {
  const qc = useQueryClient();

  return useMutation({
    mutationFn: async (input: VerifyVariantsRequest): Promise<GenerationJob> => {
      const payload = VerifyVariantsRequestSchema.parse(input);
      const res = await http.post(`/labs/${labId}/verify`, payload);
      return GenerationJobSchema.parse(res.data);
    },
    onSuccess: async () => {
      await qc.invalidateQueries({ queryKey: ['labs', labId, 'variants'] });
      await qc.invalidateQueries({ queryKey: ['labs', labId, 'verification-reports'] });
    },
  });
}

export function useVerificationReport(variantId: number | null | undefined) {
  return useQuery({
    queryKey: ['variants', variantId, 'verification'],
    enabled: typeof variantId === 'number' && Number.isFinite(variantId),
    queryFn: async (): Promise<VerificationReport> => {
      const res = await http.get(`/variants/${variantId}/verification`);
      return VerificationReportSchema.parse(res.data);
    },
  });
}

export function useLabVerificationReports(labId: number) {
  return useQuery({
    queryKey: ['labs', labId, 'verification-reports'],
    enabled: typeof labId === 'number' && Number.isFinite(labId),
    queryFn: async (): Promise<VerificationReport[]> => {
      const res = await http.get(`/labs/${labId}/verification-reports`);
      return VerificationReportsSchema.parse(res.data);
    },
  });
}