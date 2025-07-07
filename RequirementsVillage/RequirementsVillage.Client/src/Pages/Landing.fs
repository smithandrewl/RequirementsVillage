module RequirementsVillage.Client.Pages.Landing

open Feliz
open Feliz.Bulma
open RequirementsVillage.Client.Models.Domain
open RequirementsVillage.Client.ElmishApp.Types

let view (dispatch: Msg -> unit) =
  Bulma.hero [
    hero.isFullHeight
    prop.style [
      style.backgroundImage    "url('/landing-page-splash.png')"
      style.backgroundSize     "cover"
      style.backgroundPosition "center top"
      style.backgroundRepeat.noRepeat
    ]
    prop.children [
      Bulma.heroBody [
        Bulma.container [
          prop.className "has-text-centered"
          prop.children [
            Html.h1 [
              prop.className "title title-font"
              prop.style [
                style.fontSize (length.rem 6)
                style.color "#e4dcba"
                style.custom(
                  "textShadow", "2px 2px 4px rgba(0, 0, 0, 0.8)"
                )
                style.letterSpacing (length.px 2)
              ]
              prop.children [
                Html.text "REQUIREMENTS"
                Html.br []
                Html.text "VILLAGE"
              ]
            ]
            Html.p [
              prop.className "subtitle body-font"
              prop.style [
                style.fontSize (length.rem 1.5)
                style.color "#e4dcba"
                style.custom(
                  "textShadow", "2px 2px 4px rgba(0, 0, 0, 0.8)"
                )
                style.marginTop (length.rem 2)
              ]
              prop.text "Where project ideas get laid to rest"
            ]
            Html.div [
              prop.style [ style.marginTop (length.rem 4) ]
              prop.children [
                Bulma.button.a [
                  color.isPrimary
                  button.isLarge
                  prop.onClick (fun _ ->
                    dispatch (NavigateTo Dashboard)
                  )
                  prop.style [
                    style.paddingLeft   (length.rem 3)
                    style.paddingRight  (length.rem 3)
                    style.letterSpacing (length.px  1)
                  ]
                  prop.text "GET STARTED"
                ]
              ]
            ]
            Html.div [
              prop.style [
                style.marginTop (length.rem 6)
                style.opacity 0.6
              ]
              prop.children [
                Html.div [
                  prop.style [
                    style.display.flex
                    style.alignItems.center
                    style.justifyContent.center
                    style.gap (length.rem 1)
                  ]
                  prop.children [
                    Html.hr [
                      prop.style [
                        style.width (length.rem 4)
                        style.backgroundColor "#808080"
                        style.height (length.px 1)
                        style.border (
                          0, borderStyle.none, "transparent"
                        )
                      ]
                    ]
                    Html.span [
                      prop.style [ style.color "#808080" ]
                      prop.text "⚰️"
                    ]
                    Html.hr [
                      prop.style [
                        style.width (length.rem 4)
                        style.backgroundColor "#808080"
                        style.height (length.px 1)
                        style.border (
                          0, borderStyle.none, "transparent"
                        )
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
  ]
