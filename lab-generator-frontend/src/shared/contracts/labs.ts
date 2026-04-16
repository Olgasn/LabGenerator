import { z } from 'zod';

const LabCamelSchema = z.object({
  id: z.number(),
  disciplineId: z.number(),
  orderIndex: z.number().optional(),
  title: z.string(),
  initialDescription: z.string(),
});

const LabPascalSchema = z.object({
  Id: z.number(),
  DisciplineId: z.number(),
  OrderIndex: z.number().optional(),
  Title: z.string(),
  InitialDescription: z.string(),
});

export const LabSchema = z.union([LabCamelSchema, LabPascalSchema]).transform((l) => {
  if ('id' in l) return l;
  return {
    id: l.Id,
    disciplineId: l.DisciplineId,
    orderIndex: l.OrderIndex,
    title: l.Title,
    initialDescription: l.InitialDescription,
  };
});

export type Lab = z.infer<typeof LabSchema>;

export const CreateLabRequestSchema = z.object({
  disciplineId: z.number(),
  orderIndex: z.number().int().min(0),
  title: z.string().min(1),
  initialDescription: z.string().min(1),
});

export type CreateLabRequest = z.infer<typeof CreateLabRequestSchema>;