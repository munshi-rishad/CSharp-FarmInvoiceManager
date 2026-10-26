using System;
using System.Data;
using System.Windows.Forms;
using BanglaProject.Data;
using BanglaProject.Helpers;
using BanglaProject.Validation;

namespace BanglaProject.WindowsForm
{
    // Lets the logged in admin or employee change their own username and password
    public partial class FormReset : BaseForm
    {
        private readonly Form dashboard;   // admin or employee dashboard to go back to
        private readonly string userId;

        public FormReset()
        {
            InitializeComponent();
            ValidationLabel.Reset(lblNewUsernameError, lblNewPasswordError);
        }

        public FormReset(string userId, Form dashboard) : this()
        {
            this.userId = userId;
            this.dashboard = dashboard;
        }

        private void FormReset_Load(object sender, EventArgs e)
        {
            try
            {
                DataTable user = Db.GetTable("SELECT USERNAME, PASSWORD FROM UserInfo WHERE USER_ID = @ID", Db.P("@ID", userId));

                if (user.Rows.Count == 1)
                {
                    txtCurrentUsername.Text = user.Rows[0]["USERNAME"].ToString();
                    txtCurrentPassword.Text = user.Rows[0]["PASSWORD"].ToString();
                }
                else
                {
                    Msg.Warning("Unable to find the current user information.", "User Not Found");
                }
            }
            catch (Exception ex)
            {
                Msg.DatabaseError(ex);
            }
        }

        private void txtNewUsername_TextChanged(object sender, EventArgs e)
        {
            UserValidator.ValidateUsername(txtNewUsername, lblNewUsernameError);
        }

        private void txtNewPassword_TextChanged(object sender, EventArgs e)
        {
            UserValidator.ValidatePassword(txtNewPassword, lblNewPasswordError);
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (txtCurrentUsername.Text.Trim() == "" || txtCurrentPassword.Text == "")
            {
                Msg.Warning("Please enter your current username and password.", "Missing Information");
                return;
            }

            bool usernameOk = UserValidator.ValidateUsername(txtNewUsername, lblNewUsernameError);
            bool passwordOk = UserValidator.ValidatePassword(txtNewPassword, lblNewPasswordError);
            if (!usernameOk || !passwordOk)
            {
                Msg.Warning("Please fix the new username and password before saving.", "Validation Error");
                return;
            }

            try
            {
                // the current username and password must still be correct
                int count = Db.Execute(
                    "UPDATE UserInfo SET USERNAME = @NewUser, PASSWORD = @NewPassword " +
                    "WHERE USER_ID = @ID AND USERNAME = @OldUser AND PASSWORD = @OldPassword",
                    Db.P("@NewUser", txtNewUsername.Text.Trim()),
                    Db.P("@NewPassword", txtNewPassword.Text),
                    Db.P("@ID", userId),
                    Db.P("@OldUser", txtCurrentUsername.Text.Trim()),
                    Db.P("@OldPassword", txtCurrentPassword.Text));

                if (count == 1)
                {
                    Msg.Info("Your username and password have been updated successfully.", "Update Successful");
                    GoTo(dashboard);
                }
                else
                {
                    Msg.Warning("The current username or password you entered is incorrect.", "Update Failed");
                }
            }
            catch (Exception ex)
            {
                Msg.DatabaseError(ex);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            GoTo(dashboard);
        }
    }
}
