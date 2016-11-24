using ContosoBankBot.DataModels;
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
        private IMobileServiceTable<Atm_Machines> atmTable;
        private IMobileServiceTable<Accounts> accountsTable;
        private static AzureManager instance;
        private MobileServiceClient client;

        private AzureManager()
        {
            this.client = new MobileServiceClient("http://phase2-az.azurewebsites.net");
            this.branchesTable = this.client.GetTable<Branches>();
            this.atmTable = this.client.GetTable<Atm_Machines>();
            this.accountsTable = this.client.GetTable<Accounts>();
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

            return branches.First(b => b.Name.ToLower().Contains(branch.ToLower()));
        }

        public async Task<List<Atm_Machines>> GetATMs()
        {
            return await this.atmTable.ToListAsync();
        }

        public async Task AddAtm(Atm_Machines atm)
        {
            await this.atmTable.InsertAsync(atm);
        }

        public async Task<bool> DeleteAtm(string atmLoc)
        {
            List<Atm_Machines> atms = await GetATMs();

            Atm_Machines a = atms.Find(atm => atm.Location.ToLower().Contains(atmLoc.ToLower()));

            if (a != null)
            {
                await this.atmTable.DeleteAsync(a);
                return true;
            }

            return false;
        }

        public async Task<List<Accounts>> GetAccounts()
        {
            return await this.accountsTable.ToListAsync();
        }

        public async Task<Boolean> GetAccount(string username, string password)
        {
            List<Accounts> accounts = await GetAccounts();

            Accounts a = accounts.Find(acc => acc.Username == username && acc.Password == password);

            if (a != null)
            {
                return true;
            }

            return false;
        }
    }
}