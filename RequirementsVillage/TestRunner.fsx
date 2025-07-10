#!/usr/bin/env -S dotnet fsi

#r "nuget: Argu, 6.1.1"

open System
open System.IO
open System.Diagnostics
open Argu

// Command line arguments
type TestTarget =
  | All
  | Backend
  | Frontend

type Arguments =
  | [<MainCommand; ExactlyOnce>] Target of TestTarget
  | [<AltCommandLine("-c")>] Coverage
  | [<AltCommandLine("-v")>] Verbose
  | [<AltCommandLine("-f")>] Filter of string
  | [<AltCommandLine("-o")>] Output of string
  
  interface IArgParserTemplate with
    member s.Usage =
      match s with
      | Target _ -> "test target: All, Backend, or Frontend"
      | Coverage -> "run tests with code coverage"
      | Verbose -> "enable verbose output"
      | Filter pattern -> "filter tests by pattern"
      | Output dir -> "output directory for coverage reports"

// Helper functions
let private currentDir = Directory.GetCurrentDirectory()
let private solutionDir = currentDir
let private apiTestDir = Path.Combine(solutionDir, "RequirementsVillage.Api.Tests")
let private clientDir = Path.Combine(solutionDir, "RequirementsVillage.Client")
let private coverageDir = Path.Combine(solutionDir, "coverage")

let private colorPrint color (text: string) =
  let oldColor = Console.ForegroundColor
  Console.ForegroundColor <- color
  printfn "%s" text
  Console.ForegroundColor <- oldColor

let private success = colorPrint ConsoleColor.Green
let private error = colorPrint ConsoleColor.Red
let private info = colorPrint ConsoleColor.Cyan
let private warning = colorPrint ConsoleColor.Yellow

let private runCommand workingDir command args =
  let startInfo = ProcessStartInfo()
  startInfo.FileName <- command
  startInfo.Arguments <- args
  startInfo.WorkingDirectory <- workingDir
  startInfo.UseShellExecute <- false
  startInfo.RedirectStandardOutput <- true
  startInfo.RedirectStandardError <- true
  
  use proc = new Process()
  proc.StartInfo <- startInfo
  
  let output = System.Text.StringBuilder()
  let errors = System.Text.StringBuilder()
  
  proc.OutputDataReceived.Add(fun args ->
    if not (String.IsNullOrEmpty args.Data) then
      output.AppendLine(args.Data) |> ignore
      Console.WriteLine(args.Data)
  )
  
  proc.ErrorDataReceived.Add(fun args ->
    if not (String.IsNullOrEmpty args.Data) then
      errors.AppendLine(args.Data) |> ignore
      Console.Error.WriteLine(args.Data)
  )
  
  proc.Start() |> ignore
  proc.BeginOutputReadLine()
  proc.BeginErrorReadLine()
  proc.WaitForExit()
  
  (proc.ExitCode, output.ToString(), errors.ToString())

let private ensureDirectory path =
  if not (Directory.Exists path) then
    Directory.CreateDirectory path |> ignore

// Test runners
let runBackendTests (coverage: bool) (filter: string option) (outputDir: string) =
  info "Running backend tests..."
  
  let args = 
    let baseArgs = "test"
    let filterArgs = 
      match filter with
      | Some f -> sprintf "--filter \"%s\"" f
      | None -> ""
    
    if coverage then
      let backendCoverageDir = Path.Combine(outputDir, "backend")
      ensureDirectory backendCoverageDir
      sprintf "%s %s /p:CollectCoverage=true /p:CoverletOutputFormat=opencover,json,lcov /p:CoverletOutput=%s/ /p:MergeWith=%s/coverage.json" 
        baseArgs filterArgs backendCoverageDir backendCoverageDir
    else
      sprintf "%s %s" baseArgs filterArgs
  
  let (exitCode, output, errors) = runCommand apiTestDir "dotnet" args
  
  if exitCode = 0 then
    success "✓ Backend tests passed"
  else
    error "✗ Backend tests failed"
    
  exitCode

let runFrontendTests (coverage: bool) (outputDir: string) =
  info "Running frontend tests..."
  
  let script = 
    if coverage then 
      "test:coverage"
    else 
      "test:once"
  
  let (exitCode, output, errors) = runCommand clientDir "npm" (sprintf "run %s" script)
  
  if exitCode = 0 then
    success "✓ Frontend tests passed"
    
    if coverage then
      // Move coverage reports to unified location
      let frontendCoverageSource = Path.Combine(clientDir, "coverage", "frontend")
      let frontendCoverageTarget = Path.Combine(outputDir, "frontend")
      
      if Directory.Exists frontendCoverageSource then
        ensureDirectory frontendCoverageTarget
        
        // Copy coverage files
        let files = Directory.GetFiles(frontendCoverageSource, "*", SearchOption.AllDirectories)
        for file in files do
          let relativePath = Path.GetRelativePath(frontendCoverageSource, file)
          let targetFile = Path.Combine(frontendCoverageTarget, relativePath)
          let targetDir = Path.GetDirectoryName(targetFile)
          ensureDirectory targetDir
          File.Copy(file, targetFile, true)
  else
    error "✗ Frontend tests failed"
    
  exitCode

let generateCoverageReport outputDir =
  info "Generating coverage reports..."
  
  let backendHtmlReport = Path.Combine(outputDir, "backend", "index.html")
  let frontendHtmlReport = Path.Combine(outputDir, "frontend", "index.html")
  
  // Generate backend HTML report if not exists
  if File.Exists(Path.Combine(outputDir, "backend", "coverage.opencover.xml")) &&
     not (File.Exists backendHtmlReport) then
    let reportArgs = sprintf "reportgenerator -reports:%s/backend/coverage.opencover.xml -targetdir:%s/backend/html -reporttypes:Html"
      outputDir outputDir
    runCommand solutionDir "dotnet" reportArgs |> ignore
  
  // Create summary report
  let summaryPath = Path.Combine(outputDir, "summary.txt")
  use writer = new StreamWriter(summaryPath)
  
  writer.WriteLine("Test Coverage Summary")
  writer.WriteLine("====================")
  writer.WriteLine()
  writer.WriteLine(sprintf "Generated: %s" (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")))
  writer.WriteLine()
  
  if File.Exists backendHtmlReport then
    writer.WriteLine(sprintf "Backend coverage report: %s" backendHtmlReport)
  
  if File.Exists frontendHtmlReport then
    writer.WriteLine(sprintf "Frontend coverage report: %s" frontendHtmlReport)
    
  writer.Close()
  
  info (sprintf "Coverage reports saved to: %s" outputDir)

// Main test orchestrator
let runTests target coverage filter outputDir =
  let startTime = DateTime.Now
  
  printfn ""
  info "═══════════════════════════════════════"
  info "    Requirements Village Test Runner    "
  info "═══════════════════════════════════════"
  printfn ""
  
  ensureDirectory outputDir
  
  let results = 
    match target with
    | All ->
      let backendResult = runBackendTests coverage filter outputDir
      printfn ""
      let frontendResult = runFrontendTests coverage outputDir
      
      if backendResult = 0 && frontendResult = 0 then 0 else 1
      
    | Backend ->
      runBackendTests coverage filter outputDir
      
    | Frontend ->
      runFrontendTests coverage outputDir
  
  if coverage then
    printfn ""
    generateCoverageReport outputDir
  
  let duration = DateTime.Now - startTime
  printfn ""
  info (sprintf "Total time: %.2f seconds" duration.TotalSeconds)
  printfn ""
  
  if results = 0 then
    success "═══════════════════════════════════════"
    success "        All tests passed! 🎉           "
    success "═══════════════════════════════════════"
  else
    error "═══════════════════════════════════════"
    error "        Some tests failed 😞           "
    error "═══════════════════════════════════════"
    
  results

// Parse arguments and run
let parser = ArgumentParser.Create<Arguments>(programName = "TestRunner.fsx")

try
  let args = parser.Parse(fsi.CommandLineArgs.[1..])
  
  let target = args.GetResult Target
  let coverage = args.Contains Coverage
  let filter = args.TryGetResult Filter
  let outputDir = 
    match args.TryGetResult Output with
    | Some dir -> dir
    | None -> coverageDir
  
  let exitCode = runTests target coverage filter outputDir
  exit exitCode
  
with
| :? ArguParseException as ex ->
  printfn "%s" ex.Message
  printfn ""
  printfn "Usage examples:"
  printfn "  dotnet fsi TestRunner.fsx All              # Run all tests"
  printfn "  dotnet fsi TestRunner.fsx Backend -c       # Run backend tests with coverage"
  printfn "  dotnet fsi TestRunner.fsx Frontend         # Run frontend tests only"
  printfn "  dotnet fsi TestRunner.fsx All -c -o ./cov  # Run all tests with coverage to custom directory"
  printfn "  dotnet fsi TestRunner.fsx Backend -f \"ProjectService\" # Run filtered backend tests"
  exit 1