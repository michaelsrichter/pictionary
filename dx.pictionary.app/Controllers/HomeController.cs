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
            var rnd = new Random();
            var randomItems = items.OrderBy(x => rnd.Next()).ToArray();
            cookie.Value = JsonConvert.SerializeObject(randomItems, Formatting.None);
            cookie.Expires = DateTime.Now.AddHours(3);
            ControllerContext.HttpContext.Response.Cookies.Add(cookie);
        }

        internal static string GetCategoryStyle(string category)
        {
            switch (category)
            {
                case "PAAS":
                    return "panel panel-success";
                case "IAAS":
                    return "panel panel-warning";
                case "DP":
                    return "panel panel-info";
                case "MISC":
                default:
                    return "panel panel-primary";
            }
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
            if (gameCookie == null)
            {
                return RedirectToAction("Index");
            }


            var items = JsonConvert.DeserializeObject<Keyword[]>(gameCookie.Value);
            item = items[fixedIndex];

            if (item == null)
            {
                throw new Exception("Keyword not found");
            }
            return
                View(new GameViewModel
                {
                    GameId = id,
                    Title = "Game " + id,
                    Index = fixedIndex,
                    Keyword = item,
                    CategoryStyle = GetCategoryStyle(item.Category)
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
        public string CategoryStyle { get; set; }
    }
}