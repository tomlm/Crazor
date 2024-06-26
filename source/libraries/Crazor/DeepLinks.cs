using Microsoft.AspNetCore.Http.Extensions;
using Newtonsoft.Json;

namespace Crazor
{
    /// <summary>
    /// Utility class for generating deeplinks
    /// </summary>
    public static class DeepLinks
    {
        private static readonly Uri _teamsRoot = new Uri("https://teams.microsoft.com/l");

        /// <summary>
        /// Create a deep link to a url based taskmodule
        /// </summary>
        /// <param name="appId">appId</param>
        /// <param name="card">card to show</param>
        /// <param name="title">title for window</param>
        /// <param name="height">height for window (small/medium/large)</param>
        /// <param name="width">width for window (small/medium/large)</param>
        /// <param name="completionBotId">completionBotId (Not needed for url links?)</param>
        /// <returns></returns>
        public static string CreateTaskModuleCardLink(string appId, AdaptiveCard card, string? title = null, string? height = null, string? width = null, string? completionBotId = null)
        {
            QueryBuilder qb = new QueryBuilder();
            if (card != null)
            {
                qb.Add(nameof(card), JsonConvert.SerializeObject(card));

            }

            if (title != null)
            {
                qb.Add(nameof(title), title);
            }

            if (height != null)
            {
                qb.Add(nameof(height), height);
            }

            if (width != null)
            {
                qb.Add(nameof(width), width);
            }

            if (completionBotId != null)
            {
                qb.Add(nameof(completionBotId), completionBotId);
            }

            return new Uri(_teamsRoot, $"/task/{appId}{qb.ToQueryString()}").AbsoluteUri;
        }

        /// <summary>
        /// Create a deep link to a url based taskmodule
        /// </summary>
        /// <param name="appId">appId</param>
        /// <param name="url">taskmodule url</param>
        /// <param name="title">title for window</param>
        /// <param name="height">height for window (small/medium/large)</param>
        /// <param name="width">width for window (small/medium/large)</param>
        /// <param name="completionBotId">completionBotId (Not needed for url links?)</param>
        /// <returns></returns>
        public static string CreateTaskModuleUrlLink(string appId, string? url = null, string? title = null, string? height = null, string? width = null, string? completionBotId = null)
        {
            QueryBuilder qb = new QueryBuilder();
            if (completionBotId != null)
            {
                qb.Add(nameof(completionBotId), completionBotId);
            }
            if (url != null)
            {
                qb.Add(nameof(url), url);
            }
            if (title != null)
            {
                qb.Add(nameof(title), title);
            }
            if (height != null)
            {
                qb.Add(nameof(height), height);
            }
            if (width != null)
            {
                qb.Add(nameof(width), width);
            }

            return new Uri(_teamsRoot, $"/task/{appId}{qb.ToQueryString()}").AbsoluteUri;
        }

        /// <summary>
        /// Generate deep link to Navigate to a teams chat
        /// </summary>
        /// <remarks>
        /// You can navigate to or create private chats between users with TeamsJS by specifying the set of participants. 
        /// If a chat doesn’t exist with the specified participants, the user is navigated to an empty new chat. 
        /// </remarks>
        /// <param name="users">
        /// The comma-separated list of user IDs representing the participants of the chat. The user that performs the action is always 
        /// included as a participant. Currently, the User ID field supports the Microsoft Azure Active Directory (Azure AD) UserPrincipalName, such as an email address only.
        /// </param>
        /// <param name="topicName">
        /// An optional field for chat's display name, if a chat has three or more users. If this field isn't specified, 
        /// the chat's display name is based on the names of the participants.
        /// </param>
        /// <param name="message">An optional field for the message text that you want to insert into the current user's compose box while the chat is in a draft state.</param>
        /// <returns>url</returns>
        public static string CreateTeamsChatLink(string[] users, string? topicName = null, string? message = null)
        {
            QueryBuilder qb = new QueryBuilder();
            qb.Add(nameof(users), String.Join(',', users));
            if (topicName != null)
            {
                qb.Add(nameof(topicName), topicName);
            }
            if (message != null)
            {
                qb.Add(nameof(message), message);
            }

            return new Uri(_teamsRoot, $"/chat/0/0{qb.ToQueryString()}").AbsoluteUri;
        }

        /// <summary>
        /// Generate deep links to teams channel conversation
        /// </summary>
        /// <remarks>Use this deep link format to go to a particular conversation within channel thread:</remarks>
        /// <param name="channelId">Channel ID of the conversation. For example, 19:3997a8734ee5432bb9cdedb7c432ae7d@thread.tacv2.</param>
        /// <param name="tenantId">Tenant ID such as 0d9b645f-597b-41f0-a2a3-ef103fbd91bb.</param>
        /// <param name="groupId">Group ID of the file. For example, 3606f714-ec2e-41b3-9ad1-6afb331bd35d.</param>
        /// <param name="parentMessageId">Parent message ID of the conversation.</param>
        /// <param name="teamName">Name of the team.</param>
        /// <param name="channelName">Name of the team's channel</param>
        /// <returns>url</returns>
        public static string CreateTeamsChannelLink(string channelId, string tenantId, string groupId, string parentMessageId, string teamName, string channelName)
        {
            QueryBuilder qb = new QueryBuilder();
            qb.Add(nameof(channelId), channelId);
            qb.Add(nameof(tenantId), tenantId);
            qb.Add(nameof(groupId), groupId);
            qb.Add(nameof(parentMessageId), parentMessageId);
            qb.Add(nameof(teamName), teamName);
            qb.Add(nameof(channelName), channelName);

            return new Uri(_teamsRoot, $"/chat/0/0{qb.ToQueryString()}").AbsoluteUri;
        }

        /// <summary>Generate a deep link to the teams scheduling dialog</summary>
        /// <param name="attendees">The optional comma-separated list of user IDs representing the attendees of the meeting.The user performing the action is the meeting organizer.Currently, the User ID field supports only the Azure AD UserPrincipalName, typically an email address.</param>  
        /// <param name="startTime">The optional start time of the event. This should be in long ISO 8601 format, for example 2018-03-12T23:55:25+02:00.</param>  
        /// <param name="endTime">The optional end time of the event, also in ISO 8601 format.</param>  
        /// <param name="subject">An optional field for the meeting subject.</param>  
        /// <param name="content">An optional field for the meeting details field.</param>  
        /// <returns>url</returns>
        public static string CreateTeamsSchedulingDialogLink(string[]? attendees = null, DateTime? startTime = null, DateTime? endTime = null, string? subject = null, string? content = null)
        {
            QueryBuilder qb = new QueryBuilder();
            if (attendees != null)
            {
                qb.Add(nameof(attendees), String.Join(',', attendees));
            }
            if (startTime != null)
            {
                qb.Add(nameof(startTime), startTime.Value.ToString("o"));
            }
            if (endTime != null)
            {
                qb.Add(nameof(endTime), endTime.Value.ToString("o"));
            }
            if (subject != null)
            {
                qb.Add(nameof(subject), subject);
            }
            if (content != null)
            {
                qb.Add(nameof(content), content);
            }

            return new Uri(_teamsRoot, $"/meeting/new{qb.ToQueryString()}").AbsoluteUri;
        }

        /// <summary>Generate deep links to file in channel</summary>
        /// <param name="fileId">Unique file ID from Sharepoint Online, also known as sourcedoc.For example,1FA202A5-3762-4F10-B550-C04F81F6ACBD.</param>
        /// <param name="tenantId">Tenant ID such as 0d9b645f-597b-41f0-a2a3-ef103fbd91bb.</param>
        /// <param name="fileType">Supported file type, such as .docx, .pptx, .xlsx, and.pdf.</param>
        /// <param name="objectUrl">Object URL of the file.The format is https://{tenantName}.sharepoint.com/sites/{TeamName}/SharedDocuments/{ChannelName}/FileName.ext. For example, https://microsoft.sharepoint.com/teams/(filepath).</param>
        /// <param name="baseUrl">Base URL of the file.The format is https://{tenantName}.sharepoint.com/sites/{TeamName}. For example, https://microsoft.sharepoint.com/teams.</param>
        /// <param name="serviceName">Name of the service, app ID.For example, teams.</param>
        /// <param name="threadId">The threadId is the team ID of the team where the file is stored.It's optional and can't be set for files stored in a user's OneDrive folder. threadId - 19:f8fbfc4d89e24ef5b3b8692538cebeb7@thread.skype.</param>
        /// <param name="groupId">Group ID of the file.For example, ae063b79-5315-4ddb-ba70-27328ba6c31e.</param>
        /// <returns>url</returns>
        public static string CreateTeamsFileLink(string fileId, string tenantId, string fileType, string objectUrl, string baseUrl, string serviceName, string threadId, string groupId)
        {
            QueryBuilder qb = new QueryBuilder();
            qb.Add(nameof(tenantId), tenantId);
            qb.Add(nameof(objectUrl), objectUrl);
            qb.Add(nameof(baseUrl), baseUrl);
            qb.Add(nameof(serviceName), serviceName);
            qb.Add(nameof(threadId), threadId);
            qb.Add(nameof(groupId), groupId);

            return new Uri(_teamsRoot, $"/file/{fileId}{qb.ToQueryString()}").AbsoluteUri;
        }

        /// <summary>
        /// Create deep link to teams app 
        /// </summary>
        /// <param name="appId">teams appId</param>
        /// <returns>url</returns>
        public static string CreateTeamsAppLink(string appId)
        {
            return new Uri(_teamsRoot, $"/app/{appId}").AbsoluteUri;
        }


        /// <summary>Generate a deep link to a teams call</summary>
        /// <param name="users">The comma-separated list of user IDs representing the participants of the call. Currently, the User ID field supports the Azure AD UserPrincipalName, typically an email address, or in a PSTN call, it supports a pstn mri 4:</param>
        /// <param name="withVideo">This is an optional parameter, which you can use to make a video call. Setting this parameter will only turn on the caller's camera. The receiver of the call has a choice to answer through audio or audio and video call through the Teams call notification window.</param>
        /// <param name="source">This is an optional parameter, which informs about the source of the deep link.</param>
        public static string CreateTeamsCallLink(string[] users, bool? withVideo = null, string? source = null)
        {
            QueryBuilder qb = new QueryBuilder();
            qb.Add(nameof(users), String.Join(',', users));
            if (withVideo.HasValue)
            {
                qb.Add(nameof(withVideo), withVideo.Value.ToString());
            }
            if (source != null)
            {
                qb.Add(nameof(source), source);
            }

            return new Uri(_teamsRoot, $"/call/0/0{qb.ToQueryString()}").AbsoluteUri;
        }
    }
}
