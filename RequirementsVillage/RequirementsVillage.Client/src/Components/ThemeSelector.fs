module RequirementsVillage.Client.Components.ThemeSelector

open Feliz
open Feliz.Bulma
open RequirementsVillage.Client.Elm.Types
open Browser.Dom

let private themeToValue = function
  | Light -> "requirements-village"
  | Dark  -> "requirements-village-dark"

let private handleThemeClick theme dispatch =
  dispatch (SetTheme theme)

  let themeValue = themeToValue theme

  window.localStorage.setItem("theme", themeValue)

  document.documentElement.setAttribute("data-theme", themeValue)

let private createThemeItem currentTheme dispatch (theme, label: string) =
  Bulma.dropdownItem.a [
    if theme = currentTheme then
      prop.className "is-active"

    prop.onClick (fun _ -> handleThemeClick theme dispatch)
    prop.text label
  ]

let view (currentTheme: Theme) (dispatch: Msg -> unit) =
  let themes = [
    Light, "Light"
    Dark, "Dark"
  ]

  Bulma.dropdown [
    dropdown.isRight
    dropdown.isHoverable
    prop.children [
      Bulma.dropdownTrigger [
        Bulma.button.a [
          button.isSmall
          button.isText
          prop.children [
            Html.span [ prop.text "🎨 Theme" ]
          ]
        ]
      ]
      Bulma.dropdownMenu [
        Bulma.dropdownContent (
          themes |> List.map (createThemeItem currentTheme dispatch)
        )
      ]
    ]
  ]
