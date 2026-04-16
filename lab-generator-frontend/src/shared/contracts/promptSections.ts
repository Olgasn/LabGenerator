import { z } from 'zod';

export const PromptCustomSectionSchema = z.object({
  sectionKey: z.string(),
  displayName: z.string(),
  content: z.string(),
  defaultContent: z.string(),
  isCustomized: z.boolean(),
  updatedAt: z.string().nullable().optional(),
});

export type PromptCustomSection = z.infer<typeof PromptCustomSectionSchema>;
