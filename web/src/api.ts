import type {
  AuthResponse,
  CalculatedMetrics,
  ClientProfile,
  ProgramResult,
} from './types';

const API_URL = (import.meta.env.VITE_API_URL as string | undefined) ?? 'http://localhost:5030';

const TOKEN_KEY = 'gymplanner.token';
const EMAIL_KEY = 'gymplanner.email';

export const auth = {
  token: () => localStorage.getItem(TOKEN_KEY),
  email: () => localStorage.getItem(EMAIL_KEY),
  set(res: AuthResponse) {
    localStorage.setItem(TOKEN_KEY, res.token);
    localStorage.setItem(EMAIL_KEY, res.email);
  },
  clear() {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(EMAIL_KEY);
  },
};

export class ApiError extends Error {
  status: number;
  constructor(message: string, status: number) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
  }
}

async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const headers = new Headers(options.headers);
  headers.set('Content-Type', 'application/json');
  const token = auth.token();
  if (token) headers.set('Authorization', `Bearer ${token}`);

  const res = await fetch(`${API_URL}${path}`, { ...options, headers });

  if (!res.ok) {
    let message = `Request failed (${res.status})`;
    try {
      const body = await res.json();
      message = body.error ?? body.title ?? (typeof body === 'string' ? body : message);
    } catch {
      const text = await res.text().catch(() => '');
      if (text) message = text;
    }
    // A 401 on a request that carried a token means the saved session is
    // stale/expired — clear it and let the app prompt a fresh login.
    if (res.status === 401 && token) {
      auth.clear();
      window.dispatchEvent(new Event('auth-expired'));
      message = 'Your session expired — please log in again.';
    }
    throw new ApiError(message, res.status);
  }

  return res.status === 204 ? (undefined as T) : ((await res.json()) as T);
}

export const api = {
  register: (email: string, password: string) =>
    request<AuthResponse>('/api/auth/register', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    }),

  login: (email: string, password: string) =>
    request<AuthResponse>('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    }),

  calculate: (profile: ClientProfile) =>
    request<CalculatedMetrics>('/api/calculations', {
      method: 'POST',
      body: JSON.stringify(profile),
    }),

  createProgram: (profile: ClientProfile) =>
    request<ProgramResult>('/api/programs', {
      method: 'POST',
      body: JSON.stringify(profile),
    }),

  listPrograms: () => request<ProgramResult[]>('/api/programs'),
};
