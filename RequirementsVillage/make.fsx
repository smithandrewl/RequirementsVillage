#!/usr/bin/env -S dotnet fsi

open System
open System.IO
open System.Diagnostics

let run cmd args workingDir =
  let psi = ProcessStartInfo(cmd, args)
  psi.WorkingDirectory <- workingDir
  psi.UseShellExecute <- false
  let p = Process.Start(psi)
  p.WaitForExit()
  p.ExitCode

let (</>) path1 path2 = Path.Combine(path1, path2)
let root = __SOURCE_DIRECTORY__

let tasks = [
  "test", fun () ->
    printfn "Running all tests..."
    run "dotnet" "fsi TestRunner.fsx All" root
    
  "test:backend", fun () ->
    printfn "Running backend tests..."
    run "dotnet" "fsi TestRunner.fsx Backend" root
    
  "test:frontend", fun () ->
    printfn "Running frontend tests..."
    run "dotnet" "fsi TestRunner.fsx Frontend" root
    
  "test:coverage", fun () ->
    printfn "Running all tests with coverage..."
    run "dotnet" "fsi TestRunner.fsx All -c" root
    
  "test:backend:coverage", fun () ->
    printfn "Running backend tests with coverage..."
    run "dotnet" "fsi TestRunner.fsx Backend -c" root
    
  "test:frontend:coverage", fun () ->
    printfn "Running frontend tests with coverage..."
    run "dotnet" "fsi TestRunner.fsx Frontend -c" root
    
  "build", fun () ->
    printfn "Building solution..."
    run "dotnet" "build" root
    
  "clean", fun () ->
    printfn "Cleaning solution..."
    run "dotnet" "clean" root
    if Directory.Exists(root </> "coverage") then
      Directory.Delete(root </> "coverage", true)
    0
    
  "restore", fun () ->
    printfn "Restoring packages..."
    let backendResult = run "dotnet" "restore" root
    let frontendResult = run "npm" "install" (root </> "RequirementsVillage.Client")
    if backendResult = 0 && frontendResult = 0 then 0 else 1
    
  "run:api", fun () ->
    printfn "Running API..."
    run "dotnet" "run" (root </> "RequirementsVillage.Api")
    
  "run:client", fun () ->
    printfn "Running client dev server..."
    run "npm" "start" (root </> "RequirementsVillage.Client")
    
  "help", fun () ->
    printfn "Available tasks:"
    printfn "  test                  - Run all tests"
    printfn "  test:backend          - Run backend tests only"
    printfn "  test:frontend         - Run frontend tests only"
    printfn "  test:coverage         - Run all tests with coverage"
    printfn "  test:backend:coverage - Run backend tests with coverage"
    printfn "  test:frontend:coverage- Run frontend tests with coverage"
    printfn "  build                 - Build the solution"
    printfn "  clean                 - Clean build artifacts and coverage"
    printfn "  restore               - Restore all packages"
    printfn "  run:api               - Run the API server"
    printfn "  run:client            - Run the client dev server"
    printfn "  help                  - Show this help"
    0
]

let task = 
  if fsi.CommandLineArgs.Length > 1 then 
    fsi.CommandLineArgs.[1]
  else 
    "help"

match List.tryFind (fun (name, _) -> name = task) tasks with
| Some (_, action) -> exit (action())
| None -> 
    printfn "Unknown task: %s" task
    printfn "Run 'dotnet fsi make.fsx help' to see available tasks"
    exit 1