module RequirementsVillage.Client.Components.Common

open Feliz
open Feliz.Bulma
open RequirementsVillage.Client.Types

// Private helper functions
let private statusToColor = function
  | Idea       -> color.isInfo
  | InProgress -> color.isSuccess
  | Completed  -> color.isPrimary
  | Abandoned  -> color.isDark
  | OnHold     -> color.isWarning

// Public reusable components
let StatusBadge status =
  Bulma.tag [
    statusToColor status
    prop.text (ProjectStatus.toDisplayText status)
  ]

let CategoryBadge category =
  Bulma.tag [
    color.isLight
    prop.text (ProjectCategory.toDisplayText category)
  ]

let TagContainer children =
  Html.div [
    prop.style [
      style.display.flex
      style.gap (length.rem 0.5)
      style.flexWrap.wrap
    ]
    prop.children (children : Fable.React.ReactElement seq)
  ]