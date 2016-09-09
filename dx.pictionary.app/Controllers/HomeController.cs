using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace dx.pictionary.app.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            const string cookeNameBase = "Game";
            for (var i = 1; i < 7; i++)
            {
                var cookieId = cookeNameBase + i;
                if (!ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains(cookieId))
                {
                    SetKeywordCookies(cookieId);
                }
            }
            return View();
        }

        private void SetKeywordCookies(string cookieId)
        {
            var cookie = new HttpCookie(cookieId);
            var items = RetrieveKeywords(cookieId);
            cookie.Value = JsonConvert.SerializeObject(items, Formatting.None);
            cookie.Expires = DateTime.Now.AddHours(3);
            ControllerContext.HttpContext.Response.Cookies.Add(cookie);
        }

        internal static Keyword[] GetKeywords()
        {
            var items = new[]
            {
                new Keyword() {PartitionKey = "Game1", Category = "MISC", RowKey = "Azure AD B2C"},
                new Keyword() {PartitionKey = "Game1", Category = "DP", RowKey = "SQL Insert"},
                new Keyword() {PartitionKey = "Game1", Category = "PAAS", RowKey = "DocumentDb"},
                new Keyword() {PartitionKey = "Game1", Category = "IAAS", RowKey = "VMs"},
                new Keyword() {PartitionKey = "Game1", Category = "MISC", RowKey = "Azure AD B2C"},
                new Keyword() {PartitionKey = "Game1", Category = "DP", RowKey = "SQL Insert"},
                new Keyword() {PartitionKey = "Game1", Category = "PAAS", RowKey = "DocumentDb"}
            };
            return items;
        }

        internal static IEnumerable<Keyword> RetrieveKeywords(string partitionKey)
        {
            var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference("keywords");
            var query = new TableQuery<Keyword>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));
            return table.ExecuteQuery(query);
        }

        public ActionResult Game(int id, int index)
        {
            var fixedIndex = index;
            if (fixedIndex < 0) fixedIndex = 0;
            if (fixedIndex > 19) fixedIndex = 0;
            var cookieId = "Game" + id;
            Keyword item = null;
            var gameCookie = this.ControllerContext.HttpContext.Request.Cookies[cookieId];
            if (gameCookie != null)
            {
                var items = JsonConvert.DeserializeObject<Keyword[]>(gameCookie.Value);
                item = items[fixedIndex];
            }

            return
                View(new GameViewModel
                {
                    GameId = id,
                    Title =  "Game " + id,
                    Index = fixedIndex,
                    Keyword = item
                });
        }
    }

    [Serializable]
    public class Keyword : TableEntity
    {
        public string Category { get; set; }
    }

    public class GameViewModel
    {
        public Keyword Keyword { get; set; }
        public int GameId { get; set; }
        public string Title { get; set; }
        public int Index { get; set; }
       
    }
}