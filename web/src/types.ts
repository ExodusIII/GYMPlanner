// Mirrors the backend JSON contracts (camelCase, string enums).

export type Sex = 'Male' | 'Female' | 'Other';
export type Goal = 'LoseFat' | 'BuildMuscle' | 'Maintain' | 'Recomp' | 'Strength' | 'Endurance';
export type ActivityLevel = 'Sedentary' | 'Light' | 'Moderate' | 'Active' | 'VeryActive';
export type ExperienceLevel = 'Beginner' | 'Intermediate' | 'Advanced';
export type Equipment = 'Gym' | 'Home' | 'Minimal';

export interface ClientProfile {
  age: number;
  sex: Sex;
  heightCm: number;
  weightKg: number;
  bodyFatPercent?: number | null;
  goal: Goal;
  activityLevel: ActivityLevel;
  experience: ExperienceLevel;
  equipment: Equipment;
  daysPerWeek: number;
  minutesPerSession: number;
  injuries: string[];
}

export interface MacroBreakdown {
  proteinGrams: number;
  carbGrams: number;
  fatGrams: number;
  calories: number;
}

export interface TrainingRecommendation {
  split: string;
  daysPerWeek: number;
  weeklySetsPerMuscleGroup: number;
  sessionMinutes: number;
}

export interface CalculatedMetrics {
  bmi: number;
  bmiCategory: string;
  healthyWeightMinKg: number;
  healthyWeightMaxKg: number;
  bmrCalories: number;
  tdeeCalories: number;
  calorieTarget: number;
  macros: MacroBreakdown;
  training: TrainingRecommendation;
  waterLitersPerDay: number;
}

export interface Exercise {
  name: string;
  sets: number;
  reps: string;
  notes: string;
}

export interface ProgramDay {
  day: string;
  focus: string;
  exercises: Exercise[];
}

export interface NutritionPlan {
  dailyCalories: number;
  proteinGrams: number;
  carbGrams: number;
  fatGrams: number;
  mealSuggestions: string[];
}

export interface WeeklyProgram {
  days: ProgramDay[];
  nutrition: NutritionPlan;
  notes: string;
}

export interface ProgramResult {
  id: string;
  createdAt: string;
  profile: ClientProfile;
  metrics: CalculatedMetrics;
  program: WeeklyProgram;
}

export interface AuthResponse {
  token: string;
  expiresAt: string;
  email: string;
}
