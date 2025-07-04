using System;
using System.Web;

namespace WorkSpaceManager
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            // Initialize database on application start
            try
            {
                var database = new DatabaseHelper();
                database.InitializeDatabase();
            }
            catch (Exception ex)
            {
                // Log error but don't fail application start
                System.Diagnostics.Debug.WriteLine($"Failed to initialize database: {ex.Message}");
            }
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var exception = Server.GetLastError();
            
            // Log the error
            System.Diagnostics.Debug.WriteLine($"Application Error: {exception?.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack Trace: {exception?.StackTrace}");
            
            // Clear the error
            Server.ClearError();
            
            // Redirect to error page (you can create one if needed)
            // Response.Redirect("~/Error.aspx");
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            // Session initialization code here
        }

        protected void Session_End(object sender, EventArgs e)
        {
            // Session cleanup code here
        }

        protected void Application_End(object sender, EventArgs e)
        {
            // Application cleanup code here
        }
    }
}