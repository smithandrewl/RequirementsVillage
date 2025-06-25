module RequirementsVillage.FSharp.Client.Pages.Dashboard

open Feliz
open Feliz.Bulma
open RequirementsVillage.FSharp.Client.Types
open RequirementsVillage.FSharp.Client.Components

let view (model: Model) (dispatch: Msg -> unit) =
  let filteredProjects =
    match model.FilteredStatus with
    | None -> model.Projects
    | Some status -> 
      model.Projects |> List.filter (fun p -> p.Status = status)
  
  Html.div [
    Html.div [
      prop.className 
        "is-flex is-flex-wrap-wrap is-align-items-center is-justify-content-space-between"
      prop.style [ style.marginBottom (length.rem 1.5) ]
      prop.children [
        Bulma.select [
          prop.onChange (fun (e: Browser.Types.Event) ->
            let value = 
              (e.target :?> Browser.Types.HTMLSelectElement).value
            let filter = 
              match value with
              | "All" -> None
              | "Idea" -> Some Idea
              | "InProgress" -> Some InProgress
              | "Completed" -> Some Completed
              | "Abandoned" -> Some Abandoned
              | "OnHold" -> Some OnHold
              | _ -> None
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
    
    if model.IsLoading then
      Bulma.progress [
        progress.isSmall
        color.isPrimary
      ]
    elif List.isEmpty filteredProjects then
      Html.div [
        prop.className "has-text-centered"
        prop.style [ style.paddingTop (length.rem 3) ]
        prop.children [
          Html.div [
            prop.style [
              style.color "#808080"
              style.marginBottom (length.rem 1)
            ]
            prop.children [
              Html.i [
                prop.className "fas fa-folder-open"
                prop.style [ style.fontSize (length.rem 4) ]
              ]
            ]
          ]
          Html.h3 [
            prop.className "title is-5"
            prop.text "No projects"
          ]
          Html.p [
            prop.className "has-text-grey"
            prop.style [ style.marginBottom (length.rem 1) ]
            prop.text 
              "Get started by creating your first project idea."
          ]
          Bulma.button.a [
            color.isPrimary
            prop.text "Add Project"
          ]
        ]
      ]
    else
      Bulma.columns [
        columns.isMultiline
        prop.children [
          for project in filteredProjects do
            Bulma.column [
              column.isOneThirdDesktop
              column.isHalfTablet
              prop.children [ ProjectCard.view project ]
            ]
        ]
      ]
  ]