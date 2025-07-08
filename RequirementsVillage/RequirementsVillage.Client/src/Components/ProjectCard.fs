module RequirementsVillage.Client.Components.ProjectCard

open Feliz
open Feliz.Bulma
open RequirementsVillage.Client.Types
open RequirementsVillage.Client.Components.Common

// Private card-specific components
let private ProjectHeader name =
  Html.div [
    prop.className "project-header"
    prop.children [
      Html.h3 [
        prop.className "title is-5 project-title"
        prop.text (name : string)
      ]
    ]
  ]

let private ProjectDescription description =
  Html.p [
    prop.className "has-text-grey project-description"
    prop.text (description : string)
  ]

// Main component
let view (project: Project) =
  Bulma.card [
    prop.className "project-card"
    prop.onMouseEnter (fun _ ->
      // Would handle hover state in real app
      ()
    )
    prop.children [
      Bulma.cardContent [
        ProjectHeader project.Name
        ProjectDescription project.Description
        TagContainer [
          StatusBadge project.Status
          CategoryBadge project.Category
        ]
      ]
    ]
  ]
