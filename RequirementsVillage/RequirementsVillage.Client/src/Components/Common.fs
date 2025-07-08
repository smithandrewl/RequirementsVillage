module RequirementsVillage.Client.Components.Common

open Feliz
open Feliz.Bulma
open RequirementsVillage.Client.Domain

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
    prop.className "tag-container"
    prop.children (children : Fable.React.ReactElement seq)
  ]