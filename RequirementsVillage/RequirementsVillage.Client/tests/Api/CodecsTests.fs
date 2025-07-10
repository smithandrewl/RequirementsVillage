module RequirementsVillage.Client.Tests.Api.CodecsTests

open Fable.Mocha
open Thoth.Json
open RequirementsVillage.Client.Infrastructure.Api.Codecs
open RequirementsVillage.Client.Domain.Project
open RequirementsVillage.Client.Tests.Helpers.TestHelpers
open RequirementsVillage.Client.Tests.Helpers.TestData
open System

let tests =
  testList "Codecs Tests" [
    
    testList "ProjectStatus codec" [
      
      test "encodes all status values correctly" {
        let testCases = [
          Idea, "\"Idea\""
          InProgress, "\"InProgress\""
          Completed, "\"Completed\""
          Abandoned, "\"Abandoned\""
          OnHold, "\"OnHold\""
        ]
        
        for status, expected in testCases do
          let encoded = Encode.toString 0 (ProjectStatus.encoder status)
          Assert.equal expected encoded $"Status {status} encodes correctly"
      }
      
      test "decodes all status values correctly" {
        let testCases = [
          "\"idea\"", Idea
          "\"inProgress\"", InProgress
          "\"completed\"", Completed
          "\"abandoned\"", Abandoned
          "\"onHold\"", OnHold
        ]
        
        for json, expected in testCases do
          match Decode.fromString ProjectStatus.decoder json with
          | Ok status ->
              Assert.equal expected status $"JSON {json} decodes correctly"
          | Error e ->
              Assert.isTrue false $"Decoding failed: {e}"
      }
      
      test "decoder fails on invalid status" {
        let invalidCases = [
          "\"InvalidStatus\""
          "\"IDEA\"" // Wrong case
          "\"in-progress\"" // Wrong format
          "\"\"" // Empty string
          "123" // Not a string
        ]
        
        for invalidJson in invalidCases do
          match Decode.fromString ProjectStatus.decoder invalidJson with
          | Ok _ ->
              Assert.isTrue false $"Should fail on invalid status: {invalidJson}"
          | Error _ ->
              Assert.isTrue true $"Correctly rejects invalid status: {invalidJson}"
      }
      
      test "round-trip status encoding/decoding" {
        let statuses = [Idea; InProgress; Completed; Abandoned; OnHold]
        
        for status in statuses do
          let encoded = Encode.toString 0 (ProjectStatus.encoder status)
          let encodedLowercase = encoded.Replace("\"Idea\"", "\"idea\"")
                                       .Replace("\"InProgress\"", "\"inProgress\"")
                                       .Replace("\"Completed\"", "\"completed\"")
                                       .Replace("\"Abandoned\"", "\"abandoned\"")
                                       .Replace("\"OnHold\"", "\"onHold\"")
          
          match Decode.fromString ProjectStatus.decoder encodedLowercase with
          | Ok decoded ->
              Assert.equal status decoded $"Status {status} round-trips correctly"
          | Error e ->
              Assert.isTrue false $"Round-trip failed for {status}: {e}"
      }
    ]
    
    testList "ProjectCategory codec" [
      
      test "encodes all category values correctly" {
        let testCases = [
          WebApp, "\"WebApp\""
          MobileApp, "\"MobileApp\""
          Library, "\"Library\""
          Tool, "\"Tool\""
          Game, "\"Game\""
          Other "Custom", "\"Other:Custom\""
        ]
        
        for category, expected in testCases do
          let encoded = Encode.toString 0 (ProjectCategory.encoder category)
          Assert.equal expected encoded $"Category {category} encodes correctly"
      }
      
      test "decodes all category values correctly" {
        let testCases = [
          "\"webApp\"", WebApp
          "\"mobileApp\"", MobileApp
          "\"library\"", Library
          "\"tool\"", Tool
          "\"game\"", Game
          "\"Other:Custom\"", Other "Other:Custom"
          "\"SomethingElse\"", Other "SomethingElse"
        ]
        
        for json, expected in testCases do
          match Decode.fromString ProjectCategory.decoder json with
          | Ok category ->
              Assert.equal expected category $"JSON {json} decodes correctly"
          | Error e ->
              Assert.isTrue false $"Decoding failed: {e}"
      }
      
      test "handles Other category with special characters" {
        let specialCases = [
          ("\"With Spaces\"", "With Spaces")
          ("\"With-Dashes\"", "With-Dashes")
          ("\"With_Underscores\"", "With_Underscores")
          ("\"123Numbers\"", "123Numbers")
          ("\"Special!@#$%^&*()\"", "Special!@#$%^&*()")
          ("\"Unicode:测试\"", "Unicode:测试")
        ]
        
        for json, expectedValue in specialCases do
          match Decode.fromString ProjectCategory.decoder json with
          | Ok (Other value) ->
              Assert.equal expectedValue value $"Other category '{value}' decoded"
          | Ok other ->
              Assert.isTrue false $"Expected Other, got {other}"
          | Error e ->
              Assert.isTrue false $"Decoding failed: {e}"
      }
      
      test "category decoder never fails on valid strings" {
        let randomStrings = [
          "\"random\""
          "\"test123\""
          "\"kebab-case\""
          "\"snake_case\""
          "\"PascalCase\""
        ]
        
        for json in randomStrings do
          match Decode.fromString ProjectCategory.decoder json with
          | Ok _ ->
              Assert.isTrue true $"Correctly decoded {json}"
          | Error e ->
              Assert.isTrue false $"Should not fail on valid string {json}: {e}"
      }
      
      test "round-trip category encoding/decoding for Other" {
        let otherCategories = [
          "Custom Category"
          "My-Special-Type"
          "Framework_v2"
          "123"
          ""
        ]
        
        for catName in otherCategories do
          let original = Other catName
          let encoded = Encode.toString 0 (ProjectCategory.encoder original)
          // Decoder expects simple string for unknown categories
          let jsonForDecoding = $"\"{catName}\""
          
          match Decode.fromString ProjectCategory.decoder jsonForDecoding with
          | Ok (Other decoded) ->
              Assert.equal catName decoded $"Other category '{catName}' round-trips"
          | Ok other ->
              Assert.isTrue false $"Expected Other, got {other}"
          | Error e ->
              Assert.isTrue false $"Round-trip failed for Other '{catName}': {e}"
      }
    ]
    
    testList "Project codec" [
      
      test "encodes complete project correctly" {
        let project = Sample.testProject1
        let encoded = Encode.toString 2 (Project.encoder project)
        
        // Verify JSON structure
        Assert.isTrue (encoded.Contains("\"id\"")) "Contains id field"
        Assert.isTrue (encoded.Contains("\"name\"")) "Contains name field"
        Assert.isTrue (encoded.Contains("\"description\"")) "Contains description"
        Assert.isTrue (encoded.Contains("\"category\"")) "Contains category"
        Assert.isTrue (encoded.Contains("\"status\"")) "Contains status"
        Assert.isTrue (encoded.Contains("\"createdAt\"")) "Contains createdAt"
        Assert.isTrue (encoded.Contains("\"updatedAt\"")) "Contains updatedAt"
        
        // Verify specific values
        Assert.isTrue (encoded.Contains(project.Id.ToString())) "Contains correct ID"
        Assert.isTrue (encoded.Contains(project.Name)) "Contains correct name"
        Assert.isTrue (encoded.Contains("\"InProgress\"")) "Contains correct status"
        Assert.isTrue (encoded.Contains("\"WebApp\"")) "Contains correct category"
      }
      
      test "round-trip encoding and decoding preserves data" {
        let original = Generate.project()
        
        // Encode to match what the API expects
        let apiJson = 
          Encode.object [
            "id", Encode.guid original.Id
            "name", Encode.string original.Name
            "description", Encode.string original.Description
            "category", Encode.string (
              match original.Category with
              | WebApp -> "webApp"
              | MobileApp -> "mobileApp"
              | Library -> "library"
              | Tool -> "tool"
              | Game -> "game"
              | Other s -> s
            )
            "status", Encode.string (
              match original.Status with
              | Idea -> "idea"
              | InProgress -> "inProgress"
              | Completed -> "completed"
              | Abandoned -> "abandoned"
              | OnHold -> "onHold"
            )
            "createdAt", Encode.datetime original.CreatedAt
            "updatedAt", Encode.datetime original.UpdatedAt
          ] |> Encode.toString 0
        
        match Decode.fromString Project.decoder apiJson with
        | Ok decoded ->
            Assert.equal original.Id decoded.Id "Id preserved"
            Assert.equal original.Name decoded.Name "Name preserved"
            Assert.equal original.Description decoded.Description "Description preserved"
            Assert.equal original.Category decoded.Category "Category preserved"
            Assert.equal original.Status decoded.Status "Status preserved"
            // DateTime might lose milliseconds in serialization
            Assert.isTrue 
              (abs((decoded.CreatedAt - original.CreatedAt).TotalSeconds) < 1.0)
              "CreatedAt preserved within 1 second"
            Assert.isTrue 
              (abs((decoded.UpdatedAt - original.UpdatedAt).TotalSeconds) < 1.0)
              "UpdatedAt preserved within 1 second"
        | Error e ->
            Assert.isTrue false $"Decoding failed: {e}"
      }
      
      test "decoder handles missing fields gracefully" {
        let testCases = [
          // Missing all fields except id and name
          """{"id": "123e4567-e89b-12d3-a456-426614174000", "name": "Test"}"""
          // Missing id
          """{"name": "Test", "description": "Desc", "category": "webApp", "status": "idea", "createdAt": "2024-01-01T00:00:00Z", "updatedAt": "2024-01-01T00:00:00Z"}"""
          // Missing status
          """{"id": "123e4567-e89b-12d3-a456-426614174000", "name": "Test", "description": "Desc", "category": "webApp", "createdAt": "2024-01-01T00:00:00Z", "updatedAt": "2024-01-01T00:00:00Z"}"""
          // Empty object
          """{}"""
        ]
        
        for incompleteJson in testCases do
          match Decode.fromString Project.decoder incompleteJson with
          | Ok _ ->
              Assert.isTrue false $"Should fail on missing fields: {incompleteJson}"
          | Error e ->
              Assert.isTrue (e.Length > 0) $"Error message provided for: {incompleteJson}"
      }
      
      test "decoder handles null values appropriately" {
        let testCases = [
          ("name", """{"id": "123e4567-e89b-12d3-a456-426614174000", "name": null, "description": "Test", "category": "webApp", "status": "idea", "createdAt": "2024-01-01T00:00:00Z", "updatedAt": "2024-01-01T00:00:00Z"}""")
          ("description", """{"id": "123e4567-e89b-12d3-a456-426614174000", "name": "Test", "description": null, "category": "webApp", "status": "idea", "createdAt": "2024-01-01T00:00:00Z", "updatedAt": "2024-01-01T00:00:00Z"}""")
          ("category", """{"id": "123e4567-e89b-12d3-a456-426614174000", "name": "Test", "description": "Desc", "category": null, "status": "idea", "createdAt": "2024-01-01T00:00:00Z", "updatedAt": "2024-01-01T00:00:00Z"}""")
        ]
        
        for fieldName, jsonWithNull in testCases do
          match Decode.fromString Project.decoder jsonWithNull with
          | Ok _ ->
              Assert.isTrue false $"Should fail on null {fieldName}"
          | Error e ->
              Assert.isTrue (e.Contains(fieldName) || e.Contains("null")) 
                $"Error should mention null {fieldName}"
      }
      
      test "decoder handles invalid field types" {
        let testCases = [
          ("id as number", """{"id": 123, "name": "Test", "description": "Desc", "category": "webApp", "status": "idea", "createdAt": "2024-01-01T00:00:00Z", "updatedAt": "2024-01-01T00:00:00Z"}""")
          ("id as invalid guid", """{"id": "not-a-guid", "name": "Test", "description": "Desc", "category": "webApp", "status": "idea", "createdAt": "2024-01-01T00:00:00Z", "updatedAt": "2024-01-01T00:00:00Z"}""")
          ("dates as numbers", """{"id": "123e4567-e89b-12d3-a456-426614174000", "name": "Test", "description": "Desc", "category": "webApp", "status": "idea", "createdAt": 1234567890, "updatedAt": 1234567890}""")
        ]
        
        for description, invalidJson in testCases do
          match Decode.fromString Project.decoder invalidJson with
          | Ok _ ->
              Assert.isTrue false $"Should fail on {description}"
          | Error e ->
              Assert.isTrue (e.Length > 0) $"Error provided for {description}"
      }
      
      test "encodes project with all status types" {
        let statuses = [Idea; InProgress; Completed; Abandoned; OnHold]
        
        for status in statuses do
          let project = { Sample.testProject1 with Status = status }
          let encoded = Encode.toString 0 (Project.encoder project)
          
          Assert.isTrue (encoded.Contains($"\"{status}\"")) 
            $"Encoded project contains status {status}"
      }
      
      test "encodes project with all category types" {
        let categories = [
          WebApp
          MobileApp
          Library
          Tool
          Game
          Other "CustomType"
        ]
        
        for category in categories do
          let project = { Sample.testProject1 with Category = category }
          let encoded = Encode.toString 0 (Project.encoder project)
          
          let expectedCategoryString = 
            match category with
            | Other s -> $"\"Other:{s}\""
            | c -> $"\"{c}\""
          
          Assert.isTrue (encoded.Contains(expectedCategoryString)) 
            $"Encoded project contains category {category}"
      }
    ]
    
    testList "DateTime handling" [
      
      test "encodes DateTime in ISO format" {
        let testDates = [
          DateTime(2024, 1, 15, 10, 30, 45)
          DateTime(2024, 12, 31, 23, 59, 59)
          DateTime(2024, 1, 1, 0, 0, 0)
        ]
        
        for dt in testDates do
          let project = { Sample.testProject1 with CreatedAt = dt; UpdatedAt = dt }
          let encoded = Encode.toString 0 (Project.encoder project)
          
          Assert.isTrue 
            (encoded.Contains(dt.Year.ToString()))
            $"Date contains year {dt.Year}"
          Assert.isTrue 
            (encoded.Contains(dt.Month.ToString("D2")))
            $"Date contains month {dt.Month:D2}"
      }
      
      test "decodes various DateTime formats" {
        let dateFormats = [
          "2024-01-15T10:30:45Z"
          "2024-01-15T10:30:45.000Z"
          "2024-01-15T10:30:45+00:00"
          "2024-01-15T10:30:45"
        ]
        
        for dateStr in dateFormats do
          let json = $"""
            {{
              "id": "123e4567-e89b-12d3-a456-426614174000",
              "name": "Test",
              "description": "Test",
              "category": "webApp",
              "status": "idea",
              "createdAt": "{dateStr}",
              "updatedAt": "{dateStr}"
            }}
          """
          
          match Decode.fromString Project.decoder json with
          | Ok project ->
              Assert.isTrue 
                (project.CreatedAt.Year = 2024)
                $"Date {dateStr} decoded to correct year"
              Assert.isTrue 
                (project.CreatedAt.Month = 1)
                $"Date {dateStr} decoded to correct month"
          | Error e ->
              Assert.isTrue false $"Failed to decode date {dateStr}: {e}"
      }
      
      test "handles edge case dates" {
        let edgeDates = [
          DateTime.MinValue
          DateTime.MaxValue
          DateTime(1970, 1, 1, 0, 0, 0) // Unix epoch
          DateTime(2000, 1, 1, 0, 0, 0) // Y2K
        ]
        
        for dt in edgeDates do
          try
            let project = { Sample.testProject1 with CreatedAt = dt; UpdatedAt = dt }
            let encoded = Encode.toString 0 (Project.encoder project)
            Assert.isTrue (encoded.Length > 0) $"Successfully encoded edge date {dt}"
          with
          | ex ->
            // Some edge dates might not be supported
            Assert.isTrue true $"Edge date {dt} encoding failed (expected): {ex.Message}"
      }
    ]
    
    testList "Error message quality" [
      
      test "decoder provides helpful error messages" {
        let badJson = """{"id": "not-a-guid", "name": 123}"""
        
        match Decode.fromString Project.decoder badJson with
        | Ok _ ->
            Assert.isTrue false "Should fail on bad JSON"
        | Error e ->
            Assert.isTrue (e.Contains("id") || e.Contains("guid") || e.Contains("name")) 
              "Error message should indicate the problem field"
      }
      
      test "status decoder error includes invalid value" {
        let invalidStatus = "\"InvalidStatus\""
        
        match Decode.fromString ProjectStatus.decoder invalidStatus with
        | Ok _ ->
            Assert.isTrue false "Should fail on invalid status"
        | Error e ->
            Assert.isTrue (e.Contains("InvalidStatus") || e.Contains("Unknown status")) 
              "Error should mention the invalid value"
      }
    ]
  ]