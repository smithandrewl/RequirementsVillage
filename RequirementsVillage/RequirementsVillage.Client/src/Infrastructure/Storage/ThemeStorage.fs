module RequirementsVillage.Client.Infrastructure.Storage.ThemeStorage

open Browser.Dom

// Theme storage constants
module private Constants =
  let StorageKey = "requirements-village-theme"
  let LightValue = "requirements-village"
  let DarkValue  = "requirements-village-dark"

// Theme type
type Theme =
  | Light
  | Dark

// Theme storage operations
module Theme =
  let toStorageValue = function
    | Light -> Constants.LightValue
    | Dark  -> Constants.DarkValue

  let fromStorageValue = function
    | value when value = Constants.DarkValue -> Dark
    | _                                      -> Light

  let save theme =
    let value = toStorageValue theme
    
    window.localStorage.setItem(Constants.StorageKey, value)

  let load () =
    let value = window.localStorage.getItem(Constants.StorageKey)
    
    match value with
    | null -> Light
    | v    -> fromStorageValue v

  let applyToDom theme =
    let value = toStorageValue theme
    
    document.documentElement.setAttribute("data-theme", value)