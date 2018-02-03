[<AutoOpen>]
module EthereumKeyVault.KeyVault

open System
open Microsoft.Azure.KeyVault
open Microsoft.IdentityModel.Clients.ActiveDirectory
open Microsoft.Azure.KeyVault.Models
open Microsoft.Azure.KeyVault.WebKey
open Org.BouncyCastle.Crypto.Digests
open Org.BouncyCastle.Crypto

let vaultUri = "..."
let clientId = "..."
let clientSecret = "..."

let defaultKeyParams = NewKeyParameters(Kty = "EC-HSM", CurveName = "SECP256K1", KeyOps = toList [ "sign"; "verify" ])

let getAccessToken (authority:string) (resource:string) (scope:string) =    
    let clientCredential = new ClientCredential(clientId, clientSecret)
    let context = new AuthenticationContext(authority, TokenCache.DefaultShared)
    async {
        let! result = context.AcquireTokenAsync(resource, clientCredential)
        return result.AccessToken;
    } |> Async.StartAsTask

let client = 
    AuthenticationCallback getAccessToken
    |> KeyVaultCredential
    |> KeyVaultClient

let createKey name keyParams =
    async {
        let! result = client.CreateKeyAsync(vaultUri, name, parameters = keyParams)
        return result
    } |> Async.RunSynchronously

let getKey name = 
    async {
        let! result = client.GetKeyAsync(vaultUri, keyName = name)
        return result
    } |> Async.RunSynchronously

let signKey keyId digest = 
    async {
        let! result = client.SignAsync(keyId, JsonWebKeySignatureAlgorithm.ECDSA256, digest)
        return result
    } |> Async.RunSynchronously   

let getAddress (bundle:KeyBundle) =
    Array.concat [| bundle.Key.X; bundle.Key.Y |]
    |> computeHash (KeccakDigest 256)
    |> Array.skip 12
    |> toHex
    |> (+) "0x"