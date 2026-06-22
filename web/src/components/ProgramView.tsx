import type { WeeklyProgram } from '../types';

export function ProgramView({ program }: { program: WeeklyProgram }) {
  return (
    <div className="card program">
      <h2>Your weekly program</h2>

      <div className="days">
        {program.days.map((day, i) => (
          <div key={i} className="day">
            <h3>{day.day} · <span className="focus">{day.focus}</span></h3>
            <table>
              <thead>
                <tr><th>Exercise</th><th>Sets</th><th>Reps</th><th>Notes</th></tr>
              </thead>
              <tbody>
                {day.exercises.map((ex, j) => (
                  <tr key={j}>
                    <td>{ex.name}</td>
                    <td>{ex.sets}</td>
                    <td>{ex.reps}</td>
                    <td className="muted">{ex.notes}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ))}
      </div>

      <div className="nutrition">
        <h3>Nutrition</h3>
        <p>
          <strong>{program.nutrition.dailyCalories} kcal</strong> · {program.nutrition.proteinGrams}g protein ·{' '}
          {program.nutrition.carbGrams}g carbs · {program.nutrition.fatGrams}g fat
        </p>
        <ul>
          {program.nutrition.mealSuggestions.map((meal, i) => <li key={i}>{meal}</li>)}
        </ul>
      </div>

      {program.notes && (
        <div className="notes">
          <h3>Notes</h3>
          <p>{program.notes}</p>
        </div>
      )}
    </div>
  );
}
