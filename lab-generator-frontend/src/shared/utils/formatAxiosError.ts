import type { AxiosError } from 'axios';

export type FormattedHttpError = {
  title: string;
  status?: number;
  statusText?: string;
  url?: string;
  method?: string;
  message: string;
  responseText?: string;
};

function safeStringify(value: unknown): string {
  try {
    return JSON.stringify(value, null, 2);
  } catch {
    return String(value);
  }
}

export function formatAxiosError(error: unknown): FormattedHttpError {
  const e = error as AxiosError<unknown> | undefined;

  const status = e?.response?.status;
  const statusText = e?.response?.statusText;

  const cfg = e?.config;
  const url = cfg?.url;
  const method = cfg?.method?.toUpperCase();

  const data = e?.response?.data;
  const responseText = data == null ? undefined : typeof data === 'string' ? data : safeStringify(data);

  const message =
    e?.message ||
    (status != null ? `HTTP ${status}${statusText ? ` ${statusText}` : ''}` : 'Request failed');

  const title = status != null ? `HTTP ${status}` : 'Request error';

  return {
    title,
    status,
    statusText,
    url,
    method,
    message,
    responseText,
  };
}
