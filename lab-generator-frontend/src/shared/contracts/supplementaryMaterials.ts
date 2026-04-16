import { z } from 'zod';

export const LabSupplementaryMaterialSchema = z.object({
  id: z.number(),
  labId: z.number(),
  theoryMarkdown: z.string(),
  controlQuestions: z.array(z.string()),
  sourceFingerprint: z.string(),
  createdAt: z.string(),
  updatedAt: z.string().nullable().optional(),
});

export type LabSupplementaryMaterial = z.infer<typeof LabSupplementaryMaterialSchema>;

export const GenerateSupplementaryMaterialRequestSchema = z.object({
  force: z.boolean().optional(),
});

export type GenerateSupplementaryMaterialRequest = z.infer<typeof GenerateSupplementaryMaterialRequestSchema>;
