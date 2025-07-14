module RequirementsVillage.Client.Routes

open System

// Define the routes for our application
type Route =
  | Landing
  | Dashboard

// Convert Route to URL segments
let toUrlSegments = function
  | Landing -> []
  | Dashboard -> [ "dashboard" ]

// Parse URL segments to Route
let parseUrl = function
  | [] -> Landing
  | [ "dashboard" ] -> Dashboard
  | _ -> Landing // Default to landing for unknown routes