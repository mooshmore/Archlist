using Google.Apis.YouTube.v3.Data;
using PlaylistSaver.PlaylistMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PlaylistSaver.Enums;
using static PlaylistSaver.PlaylistMethods.SharedClasses;

namespace PlaylistSaver.Helpers
{
    internal static class LocalHelpers
    {
        /// <summary>
        /// Translates a string privacy status to a PrivacyStatus enum.
        /// </summary>
        /// <remarks>Will throw an exception if the given privacy status doesn't match to any filters.</remarks>
        /// <returns>The translated privacy status.</returns>
        internal static PrivacyStatus PrivacyStatusTranslator(string privacyStatus)
        {
            return privacyStatus.ToLower() switch
            {
                "private" => PrivacyStatus.Private,
                "public" => PrivacyStatus.Public,
                "unlisted" => PrivacyStatus.Unlisted,
                _ => throw new Exception("#LH1 No privacy status"),
            };
        }

        /// <summary>
        /// Translates the <paramref name="imageQuality"/> to their corresponding url string names.
        /// </summary>
        internal static string ImageQualityTranslator(ImageQuality? imageQuality)
        {
            return imageQuality switch
            {
                ImageQuality.Minimum => "default",
                ImageQuality.Low => "mqdefault",
                ImageQuality.Medium => "hqdefault",
                ImageQuality.High => "sddefault",
                ImageQuality.Maximum => "maxresdefault",
                _ => throw new Exception("No saved image quality"),
            };
        }

        /// <summary>
        /// Saves maximum available quality and saved image quality.
        /// </summary>
        /// <param name="apiThumbnails"></param>
        /// <param name="thumbnail"></param>
        internal static void SaveThumbnailResolutionData(ThumbnailDetails apiThumbnails, ThumbnailInfo thumbnail)
        {
            // Save maximum available quality
            if (apiThumbnails.Maxres != null)
                thumbnail.MaximumAvailableQuality = ImageQuality.Maximum;
            else if (apiThumbnails.Standard != null)
                thumbnail.MaximumAvailableQuality = ImageQuality.High;
            else if (apiThumbnails.High != null)
                thumbnail.MaximumAvailableQuality = ImageQuality.Medium;
            else if (apiThumbnails.Medium != null)
                thumbnail.MaximumAvailableQuality = ImageQuality.Low;
            else if (apiThumbnails.Default__ != null)
                thumbnail.MaximumAvailableQuality = ImageQuality.Minimum;

            // Chosen image quality is available
            if (thumbnail.MaximumAvailableQuality >= Settings.ImageQuality)
                thumbnail.SavedImageQuality = (ImageQuality)Settings.ImageQuality;
            // If it is not available then save the best possible one
            else
                thumbnail.SavedImageQuality = thumbnail.MaximumAvailableQuality;
        }

        internal static string ExtractThumbnailId(string videoURL)
        {
            return videoURL.TrimFromFirst("vi/", false).TrimToFirst("/");
        }
    }
}
