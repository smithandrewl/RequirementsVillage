module RequirementsVillage.Client.Tests.Main

open Fable.Mocha

// Import all test modules
open RequirementsVillage.Client.Tests.State.TypesTests
open RequirementsVillage.Client.Tests.State.UpdateTests
open RequirementsVillage.Client.Tests.State.StatePropertyTests
open RequirementsVillage.Client.Tests.State.CommandTests
open RequirementsVillage.Client.Tests.Components.ProjectCardTests
open RequirementsVillage.Client.Tests.Components.ThemeSelectorTests
open RequirementsVillage.Client.Tests.Components.CommonTests
open RequirementsVillage.Client.Tests.Components.LayoutTests
open RequirementsVillage.Client.Tests.Api.ProjectApiTests
open RequirementsVillage.Client.Tests.Api.CodecsTests

// Combine all tests
let allTests =
  testList "Requirements Village Client Tests" [
    RequirementsVillage.Client.Tests.State.TypesTests.tests
    RequirementsVillage.Client.Tests.State.UpdateTests.tests
    RequirementsVillage.Client.Tests.State.StatePropertyTests.tests
    RequirementsVillage.Client.Tests.State.CommandTests.tests
    RequirementsVillage.Client.Tests.Components.ProjectCardTests.tests
    RequirementsVillage.Client.Tests.Components.ThemeSelectorTests.tests
    RequirementsVillage.Client.Tests.Components.CommonTests.tests
    RequirementsVillage.Client.Tests.Components.LayoutTests.tests
    RequirementsVillage.Client.Tests.Api.ProjectApiTests.tests
    RequirementsVillage.Client.Tests.Api.CodecsTests.tests
  ]

// Run tests
[<EntryPoint>]
let main args =
  Mocha.runTests allTests