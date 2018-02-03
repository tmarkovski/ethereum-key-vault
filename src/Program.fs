// Learn more about F# at http://fsharp.org

open System
open System.Threading.Tasks
open System.Collections.Generic
open EthereumKeyVault

[<EntryPoint>]
let main argv =

    getKey "alice"
    |> getAddress
    |> printfn "%s"

    0 // return an integer exit code
