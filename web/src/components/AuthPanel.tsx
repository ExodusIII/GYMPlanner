import { useState } from 'react';
import { api, auth } from '../api';

interface Props {
  email: string | null;
  onAuthChange: (email: string | null) => void;
}

export function AuthPanel({ email, onAuthChange }: Props) {
  const [mode, setMode] = useState<'login' | 'register'>('login');
  const [emailInput, setEmailInput] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  if (email) {
    return (
      <div className="auth signed-in">
        <span>
          Signed in as <strong>{email}</strong>
        </span>
        <button
          onClick={() => {
            auth.clear();
            onAuthChange(null);
          }}
        >
          Log out
        </button>
      </div>
    );
  }

  async function submit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    setBusy(true);
    try {
      const res = mode === 'login'
        ? await api.login(emailInput, password)
        : await api.register(emailInput, password);
      auth.set(res);
      onAuthChange(res.email);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Authentication failed');
    } finally {
      setBusy(false);
    }
  }

  return (
    <form className="auth" onSubmit={submit}>
      <div className="tabs">
        <button type="button" className={mode === 'login' ? 'active' : ''} onClick={() => setMode('login')}>
          Log in
        </button>
        <button type="button" className={mode === 'register' ? 'active' : ''} onClick={() => setMode('register')}>
          Register
        </button>
      </div>
      <input
        type="email"
        placeholder="email"
        value={emailInput}
        onChange={(e) => setEmailInput(e.target.value)}
        required
      />
      <input
        type="password"
        placeholder="password (min 8 chars)"
        value={password}
        onChange={(e) => setPassword(e.target.value)}
        required
        minLength={8}
      />
      <button type="submit" disabled={busy}>
        {busy ? '…' : mode === 'login' ? 'Log in' : 'Create account'}
      </button>
      {error && <p className="error">{error}</p>}
    </form>
  );
}
