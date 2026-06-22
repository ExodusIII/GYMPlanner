import type { CalculatedMetrics } from '../types';

export function MetricsView({ metrics }: { metrics: CalculatedMetrics }) {
  return (
    <div className="card">
      <h2>Your numbers</h2>
      <div className="stats">
        <Stat label="BMI" value={`${metrics.bmi} (${metrics.bmiCategory})`} />
        <Stat label="Healthy weight" value={`${metrics.healthyWeightMinKg}–${metrics.healthyWeightMaxKg} kg`} />
        <Stat label="BMR" value={`${metrics.bmrCalories} kcal`} />
        <Stat label="TDEE" value={`${metrics.tdeeCalories} kcal`} />
        <Stat label="Daily target" value={`${metrics.calorieTarget} kcal`} />
        <Stat label="Water" value={`${metrics.waterLitersPerDay} L/day`} />
        <Stat label="Protein" value={`${metrics.macros.proteinGrams} g`} />
        <Stat label="Carbs" value={`${metrics.macros.carbGrams} g`} />
        <Stat label="Fat" value={`${metrics.macros.fatGrams} g`} />
        <Stat label="Split" value={metrics.training.split} />
        <Stat label="Sets / muscle / week" value={`${metrics.training.weeklySetsPerMuscleGroup}`} />
        <Stat label="Days / week" value={`${metrics.training.daysPerWeek}`} />
      </div>
    </div>
  );
}

function Stat({ label, value }: { label: string; value: string }) {
  return (
    <div className="stat">
      <span className="stat-label">{label}</span>
      <span className="stat-value">{value}</span>
    </div>
  );
}
