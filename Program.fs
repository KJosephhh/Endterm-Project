open System
open System.IO
open System.Net
open System.Text
open System.Net.Http
open System.Text.Json

let respond (ctx: HttpListenerContext) (status: int) (contentType: string) (content: string) =
    let bytes = Encoding.UTF8.GetBytes(content)
    ctx.Response.StatusCode <- status
    ctx.Response.ContentType <- contentType
    ctx.Response.OutputStream.Write(bytes, 0, bytes.Length)
    ctx.Response.OutputStream.Close()

let mutable lastResponse = ""

let ollamaCall (prompt: string) =
    task {
        use client = new HttpClient()
        let uri = "http://localhost:11434/api/generate"
        let content = $"""{{ "model": "llama3.2", "prompt": "{prompt}" }}"""
        use httpContent = new StringContent(content, Encoding.UTF8, "application/json")

        let! response = client.PostAsync(uri, httpContent)
        let! responseText = response.Content.ReadAsStringAsync()

        let lines = responseText.Split('\n', StringSplitOptions.RemoveEmptyEntries)

        let responseParts =
            lines
            |> Array.choose (fun line ->
                try
                    let doc = JsonDocument.Parse(line)
                    let mutable value = Unchecked.defaultof<JsonElement>
                    if doc.RootElement.TryGetProperty("response", &value) then
                        Some (value.GetString())
                    else None
                with _ -> None
            )

        return String.Concat(responseParts)
    }

let startServer () =
    let listener = new HttpListener()
    listener.Prefixes.Add("http://localhost:5000/")
    listener.Start()
    printfn "Server running at http://localhost:5000"

    while true do
        let ctx = listener.GetContext()
        let req = ctx.Request

        match req.HttpMethod, req.Url.AbsolutePath with
        | "GET", "/" ->
            let html = File.ReadAllText("index.html")
            respond ctx 200 "text/html" html

        | "POST", "/chat" ->
            use reader = new StreamReader(req.InputStream, req.ContentEncoding)
            let body = reader.ReadToEnd()
            let prompt = Uri.UnescapeDataString(body).Replace("prompt=", "")
            let reply = ollamaCall prompt |> Async.AwaitTask |> Async.RunSynchronously
            lastResponse <- reply

            let html = File.ReadAllText("index.html")
            let updatedHtml = html.Replace("{{response}}", System.Net.WebUtility.HtmlEncode(reply))
            respond ctx 200 "text/html" updatedHtml

        | _ ->
            respond ctx 404 "text/plain" "Not found"

startServer()
