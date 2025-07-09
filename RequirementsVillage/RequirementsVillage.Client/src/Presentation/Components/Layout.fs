module RequirementsVillage.Client.Presentation.Components.Layout

open Feliz
open Feliz.Bulma
open RequirementsVillage.Client.Presentation.State.Types
open RequirementsVillage.Client.Presentation.Components

// Landing page layout - full screen, no header
let landingView (model: Model) (dispatch: Msg -> unit) (content: ReactElement) =
  content

// App layout - with header and navigation
let appView (model: Model) (dispatch: Msg -> unit) (content: ReactElement) =
  Html.div [
    prop.className "layout-container"
    prop.children [
      Html.div [
        prop.className "layout-content"
        prop.children [
          Html.header [
            prop.className "layout-header"
            prop.children [
              Html.div [
                prop.className 
                  "is-flex is-justify-content-space-between is-align-items-center"
                prop.children [
                  Html.div [
                    Html.h1 [
                      prop.className "title is-4"
                      prop.className "title is-4 mb-xs line-height-tight"
                      prop.text "Requirements Village"
                    ]
                    Html.p [
                      prop.className "subtitle is-6 has-text-grey"
                      prop.text "Where project ideas get laid to rest"
                    ]
                  ]
                  ThemeSelector.view model.UI.CurrentTheme dispatch
                ]
              ]
            ]
          ]
          Html.main [
            prop.className "layout-main"
            prop.children [ content ]
          ]
        ]
      ]
    ]
  ]