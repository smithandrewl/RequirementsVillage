# F# Fable Client

This is the F# version of the Requirements Village frontend, built with Fable, Elmish, and Feliz.

## Prerequisites

- .NET 9 SDK
- Node.js (for npm)
- Fable CLI tool (installed globally)

## Setup Instructions

1. **Install .NET dependencies:**
   ```bash
   cd RequirementsVillage.FSharp.Client
   dotnet restore
   ```

2. **Install npm dependencies:**
   ```bash
   npm install
   ```

## Running the Application

### Option 1: Development Mode (Recommended)

Run both the API and F# client with hot reload:

**Terminal 1 - Start the API:**
```bash
cd ../RequirementsVillage.Api
dotnet run
```

**Terminal 2 - Start the F# client:**
```bash
cd RequirementsVillage.FSharp.Client
npm start
```

This will:
- Compile F# to JavaScript using Fable
- Start webpack-dev-server on http://localhost:8080
- Enable hot module replacement
- Proxy API calls to the backend

### Option 2: Build for Production

```bash
# Build the F# client
npm run build

# Run the API (which serves the built files)
cd ../RequirementsVillage.Api
dotnet run
```

Then visit http://localhost:5000

## Project Structure

```
src/
├── Types.fs            # Domain models and message types
├── Api/
│   └── Projects.fs     # API client with error handling
├── Components/         # Reusable UI components
│   ├── ThemeSelector.fs
│   ├── ProjectCard.fs
│   └── Layout.fs
├── Pages/             # Page components
│   ├── Landing.fs
│   └── Dashboard.fs
├── State.fs           # Elmish update logic
└── App.fs             # Main entry point
```

## Key Features

- **Type Safety**: Full F# type safety throughout
- **Elmish Architecture**: Predictable state management
- **Error Handling**: Railway-oriented programming with Result types
- **Bulma Styling**: Using Feliz.Bulma for UI components
- **Theme Support**: Light/Dark theme switching

## Troubleshooting

If you get build errors:
1. Make sure you have .NET 9 SDK installed
2. Run `dotnet tool install fable --global` if Fable is not found
3. Clear the build cache: `rm -rf obj bin node_modules`
4. Reinstall dependencies: `dotnet restore && npm install`