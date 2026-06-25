import { useCallback, useEffect, useState } from 'react';
import { api, auth } from './api';
import type { CalculatedMetrics, ClientProfile, ProgramResult, WeeklyProgram } from './types';
import { AuthPanel } from './components/AuthPanel';
import { IntakeForm } from './components/IntakeForm';
import { MetricsView } from './components/MetricsView';
import { ProgramView } from './components/ProgramView';

export default function App() {
  const [email, setEmail] = useState<string | null>(auth.email());
  const [profile, setProfile] = useState<ClientProfile | null>(null);
  const [metrics, setMetrics] = useState<CalculatedMetrics | null>(null);
  const [program, setProgram] = useState<WeeklyProgram | null>(null);
  const [saved, setSaved] = useState<ProgramResult[]>([]);
  const [calculating, setCalculating] = useState(false);
  const [generating, setGenerating] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const refreshSaved = useCallback(async () => {
    if (!auth.token()) {
      setSaved([]);
      return;
    }
    try {
      setSaved(await api.listPrograms());
    } catch {
      // Ignore list errors (e.g. expired token); the auth panel handles re-login.
    }
  }, []);

  useEffect(() => {
    void refreshSaved();
  }, [email, refreshSaved]);

  // The API clears the token and fires this when an authenticated request gets a 401.
  useEffect(() => {
    function onExpired() {
      setEmail(null);
      setError('Your session expired — please log in again.');
    }
    window.addEventListener('auth-expired', onExpired);
    return () => window.removeEventListener('auth-expired', onExpired);
  }, []);

  async function calculate(p: ClientProfile) {
    setError(null);
    setProgram(null);
    setCalculating(true);
    try {
      setProfile(p);
      setMetrics(await api.calculate(p));
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Calculation failed');
    } finally {
      setCalculating(false);
    }
  }

  async function generate() {
    if (!profile) return;
    setError(null);
    setGenerating(true);
    try {
      const result = await api.createProgram(profile);
      setProgram(result.program);
      await refreshSaved();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Program generation failed');
    } finally {
      setGenerating(false);
    }
  }

  return (
    <div className="app">
      <header>
        <h1>🏋️ GYM Planner</h1>
        <AuthPanel email={email} onAuthChange={setEmail} />
      </header>

      <p className="disclaimer">
        General fitness guidance only — <strong>not medical advice</strong>. Consult a professional before starting a new program.
      </p>

      {error && <p className="error banner">{error}</p>}

      <IntakeForm busy={calculating} onSubmit={calculate} />

      {metrics && <MetricsView metrics={metrics} />}

      {metrics && (
        <div className="card generate">
          <h2>Weekly program (AI)</h2>
          {email ? (
            <button onClick={generate} disabled={generating}>
              {generating ? 'Generating with Gemini…' : 'Generate weekly program'}
            </button>
          ) : (
            <p className="muted">Log in to generate and save an AI-written weekly program.</p>
          )}
        </div>
      )}

      {program && <ProgramView program={program} />}

      {email && saved.length > 0 && (
        <div className="card">
          <h2>Saved programs</h2>
          <ul className="saved">
            {saved.map((s) => (
              <li key={s.id}>
                <button className="link" onClick={() => { setProgram(s.program); setMetrics(s.metrics); }}>
                  {new Date(s.createdAt).toLocaleString()} · {s.profile.goal} · {s.metrics.calorieTarget} kcal
                </button>
              </li>
            ))}
          </ul>
        </div>
      )}
    </div>
  );
}
