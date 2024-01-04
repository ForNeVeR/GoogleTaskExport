module GoogleTaskExport.GoogleTasks

open System
open System.Collections.Generic
open System.Threading.Tasks

open Google.Apis.Tasks.v1
open Google.Apis.Tasks.v1.Data

[<Literal>]
let TasksScope = "https://www.googleapis.com/auth/tasks"

[<Literal>]
let MaxEntitiesPerRequest = 100

type TaskService(oAuthToken: string) =
    let googleService = new TasksService()

    let executeChainedLoad request itemsExtractor nextPageTokenExtractor: Task<IReadOnlyList<_>> = task {
        let result = ResizeArray()
        let mutable nextPageToken = null
        let mutable proceed = true
        while proceed do
            let! response = request nextPageToken
            result.AddRange <| itemsExtractor response

            match nextPageTokenExtractor response with
            | null ->
                proceed <- false
            | token ->
                nextPageToken <- token

        return result
    }

    interface IDisposable with
        member _.Dispose() =
            googleService.Dispose()

    member _.GetAllTaskLists(): Task<IReadOnlyList<TaskList>> =
        executeChainedLoad (
            fun pageToken ->
                let request = googleService.Tasklists.List()
                request.OauthToken <- oAuthToken
                request.MaxResults <- MaxEntitiesPerRequest
                request.PageToken <- pageToken
                request.ExecuteAsync()
            ) (_.Items) (_.NextPageToken)

    member _.GetAllTasks(taskListId: string): Task<IReadOnlyList<Task>> =
        executeChainedLoad (
            fun pageToken ->
                let request = googleService.Tasks.List taskListId
                request.OauthToken <- oAuthToken
                request.MaxResults <- MaxEntitiesPerRequest
                request.PageToken <- pageToken

                // request.ShowCompleted is already true by default, but to show tasks completed in Google Tasks web
                // interface, we also need this:
                request.ShowHidden <- true

                request.ExecuteAsync()
            ) (_.Items) (_.NextPageToken)
