namespace RequirementsVillage.Api.Models

open System
open RequirementsVillage.Shared

// Import shared models for convenience
type Project      = RequirementsVillage.Shared.Project
type ProjectStatus = RequirementsVillage.Shared.ProjectStatus
type ProjectCategory = RequirementsVillage.Shared.ProjectCategory
type ProjectError = RequirementsVillage.Shared.ProjectError

// API-specific serialization support for System.Text.Json
module Serialization =
  open System.Text.Json
  open System.Text.Json.Serialization

  // Custom converters for discriminated unions
  type ProjectStatusConverter() =
    inherit JsonConverter<ProjectStatus>()

    override _.Read(reader, typeToConvert, options) =
      match ProjectStatus.fromString (reader.GetString()) with
      | Ok status -> status
      | Error msg -> failwith msg

    override _.Write(writer, value, options) =
      writer.WriteStringValue(ProjectStatus.toString value)

  type ProjectCategoryConverter() =
    inherit JsonConverter<ProjectCategory>()

    override _.Read(reader, typeToConvert, options) =
      reader.GetString() |> ProjectCategory.fromString

    override _.Write(writer, value, options) =
      writer.WriteStringValue(ProjectCategory.toString value)

  // JSON serialization options
  let jsonOptions =
    let options = JsonSerializerOptions()

    options.Converters.Add(ProjectStatusConverter())
    options.Converters.Add(ProjectCategoryConverter())

    options.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase

    options