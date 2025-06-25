namespace RequirementsVillage.FSharp.Api.Services

open System
open RequirementsVillage.FSharp.Api.Models
open RequirementsVillage.FSharp.Api.Persistence

// Service interface for dependency injection
type IProjectService =
  abstract member GetAllProjectsAsync:
    unit -> Async<Result<Project list, ProjectError>>
  abstract member GetProjectByIdAsync:
    Guid -> Async<Result<Project option, ProjectError>>
  abstract member CreateProjectAsync:
    name: string * description: string * category: ProjectCategory
    -> Async<Result<Project, ProjectError>>
  abstract member UpdateProjectAsync:
    Project -> Async<Result<unit, ProjectError>>
  abstract member UpdateProjectStatusAsync:
    id: Guid * status: ProjectStatus
    -> Async<Result<unit, ProjectError>>
  abstract member DeleteProjectAsync:
    Guid -> Async<Result<unit, ProjectError>>

// Concrete service implementation with business logic
type ProjectService(repository: IProjectRepository) =

  // Validation helpers
  let validateProjectName (name: string) =
    if String.IsNullOrWhiteSpace(name) then
      Error (
        ValidationFailed(
          "name", "Project name cannot be empty", name
        )
      )
    elif name.Length > 100 then
      Error (
        ValidationFailed(
          "name",
          "Project name cannot exceed 100 characters",
          name
        )
      )
    else
      Ok name

  let validateProjectDescription (description: string) =
    if String.IsNullOrWhiteSpace(description) then
      Error (
        ValidationFailed(
          "description",
          "Project description cannot be empty",
          description
        )
      )
    elif description.Length > 1000 then
      Error (
        ValidationFailed(
          "description",
          "Project description cannot exceed 1000 characters",
          description
        )
      )
    else
      Ok description

  // Business rule: Cannot transition directly from Idea to Completed
  let validateStatusTransition
    (currentStatus: ProjectStatus)
    (newStatus: ProjectStatus) =
    match currentStatus, newStatus with
    | Idea, Completed ->
      Error (
        ValidationFailed(
          "status",
          "Cannot transition directly from Idea to Completed",
          newStatus
        )
      )
    | _ ->
      Ok newStatus

  interface IProjectService with
    member _.GetAllProjectsAsync() =
      repository.GetAllAsync()

    member _.GetProjectByIdAsync(id: Guid) =
      repository.GetByIdAsync(id)

    member _.CreateProjectAsync(
      name:        string,
      description: string,
      category:    ProjectCategory
    ) =
      async {
        // Validate inputs
        match
          validateProjectName name,
          validateProjectDescription description
        with
        | Ok validName, Ok validDescription ->
          let newProject = {
            Id          = Guid.NewGuid()
            Name        = validName
            Description = validDescription
            Category    = category
            Status      = Idea // All projects start as ideas
            CreatedAt   = DateTime.UtcNow
            UpdatedAt   = DateTime.UtcNow
          }

          match! repository.CreateAsync(newProject) with
          | Ok ()   -> return Ok newProject
          | Error e -> return Error e

        | Error e, _ -> return Error e
        | _, Error e -> return Error e
      }

    member _.UpdateProjectAsync(project: Project) =
      async {
        // Validate the updated fields
        match validateProjectName project.Name,
              validateProjectDescription project.Description with
        | Ok _, Ok _ ->
          // Check if project exists
          match! repository.GetByIdAsync(project.Id) with
          | Ok (Some existingProject) ->
            // Validate status transition if status changed
            if existingProject.Status <> project.Status then
              match validateStatusTransition
                      existingProject.Status
                      project.Status with
              | Ok _ ->
                let updatedProject =
                  { project with UpdatedAt = DateTime.UtcNow }
                return! repository.UpdateAsync(updatedProject)
              | Error e -> return Error e
            else
              let updatedProject =
                { project with UpdatedAt = DateTime.UtcNow }
              return! repository.UpdateAsync(updatedProject)
          | Ok None ->
            return Error (NotFound(project.Id, "UpdateProject"))
          | Error e ->
            return Error e

        | Error e, _ -> return Error e
        | _, Error e -> return Error e
      }

    member _.UpdateProjectStatusAsync(
      id: Guid,
      status: ProjectStatus
    ) =
      async {
        match! repository.GetByIdAsync(id) with
        | Ok (Some project) ->
          match validateStatusTransition project.Status status with
          | Ok validStatus ->
            let updatedProject =
              { project with
                  Status = validStatus
                  UpdatedAt = DateTime.UtcNow }
            return! repository.UpdateAsync(updatedProject)
          | Error e -> return Error e
        | Ok None ->
          return Error (NotFound(id, "UpdateProjectStatus"))
        | Error e ->
          return Error e
      }

    member _.DeleteProjectAsync(id: Guid) =
      async {
        // Business rule: Can only delete projects in Abandoned status
        match! repository.GetByIdAsync(id) with
        | Ok (Some project) ->
          if project.Status = Abandoned then
            return! repository.DeleteAsync(id)
          else
            return Error (
              ValidationFailed(
                "status",
                "Can only delete projects in Abandoned status",
                project.Status
              )
            )
        | Ok None ->
          return Error (NotFound(id, "DeleteProject"))
        | Error e ->
          return Error e
      }
