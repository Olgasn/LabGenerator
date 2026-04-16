export type LlmProvider = 'Ollama' | 'OpenRouter';

export type LlmSettings = {
  id: number;
  provider: string;
  model: string;
};

export type UpdateLlmSettingsRequest = {
  provider: string;
  model: string;
};
