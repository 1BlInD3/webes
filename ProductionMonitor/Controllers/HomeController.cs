namespace ProductionMonitor.Controllers
{
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Web.Mvc;
    using ProductionMonitor.Models;

    public class HomeController : Controller
    {
        public ActionResult GroupView(string id)
        {
            string[] selected;
            var histories = new List<ProductionHistory>();
            switch (id)
            {
                case "4":
                    selected = Utils.StringData.Group4;
                    break;
                case "3":
                    selected = Utils.StringData.Group3;
                    break;
                case "2":
                    selected = Utils.StringData.Group2;
                    break;
                default:
                    selected = Utils.StringData.Group1;
                    break;
            }
            foreach (var u in selected) histories.Add(new ProductionHistory(u));

            dynamic mymodel = new ExpandoObject();
            mymodel.Units = histories;
            //mymodel.Monthlies = new MonthlyFacts();
            
            return View(mymodel);
        }

        DailyProductivityReportCollection daily;
        public ActionResult DailyProd()
        {
            dynamic mymodel = new ExpandoObject();
            mymodel.Daily = daily ?? (daily = DailyProductivityReportCollection.GetLastProductiveDaysReport());
            return View(mymodel);
        }

        YearlyProductivityReportCollection yearly;
        WeeklyProductivityReportCollection weekly;
        public ActionResult WeeklyProd()
        {
            dynamic mymodel = new ExpandoObject();
            mymodel.Yearly = yearly ?? (yearly = YearlyProductivityReportCollection.GetLastYearsReport());
            mymodel.Weekly = weekly ?? (weekly = WeeklyProductivityReportCollection.GetWeeklyReport());
            return View(mymodel);
        }

        public ActionResult Test(string id)
        {
            string[] selected;
            var histories = new List<ProductionHistory>();
            switch (id)
            {
                case "4":
                    selected = Utils.StringData.Group4;
                    break;
                case "3":
                    selected = Utils.StringData.Group3;
                    break;
                case "2":
                    selected = Utils.StringData.Group2;
                    break;
                default:
                    selected = Utils.StringData.Group1;
                    break;
            }
            foreach (var u in selected) histories.Add(new ProductionHistory(u));

            dynamic mymodel = new ExpandoObject();
            mymodel.Units = histories;
            mymodel.Monthlies = new MonthlyFacts();

            return View(mymodel);
        }
    }
}