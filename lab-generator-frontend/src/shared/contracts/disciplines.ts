import { z } from 'zod';

const DisciplineCamelSchema = z.object({
  id: z.number(),
  name: z.string(),
  description: z.string().nullable().optional(),
  labsCount: z.number().optional(),
});

const DisciplinePascalSchema = z.object({
  Id: z.number(),
  Name: z.string(),
  Description: z.string().nullable().optional(),
  LabsCount: z.number().optional(),
});

export const DisciplineSchema = z.union([DisciplineCamelSchema, DisciplinePascalSchema]).transform((d) => {
  if ('id' in d) return d;
  return {
    id: d.Id,
    name: d.Name,
    description: d.Description,
    labsCount: d.LabsCount,
  };
});

export type Discipline = z.infer<typeof DisciplineSchema>;

export const CreateDisciplineRequestSchema = z.object({
  name: z.string().min(1),
  description: z.string().optional(),
});

export type CreateDisciplineRequest = z.infer<typeof CreateDisciplineRequestSchema>;
