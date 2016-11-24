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
using ContosoBankBot.DataModels;

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

                StateClient stateClient = activity.GetStateClient();
                BotData userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);

                var userMessage = activity.Text;
                var endOutput = "Welcome to Contoso Bank. Type a command or type Help for a list of available commands";

                //if (userData.GetProperty<bool>("AdminRights"))
                //{
                //    Activity reply1 = activity.CreateReply("You have admin rights");
                //    await connector.Conversations.ReplyToActivityAsync(reply1);
                //}

                if (userMessage.ToLower().Equals("help"))
                {
                    endOutput = "Avaliable commands: \n\n Branches - Return list of branches \n Branch <Branch name> - Return Branch information  \n ATMS - Return a list of ATMS and their availablity ";

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
                else if (userMessage.ToLower().Equals("atms"))
                {
                    List<Atm_Machines> atms = await AzureManager.AzureManagerInstance.GetATMs();
                    endOutput = "";

                    foreach (Atm_Machines a in atms)
                    {
                        endOutput += "\nATM Location: " + a.Location + "\n Available: " + a.Available + "\n";
                    }
                }
                else if (userMessage.ToLower().Equals("login"))
                {

                }
                else if (userMessage.ToLower().Equals("logout"))
                {
                    userData.SetProperty<bool>("AdminRights", false);
                    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                    endOutput = "Successfully logged out";
                }
                else if (userMessage.Length > 6)
                {
                    if (userMessage.ToLower().Substring(0, 6).Equals("branch"))
                    {
                        string branch = userMessage.Substring(7);

                        Branches b = await AzureManager.AzureManagerInstance.GetBranch(branch);
                        endOutput = "Bank Name: " + b.Name + " \nLocation: " + b.Location + " \nWeekday Open Hours: " + b.WeekdayOpen + " - " + b.WeekdayClose + " \nWeekend Open Hours: " + b.WeekendOpen + " - " + b.WeekendClose;

                    }
                    else if (userMessage.ToLower().Substring(0, 10).Equals("create-atm") && userData.GetProperty<bool>("AdminRights"))
                    {
                        string atmLoc = userMessage.Substring(11);

                        Atm_Machines a = new Atm_Machines()
                        {
                            Location = atmLoc,
                            Available = true
                        };

                        await AzureManager.AzureManagerInstance.AddAtm(a);
                        endOutput = "Added new ATM at [" + atmLoc + "]";
                    }
                    else if (userMessage.ToLower().Substring(0, 10).Equals("delete-atm") && userData.GetProperty<bool>("AdminRights"))
                    {
                        string atmLoc = userMessage.Substring(11);

                        //Activity reply1 = activity.CreateReply(atmLoc);
                        //await connector.Conversations.ReplyToActivityAsync(reply1);
                        if (await AzureManager.AzureManagerInstance.DeleteAtm(atmLoc))
                        {
                            endOutput = "Deleted ATM [" + atmLoc + "]";
                        }
                        else
                        {
                            endOutput = "Could not delete ATM [" + atmLoc + "]. Please make sure ATM exists";
                        }


                    }
                    else if (userMessage.ToLower().Substring(0, 10).Equals("update-atm") && userData.GetProperty<bool>("AdminRights"))
                    {

                    }

                    if ((userMessage.ToLower().Substring(0, 10).Equals("update-atm") || userMessage.ToLower().Substring(0, 10).Equals("delete-atm") || userMessage.ToLower().Substring(0, 10).Equals("create-atm")) && !userData.GetProperty<bool>("AdminRights"))
                    {
                        endOutput = "Please login with an account with admin rights";
                    }

                    if (userMessage.Length > 5)
                    {
                        if (userMessage.ToLower().Substring(0, 5).Equals("login"))
                        {
                            string temp = userMessage.Substring(6);
                            string[] userInfo = temp.Split(' ');

                            endOutput = "Username: " + userInfo[0] + " Password: " + userInfo[1];

                            if (await AzureManager.AzureManagerInstance.GetAccount(userInfo[0], userInfo[1]))
                            {
                                userData.SetProperty<bool>("AdminRights", true);
                                await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                                endOutput = "Successfully logged in as " + userInfo[0];
                            }
                        }
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