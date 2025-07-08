module RequirementsVillage.Client.Components.ProjectCard

open Feliz
open Feliz.Bulma
open RequirementsVillage.Client.Models.Domain

// Private helper functions
let private statusToColor = function
  | Idea       -> color.isInfo
  | InProgress -> color.isSuccess
  | Completed  -> color.isPrimary
  | Abandoned  -> color.isDark
  | OnHold     -> color.isWarning

// Public reusable components
let StatusBadge status =
  Bulma.tag [
    statusToColor status
    prop.text (ProjectStatus.toDisplayText status)
  ]

let CategoryBadge category =
  Bulma.tag [
    color.isLight
    prop.text (ProjectCategory.toDisplayText category)
  ]

let TagContainer (children: ReactElement list) =
  Html.div [
    prop.style [
      style.display.flex
      style.gap (length.rem 0.5)
      style.flexWrap.wrap
    ]
    prop.children children
  ]

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
