using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Security.AccessControl;
using System.Web.UI;

namespace WAPP_Asm.Asm_WebPage
{
    public partial class AdminDashboard : System.Web.UI.Page
    {
        private readonly string _connStr =
            ConfigurationManager.ConnectionStrings["KeyCodeDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Session["username"] != null)
                    litWelcomeName.Text = $"{Session["username"]}!";
                else
                    litWelcomeName.Text = "Admin";

                LoadUserCount();
                LoadAppealCounts();
            }
        }

        protected void btnAnalytics_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Asm_WebPage/Analytics.aspx");
        }

        protected void btnReactivationRequest_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Asm_WebPage/ReactivationRequest.aspx");
        }

        protected void btnUserManagement_Click(object sender, EventArgs e)
        {
            Response.Redirect($"~/Asm_WebPage/UserManagement.aspx");
        }

        private void LoadAppealCounts()
        {
            string sql = @"SELECT
                            SUM(CASE WHEN status = 'Pending'  THEN 1 ELSE 0 END) AS PendingCount,
                            SUM(CASE WHEN status = 'Approved' THEN 1 ELSE 0 END) AS ApprovedCount,
                            COUNT(*) AS TotalCount
                           FROM ReactivationRequests";

            SqlConnection con = new SqlConnection(_connStr);
            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(sql, con);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    litPendingCount.Text = reader["PendingCount"].ToString();
                    litApprovedCount.Text = reader["ApprovedCount"].ToString();
                    litTotalAppealsCount.Text = reader["TotalCount"].ToString();
                }
                else
                {
                    //if no rows, set all counts to 0
                    litPendingCount.Text = "0";
                    litApprovedCount.Text = "0";
                    litTotalAppealsCount.Text = "0";
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                litPendingCount.Text = ex.Message;
                litApprovedCount.Text = ex.Message;
                litTotalAppealsCount.Text = ex.Message;
            }
            finally
            {
                con.Close();
            }
        }

        private void LoadUserCount()
        {
            SqlConnection con = new SqlConnection(_connStr);
            try
            {
                con.Open();

                SqlCommand studentCmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE role = 'Student' AND status ='Active'", con);
                litStudentCount.Text = studentCmd.ExecuteScalar().ToString();

                SqlCommand tutorCmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE role = 'Tutor' AND status ='Active'", con);
                litTutorsCount.Text = tutorCmd.ExecuteScalar().ToString();

                SqlCommand totalCmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE role != 'Admin' AND status ='Active'", con);
                litTotalCount.Text = totalCmd.ExecuteScalar().ToString();
            }
            catch (Exception ex)
            {
                litStudentCount.Text = ex.Message;
                litTutorsCount.Text = ex.Message;
                litTotalCount.Text = ex.Message;
            }
            finally
            {
                con.Close();
            }
        }
    }
}