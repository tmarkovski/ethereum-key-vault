[<AutoOpen>]
module EthereumKeyVault.Rpc

open System
open Nethereum.JsonRpc.Client
open Nethereum.RPC.Eth.Transactions

let rpcClient = RpcClient (new Uri("http://localhost:8485"))
let rawTx = EthSendRawTransaction rpcClient

let sendRawTransaction txHash =
    async {
        let! result = rawTx.SendRequestAsync(txHash)
        return result
    } |> Async.RunSynchronously