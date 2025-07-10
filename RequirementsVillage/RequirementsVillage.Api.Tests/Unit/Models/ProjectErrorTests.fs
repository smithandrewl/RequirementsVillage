namespace RequirementsVillage.Api.Tests.Unit.Models

open System
open System.Text.Json
open Xunit
open FsUnit.Xunit
open FsCheck
open FsCheck.Xunit
open RequirementsVillage.Api.Models
open RequirementsVillage.Api.Tests.Helpers

module ProjectErrorTests =
  
  [<Fact>]
  let ``NotFound error should contain project ID and context`` () =
    let projectId = Guid.NewGuid()
    let context = "GetProjectById operation"
    let error = NotFound (projectId, context)
    
    match error with
    | NotFound (id, ctx) ->
      id  |> should equal projectId
      ctx |> should equal context
    | _ -> failwith "Expected NotFound error"
  
  [<Fact>]
  let ``ValidationFailed error should contain field, reason, and value`` () =
    let field = "Name"
    let reason = "Name cannot be empty"
    let attemptedValue = box ""
    let error = ValidationFailed (field, reason, attemptedValue)
    
    match error with
    | ValidationFailed (f, r, v) ->
      f |> should equal field
      r |> should equal reason
      v |> should equal attemptedValue
    | _ -> failwith "Expected ValidationFailed error"
  
  [<Fact>]
  let ``DatabaseError should contain operation, table, and inner exception`` () =
    let operation = "INSERT"
    let tableName = "Projects"
    let innerEx = Exception("Connection timeout")
    let error = DatabaseError (operation, tableName, innerEx)
    
    match error with
    | DatabaseError (op, table, ex) ->
      op    |> should equal operation
      table |> should equal tableName
      ex    |> should equal innerEx
    | _ -> failwith "Expected DatabaseError"
  
  [<Fact>]
  let ``UnknownError should contain message`` () =
    let message = "An unexpected error occurred"
    let error = UnknownError message
    
    match error with
    | UnknownError msg -> msg |> should equal message
    | _                -> failwith "Expected UnknownError"
  
  [<Property>]
  let ``NotFound error properties should preserve values`` (id: Guid) (context: string) =
    (TestHelpers.nonEmptyString context) ==> lazy (
      let error = NotFound (id, context)
      match error with
      | NotFound (projectId, searchContext) ->
        projectId     = id &&
        searchContext = context
      | _ -> false
    )
  
  [<Property>]
  let ``ValidationFailed error properties should preserve values``
    (field: string)
    (reason: string)
    (value: int) =
    (TestHelpers.nonEmptyString field && TestHelpers.nonEmptyString reason) ==> lazy (
      let error = ValidationFailed (field, reason, box value)
      match error with
      | ValidationFailed (f, r, v) ->
        f = field &&
        r = reason &&
        (unbox v : int) = value
      | _ -> false
    )
  
  module ErrorMatching =
    
    [<Fact>]
    let ``Error matching helpers should correctly identify error types`` () =
      let notFoundError = NotFound (Guid.NewGuid(), "test")
      let validationError = ValidationFailed ("field", "reason", box "value")
      let databaseError = DatabaseError ("op", "table", Exception())
      let unknownError = UnknownError "message"
      
      // Test NotFound matching
      TestHelpers.isNotFoundError notFoundError     |> should equal true
      TestHelpers.isNotFoundError validationError   |> should equal false
      TestHelpers.isNotFoundError databaseError     |> should equal false
      TestHelpers.isNotFoundError unknownError      |> should equal false
      
      // Test ValidationFailed matching
      TestHelpers.isValidationError notFoundError   |> should equal false
      TestHelpers.isValidationError validationError |> should equal true
      TestHelpers.isValidationError databaseError   |> should equal false
      TestHelpers.isValidationError unknownError    |> should equal false
      
      // Test DatabaseError matching
      TestHelpers.isDatabaseError notFoundError     |> should equal false
      TestHelpers.isDatabaseError validationError   |> should equal false
      TestHelpers.isDatabaseError databaseError     |> should equal true
      TestHelpers.isDatabaseError unknownError      |> should equal false
  
  module ErrorCreation =
    
    [<Fact>]
    let ``Common validation errors should be easily creatable`` () =
      let emptyNameError = ValidationFailed ("Name", "Name is required", box "")
      let longNameError = ValidationFailed ("Name", "Name exceeds maximum length", box (String.replicate 101 "a"))
      let emptyDescError = ValidationFailed ("Description", "Description is required", box "")
      
      [ emptyNameError; longNameError; emptyDescError ]
      |> List.iter (fun error ->
        match error with
        | ValidationFailed (field, reason, value) ->
          field  |> should not' (be NullOrEmptyString)
          reason |> should not' (be NullOrEmptyString)
          value  |> should not' (be null)
        | _ -> failwith "Expected ValidationFailed"
      )
    
    [<Fact>]
    let ``Database errors should include meaningful context`` () =
      let insertError = DatabaseError ("INSERT", "Projects", Exception("Unique constraint violation"))
      let updateError = DatabaseError ("UPDATE", "Projects", Exception("Record not found"))
      let deleteError = DatabaseError ("DELETE", "Projects", Exception("Foreign key constraint"))
      
      [ insertError; updateError; deleteError ]
      |> List.iter (fun error ->
        match error with
        | DatabaseError (op, table, ex) ->
          op    |> should not' (be NullOrEmptyString)
          table |> should equal "Projects"
          ex.Message |> should not' (be NullOrEmptyString)
        | _ -> failwith "Expected DatabaseError"
      )
  
  module ErrorHandling =
    
    [<Fact>]
    let ``Result type should work with ProjectError`` () =
      let successResult : Result<Project, ProjectError> = 
        Ok (TestHelpers.createTestProject())
      
      let errorResult : Result<Project, ProjectError> = 
        Error (NotFound (Guid.NewGuid(), "test"))
      
      successResult |> TestHelpers.shouldBeOk |> ignore
      errorResult |> TestHelpers.shouldBeError |> TestHelpers.isNotFoundError |> should equal true
    
    [<Fact>]
    let ``Multiple error types should be distinguishable in pattern matching`` () =
      let errors = [
        NotFound (Guid.NewGuid(), "search")
        ValidationFailed ("Name", "Too long", box "test")
        DatabaseError ("SELECT", "Projects", Exception())
        UnknownError "Something went wrong"
      ]
      
      let errorTypes =
        errors
        |> List.map (fun error ->
          match error with
          | NotFound _         -> "NotFound"
          | ValidationFailed _ -> "ValidationFailed"
          | DatabaseError _    -> "DatabaseError"
          | UnknownError _     -> "UnknownError"
        )
      
      errorTypes |> should equal [ "NotFound"; "ValidationFailed"; "DatabaseError"; "UnknownError" ]
  
  module EdgeCases =
    
    [<Fact>]
    let ``ValidationFailed should handle null values gracefully`` () =
      let error = ValidationFailed ("Field", "Value was null", null)
      
      match error with
      | ValidationFailed (field, reason, value) ->
        field  |> should equal "Field"
        reason |> should equal "Value was null"
        value  |> should equal null
      | _ -> failwith "Expected ValidationFailed"
    
    [<Fact>]
    let ``DatabaseError should preserve exception stack trace`` () =
      let innerEx = 
        try
          failwith "Simulated database error"
        with ex -> ex
      
      let error = DatabaseError ("UPDATE", "Projects", innerEx)
      
      match error with
      | DatabaseError (_, _, ex) ->
        ex.StackTrace |> should not' (be null)
        ex.Message    |> should equal "Simulated database error"
      | _ -> failwith "Expected DatabaseError"
    
    [<Fact>]
    let ``NotFound error context should support multiline strings`` () =
      let context = """
        Searching for project in multiple locations:
        - Database: Projects table
        - Cache: Redis key 'project:123'
        - Fallback: Archive storage
      """
      
      let error = NotFound (Guid.NewGuid(), context.Trim())
      
      match error with
      | NotFound (_, ctx) ->
        ctx |> should haveSubstring "Database"
        ctx |> should haveSubstring "Cache"
        ctx |> should haveSubstring "Fallback"
      | _ -> failwith "Expected NotFound"
    
    [<Property>]
    let ``UnknownError should handle any string message`` (message: string) =
      (message <> null) ==> lazy (
        let error = UnknownError message
        match error with
        | UnknownError msg -> msg = message
        | _                -> false
      )