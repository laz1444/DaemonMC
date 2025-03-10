﻿using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;
using DaemonMC.Network.Bedrock;
using DaemonMC.Network;
using DaemonMC.Network.RakNet;
using DaemonMC.Utils.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;

namespace DaemonMC.Utils
{
    public class JWTObject
    {
        public List<string> Chain { get; set; } = new List<string>();
    }

    public class JWT
    {
        public const string RootKey = "MHYwEAYHKoZIzj0CAQYFK4EEACIDYgAECRXueJeTDqNRRgJi/vlRufByu/2G0i2Ebt6YMar5QX/R0DIIyrJMcUpruK4QveTfJSTp3Shlq4Gk34cD/4GUWwkv0DVuzeuB+tXija7HBxii03NHDbPAD0AKnLr2wdAp";
        public static bool XboxAuth { get; set; } = true;

        public static void processJWTchain(string jsonString, IPEndPoint clientEp)
        {
            var player = RakSessionManager.getSession(clientEp);
            JWTObject decodedObject = JsonConvert.DeserializeObject<JWTObject>(jsonString);
            var handler = new JwtSecurityTokenHandler();

            if (decodedObject == null)
            {
                return;
            }

            if (decodedObject.Chain.Count == 3)
            {
                var jsonToken = handler.ReadToken(decodedObject.Chain[1]) as JwtSecurityToken;  //mojang chain
                var x5u = jsonToken.Header["x5u"].ToString();
                if (x5u == RootKey)
                {
                    Log.debug("Mojang RootKey: OK");
                }

                var clientToken = handler.ReadToken(decodedObject.Chain[2]) as JwtSecurityToken;  //client chain
                var extraDataClaim = clientToken.Claims.FirstOrDefault(claim => claim.Type == "extraData");

                ExtraData extraData = JsonConvert.DeserializeObject<ExtraData>(extraDataClaim.Value);
                player.username = extraData.DisplayName;
                player.identity = extraData.Identity;
                player.identityPublicKey = clientToken.Claims.FirstOrDefault(claim => claim.Type == "identityPublicKey").Value;
                player.XUID = extraData.XUID;
            }
            else if (!XboxAuth)
            {
                var jsonToken2 = handler.ReadToken(decodedObject.Chain[0]) as JwtSecurityToken;  //client chain
                var extraDataClaim = jsonToken2.Claims.FirstOrDefault(claim => claim.Type == "extraData");

                ExtraData extraData = JsonConvert.DeserializeObject<ExtraData>(extraDataClaim.Value);
                player.username = extraData.DisplayName;
                player.identity = extraData.Identity;
                player.XUID = extraData.XUID;
            }
            else
            {
                PacketEncoder encoder = PacketEncoderPool.Get(clientEp);
                var packet = new Disconnect
                {
                    Message = "You need to login to Xbox Live"
                };
                packet.EncodePacket(encoder);
            }
        }

        public static void processJWTtoken(string rawToken, IPEndPoint clientEp)
        {
            var player = RakSessionManager.getSession(clientEp);
            int index = rawToken.IndexOf("ey");
            string[] tokenParts = rawToken.Substring(index).Split('.');

            string headerJson = DecodeBase64Url(tokenParts[0]);
            string payloadJson = DecodeBase64Url(tokenParts[1]);

            JObject header = JObject.Parse(headerJson);
            JwtPayload payload = JsonConvert.DeserializeObject<JwtPayload>(payloadJson);

            try
            {
                player.skin = new Skin()
                {
                    ArmSize = payload.ArmSize,
                    AnimatedImageData = payload.AnimatedImageData,
                    OverrideSkin = payload.OverrideSkin,
                    PersonaPieces = payload.PersonaPieces,
                    PersonaSkin = payload.PersonaSkin,
                    PlayFabId = payload.PlayFabId,
                    PremiumSkin = payload.PremiumSkin,
                    SkinAnimationData = payload.SkinAnimationData,
                    SkinColor = payload.SkinColor,
                    PieceTintColors = payload.PieceTintColors,
                    SkinData = Convert.FromBase64String(payload.SkinData),
                    SkinGeometryData = Encoding.UTF8.GetString(Convert.FromBase64String(payload.SkinGeometryData)),
                    SkinGeometryDataEngineVersion = Encoding.UTF8.GetString(Convert.FromBase64String(payload.SkinGeometryDataEngineVersion)),
                    SkinId = payload.SkinId,
                    SkinImageHeight = payload.SkinImageHeight,
                    SkinImageWidth = payload.SkinImageWidth,
                    SkinResourcePatch = Encoding.UTF8.GetString(Convert.FromBase64String(payload.SkinResourcePatch)),
                    CapeOnClassicSkin = payload.CapeOnClassicSkin,
                    Cape = new Cape()
                    {
                        CapeData = Convert.FromBase64String(payload.CapeData),
                        CapeId = payload.CapeId,
                        CapeImageHeight = payload.CapeImageHeight,
                        CapeImageWidth = payload.CapeImageWidth,
                        CapeOnClassicSkin = payload.CapeOnClassicSkin
                    }
                };
            }
            catch (FormatException ex)
            {
                PacketEncoder encoder = PacketEncoderPool.Get(clientEp);
                var packet = new Disconnect
                {
                    Message = $"Skin decoding failed"
                };
                packet.EncodePacket(encoder);
                Log.error($"Skin decoding failed: {ex.Message}");
            }

            Log.info($"{player.username} with client version {payload.GameVersion} doing login...");
        }

        public static string CreateHandshakeJwt(byte[] secret, ECDsa ecdsa)
        {
            ECParameters signParams = ecdsa.ExportParameters(true);

            var headerJson = $"{{\"alg\":\"ES384\",\"x5u\":\"{Convert.ToBase64String(ecdsa.ExportSubjectPublicKeyInfo())}\"}}";
            string headerBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes(headerJson));

            var payloadJson = $"{{\"salt\":\"{Convert.ToBase64String(secret)}\"}}";
            string payloadBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));

            string message = $"{headerBase64}.{payloadBase64}";
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            byte[] derSignature = ecdsa.SignData(messageBytes, HashAlgorithmName.SHA384);
            Log.debug($"Raw Signature (Hex): {BitConverter.ToString(derSignature)}");
            string signatureBase64 = Base64UrlEncode(derSignature);
            return $"{message}.{signatureBase64}";
        }

        private static string DecodeBase64Url(string base64Url)
        {
            string base64 = base64Url.Replace('-', '+').Replace('_', '/');
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }

            byte[] data = Convert.FromBase64String(base64);
            return Encoding.UTF8.GetString(data);
        }

        private static string Base64UrlEncode(byte[] bytes)
        {
            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }
    }
}