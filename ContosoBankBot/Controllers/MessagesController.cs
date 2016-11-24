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
using Newtonsoft.Json.Linq;

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

                string[] userMessage = activity.Text.Split(' ');
                string rawMessage = activity.Text;
                var endOutput = "Welcome to Contoso Bank. Type a command or type Help for a list of available commands";

                if (userMessage[0].ToLower().Equals("help"))
                {
                    endOutput = "Avaliable commands: \n\n Branches - Return list of branches \n Branch <Branch name> - Return Branch information  \n ATMS - Return a list of ATMS and their availablity ";

                }
                else if (userMessage[0].ToLower().Equals("branches"))
                {
                    List<Branches> branches = await AzureManager.AzureManagerInstance.GetBranches();
                    endOutput = "";

                    foreach (Branches b in branches)
                    {
                        endOutput += "\nBank Name: " + b.Name + "\nLocation: " + b.Location + "\n";
                    }

                }
                else if (userMessage[0].ToLower().Equals("atms"))
                {
                    List<Atm_Machines> atms = await AzureManager.AzureManagerInstance.GetATMs();
                    endOutput = "";

                    foreach (Atm_Machines a in atms)
                    {
                        endOutput += "\nATM Location: " + a.Location + "\n Available: " + a.Available + "\n";
                    }
                }
                else if (userMessage[0].ToLower().Equals("logout"))
                {
                    userData.SetProperty<bool>("AdminRights", false);
                    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                    endOutput = "Successfully logged out";
                }
                else if (userMessage[0].ToLower().Equals("branch"))
                {
                    string branch = rawMessage.Substring(7);

                    Branches b = await AzureManager.AzureManagerInstance.GetBranch(branch);
                    endOutput = "Bank Name: " + b.Name + " \nLocation: " + b.Location + " \nWeekday Open Hours: " + b.WeekdayOpen + " - " + b.WeekdayClose + " \nWeekend Open Hours: " + b.WeekendOpen + " - " + b.WeekendClose;

                }
                else if (userMessage[0].ToLower().Equals("create-atm") && userData.GetProperty<bool>("AdminRights"))
                {
                    string atmLoc = rawMessage.Substring(11);

                    Atm_Machines a = new Atm_Machines()
                    {
                        Location = atmLoc,
                        Available = true
                    };

                    await AzureManager.AzureManagerInstance.AddAtm(a);
                    endOutput = "Added new ATM at [" + atmLoc + "]";
                }
                else if (userMessage[0].ToLower().Equals("delete-atm") && userData.GetProperty<bool>("AdminRights"))
                {
                    string atmLoc = rawMessage.Substring(11);

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
                else if (userMessage[0].ToLower().Equals("update-atm") && userData.GetProperty<bool>("AdminRights"))
                {

                }
                else if (userMessage[0].ToLower().Equals("exchange-rate"))
                {

                    HttpClient client = new HttpClient();
                    string x = await client.GetStringAsync(new Uri("http://api.fixer.io/latest?base=" + userMessage[1]));

                    CurrencyObject.RootObject currencyObject = JsonConvert.DeserializeObject<CurrencyObject.RootObject>(x);
                    //CurrencyObject.Rates ratesObject = JsonConvert.DeserializeObject<CurrencyObject.Rates>(x);

                    //Activity reply1 = activity.CreateReply(exchangeRate);
                    //await connector.Conversations.ReplyToActivityAsync(reply1);
                }
                else if ((userMessage[0].ToLower().Equals("update-atm") || userMessage[0].ToLower().Equals("delete-atm") || userMessage[0].ToLower().Equals("create-atm")) && !userData.GetProperty<bool>("AdminRights"))
                {
                    endOutput = "Please login with an account with admin rights";
                }
                else if (userMessage[0].ToLower().Equals("login"))
                {
                    if (await AzureManager.AzureManagerInstance.GetAccount(userMessage[1], userMessage[2]))
                    {
                        userData.SetProperty<bool>("AdminRights", true);
                        await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                        endOutput = "Successfully logged in as " + userMessage[1];
                    }
                    else
                    {
                        endOutput = "Invalid username/password";
                    }
                }

                else if (userMessage[0].ToLower().Equals("stock"))
                {

                    string stock = userMessage[1];
                    try
                    {


                        HttpClient client = new HttpClient();
                        var x = await client.GetStringAsync(new Uri("http://finance.google.com/finance/info?client=ig&q=" + stock));
                        x = x.Replace("//", "");


                        var v = JArray.Parse(x);

                        foreach (var i in v)
                        {
                            var price = (decimal)i.SelectToken("l");
                            endOutput = "The stock price for [" + stock + "] is $" + price;
                        }
                    }
                    catch (Exception e)
                    {
                        endOutput = "The stock [" + stock + "] could not be found";
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