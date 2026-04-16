import { z } from 'zod';

export const GenerationJobStatusSchema = z.number();
export const GenerationJobTypeSchema = z.number();

export const GenerationJobSchema = z.object({
  id: z.number(),
  type: GenerationJobTypeSchema,
  status: GenerationJobStatusSchema,
  disciplineId: z.number().nullable().optional(),
  labId: z.number().nullable().optional(),
  masterAssignmentId: z.number().nullable().optional(),
  variationProfileId: z.number().nullable().optional(),
  payloadJson: z.string().nullable().optional(),
  resultJson: z.string().nullable().optional(),
  error: z.string().nullable().optional(),
  progress: z.number(),
  createdAt: z.string(),
  startedAt: z.string().nullable().optional(),
  finishedAt: z.string().nullable().optional(),
});

export type GenerationJob = z.infer<typeof GenerationJobSchema>;

export const JobStatus = {
  Pending: 0,
  InProgress: 1,
  Succeeded: 2,
  Failed: 3,
  Canceled: 4,
} as const;

export type JobStatusValue = (typeof JobStatus)[keyof typeof JobStatus];