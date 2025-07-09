module RequirementsVillage.Client.Presentation.Components.ThemeSelector

open Feliz
open Feliz.Bulma
open RequirementsVillage.Client.Infrastructure.Storage.ThemeStorage
open RequirementsVillage.Client.Presentation.State.Types

let private handleThemeClick theme dispatch =
  dispatch (SetTheme theme)
  
  Theme.save theme
  Theme.applyToDom theme

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
