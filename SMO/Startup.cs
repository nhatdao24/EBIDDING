using Microsoft.Owin;
using Owin;
using Hangfire;
using SMO.Service.AD;
using System.Web.Services.Description;

[assembly: OwinStartupAttribute(typeof(SMO.Startup))]
namespace SMO
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            app.MapSignalR();

            GlobalConfiguration.Configuration.UseSqlServerStorage("HangfireDB");

            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                AuthorizationFilters = new[] { new MyRestrictiveAuthorizationFilter() }
            });
            app.UseHangfireServer();

            InitBackgroundJob();
        }

        protected void InitBackgroundJob()
        {
            RecurringJob.AddOrUpdate("SendEmail", () => SMOUtilities.SendEmail(), Cron.MinuteInterval(2));
            //RecurringJob.AddOrUpdate("BroadcastMail", () => SMOUtilities.BroadcastMail(), Cron.MinuteInterval(1));
            RecurringJob.AddOrUpdate("AutoCloseReview", () => SMOUtilities.AutoCloseReview(), Cron.MinuteInterval(5));
            RecurringJob.AddOrUpdate("AutoSendMail", () => SMOUtilities.AutoSendMail2DayEndBid(), Cron.MinuteInterval(5));
        }
    }
}
