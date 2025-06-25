module RequirementsVillage.FSharp.Client.Components.ProjectCard

open Feliz
open Feliz.Bulma
open RequirementsVillage.FSharp.Client.Types

let view (project: Project) =
  Bulma.card [
    prop.style [
      style.marginBottom (length.rem 1)
      style.border (1, borderStyle.solid, "#e5e5e5")
      style.transitionDuration 
        (System.TimeSpan.FromMilliseconds 200.0)
    ]
    prop.onMouseEnter (fun _ -> 
      // Would handle hover state in real app
      ()
    )
    prop.children [
      Bulma.cardContent [
        Html.div [
          prop.style [
            style.display.flex
            style.justifyContent.spaceBetween
            style.alignItems.flexStart
            style.marginBottom (length.rem 0.75)
          ]
          prop.children [
            Html.h3 [
              prop.className "title is-5"
              prop.style [
                style.marginBottom 0
                style.fontWeight 600
              ]
              prop.text project.Name
            ]
          ]
        ]
        Html.p [
          prop.className "has-text-grey"
          prop.style [
            style.fontSize (length.rem 0.875)
            style.marginBottom (length.rem 1)
            style.lineHeight 1.5
          ]
          prop.text project.Description
        ]
        Html.div [
          prop.style [
            style.display.flex
            style.gap (length.rem 0.5)
            style.flexWrap.wrap
          ]
          prop.children [
            Bulma.tag [
              match project.Status with
              | Idea -> color.isInfo
              | InProgress -> color.isSuccess  
              | Completed -> color.isPrimary
              | Abandoned -> color.isDark
              | OnHold -> color.isWarning
              prop.text (
                match project.Status with
                | Idea -> "Idea"
                | InProgress -> "In Progress"
                | Completed -> "Completed"
                | Abandoned -> "Abandoned"
                | OnHold -> "On Hold"
              )
            ]
            Bulma.tag [
              color.isLight
              prop.text (
                match project.Category with
                | WebApp -> "Web App"
                | MobileApp -> "Mobile App"
                | Library -> "Library"
                | Tool -> "Tool"
                | Game -> "Game"
                | Other s -> s
              )
            ]
          ]
        ]
      ]
    ]
  ]