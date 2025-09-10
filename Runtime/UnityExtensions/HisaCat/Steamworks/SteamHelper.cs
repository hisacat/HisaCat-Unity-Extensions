#if STEAMWORKS_NET
using Steamworks;
using UnityEngine;

namespace HisaCat.Steamworks
{
    // Steamworks의 사용법에 관해서는 아래 링크 참고:
    // https://github.com/rlabrecque/Steamworks.NET-Test/tree/master
    // https://github.com/rlabrecque/Steamworks.NET-Example
    public static class SteamHelper
    {
        // It required?
        // https://partner.steamgames.com/doc/api/ISteamFriends#RequestUserInformation
        // SteamFriends.RequestUserInformation(cSteamId, bRequireNameOnly: false);

        public static CSteamID GetLocalUserId() => SteamUser.GetSteamID();
        public static string GetLocalUserPersonaName() => SteamFriends.GetPersonaName();
        public static string GetPersonaName(CSteamID steamId) => SteamFriends.GetFriendPersonaName(steamId);

        public static string GetUserProfileUrl(CSteamID steamId)
        {
            return $"https://steamcommunity.com/profiles/{steamId.m_SteamID}";
        }

        public static Texture2D GetLargeFriendAvatarAsTexture2D(CSteamID steamId)
        {
            var iAvatarImage = SteamFriends.GetLargeFriendAvatar(steamId);
            return GetSteamImageAsTexture2D(iAvatarImage);
        }

        // https://github.com/rlabrecque/Steamworks.NET-Test/blob/master/Assets/Scripts/SteamUtilsTest.cs#L37
        public static Texture2D GetSteamImageAsTexture2D(int iImage)
        {
            bool isValid = SteamUtils.GetImageSize(iImage, out var width, out var height);
            if (isValid == false) return null;

            byte[] data = new byte[width * height * 4];
            isValid = SteamUtils.GetImageRGBA(iImage, data, (int)(width * height * 4));
            if (isValid == false) return null;

            // Steam 이미지를 수직으로 뒤집기 (상하반전 해결)
            FlipImageVertically(data, (int)width, (int)height);

            // sRGB 색공간 사용 (색상 바래짐 문제 해결)
            var tex = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, false);
            tex.LoadRawTextureData(data);
            tex.Apply();

            return tex;
        }

        private static void FlipImageVertically(byte[] imageData, int width, int height)
        {
            int bytesPerPixel = 4; // RGBA
            int stride = width * bytesPerPixel;
            byte[] tempRow = new byte[stride];

            for (int y = 0; y < height / 2; y++)
            {
                int topRowOffset = y * stride;
                int bottomRowOffset = (height - 1 - y) * stride;

                // 상단 행을 임시 버퍼에 복사
                System.Array.Copy(imageData, topRowOffset, tempRow, 0, stride);

                // 하단 행을 상단으로 복사
                System.Array.Copy(imageData, bottomRowOffset, imageData, topRowOffset, stride);

                // 임시 버퍼(원래 상단)를 하단으로 복사
                System.Array.Copy(tempRow, 0, imageData, bottomRowOffset, stride);
            }
        }
    }
}
#endif
