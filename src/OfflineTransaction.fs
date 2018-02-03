[<AutoOpen>]
module EthereumKeyVault.OfflineTransaction

open System.Numerics
open Microsoft.Azure.KeyVault
open Microsoft.Azure.KeyVault.Models
open Microsoft.Azure.KeyVault.WebKey
open Org.BouncyCastle.Crypto.Digests
open Nethereum.Signer
open Nethereum.RLP
open Nethereum.Signer.Crypto
open Nethereum.Util

let createTransaction receiver amount nonce =
    Transaction(receiver, amount, nonce)

let createContractTransaction address amount nonce data =
    Transaction(address, amount, nonce, data)

let transactionToMessage (transaction:Transaction) =
    [| transaction.Nonce
       transaction.GasPrice
       transaction.GasLimit
       transaction.ReceiveAddress
       transaction.Value
       transaction.Data |]

let encodeRlp (data:byte array array) (signature:ECDSASignature option) =
    [| (data 
        |> Array.map RLP.EncodeElement 
        |> Seq.toArray);

       (match (signature) with
       | Some s ->
           [| RLP.EncodeByte(s.V)
              RLP.EncodeElement(s.R.ToByteArrayUnsigned())
              RLP.EncodeElement(s.S.ToByteArrayUnsigned()) |]
       | None -> Array.empty) |]
    |> Array.concat
    |> fun x -> RLP.EncodeList(x)

let findRecoveryId signature message publicKey =
    let isMatch i =
        let recovered = ECKey.RecoverFromSignature(i, signature, message, false)
        recovered <> null 
        && areEqual (recovered.GetPubKey(false)) publicKey

    [0 .. 3]
    |> List.tryFind isMatch
    |> fun x -> defaultArg x -1
    
let signMessage (keyBundle:KeyBundle) message =
    // Construct public key and append 0x04
    let pubKey = Array.concat [| [| byte 4|]; keyBundle.Key.X; keyBundle.Key.Y |]
    let keyId = keyBundle.KeyIdentifier.Identifier

    // Compute the hash of the message
    let rawHash = 
        encodeRlp message None
        |> computeHash (KeccakDigest 256)

    // Sign the hash with key vault and return ECDSASignature
    let signature = 
        let result = signKey keyId rawHash
        
        let R = Array.take 32 result.Result
        let S = Array.skip 32 result.Result

        [| BigInt(1, R)
           BigInt(1, S) |]
        |> ECDSASignature
        |> fun x -> x.MakeCanonical()
       
    // Find the recovery id
    let recId = findRecoveryId signature rawHash pubKey

    // We must throw here, something went wrong
    if recId = -1 then failwith "Invalid signature"

    signature.V <- byte (recId + 27)
    Some signature
