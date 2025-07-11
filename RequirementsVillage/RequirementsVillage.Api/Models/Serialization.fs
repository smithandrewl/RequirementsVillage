namespace RequirementsVillage.Api.Models

module Serialization =
  
  open System.Text.Json
  open System.Text.Json.Serialization
  open RequirementsVillage.Shared
  
  let jsonOptions =
    let options = JsonSerializerOptions()
    options.Converters.Add(Serialization.ProjectStatusConverter())
    options.Converters.Add(Serialization.ProjectCategoryConverter())
    options.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
    options.Converters.Add(JsonFSharpConverter())
    options