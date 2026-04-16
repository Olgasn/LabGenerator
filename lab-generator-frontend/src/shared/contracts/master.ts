import { z } from 'zod';

export const MasterAssignmentSchema = z.object({
  id: z.number(),
  labId: z.number(),
  version: z.number(),
  isCurrent: z.boolean(),
  status: z.number(),
  content: z.string(),
  createdAt: z.string(),
  updatedAt: z.string().nullable().optional(),
});

export type MasterAssignment = z.infer<typeof MasterAssignmentSchema>;

export const UpdateMasterAssignmentRequestSchema = z.object({
  content: z.string(),
});

export type UpdateMasterAssignmentRequest = z.infer<typeof UpdateMasterAssignmentRequestSchema>;