import { z } from 'zod';

export const AssignmentVariantSchema = z.object({
  id: z.number(),
  labId: z.number(),
  variantIndex: z.number(),
  title: z.string(),
  content: z.string(),
  variantParamsJson: z.string(),
  difficultyProfileJson: z.string(),
  fingerprint: z.string(),
  createdAt: z.string(),
});

export type AssignmentVariant = z.infer<typeof AssignmentVariantSchema>;

export const GenerateVariantsRequestSchema = z.object({
  count: z.number().int().min(1),
  variationProfileId: z.number().int().optional(),
});

export type GenerateVariantsRequest = z.infer<typeof GenerateVariantsRequestSchema>;