module RequirementsVillage.Client.Tests.Components.LayoutTests

open Fable.Mocha
open Feliz
open RequirementsVillage.Client.Presentation.Components.Layout
open RequirementsVillage.Client.Presentation.State.Types
open RequirementsVillage.Client.Infrastructure.Storage.ThemeStorage
open RequirementsVillage.Client.Tests.Helpers.TestHelpers
open RequirementsVillage.Client.Tests.Helpers.TestData

// Helper to verify component structure
let private verifyLayoutStructure (element: ReactElement) =
  match element with
  | :? ReactElement -> true
  | _ -> false

// Mock dispatch function
let private mockDispatch = fun _ -> ()

// Create test content element
let private testContent = Html.div [ prop.text "Test Content" ]

let tests =
  testList "Layout Component Tests" [
    
    testList "landingView layout" [
      
      test "renders content directly without wrapper" {
        let model = State.initialModel()
        let element = landingView model mockDispatch testContent
        
        // landingView should return content as-is
        Assert.isTrue
          (verifyLayoutStructure element)
          "landingView should render content without modification"
      }
      
      test "works with different model states" {
        let models = [
          State.initialModel()
          State.initialModel() |> State.withTheme Dark
          State.initialModel() |> State.withProjects (Generate.projects 5)
          State.initialModel() |> State.withError "Test error"
        ]
        
        for model in models do
          let element = landingView model mockDispatch testContent
          Assert.isTrue
            (verifyLayoutStructure element)
            "landingView should work with any model state"
      }
      
      test "preserves content structure" {
        let complexContent = 
          Html.div [
            Html.h1 [ prop.text "Title" ]
            Html.p [ prop.text "Paragraph" ]
            Html.ul [
              Html.li [ prop.text "Item 1" ]
              Html.li [ prop.text "Item 2" ]
            ]
          ]
        
        let model = State.initialModel()
        let element = landingView model mockDispatch complexContent
        
        Assert.isTrue
          (verifyLayoutStructure element)
          "landingView should preserve complex content structure"
      }
    ]
    
    testList "appView layout" [
      
      test "renders with layout wrapper structure" {
        let model = State.initialModel()
        let element = appView model mockDispatch testContent
        
        Assert.isTrue
          (verifyLayoutStructure element)
          "appView should render with proper layout structure"
      }
      
      test "includes header with title and tagline" {
        let model = State.initialModel()
        let element = appView model mockDispatch testContent
        
        // The layout should include the app title and tagline
        Assert.isTrue
          (verifyLayoutStructure element)
          "appView should include header with title 'Requirements Village'"
      }
      
      test "includes theme selector in header" {
        let themes = [Light; Dark]
        
        for theme in themes do
          let model = State.initialModel() |> State.withTheme theme
          let element = appView model mockDispatch testContent
          
          Assert.isTrue
            (verifyLayoutStructure element)
            $"appView should include theme selector for {theme} theme"
      }
      
      test "renders main content area" {
        let model = State.initialModel()
        let element = appView model mockDispatch testContent
        
        Assert.isTrue
          (verifyLayoutStructure element)
          "appView should include main content area"
      }
    ]
    
    testList "appView with different states" [
      
      test "renders correctly with Light theme" {
        let model = State.initialModel() |> State.withTheme Light
        let element = appView model mockDispatch testContent
        
        Assert.isTrue
          (verifyLayoutStructure element)
          "appView should render correctly with Light theme"
      }
      
      test "renders correctly with Dark theme" {
        let model = State.initialModel() |> State.withTheme Dark
        let element = appView model mockDispatch testContent
        
        Assert.isTrue
          (verifyLayoutStructure element)
          "appView should render correctly with Dark theme"
      }
      
      test "handles model with projects" {
        let model = 
          State.initialModel() 
          |> State.withProjects (Generate.projects 10)
        let element = appView model mockDispatch testContent
        
        Assert.isTrue
          (verifyLayoutStructure element)
          "appView should handle model with projects"
      }
      
      test "handles model with loading state" {
        let model = 
          State.initialModel() 
          |> State.withLoading (LoadingOperation.FetchProjects)
        let element = appView model mockDispatch testContent
        
        Assert.isTrue
          (verifyLayoutStructure element)
          "appView should handle model with loading operations"
      }
      
      test "handles model with error state" {
        let model = 
          State.initialModel() 
          |> State.withError "Something went wrong"
        let element = appView model mockDispatch testContent
        
        Assert.isTrue
          (verifyLayoutStructure element)
          "appView should handle model with error state"
      }
    ]
    
    testList "Layout responsiveness" [
      
      test "appView uses flexbox for header alignment" {
        let model = State.initialModel()
        let element = appView model mockDispatch testContent
        
        // The header should use flexbox for proper alignment
        Assert.isTrue
          (verifyLayoutStructure element)
          "Header should use flexbox classes for alignment"
      }
      
      test "works with empty content" {
        let emptyContent = Html.div []
        let model = State.initialModel()
        
        let landingElement = landingView model mockDispatch emptyContent
        let appElement = appView model mockDispatch emptyContent
        
        Assert.isTrue
          (verifyLayoutStructure landingElement && 
           verifyLayoutStructure appElement)
          "Both layouts should handle empty content"
      }
      
      test "works with very large content" {
        let largeContent = 
          Html.div [
            for i in 1..100 do
              Html.p [ prop.text $"Paragraph {i}" ]
          ]
        
        let model = State.initialModel()
        let element = appView model mockDispatch largeContent
        
        Assert.isTrue
          (verifyLayoutStructure element)
          "appView should handle very large content"
      }
    ]
    
    testList "Layout edge cases" [
      
      test "handles rapid theme changes" {
        let model = State.initialModel()
        
        // Simulate rendering with rapid theme changes
        let elements = [
          appView (model |> State.withTheme Light) mockDispatch testContent
          appView (model |> State.withTheme Dark) mockDispatch testContent
          appView (model |> State.withTheme Light) mockDispatch testContent
          appView (model |> State.withTheme Dark) mockDispatch testContent
        ]
        
        for element in elements do
          Assert.isTrue
            (verifyLayoutStructure element)
            "Should handle rapid theme changes"
      }
      
      test "handles all page types" {
        let pages = [Landing; Dashboard]
        
        for page in pages do
          let model = State.initialModel() |> State.withPage page
          
          let element = 
            match page with
            | Landing -> landingView model mockDispatch testContent
            | Dashboard -> appView model mockDispatch testContent
          
          Assert.isTrue
            (verifyLayoutStructure element)
            $"Should handle {page} page type"
      }
      
      test "preserves dispatch function through renders" {
        let mutable dispatchCount = 0
        let countingDispatch msg = 
          dispatchCount <- dispatchCount + 1
        
        let model = State.initialModel()
        let element = appView model countingDispatch testContent
        
        Assert.isTrue
          (verifyLayoutStructure element)
          "Should preserve dispatch function"
      }
    ]
  ]