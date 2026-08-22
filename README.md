# StudyPal
your study pal that guarantees success

A study management web app I built to help students (mainly myself, lol) actually stick to studying instead of just planning to.

## What it does

- **Study timetable generator** — add your subjects, how difficult each one is, and when you're free during the week, and it builds you a timetable around that
- **Focus timer** — pick 25 min, 45 min, 1hr, 1.5hr, or set a custom time, and it tracks your session. Keeps running in a small floating widget so you can still move around the app while studying
- **Flashcards** — make flashcards per subject, review them, mark if you got it right or wrong. Cards you keep missing show up more often than ones you already know
- **Dashboard** — shows your current streak, longest streak, total hours studied, and how many flashcards you've reviewed, based on your actual sessions

## Built with
C#, ASP.NET MVC, JavaScript, SQL Server, HTML/CSS

## Project structure
- `StudyPal` — the main app (controllers + views)
- `DataAccess` — talks to the database
- `MyModels` — data models
- `Utility` — helper functions used across the project

## Running it locally
1. Clone the repo
2. Open `StudyPal.sln` in Visual Studio, or just run `dotnet run` from the `StudyPal` folder
3. Update the connection string in `appsettings.json` to your own SQL Server instance
4. Run migrations if you need to set up the DB
5. Build and run

## Notes
This is a personal project I built to get better at full-stack .NET development. Still adding to it when I have time.
