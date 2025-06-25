module RequirementsVillage.FSharp.Client.Components.ProjectCard

open Feliz
open Feliz.Bulma
open RequirementsVillage.FSharp.Client.Types

let view (project: Project) =
    Bulma.card [
        prop.style [
            style.marginBottom (length.rem 1)
            style.border (1, borderStyle.solid, "#e5e5e5")
            style.transitionDuration (System.TimeSpan.FromMilliseconds 200.0)
        ]
        prop.onMouseEnter (fun _ -> 
            // Would handle hover state in real app
            ()
        )
        prop.children [
            Bulma.cardContent [
                Html.div [
                    prop.style [
                        style.display.flex
                        style.justifyContent.spaceBetween
                        style.alignItems.flexStart
                        style.marginBottom (length.rem 0.75)
                    ]
                    prop.children [
                        Html.h3 [
                            prop.className "title is-5"
                            prop.style [
                                style.marginBottom 0
                                style.fontWeight 600
                            ]
                            prop.text project.Title
                        ]
                    ]
                ]
                Html.p [
                    prop.className "has-text-grey"
                    prop.style [
                        style.fontSize (length.rem 0.875)
                        style.marginBottom (length.rem 1)
                        style.lineHeight 1.5
                    ]
                    prop.text project.Description
                ]
                Bulma.tag [
                    match project.Status with
                    | Someday -> color.isInfo
                    | Current -> color.isSuccess  
                    | Archive -> color.isLight
                    prop.text (
                        match project.Status with
                        | Someday -> "Someday"
                        | Current -> "Current"
                        | Archive -> "Archived"
                    )
                ]
            ]
        ]
    ]