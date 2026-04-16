export type LlmProviderSettings = {
  id: number;
  provider: string;
  model: string | null;
  hasApiKey: boolean;
  apiKeyMasked: string | null;
  temperature: number | null;
  maxOutputTokens: number | null;
  updatedAt: string;
};

export type UpsertLlmProviderSettingsRequest = {
  provider: string;
  model: string | null;
  apiKey?: string | null;
  clearApiKey?: boolean;
  temperature: number | null;
  maxOutputTokens: number | null;
};
