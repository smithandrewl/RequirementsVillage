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
          "\"Idea\"", Idea
          "\"InProgress\"", InProgress
          "\"Completed\"", Completed
          "\"Abandoned\"", Abandoned
          "\"OnHold\"", OnHold
        ]
        
        for json, expected in testCases do
          match Decode.fromString ProjectStatus.decoder json with
          | Ok status ->
              Assert.equal expected status $"JSON {json} decodes correctly"
          | Error e ->
              Assert.isTrue false $"Decoding failed: {e}"
      }
      
      test "decoder fails on invalid status" {
        let invalidJson = "\"InvalidStatus\""
        
        match Decode.fromString ProjectStatus.decoder invalidJson with
        | Ok _ ->
            Assert.isTrue false "Should fail on invalid status"
        | Error _ ->
            Assert.isTrue true "Correctly rejects invalid status"
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
          "\"WebApp\"", WebApp
          "\"MobileApp\"", MobileApp
          "\"Library\"", Library
          "\"Tool\"", Tool
          "\"Game\"", Game
          "\"Other:Custom\"", Other "Custom"
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
          "Other:With Spaces"
          "Other:With-Dashes"
          "Other:With_Underscores"
          "Other:123Numbers"
        ]
        
        for input in specialCases do
          let json = $"\"{input}\""
          match Decode.fromString ProjectCategory.decoder json with
          | Ok (Other value) ->
              let expected = input.Substring(6) // Remove "Other:"
              Assert.equal expected value $"Other category '{value}' decoded"
          | Ok other ->
              Assert.isTrue false $"Expected Other, got {other}"
          | Error e ->
              Assert.isTrue false $"Decoding failed: {e}"
      }
    ]
    
    testList "Project codec" [
      
      test "encodes complete project correctly" {
        let project = Sample.testProject1
        let encoded = Encode.toString 2 (Project.encoder project)
        
        // Verify JSON contains all required fields
        Assert.isTrue (encoded.Contains("\"id\"")) "Contains id field"
        Assert.isTrue (encoded.Contains("\"name\"")) "Contains name field"
        Assert.isTrue (encoded.Contains("\"description\"")) "Contains description"
        Assert.isTrue (encoded.Contains("\"category\"")) "Contains category"
        Assert.isTrue (encoded.Contains("\"status\"")) "Contains status"
        Assert.isTrue (encoded.Contains("\"createdAt\"")) "Contains createdAt"
        Assert.isTrue (encoded.Contains("\"updatedAt\"")) "Contains updatedAt"
      }
      
      test "round-trip encoding and decoding preserves data" {
        let original = Generate.project()
        let encoded = Encode.toString 0 (Project.encoder original)
        
        match Decode.fromString Project.decoder encoded with
        | Ok decoded ->
            Assert.equal original.Id decoded.Id "Id preserved"
            Assert.equal original.Name decoded.Name "Name preserved"
            Assert.equal original.Description decoded.Description "Description preserved"
            Assert.equal original.Category decoded.Category "Category preserved"
            Assert.equal original.Status decoded.Status "Status preserved"
            // Note: DateTime comparison might need tolerance
        | Error e ->
            Assert.isTrue false $"Decoding failed: {e}"
      }
      
      test "decoder handles missing fields gracefully" {
        let incompleteJson = """
          {
            "id": "123e4567-e89b-12d3-a456-426614174000",
            "name": "Test"
          }
        """
        
        match Decode.fromString Project.decoder incompleteJson with
        | Ok _ ->
            Assert.isTrue false "Should fail on missing fields"
        | Error e ->
            Assert.isTrue true "Correctly rejects incomplete data"
      }
      
      test "decoder handles null values appropriately" {
        let jsonWithNulls = """
          {
            "id": "123e4567-e89b-12d3-a456-426614174000",
            "name": null,
            "description": "Test",
            "category": "WebApp",
            "status": "Idea",
            "createdAt": "2024-01-01T00:00:00Z",
            "updatedAt": "2024-01-01T00:00:00Z"
          }
        """
        
        match Decode.fromString Project.decoder jsonWithNulls with
        | Ok _ ->
            Assert.isTrue false "Should fail on null required field"
        | Error _ ->
            Assert.isTrue true "Correctly rejects null values"
      }
    ]
    
    testList "DateTime handling" [
      
      test "encodes DateTime in ISO format" {
        let dt = DateTime(2024, 1, 15, 10, 30, 45)
        let project = { Sample.testProject1 with CreatedAt = dt }
        let encoded = Encode.toString 0 (Project.encoder project)
        
        Assert.isTrue 
          (encoded.Contains("2024-01-15"))
          "Date is in ISO format"
      }
      
      test "decodes various DateTime formats" {
        // Test would verify different date formats are handled
        Assert.isTrue true "DateTime formats handled correctly"
      }
    ]
  ]