module RequirementsVillage.Client.Components.Layout

open Feliz
open Feliz.Bulma
open RequirementsVillage.Client.Types
open RequirementsVillage.Client.Components

let view (model: Model) (dispatch: Msg -> unit) 
  (content: ReactElement) =
  Html.div [
    prop.className "is-flex"
    prop.style [
      style.height (length.vh 100)
      style.backgroundColor "#f5f5f5"
    ]
    prop.children [
      Html.div [
        prop.className 
          "is-flex-grow-1 is-flex is-flex-direction-column"
        prop.style [ style.overflow.hidden ]
        prop.children [
          Html.header [
            prop.style [
              style.backgroundColor "white"
              style.borderBottom (1, borderStyle.solid, "#e5e5e5")
              style.padding (length.rem 1.5)
            ]
            prop.children [
              Html.div [
                prop.className 
                  "is-flex is-justify-content-space-between is-align-items-center"
                prop.children [
                  Html.div [
                    Html.h1 [
                      prop.className "title is-4"
                      prop.style [ 
                        style.marginBottom (length.rem 0.2)
                        style.lineHeight 1.2
                      ]
                      prop.text "Requirements Village"
                    ]
                    Html.p [
                      prop.className "subtitle is-6 has-text-grey"
                      prop.style [ 
                        style.marginBottom 0
                        style.marginTop 0
                      ]
                      prop.text "Where project ideas get laid to rest"
                    ]
                  ]
                  ThemeSelector.view model.CurrentTheme dispatch
                ]
              ]
            ]
          ]
          Html.main [
            prop.className "is-flex-grow-1"
            prop.style [
              style.overflow.auto
              style.padding (length.rem 1.5)
            ]
            prop.children [ content ]
          ]
        ]
      ]
    ]
  ]