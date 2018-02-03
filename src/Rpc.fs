[<AutoOpen>]
module EthereumKeyVault.Rpc

open System
open Nethereum.JsonRpc.Client
open Nethereum.RPC.Eth.Transactions

let rpcClient = RpcClient (new Uri("http://localhost:8485"))

let sendRawTransaction txHash =
    async {
        let! result = (EthSendRawTransaction rpcClient).SendRequestAsync(txHash)
        return result
    } |> Async.RunSynchronously

let getTransactionCount address =
    async {
        let! result = (EthGetTransactionCount rpcClient).SendRequestAsync(address)
        return result
    } |> Async.RunSynchronously
