module RequirementsVillage.Client.Components.ThemeSelector

open Feliz
open Feliz.Bulma
open RequirementsVillage.Client.Models.Domain
open RequirementsVillage.Client.ElmishApp.Types
open Browser.Dom

// Theme constants
module private Theme =
  let Light = "light"
  let Dark = "dark"
  let StorageKey = "requirements-village-theme"

let private themeToValue = function
  | Light -> Theme.Light
  | Dark  -> Theme.Dark

let private handleThemeClick theme dispatch =
  dispatch (SetTheme theme)

  let themeValue = themeToValue theme

  window.localStorage.setItem(Theme.StorageKey, themeValue)

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
