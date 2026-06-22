import { useState } from 'react';
import type {
  ActivityLevel,
  ClientProfile,
  Equipment,
  ExperienceLevel,
  Goal,
  Sex,
} from '../types';

const sexes: Sex[] = ['Male', 'Female', 'Other'];
const goals: Goal[] = ['LoseFat', 'BuildMuscle', 'Maintain', 'Recomp', 'Strength', 'Endurance'];
const activityLevels: ActivityLevel[] = ['Sedentary', 'Light', 'Moderate', 'Active', 'VeryActive'];
const experiences: ExperienceLevel[] = ['Beginner', 'Intermediate', 'Advanced'];
const equipments: Equipment[] = ['Gym', 'Home', 'Minimal'];

const defaultProfile: ClientProfile = {
  age: 30,
  sex: 'Male',
  heightCm: 180,
  weightKg: 80,
  bodyFatPercent: null,
  goal: 'LoseFat',
  activityLevel: 'Moderate',
  experience: 'Intermediate',
  equipment: 'Gym',
  daysPerWeek: 4,
  minutesPerSession: 60,
  injuries: [],
};

interface Props {
  busy: boolean;
  onSubmit: (profile: ClientProfile) => void;
}

export function IntakeForm({ busy, onSubmit }: Props) {
  const [form, setForm] = useState<ClientProfile>(defaultProfile);
  const [injuriesText, setInjuriesText] = useState('');

  function num(value: string): number {
    return value === '' ? 0 : Number(value);
  }

  function submit(e: React.FormEvent) {
    e.preventDefault();
    const injuries = injuriesText
      .split(',')
      .map((s) => s.trim())
      .filter(Boolean);
    onSubmit({ ...form, injuries });
  }

  return (
    <form className="intake card" onSubmit={submit}>
      <h2>Your details</h2>
      <div className="grid">
        <label>
          Age
          <input type="number" min={10} max={100} value={form.age}
            onChange={(e) => setForm({ ...form, age: num(e.target.value) })} />
        </label>
        <label>
          Sex
          <select value={form.sex} onChange={(e) => setForm({ ...form, sex: e.target.value as Sex })}>
            {sexes.map((s) => <option key={s} value={s}>{s}</option>)}
          </select>
        </label>
        <label>
          Height (cm)
          <input type="number" min={100} max={250} value={form.heightCm}
            onChange={(e) => setForm({ ...form, heightCm: num(e.target.value) })} />
        </label>
        <label>
          Weight (kg)
          <input type="number" min={30} max={300} value={form.weightKg}
            onChange={(e) => setForm({ ...form, weightKg: num(e.target.value) })} />
        </label>
        <label>
          Body fat % (optional)
          <input type="number" min={0} max={70} value={form.bodyFatPercent ?? ''}
            onChange={(e) => setForm({ ...form, bodyFatPercent: e.target.value === '' ? null : num(e.target.value) })} />
        </label>
        <label>
          Goal
          <select value={form.goal} onChange={(e) => setForm({ ...form, goal: e.target.value as Goal })}>
            {goals.map((g) => <option key={g} value={g}>{g}</option>)}
          </select>
        </label>
        <label>
          Activity level
          <select value={form.activityLevel} onChange={(e) => setForm({ ...form, activityLevel: e.target.value as ActivityLevel })}>
            {activityLevels.map((a) => <option key={a} value={a}>{a}</option>)}
          </select>
        </label>
        <label>
          Experience
          <select value={form.experience} onChange={(e) => setForm({ ...form, experience: e.target.value as ExperienceLevel })}>
            {experiences.map((x) => <option key={x} value={x}>{x}</option>)}
          </select>
        </label>
        <label>
          Equipment
          <select value={form.equipment} onChange={(e) => setForm({ ...form, equipment: e.target.value as Equipment })}>
            {equipments.map((x) => <option key={x} value={x}>{x}</option>)}
          </select>
        </label>
        <label>
          Training days / week
          <input type="number" min={1} max={7} value={form.daysPerWeek}
            onChange={(e) => setForm({ ...form, daysPerWeek: num(e.target.value) })} />
        </label>
        <label>
          Minutes / session
          <input type="number" min={10} max={180} value={form.minutesPerSession}
            onChange={(e) => setForm({ ...form, minutesPerSession: num(e.target.value) })} />
        </label>
        <label className="wide">
          Injuries / limitations (comma-separated)
          <input type="text" placeholder="e.g. lower back, left knee" value={injuriesText}
            onChange={(e) => setInjuriesText(e.target.value)} />
        </label>
      </div>
      <button type="submit" disabled={busy}>{busy ? 'Calculating…' : 'Calculate metrics'}</button>
    </form>
  );
}
