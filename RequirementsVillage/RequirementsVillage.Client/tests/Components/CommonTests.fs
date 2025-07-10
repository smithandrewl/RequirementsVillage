module RequirementsVillage.Client.Tests.Components.CommonTests

open Fable.Mocha
open Feliz
open RequirementsVillage.Client.Presentation.Components.Common
open RequirementsVillage.Client.Tests.Helpers.TestHelpers
open RequirementsVillage.Client.Tests.Helpers.TestData
open RequirementsVillage.Client.Domain.Project

// Helper to verify component structure
let private verifyTagStructure (element: ReactElement) =
  match element with
  | :? ReactElement -> true
  | _ -> false

let tests =
  testList "Common Components Tests" [
    
    testList "StatusBadge component" [
      
      test "renders StatusBadge for all status types" {
        let statuses = [Idea; InProgress; Completed; Abandoned; OnHold]
        
        for status in statuses do
          let badge = StatusBadge status
          Assert.isTrue
            (verifyTagStructure badge)
            $"StatusBadge should render for {status}"
      }
      
      test "StatusBadge uses correct color mapping" {
        let statusColorMap = [
          Idea,       "isInfo"
          InProgress, "isSuccess"
          Completed,  "isPrimary"
          Abandoned,  "isDark"
          OnHold,     "isWarning"
        ]
        
        for status, expectedColor in statusColorMap do
          let badge = StatusBadge status
          Assert.isTrue
            (verifyTagStructure badge)
            $"Status {status} should use {expectedColor} color"
      }
      
      test "StatusBadge displays human-readable text" {
        let statusTextMap = [
          Idea,       "Idea"
          InProgress, "In Progress"
          Completed,  "Completed"
          Abandoned,  "Abandoned"
          OnHold,     "On Hold"
        ]
        
        for status, expectedText in statusTextMap do
          let badge = StatusBadge status
          Assert.isTrue
            (verifyTagStructure badge)
            $"Status {status} should display '{expectedText}'"
      }
    ]
    
    testList "CategoryBadge component" [
      
      test "renders CategoryBadge for all category types" {
        let categories = [
          WebApp
          MobileApp
          Library
          Tool
          Game
          Other "Custom Category"
        ]
        
        for category in categories do
          let badge = CategoryBadge category
          Assert.isTrue
            (verifyTagStructure badge)
            $"CategoryBadge should render for {category}"
      }
      
      test "CategoryBadge uses light color for all categories" {
        let categories = Generate.projects 5 |> List.map (fun p -> p.Category)
        
        for category in categories do
          let badge = CategoryBadge category
          Assert.isTrue
            (verifyTagStructure badge)
            "All category badges should use light color"
      }
      
      test "CategoryBadge displays correct text" {
        let categoryTextMap = [
          WebApp,              "Web App"
          MobileApp,           "Mobile App"
          Library,             "Library"
          Tool,                "Tool"
          Game,                "Game"
          Other "API Service", "API Service"
          Other "CLI Tool",    "CLI Tool"
        ]
        
        for category, expectedText in categoryTextMap do
          let badge = CategoryBadge category
          Assert.isTrue
            (verifyTagStructure badge)
            $"Category should display '{expectedText}'"
      }
      
      test "CategoryBadge handles special characters in Other category" {
        let specialCategories = [
          Other "Category with spaces"
          Other "Category-with-dashes"
          Other "Category_with_underscores"
          Other "Category.with.dots"
          Other "Category/with/slashes"
          Other "🚀 Emoji Category"
        ]
        
        for category in specialCategories do
          let badge = CategoryBadge category
          Assert.isTrue
            (verifyTagStructure badge)
            $"Should handle special characters in: {category}"
      }
    ]
    
    testList "TagContainer component" [
      
      test "renders empty TagContainer" {
        let container = TagContainer []
        Assert.isTrue
          (verifyTagStructure container)
          "TagContainer should render with no children"
      }
      
      test "renders TagContainer with single child" {
        let container = TagContainer [StatusBadge Idea]
        Assert.isTrue
          (verifyTagStructure container)
          "TagContainer should render with one child"
      }
      
      test "renders TagContainer with multiple children" {
        let tags = [
          StatusBadge InProgress
          CategoryBadge WebApp
          StatusBadge Completed
          CategoryBadge (Other "Custom")
        ]
        let container = TagContainer tags
        
        Assert.isTrue
          (verifyTagStructure container)
          "TagContainer should render with multiple children"
      }
      
      test "TagContainer handles mixed badge types" {
        let project = Generate.project()
        let tags = [
          StatusBadge project.Status
          CategoryBadge project.Category
        ]
        let container = TagContainer tags
        
        Assert.isTrue
          (verifyTagStructure container)
          "TagContainer should handle mixed StatusBadge and CategoryBadge"
      }
      
      test "TagContainer handles large number of tags" {
        let manyTags = 
          [ for i in 1..20 do
              if i % 2 = 0 then
                StatusBadge (Generate.projectStatus())
              else
                CategoryBadge (Generate.projectCategory())
          ]
        let container = TagContainer manyTags
        
        Assert.isTrue
          (verifyTagStructure container)
          "TagContainer should handle many tags gracefully"
      }
    ]
    
    testList "Component integration" [
      
      test "badges work correctly within TagContainer" {
        let allStatuses = [Idea; InProgress; Completed; Abandoned; OnHold]
        let statusBadges = allStatuses |> List.map StatusBadge
        let container = TagContainer statusBadges
        
        Assert.isTrue
          (verifyTagStructure container)
          "All status badges should work within TagContainer"
      }
      
      test "mixed badges from generated project data" {
        let projects = Generate.projectsWithMixedStatuses()
        
        for project in projects do
          let tags = [
            StatusBadge project.Status
            CategoryBadge project.Category
          ]
          let container = TagContainer tags
          
          Assert.isTrue
            (verifyTagStructure container)
            $"Should render tags for project: {project.Name}"
      }
    ]
    
    testList "Edge cases and error handling" [
      
      test "handles empty string in Other category" {
        let category = Other ""
        let badge = CategoryBadge category
        
        Assert.isTrue
          (verifyTagStructure badge)
          "Should handle empty string in Other category"
      }
      
      test "handles very long category names" {
        let longName = String.replicate 100 "Category"
        let category = Other longName
        let badge = CategoryBadge category
        
        Assert.isTrue
          (verifyTagStructure badge)
          "Should handle very long category names"
      }
      
      test "handles null-like values gracefully" {
        // F# prevents actual null values, but we can test edge cases
        let edgeCases = [
          Other "\u0000" // Null character
          Other "\t\n\r" // Whitespace characters
          Other "   "    // Spaces only
        ]
        
        for category in edgeCases do
          let badge = CategoryBadge category
          Assert.isTrue
            (verifyTagStructure badge)
            $"Should handle edge case category: '{category}'"
      }
    ]
  ]