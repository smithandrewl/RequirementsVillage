namespace RequirementsVillage.FSharp.Api.Models

open System

// Discriminated unions for domain concepts
type ProjectStatus =
  | Idea
  | InProgress
  | Completed
  | Abandoned
  | OnHold

type ProjectCategory =
  | WebApp
  | MobileApp
  | Library
  | Tool
  | Game
  | Other of string

// Domain error types using discriminated unions
type ProjectError =
  | NotFound of projectId: Guid * searchContext: string
  | ValidationFailed of 
      field: string * reason: string * attemptedValue: obj
  | DatabaseError of 
      operation: string * tableName: string * innerError: exn
  | UnknownError of message: string

// Main domain model as an F# record
type Project = {
  Id: Guid
  Name: string
  Description: string
  Category: ProjectCategory
  Status: ProjectStatus
  CreatedAt: DateTime
  UpdatedAt: DateTime
}

// Module for serialization helpers
module Serialization =
  open System.Text.Json
  open System.Text.Json.Serialization
  
  // Custom converters for discriminated unions
  type ProjectStatusConverter() =
    inherit JsonConverter<ProjectStatus>()
    
    override _.Read(reader, typeToConvert, options) =
      match reader.GetString() with
      | "idea" -> Idea
      | "inProgress" -> InProgress
      | "completed" -> Completed
      | "abandoned" -> Abandoned
      | "onHold" -> OnHold
      | s -> failwithf "Unknown ProjectStatus: %s" s
      
    override _.Write(writer, value, options) =
      let stringValue =
        match value with
        | Idea -> "idea"
        | InProgress -> "inProgress"
        | Completed -> "completed"
        | Abandoned -> "abandoned"
        | OnHold -> "onHold"
      writer.WriteStringValue(stringValue)
  
  type ProjectCategoryConverter() =
    inherit JsonConverter<ProjectCategory>()
    
    override _.Read(reader, typeToConvert, options) =
      match reader.GetString() with
      | "webApp" -> WebApp
      | "mobileApp" -> MobileApp
      | "library" -> Library
      | "tool" -> Tool
      | "game" -> Game
      | s -> Other s
      
    override _.Write(writer, value, options) =
      let stringValue =
        match value with
        | WebApp -> "webApp"
        | MobileApp -> "mobileApp"
        | Library -> "library"
        | Tool -> "tool"
        | Game -> "game"
        | Other s -> s
      writer.WriteStringValue(stringValue)
  
  // JSON serialization options
  let jsonOptions =
    let options = JsonSerializerOptions()
    options.Converters.Add(ProjectStatusConverter())
    options.Converters.Add(ProjectCategoryConverter())
    options.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
    options