namespace RequirementsVillage.Shared.Tests.Helpers

open System
open FsUnit.Xunit
open RequirementsVillage.Shared

module TestHelpers =
  
  // Helper function to create a valid project with default values
  let createTestProject() = {
    Id          = Guid.NewGuid()
    Name        = "Test Project"
    Description = "A test project description"
    Category    = WebApp
    Status      = Idea
    CreatedAt   = DateTime.UtcNow
    UpdatedAt   = DateTime.UtcNow
  }
  
  // Helper to create a project with specific values
  let createProject
    (name: string)
    (description: string)
    (category: ProjectCategory)
    (status: ProjectStatus) = {
    Id          = Guid.NewGuid()
    Name        = name
    Description = description
    Category    = category
    Status      = status
    CreatedAt   = DateTime.UtcNow
    UpdatedAt   = DateTime.UtcNow
  }
  
  // Assert Result is Ok
  let shouldBeOk (result: Result<'a, 'b>) =
    match result with
    | Ok value -> value
    | Error e  -> failwithf "Expected Ok but got Error: %A" e
  
  // Assert Result is Error
  let shouldBeError (result: Result<'a, 'b>) =
    match result with
    | Ok value -> failwithf "Expected Error but got Ok: %A" value
    | Error e  -> e
  
  // Assert Option is Some
  let shouldBeSome (option: 'a option) =
    match option with
    | Some value -> value
    | None       -> failwith "Expected Some but got None"
  
  // Assert Option is None
  let shouldBeNone (option: 'a option) =
    match option with
    | Some value -> failwithf "Expected None but got Some: %A" value
    | None       -> ()
  
  // Time helpers for testing
  let yesterday = DateTime.UtcNow.AddDays(-1.0)
  let tomorrow = DateTime.UtcNow.AddDays(1.0)
  let lastWeek = DateTime.UtcNow.AddDays(-7.0)
  let nextWeek = DateTime.UtcNow.AddDays(7.0)
  
  // String generators for property-based testing
  let nonEmptyString (s: string) =
    not (String.IsNullOrWhiteSpace(s))
  
  let reasonableString (s: string) =
    nonEmptyString s && s.Length <= 1000
  
  let validProjectName (s: string) =
    nonEmptyString s && s.Length <= 100
  
  let validProjectDescription (s: string) =
    nonEmptyString s && s.Length <= 1000
  
  // Error matching helpers
  let isValidationError (error: ProjectError) =
    match error with
    | ValidationFailed _ -> true
    | _                  -> false
  
  let isNotFoundError (error: ProjectError) =
    match error with
    | NotFound _ -> true
    | _          -> false
  
  let isDatabaseError (error: ProjectError) =
    match error with
    | DatabaseError _ -> true
    | _               -> false
  
  // Async helpers
  let runAsync computation =
    computation |> Async.RunSynchronously
  
  // List helpers for testing collections
  let shouldContain (item: 'a) (list: 'a list) =
    list |> should contain item
  
  let shouldNotContain (item: 'a) (list: 'a list) =
    list |> should not' (contain item)
  
  let shouldHaveLength (expected: int) (list: 'a list) =
    list |> should haveLength expected
  
  // Comparison helpers for dates (allowing small differences)
  let shouldBeCloseTo (expected: DateTime) (tolerance: TimeSpan) (actual: DateTime) =
    let diff = abs (expected - actual).TotalMilliseconds
    let toleranceMs = tolerance.TotalMilliseconds
    if diff > toleranceMs then
      failwithf "Expected %A to be within %A of %A, but difference was %A"
        actual tolerance expected (TimeSpan.FromMilliseconds(diff))
  
  // Assert strings are equal ignoring case
  let shouldEqualIgnoreCase (expected: string) (actual: string) =
    actual.ToLowerInvariant() |> should equal (expected.ToLowerInvariant())
  
  // Validation test data
  let validProjectNameSample = "Valid Project Name"
  let validProjectDescriptionSample = "This is a valid project description."
  
  let invalidProjectName = ""
  let invalidProjectDescription = ""
  
  let tooLongProjectName = String.replicate 101 "x"
  let tooLongProjectDescription = String.replicate 1001 "x"