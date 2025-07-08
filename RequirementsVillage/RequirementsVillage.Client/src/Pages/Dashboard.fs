module RequirementsVillage.Client.Pages.Dashboard

open Feliz
open Feliz.Bulma
open RequirementsVillage.Client.Domain
open RequirementsVillage.Client.Elm.Types
open RequirementsVillage.Client.Components

// Private components
let private StatusFilter (currentFilter: ProjectStatus option) (dispatch: Msg -> unit) =
  Html.div [
    prop.className
      "is-flex is-flex-wrap-wrap is-align-items-center is-justify-content-space-between"
    prop.className "dashboard-filter"
    prop.children [
      Bulma.select [
        prop.onChange (fun (e: Browser.Types.Event) ->
          let value =
            (e.target :?> Browser.Types.HTMLSelectElement).value
          let filter =
            match value with
            | "All"        -> None
            | "Idea"       -> Some Idea
            | "InProgress" -> Some InProgress
            | "Completed"  -> Some Completed
            | "Abandoned"  -> Some Abandoned
            | "OnHold"     -> Some OnHold
            | _            -> None
          dispatch (FilterByStatus filter)
        )
        prop.children [
          Html.option [ prop.value "All"; prop.text "All" ]
          Html.option [ prop.value "Idea"; prop.text "Ideas" ]
          Html.option [
            prop.value "InProgress"; prop.text "In Progress"
          ]
          Html.option [
            prop.value "Completed"; prop.text "Completed"
          ]
          Html.option [
            prop.value "Abandoned"; prop.text "Abandoned"
          ]
          Html.option [ prop.value "OnHold"; prop.text "On Hold" ]
        ]
      ]
    ]
  ]

let private EmptyState (dispatch: Msg -> unit) =
  Html.div [
    prop.className "has-text-centered dashboard-empty"
    prop.children [
      Html.div [
        prop.className "dashboard-empty-icon-container"
        prop.children [
          Html.i [
            prop.className "fas fa-folder-open dashboard-empty-icon"
          ]
        ]
      ]
      Html.h3 [
        prop.className "title is-5"
        prop.text "No projects"
      ]
      Html.p [
        prop.className "has-text-grey dashboard-empty-text"
        prop.text "Get started by creating your first project idea."
      ]
      Bulma.button.a [
        color.isPrimary
        prop.text "Add Project"
      ]
    ]
  ]

let private ProjectGrid (projects: Project list) =
  Bulma.columns [
    columns.isMultiline
    prop.children [
      for project in projects do
        Bulma.column [
          column.isOneThirdDesktop
          column.isHalfTablet
          prop.children [ ProjectCard.view project ]
        ]
    ]
  ]

let view (model: Model) (dispatch: Msg -> unit) =
  let filteredProjects =
    match model.FilteredStatus with
    | None        -> model.Projects
    | Some status -> model.Projects |> List.filter (fun p -> p.Status = status)

  Html.div [
    StatusFilter model.FilteredStatus dispatch

    if model.IsLoading then
      Bulma.progress [
        progress.isSmall
        color.isPrimary
      ]
    elif List.isEmpty filteredProjects then
      EmptyState dispatch
    else
      ProjectGrid filteredProjects
  ]
