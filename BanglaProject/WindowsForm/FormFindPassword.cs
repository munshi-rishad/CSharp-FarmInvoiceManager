using System;
using System.Data;
using BanglaProject.Data;
using BanglaProject.Helpers;

namespace BanglaProject.WindowsForm
{
    // Shows a forgotten password when User ID, username and mobile number all match
    public partial class FormFindPassword : BaseForm
    {
        private readonly FormLogin loginForm;

        public FormFindPassword()
        {
            InitializeComponent();
            lblSearch.Text = "";
        }

        public FormFindPassword(FormLogin loginForm) : this()
        {
            this.loginForm = loginForm;
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            GoTo(loginForm);
        }

        private void btnSearch_Click_1(object sender, EventArgs e)
        {
            string userId = txtUserId.Text.Trim();
            string username = txtUsername.Text.Trim();
            string mobile = txtMobile.Text.Trim();

            if (userId == "" || username == "" || mobile == "")
            {
                Msg.Warning("Please fill all fields.", "Missing Information");
                return;
            }

            try
            {
                DataTable result = Db.GetTable(
                    "SELECT PASSWORD FROM UserInfo WHERE USER_ID = @ID AND USERNAME = @Username AND MOBILE = @Mobile",
                    Db.P("@ID", userId), Db.P("@Username", username), Db.P("@Mobile", mobile));

                bool found = result.Rows.Count == 1;
                txtPassword.Text = found ? result.Rows[0]["PASSWORD"].ToString() : "";
                ShowFound(lblSearch, found);
            }
            catch (Exception ex)
            {
                Msg.DatabaseError(ex);
            }
        }
    }
}
