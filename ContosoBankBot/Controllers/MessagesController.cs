using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using ContosoBankBot.Models;
using System.Collections.Generic;

namespace ContosoBankBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {

                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                var userMessage = activity.Text;
                var endOutput = "Welcome to Contoso Bank. Type a command or type Help for a list of available commands";

                if (userMessage.ToLower().Equals("help"))
                {
                    endOutput = "Avaliable commands: \n\n Branches - Return list of branches \n Branch <Branch name> - Return Branch information";
                }
                else if (userMessage.ToLower().Equals("branches"))
                {
                    List<Branches> branches = await AzureManager.AzureManagerInstance.GetBranches();
                    endOutput = "";

                    foreach (Branches b in branches)
                    {
                        endOutput += "\nBank Name: " + b.Name + "\nLocation: " + b.Location + "\n";
                    }

                }
                else if (userMessage.Length > 6)
                {
                    if (userMessage.ToLower().Substring(0, 6).Equals("branch"))
                    {
                        string branch = userMessage.Substring(7);

                        Branches b = await AzureManager.AzureManagerInstance.GetBranch(branch);
                        endOutput = "Bank Name: " + b.Name + " \nLocation: " + b.Location + " \nWeekday Open Hours: " + b.WeekdayOpen + " - " + b.WeekdayClose + " \nWeekend Open Hours: " + b.WeekendOpen + " - " + b.WeekendClose;

                    }
                }

                // return our reply to the user
                Activity reply = activity.CreateReply(endOutput);
                await connector.Conversations.ReplyToActivityAsync(reply);
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}