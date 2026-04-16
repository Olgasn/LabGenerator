import { z } from 'zod';

export const VerificationReportSchema = z.object({
  id: z.number(),
  assignmentVariantId: z.number(),
  passed: z.boolean(),
  judgeScoreJson: z.string(),
  issuesJson: z.string(),
  judgeRunId: z.number().nullable().optional(),
  solverRunId: z.number().nullable().optional(),
  createdAt: z.string(),
});

export type VerificationReport = z.infer<typeof VerificationReportSchema>;

export const VerifyVariantsRequestSchema = z.object({
  variantId: z.number().int().optional(),
});

export type VerifyVariantsRequest = z.infer<typeof VerifyVariantsRequestSchema>;