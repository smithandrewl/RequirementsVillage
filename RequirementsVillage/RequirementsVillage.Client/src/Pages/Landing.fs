module RequirementsVillage.Client.Pages.Landing

open Feliz
open Feliz.Bulma
open RequirementsVillage.Client.Elm.Types

let view (dispatch: Msg -> unit) =
  Bulma.hero [
    hero.isFullHeight
    prop.className "landing-hero"
    prop.children [
      Bulma.heroBody [
        Bulma.container [
          prop.className "has-text-centered"
          prop.children [
            Html.h1 [
              prop.className "title title-font text-hero"
              prop.children [
                Html.text "REQUIREMENTS"
                Html.br []
                Html.text "VILLAGE"
              ]
            ]
            Html.p [
              prop.className "subtitle body-font text-subtitle-large mt-lg"
              prop.text "Where project ideas get laid to rest"
            ]
            Html.div [
              prop.className "mt-xl"
              prop.children [
                Bulma.button.a [
                  color.isPrimary
                  button.isLarge
                  prop.onClick (fun _ ->
                    dispatch (NavigateTo Dashboard)
                  )
                  prop.className "landing-button"
                  prop.text "GET STARTED"
                ]
              ]
            ]
            Html.div [
              prop.className "landing-decorative"
              prop.children [
                Html.div [
                  prop.className "landing-decorative-flex"
                  prop.children [
                    Html.hr [
                      prop.className "landing-decorative-hr"
                    ]
                    Html.span [
                      prop.className "landing-decorative-text"
                      prop.text "⚰️"
                    ]
                    Html.hr [
                      prop.className "landing-decorative-hr"
                    ]
                  ]
                ]
              ]
            ]
          ]
        ]
      ]
    ]
  ]
