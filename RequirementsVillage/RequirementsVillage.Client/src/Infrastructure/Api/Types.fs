module RequirementsVillage.Client.Infrastructure.Api.Types

// API error types
type ApiError =
  | NetworkError  of string
  | DecodingError of string
  | ServerError   of int * string