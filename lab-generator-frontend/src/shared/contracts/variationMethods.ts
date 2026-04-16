import { z } from 'zod';

const VariationMethodCamelSchema = z.object({
  id: z.number(),
  code: z.string(),
  name: z.string(),
  description: z.string().nullable().optional(),
  isSystem: z.boolean().optional(),
});

const VariationMethodPascalSchema = z.object({
  Id: z.number(),
  Code: z.string(),
  Name: z.string(),
  Description: z.string().nullable().optional(),
  IsSystem: z.boolean().optional(),
});

export const VariationMethodSchema = z
  .union([VariationMethodCamelSchema, VariationMethodPascalSchema])
  .transform((m) => {
    if ('id' in m) return m;
    return {
      id: m.Id,
      code: m.Code,
      name: m.Name,
      description: m.Description ?? null,
      isSystem: m.IsSystem ?? false,
    };
  });

export type VariationMethod = z.infer<typeof VariationMethodSchema>;

export const CreateVariationMethodRequestSchema = z.object({
  name: z.string().min(1),
  description: z.string().nullable().optional(),
  code: z.string().min(1).optional(),
});

export type CreateVariationMethodRequest = z.infer<typeof CreateVariationMethodRequestSchema>;

export const UpdateVariationMethodRequestSchema = z.object({
  code: z.string().min(1),
  name: z.string().min(1),
  description: z.string().nullable().optional(),
});

export type UpdateVariationMethodRequest = z.infer<typeof UpdateVariationMethodRequestSchema>;
