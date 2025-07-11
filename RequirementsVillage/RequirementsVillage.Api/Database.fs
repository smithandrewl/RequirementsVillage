namespace RequirementsVillage.Api.Persistence

open System
open System.Data
open Dapper
open Microsoft.Data.Sqlite
open RequirementsVillage.Shared

// Repository interface for dependency injection
type IProjectRepository =
  abstract member GetAllAsync:
    unit -> Async<Result<Project list, ProjectError>>
  abstract member GetByIdAsync:
    Guid -> Async<Result<Project option, ProjectError>>
  abstract member CreateAsync:
    Project -> Async<Result<unit, ProjectError>>
  abstract member UpdateAsync:
    Project -> Async<Result<unit, ProjectError>>
  abstract member DeleteAsync:
    Guid -> Async<Result<unit, ProjectError>>

// Dapper type handlers for F# discriminated unions
module DapperTypeHandlers =
  type ProjectStatusHandler() =
    inherit SqlMapper.TypeHandler<ProjectStatus>()

    override _.SetValue(param, value) =
      param.Value <- ProjectStatus.toString value

    override _.Parse(value) =
      match ProjectStatus.fromString (value :?> string) with
      | Ok status -> status
      | Error msg -> failwith msg

  type ProjectCategoryHandler() =
    inherit SqlMapper.TypeHandler<ProjectCategory>()

    override _.SetValue(param, value) =
      param.Value <- ProjectCategory.toString value

    override _.Parse(value) =
      value :?> string |> ProjectCategory.fromString

  let registerHandlers() =
    SqlMapper.AddTypeHandler(ProjectStatusHandler())
    SqlMapper.AddTypeHandler(ProjectCategoryHandler())

// SQL queries as raw strings
module Queries =
  let selectAll = """
    SELECT
      Id,
      Name,
      Description,
      Category,
      Status,
      CreatedAt,
      UpdatedAt
    FROM
      Projects
    ORDER BY
      UpdatedAt DESC
  """

  let selectById = """
    SELECT
      Id,
      Name,
      Description,
      Category,
      Status,
      CreatedAt,
      UpdatedAt
    FROM
      Projects
    WHERE
      Id = @Id
  """

  let insert = """
    INSERT INTO Projects (
      Id,
      Name,
      Description,
      Category,
      Status,
      CreatedAt,
      UpdatedAt
    )
    VALUES (
      @Id,
      @Name,
      @Description,
      @Category,
      @Status,
      @CreatedAt,
      @UpdatedAt
    )
  """

  let update = """
    UPDATE
      Projects
    SET
      Name        = @Name,
      Description = @Description,
      Category    = @Category,
      Status      = @Status,
      UpdatedAt   = @UpdatedAt
    WHERE
      Id = @Id
  """

  let delete = """
    DELETE FROM Projects
    WHERE Id = @Id
  """

  let createTable = """
    CREATE TABLE IF NOT EXISTS Projects (
      Id          TEXT PRIMARY KEY,
      Name        TEXT NOT NULL,
      Description TEXT NOT NULL,
      Category    TEXT NOT NULL,
      Status      TEXT NOT NULL,
      CreatedAt   TEXT NOT NULL,
      UpdatedAt   TEXT NOT NULL
    )
  """

type ProjectRepository(connectionString: string) =
  do DapperTypeHandlers.registerHandlers()

  let createConnection() = new SqliteConnection(connectionString)

  let executeAsync (operation: string)
    (action: IDbConnection -> Async<'T>)
    : Async<Result<'T, ProjectError>> =
    async {
      try
        use conn = createConnection()
        let! result = action conn
        return Ok result
      with
      | :? SqliteException as ex ->
        return Error (DatabaseError(operation, "Projects", ex))
      | ex ->
        return Error (UnknownError ex.Message)
    }

  // Initialize database
  member _.InitializeAsync() =
    executeAsync "CREATE_TABLE" (fun conn ->
      async {
        conn.Open()
        let! _ =
          conn.ExecuteAsync(Queries.createTable)
          |> Async.AwaitTask
        return ()
      })

  interface IProjectRepository with
    member _.GetAllAsync() =
      executeAsync "SELECT_ALL" (fun conn ->
        async {
          conn.Open()
          let! results =
            conn.QueryAsync<Project>(Queries.selectAll)
            |> Async.AwaitTask
          return results |> Seq.toList
        })

    member _.GetByIdAsync(id: Guid) =
      executeAsync "SELECT_BY_ID" (fun conn ->
        async {
          conn.Open()
          let parameters = {| Id = id.ToString() |}
          let! result =
            conn.QuerySingleOrDefaultAsync<Project>(
              Queries.selectById, parameters
            ) |> Async.AwaitTask
          return
            if isNull (box result) then None
            else Some result
        })

    member _.CreateAsync(project: Project) =
      executeAsync "INSERT" (fun conn ->
        async {
          conn.Open()
          let parameters = {|
            Id = project.Id.ToString()
            Name = project.Name
            Description = project.Description
            Category = project.Category
            Status = project.Status
            CreatedAt = project.CreatedAt
            UpdatedAt = project.UpdatedAt
          |}
          let! _ =
            conn.ExecuteAsync(Queries.insert, parameters)
            |> Async.AwaitTask
          return ()
        })

    member _.UpdateAsync(project: Project) =
      executeAsync "UPDATE" (fun conn ->
        async {
          conn.Open()
          let parameters = {|
            Id = project.Id.ToString()
            Name = project.Name
            Description = project.Description
            Category = project.Category
            Status = project.Status
            UpdatedAt = project.UpdatedAt
          |}
          let! rowsAffected =
            conn.ExecuteAsync(Queries.update, parameters)
            |> Async.AwaitTask
          if rowsAffected = 0 then
            return! async {
              return raise (Exception("Project not found"))
            }
          else
            return ()
        })

    member _.DeleteAsync(id: Guid) =
      executeAsync "DELETE" (fun conn ->
        async {
          conn.Open()
          let parameters = {| Id = id.ToString() |}
          let! _ =
            conn.ExecuteAsync(Queries.delete, parameters)
            |> Async.AwaitTask
          return ()
        })

// In-memory repository for development/testing
type InMemoryProjectRepository() =
  let mutable projects = [
    { Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440000")
      Name = "Task Manager"
      Description = "A web-based task tracking application"
      Category = WebApp
      Status = Idea
      CreatedAt = DateTime.UtcNow.AddDays(-30.0)
      UpdatedAt = DateTime.UtcNow.AddDays(-2.0) }

    { Id = Guid.Parse("6ba7b810-9dad-11d1-80b4-00c04fd430c8")
      Name = "Expense Tracker"
      Description =
        "An application for monitoring personal expenses"
      Category = WebApp
      Status = InProgress
      CreatedAt = DateTime.UtcNow.AddDays(-15.0)
      UpdatedAt = DateTime.UtcNow.AddDays(-1.0) }

    { Id = Guid.Parse("7ba7b810-9dad-11d1-80b4-00c04fd430c8")
      Name = "Weather App"
      Description = "A weather forecasting application"
      Category = MobileApp
      Status = Idea
      CreatedAt = DateTime.UtcNow.AddDays(-60.0)
      UpdatedAt = DateTime.UtcNow.AddDays(-45.0) }

    { Id = Guid.Parse("8ba7b810-9dad-11d1-80b4-00c04fd430c8")
      Name = "Blog Platform"
      Description =
        "A simple platform for creating and managing blogs"
      Category = WebApp
      Status = Abandoned
      CreatedAt = DateTime.UtcNow.AddDays(-120.0)
      UpdatedAt = DateTime.UtcNow.AddDays(-100.0) }
  ]

  interface IProjectRepository with
    member _.GetAllAsync() =
      async { 
        // Return projects in the order they were added (newest first)
        return Ok projects
      }

    member _.GetByIdAsync(id: Guid) =
      async {
        let project =
          projects |> List.tryFind (fun p -> p.Id = id)
        return Ok project
      }

    member _.CreateAsync(project: Project) =
      async {
        // Check if project with same ID already exists
        match projects |> List.tryFindIndex (fun p -> p.Id = project.Id) with
        | Some idx ->
          // Project already exists, replace it
          projects <- projects |> List.updateAt idx project
          return Ok ()
        | None ->
          projects <- project :: projects
          return Ok ()
      }

    member _.UpdateAsync(project: Project) =
      async {
        match projects
          |> List.tryFindIndex (fun p -> p.Id = project.Id) with
        | Some idx ->
          projects <- projects |> List.updateAt idx project
          return Ok ()
        | None ->
          return Error (
            NotFound(project.Id, "InMemoryRepository")
          )
      }

    member _.DeleteAsync(id: Guid) =
      async {
        projects <- projects |> List.filter (fun p -> p.Id <> id)
        return Ok ()
      }
