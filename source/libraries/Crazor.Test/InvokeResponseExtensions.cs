// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Crazor.Test.MSTest
{
    public static class InvokeResponseExtensions
    {
        /// <summary>
        /// Assert there is a textblock[id] has text
        /// </summary>
        /// <param name="task"></param>
        /// <param name="id"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static AdaptiveCard GetCardFromResponse<T>(this InvokeResponse invokeResponse, Action<T>? callback = null)
        {
            if (invokeResponse.Body is AdaptiveCardInvokeResponse ac)
            {
                if (callback != null)
                    callback((T)(object)ac);

                return ac.GetCardFromResponse();
            }
            else if (invokeResponse.Body is MessagingExtensionResponse mer)
            {
                if (callback != null)
                    callback((T)(object)mer);

                return mer.GetCardFromResponse();
            }
            else if (invokeResponse.Body is MessagingExtensionActionResponse mear)
            {
                if (callback != null)
                {
                    if (typeof(T) == typeof(MessagingExtensionActionResponse))
                        callback((T)(object)mear);
                    else if (mear.Task is T t)
                        callback(t);
                }

                return mear.GetCardFromResponse();

            }
            return null;
        }

        public static AdaptiveCard GetCardFromResponse(this AdaptiveCardInvokeResponse response)
        {
            return ObjectPath.MapValueTo<AdaptiveCard>(response.Value);
        }

        public static AdaptiveCard GetCardFromResponse(this MessagingExtensionResponse response)
        {
            return response.ComposeExtension.Attachments
                    .Where(a => a.ContentType == AdaptiveCard.ContentType)
                    .Select(a => ObjectPath.MapValueTo<AdaptiveCard>(a.Content))
                    .Single()!;
        }

        public static AdaptiveCard GetCardFromResponse(this MessagingExtensionActionResponse response)
        {
            if (response.Task is TaskModuleContinueResponse continueResponse)
            {
                return continueResponse.GetCardFromResponse();
            }

            if (response.ComposeExtension?.Type == "botMessagePreview")
            {
                return response.ComposeExtension.ActivityPreview.Attachments
                    .Where(a => a.ContentType == AdaptiveCard.ContentType)
                    .Select(a => ObjectPath.MapValueTo<AdaptiveCard>(a.Content))
                    .Single()!;;
            }

            return response.ComposeExtension.Attachments
                    .Where(a => a.ContentType == AdaptiveCard.ContentType)
                    .Select(a => ObjectPath.MapValueTo<AdaptiveCard>(a.Content))
                    .Single()!;
        }

        public static AdaptiveCard GetCardFromResponse(this TaskModuleContinueResponse response)
        {
            Assert.AreEqual(AdaptiveCard.ContentType, response.Value.Card.ContentType);
            return ObjectPath.MapValueTo<AdaptiveCard>(response.Value.Card.Content);
        }

        public static AdaptiveCard GetCardFromResponse(this TaskModuleCardResponse response)
        {
            throw new NotImplementedException();
            //            Assert.AreEqual(AdaptiveCard.ContentType, response.Value.Card.ContentType);
            //            return JObject.FromObject(response.Value.Card.Content).ToObject<AdaptiveCard>();
        }

        public static AdaptiveCard GetCardFromResponse(this TaskModuleResponse response)
        {
            throw new NotImplementedException();
        }

        public static AdaptiveCard GetCardFromResponse(this TabResponse response)
        {
            throw new NotImplementedException();
        }
    }
}