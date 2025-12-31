// Helpers/VideoHelper.cs
using System;

namespace Abig2025.Helpers
{
    public static class VideoHelper
    {
        public static string GetEmbedUrl(string videoUrl)
        {
            if (string.IsNullOrWhiteSpace(videoUrl))
                return string.Empty;

            // URL de YouTube (watch)
            if (videoUrl.Contains("youtube.com/watch?v="))
            {
                string videoId = videoUrl.Split("v=")[1];
                if (!string.IsNullOrEmpty(videoId))
                {
                    int ampersandPosition = videoId.IndexOf('&');
                    if (ampersandPosition != -1)
                    {
                        videoId = videoId.Substring(0, ampersandPosition);
                    }

                    return $"https://www.youtube.com/embed/{videoId}";
                }
            }
            // URL corta de YouTube (youtu.be)
            else if (videoUrl.Contains("youtu.be/"))
            {
                string videoId = videoUrl.Split("youtu.be/")[1];
                if (!string.IsNullOrEmpty(videoId))
                {
                    int questionMarkPosition = videoId.IndexOf('?');
                    string cleanVideoId = questionMarkPosition != -1
                        ? videoId.Substring(0, questionMarkPosition)
                        : videoId;

                    return $"https://www.youtube.com/embed/{cleanVideoId}";
                }
            }
            // URL de YouTube embed (ya está bien formada)
            else if (videoUrl.Contains("youtube.com/embed/"))
            {
                return videoUrl;
            }
            // URL de Vimeo
            else if (videoUrl.Contains("vimeo.com/"))
            {
                string videoId = videoUrl.Split("vimeo.com/")[1];
                if (!string.IsNullOrEmpty(videoId))
                {
                    int questionMarkPosition = videoId.IndexOf('?');
                    string cleanVideoId = questionMarkPosition != -1
                        ? videoId.Substring(0, questionMarkPosition)
                        : videoId;

                    return $"https://player.vimeo.com/video/{cleanVideoId}";
                }
            }
            // URL de Vimeo player 
            else if (videoUrl.Contains("player.vimeo.com/video/"))
            {
                return videoUrl;
            }

            // Si no coincide con ningún patrón conocido, devolver la URL original
            return videoUrl;
        }

        public static string GetVideoThumbnail(string videoUrl)
        {
            if (string.IsNullOrWhiteSpace(videoUrl))
                return string.Empty;

            string embedUrl = GetEmbedUrl(videoUrl);

            // YouTube thumbnail
            if (embedUrl.Contains("youtube.com/embed/"))
            {
                string videoId = embedUrl.Split("embed/")[1];
                return $"https://img.youtube.com/vi/{videoId}/maxresdefault.jpg";
            }
            // Vimeo thumbnail (necesitarías API, pero puedes usar una genérica)
            else if (embedUrl.Contains("vimeo.com/video/"))
            {
                return "/images/video-placeholder.jpg";
            }

            return "/images/video-placeholder.jpg";
        }

        public static bool IsValidVideoUrl(string videoUrl)
        {
            if (string.IsNullOrWhiteSpace(videoUrl))
                return false;

            return videoUrl.Contains("youtube.com") ||
                   videoUrl.Contains("youtu.be") ||
                   videoUrl.Contains("vimeo.com");
        }
    }
}