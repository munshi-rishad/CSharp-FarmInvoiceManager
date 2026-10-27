using System;
using System.Data;
using System.Windows.Forms;
using BanglaProject.Data;
using BanglaProject.Helpers;
using BanglaProject.WindowsForm;

namespace BanglaProject
{
    // First window of the application. Admins go to the admin dashboard,
    // employees go to the employee dashboard.
    public partial class FormLogin : BaseForm
    {
        public FormLogin()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();   // passwords never contain spaces

            if (username == "" || password == "")
            {
                Msg.Warning("Please enter your username (or User ID) and password to continue.", "Missing Information");
                return;
            }

            try
            {
                // the user can log in with the username or with the User ID
                DataTable user = Db.GetTable(
                    "SELECT USER_ID, FULL_NAME, ROLE, STATUS FROM UserInfo " +
                    "WHERE (USERNAME = @User OR USER_ID = @User) AND PASSWORD = @Password",
                    Db.P("@User", username), Db.P("@Password", password));

                if (user.Rows.Count == 0)
                {
                    ShowLoginProblem(username);
                    return;
                }

                DataRow row = user.Rows[0];
                string userId = row["USER_ID"].ToString();
                string fullName = row["FULL_NAME"].ToString();
                string role = row["ROLE"].ToString().ToLower();

                if (!(bool)row["STATUS"])
                {
                    Msg.Info("Your account is currently inactive. Please contact the administrator for activation.", "Account Inactive");
                    return;
                }

                Form dashboard;
                if (role == "admin")
                    dashboard = new FormAdminView(fullName, userId, this);
                else if (role == "employee")
                    dashboard = new FormEmployee(fullName, userId, this);
                else
                {
                    Msg.Error("Access denied. Your account role is not recognized. Please contact the administrator.", "Access Denied");
                    return;
                }

                Msg.Info("Welcome! You have logged in as " + (role == "admin" ? "Admin" : "Employee") + " successfully.", "Login Successful");
                txtUsername.Clear();
                txtPassword.Clear();
                GoTo(dashboard);
            }
            catch (Exception ex)
            {
                Msg.Error("Something went wrong while trying to log you in.\n\nPlease try again.\n\nDetails: " + ex.Message, "Login Error");
            }
        }

        // Explains why the login failed: empty database, unknown user or wrong password
        private void ShowLoginProblem(string username)
        {
            int totalUsers = Convert.ToInt32(Db.Scalar("SELECT COUNT(*) FROM UserInfo"));
            if (totalUsers == 0)
            {
                Msg.Error("There are no users in the database (" + Db.ServerAndDatabase() + ").\n\n" +
                          "Run Database/FarmInvoiceManager.sql in SSMS, or check the connection string in App.config.",
                          "Database Is Empty");
                return;
            }

            int found = Convert.ToInt32(Db.Scalar(
                "SELECT COUNT(*) FROM UserInfo WHERE USERNAME = @User OR USER_ID = @User", Db.P("@User", username)));

            if (found == 0)
                Msg.Error("No account was found with this username or User ID.", "Login Failed");
            else
                Msg.Error("The password you entered is incorrect.", "Login Failed");
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtUsername.Clear();
            txtPassword.Clear();
        }

        private void chkPassShow_CheckedChanged(object sender, EventArgs e)
        {
            txtPassword.UseSystemPasswordChar = !chkPassShow.Checked;
        }

        private void lblForget_Click(object sender, EventArgs e)
        {
            GoTo(new FormFindPassword(this));
        }
    }
}
