module RequirementsVillage.Shared.Serialization

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