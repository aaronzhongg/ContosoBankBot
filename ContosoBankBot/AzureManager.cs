using ContosoBankBot.Models;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ContosoBankBot
{
    public class AzureManager
    {

        private IMobileServiceTable<Branches> branchesTable;
        private static AzureManager instance;
        private MobileServiceClient client;

        private AzureManager()
        {
            this.client = new MobileServiceClient("http://phase2-az.azurewebsites.net");
            this.branchesTable = this.client.GetTable<Branches>();
        }

        public MobileServiceClient AzureClient
        {
            get { return client; }
        }

        public static AzureManager AzureManagerInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AzureManager();
                }

                return instance;
            }
        }

        public async Task<List<Branches>> GetBranches()
        {
            return await this.branchesTable.ToListAsync();
        }

        public async Task<Branches> GetBranch(String branch)
        {
            List<Branches> branches = await GetBranches();

            return branches.First(b => b.Name.ToLower() == branch.ToLower());
        }
    }
}