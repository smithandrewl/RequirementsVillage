module RequirementsVillage.Client.Configuration.Constants

// API Configuration  
module Api =
  let BaseUrl      = "/api"
  let ProjectsPath = "/projects"
  let HealthPath   = "/health"
  
  // Helper to construct full API URLs
  let url path = BaseUrl + path

// UI Configuration
module UI =
  let TransitionDuration = 200.0 // milliseconds