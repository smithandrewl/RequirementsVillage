module RequirementsVillage.Api.Program

open System
open System.IO
open System.Text.Json
open System.Text.Json.Serialization
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Giraffe
open RequirementsVillage.Api.Models
open RequirementsVillage.Api.Persistence
open RequirementsVillage.Api.Services
open RequirementsVillage.Api.Endpoints
open FSharp.SystemTextJson

// Entry point for WebApplicationFactory
type Program() = class end

// Configure services
let configureServices (services: IServiceCollection) =
  // Add Giraffe
  services.AddGiraffe() |> ignore
  
  // Configure JSON serialization with F# support
  let jsonOptions = JsonSerializerOptions()
  jsonOptions.Converters.Add(
    Serialization.ProjectStatusConverter()
  )
  jsonOptions.Converters.Add(
    Serialization.ProjectCategoryConverter()
  )
  jsonOptions.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
  jsonOptions.Converters.Add(JsonFSharpConverter())
  
  // Add SystemTextJson serializer to Giraffe
  services.AddSingleton<Json.ISerializer>(
    SystemTextJson.Serializer(jsonOptions)
  ) |> ignore
  
  // Register repositories and services
  services.AddScoped<IProjectRepository>(fun _ -> 
    // Using in-memory repository for now
    InMemoryProjectRepository() :> IProjectRepository
  ) |> ignore
  
  services.AddScoped<IProjectService>(fun provider ->
    let repository = 
      provider.GetRequiredService<IProjectRepository>()
    ProjectService(repository) :> IProjectService
  ) |> ignore
  
  // Add CORS for development
  services.AddCors(fun options ->
    options.AddDefaultPolicy(fun builder ->
      builder
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader() |> ignore
    )
  ) |> ignore

// Configure the application
let configureApp (app: IApplicationBuilder) =
  app
    .UseDefaultFiles()
    .UseStaticFiles()
    .UseCors()
    .UseGiraffe(apiRouter)

[<EntryPoint>]
let main args =
  let builder = WebApplication.CreateBuilder(args)
  
  configureServices builder.Services
  
  let app = builder.Build()
  
  // Use developer exception page in development
  if app.Environment.IsDevelopment() then
    app.UseDeveloperExceptionPage() |> ignore
  
  configureApp app |> ignore
  
  // SPA fallback - must be after Giraffe routes
  app.MapFallbackToFile("index.html") |> ignore
  
  app.Run()
  
  0