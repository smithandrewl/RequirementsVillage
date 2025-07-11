module RequirementsVillage.Api.Models.Serialization

open System.Text.Json
open System.Text.Json.Serialization
open FSharp.SystemTextJson
open RequirementsVillage.Shared

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

// JSON options for serialization
let jsonOptions =
  let options = JsonSerializerOptions()
  options.Converters.Add(ProjectStatusConverter())
  options.Converters.Add(ProjectCategoryConverter())
  options.Converters.Add(JsonFSharpConverter())
  options.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
  options