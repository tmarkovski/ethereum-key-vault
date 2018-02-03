[<AutoOpen>]
module EthereumKeyVault.Extensions

open System
open System.Threading.Tasks
open System.Collections.Generic
open Microsoft.Azure.KeyVault
open Org.BouncyCastle.Crypto
open Nethereum.Util

/// Implements an extension method that overloads the standard
/// 'Bind' of the 'async' builder. The new overload awaits on 
/// a standard .NET task
type AsyncBuilder with
    member __.Bind(t:Task<'T>, f:'T -> Async<'R>) : Async<'R>  = 
        async.Bind(Async.AwaitTask t, f)

// type aliases
type AuthenticationCallback = KeyVaultClient.AuthenticationCallback
type BigInt = Org.BouncyCastle.Math.BigInteger

let toList<'a> (collection:'a seq) = new List<'a>(collection)

let etherToWei (i:bigint) = UnitConversion.Convert.ToWei(i, UnitConversion.EthUnit.Ether)

let toHex (buffer:byte array) =
    buffer 
    |> Array.map (fun x -> x.ToString("x2"))
    |> String.Concat

let hexToArray (value:string) =
    [| 0 .. value.Length - 1 |]
    |> Array.where (fun x -> x % 2 = 0)
    |> Array.map (fun x -> value.Substring(x, 2))
    |> Array.map (fun x -> Convert.ToByte(x, 16))

let areEqual (x:seq<'a>) (y:seq<'a>) =
       x <> null 
    && y <> null 
    && Seq.length x = Seq.length y 
    && [ 0 .. Seq.length x - 1 ] 
       |> Seq.tryFind (fun a -> Seq.item a x <> Seq.item a y)
       |> fun a -> a.IsNone

let computeHash (digest:IDigest) data =
    let result = digest.GetDigestSize() |> Array.zeroCreate
    digest.BlockUpdate(data, 0, data.Length)
    digest.DoFinal(result, 0) |> ignore
    result