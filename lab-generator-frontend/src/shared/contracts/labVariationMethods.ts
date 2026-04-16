import { z } from 'zod';

const LabVariationMethodCamelSchema = z.object({
  id: z.number(),
  labId: z.number(),
  variationMethodId: z.number(),
  preserveAcrossLabs: z.boolean(),
});

const LabVariationMethodPascalSchema = z.object({
  Id: z.number(),
  LabId: z.number(),
  VariationMethodId: z.number(),
  PreserveAcrossLabs: z.boolean(),
});

export const LabVariationMethodSchema = z
  .union([LabVariationMethodCamelSchema, LabVariationMethodPascalSchema])
  .transform((x) => {
    if ('id' in x) return x;
    return {
      id: x.Id,
      labId: x.LabId,
      variationMethodId: x.VariationMethodId,
      preserveAcrossLabs: x.PreserveAcrossLabs,
    };
  });

export type LabVariationMethod = z.infer<typeof LabVariationMethodSchema>;

export const UpsertLabVariationMethodsRequestSchema = z.object({
  items: z.array(
    z.object({
      variationMethodId: z.number(),
      preserveAcrossLabs: z.boolean(),
    })
  ),
});

export type UpsertLabVariationMethodsRequest = z.infer<typeof UpsertLabVariationMethodsRequestSchema>;
