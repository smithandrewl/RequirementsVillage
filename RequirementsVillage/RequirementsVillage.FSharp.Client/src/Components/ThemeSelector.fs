module RequirementsVillage.FSharp.Client.Components.ThemeSelector

open Feliz
open Feliz.Bulma
open RequirementsVillage.FSharp.Client.Types
open Browser.Dom

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
        Bulma.dropdownContent [
          for (theme, label) in themes do
            Bulma.dropdownItem.a [
              if theme = currentTheme then
                prop.className "is-active"
              prop.onClick (fun _ -> 
                dispatch (SetTheme theme)
                let themeValue = 
                  match theme with
                  | Light -> "requirements-village"
                  | Dark -> "requirements-village-dark"
                window.localStorage.setItem("theme", themeValue)
                document.documentElement.setAttribute(
                  "data-theme", themeValue
                )
              )
              prop.text label
            ]
        ]
      ]
    ]
  ]