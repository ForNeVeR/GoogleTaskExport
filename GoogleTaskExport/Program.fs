open System
open System.Text

open System.Text.Json
open Google.Apis.Tasks.v1.Data

open GoogleTaskExport
open GoogleTaskExport.GoogleTasks

let private getAuthToken userName clientSecretFile = task {
    let! clientSecrets = GoogleAuth.readClientSecretFile clientSecretFile
    let! authToken = GoogleAuth.getAuthenticationToken userName [| TasksScope |] clientSecrets
    return authToken.AccessToken
}

let private print color (text: string) =
    Console.ForegroundColor <- color
    Console.Write text
let private println() = Console.WriteLine()
let private printEnv action =
    let oldColor = Console.ForegroundColor
    try
        action()
    finally
        Console.ForegroundColor <- oldColor

let private printTaskListHeader(taskList: TaskList) =
    printEnv(fun () ->
        print ConsoleColor.White $"## {taskList.Title}"
        println()
    )

let private printTask(task: Task) =
    let datePattern = "yyyy-MM-dd"
    let dueDate =
        if String.IsNullOrWhiteSpace task.Due
        then "<no date>".PadLeft(datePattern.Length)
        else
            DateTimeOffset.Parse(task.Due).ToLocalTime().ToString datePattern
    printEnv(fun () ->
        print ConsoleColor.White "- "
        print ConsoleColor.Yellow dueDate
        print ConsoleColor.White $" {task.Title}"
        print ConsoleColor.DarkGray $" ({task.Status})"
        if not <| String.IsNullOrWhiteSpace task.Notes then
            let notes = task.Notes.Split '\n'
            notes |> Array.iter(fun note ->
                println()
                print ConsoleColor.White $"  {note}"
            )

        println()
    )

let private printTasks userName clientSecretFile = task {
    Console.OutputEncoding <- Encoding.UTF8
    let! authToken = getAuthToken userName clientSecretFile
    use service = new TaskService(authToken)
    let! taskLists = service.GetAllTaskLists()
    for taskList in taskLists do
        printTaskListHeader taskList
        let! tasks = service.GetAllTasks taskList.Id
        for task in tasks do
            printTask task
}

let private streamJson userName clientSecretFile = task {
    let! authToken = getAuthToken userName clientSecretFile
    use service = new TaskService(authToken)
    let! taskLists = service.GetAllTaskLists()
    let! tasksPerList =
        taskLists
        |> Seq.map(fun tl -> task {
            let! tasks = service.GetAllTasks tl.Id
            return struct(tl, tasks)
        })
        |> System.Threading.Tasks.Task.WhenAll
    let result =
        tasksPerList
        |> Seq.map(fun struct(tl, tasks) ->
            {| TaskList = tl
               Tasks = tasks |}
        )

    Console.OutputEncoding <- Encoding.UTF8
    use stream = Console.OpenStandardOutput()
    do! JsonSerializer.SerializeAsync(stream, result)
}

let private runSynchronously(task: System.Threading.Tasks.Task) =
    task.GetAwaiter().GetResult()

let private printUsage() =
    printfn "Accepted arguments: [print|json] <userName> <clientSecretFile>"

[<EntryPoint>]
let main: string[] -> int = function
    | [| "json"; userName; clientSecretFile |] ->
        runSynchronously <| streamJson userName clientSecretFile
        0
    | [| "print"; userName; clientSecretFile |] ->
        runSynchronously <| printTasks userName clientSecretFile
        0
    | args ->
        printUsage()
        if Array.isEmpty args then 0 else 1
