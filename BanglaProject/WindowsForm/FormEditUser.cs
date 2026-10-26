using System;
using System.Data;
using System.Windows.Forms;
using BanglaProject.Data;
using BanglaProject.Helpers;
using BanglaProject.Validation;

namespace BanglaProject
{
    // Admin edits the selected user. The role cannot be changed here.
    public partial class FormEditUser : BaseForm
    {
        private readonly FormAdminView adminView;
        private readonly string userId;

        public FormEditUser()
        {
            InitializeComponent();
        }

        public FormEditUser(string userId, FormAdminView adminView) : this()
        {
            this.userId = userId;
            this.adminView = adminView;
        }

        private void FormEditUser_Load(object sender, EventArgs e)
        {
            cmbRole.Enabled = false;
            LoadUser();
        }

        private void LoadUser()
        {
            try
            {
                DataTable table = Db.GetTable("SELECT * FROM UserInfo WHERE USER_ID = @ID", Db.P("@ID", userId));
                if (table.Rows.Count == 0)
                {
                    Msg.Warning("User not found.", "Not Found");
                    return;
                }

                DataRow user = table.Rows[0];
                txtFullName.Text = user["FULL_NAME"].ToString();
                txtUsername.Text = user["USERNAME"].ToString();
                txtPassword.Text = user["PASSWORD"].ToString();
                txtMobile.Text = user["MOBILE"].ToString();
                txtEmail.Text = user["EMAIL"].ToString();
                txtNid.Text = user["NID"].ToString();
                txtAddress.Text = user["ADDRESS"].ToString();
                cmbRole.SelectedIndex = cmbRole.FindStringExact(user["ROLE"].ToString());
                chkIsActive.Checked = Convert.ToBoolean(user["STATUS"]);
            }
            catch (Exception ex)
            {
                Msg.DatabaseError(ex);
            }
        }

        // live validation while typing
        private void txtFullName_TextChanged(object sender, EventArgs e) => UserValidator.ValidateFullName(txtFullName, lblFullNameError);
        private void txtEmail_TextChanged(object sender, EventArgs e) => UserValidator.ValidateEmail(txtEmail, lblEmailError);
        private void txtUsername_TextChanged(object sender, EventArgs e) => UserValidator.ValidateUsername(txtUsername, lblUsernameError);
        private void txtPassword_TextChanged(object sender, EventArgs e) => UserValidator.ValidatePassword(txtPassword, lblPasswordError);
        private void txtMobile_TextChanged(object sender, EventArgs e) => UserValidator.ValidateMobile(txtMobile, lblMobileError);
        private void txtNid_TextChanged(object sender, EventArgs e) => UserValidator.ValidateNID(txtNid, lblNidError);
        private void txtAddress_TextChanged(object sender, EventArgs e) => UserValidator.ValidateAddress(txtAddress, lblAddressError);

        private bool AllFieldsValid()
        {
            bool ok = true;
            ok &= UserValidator.ValidateFullName(txtFullName, lblFullNameError);
            ok &= UserValidator.ValidateEmail(txtEmail, lblEmailError);
            ok &= UserValidator.ValidateUsername(txtUsername, lblUsernameError);
            ok &= UserValidator.ValidateMobile(txtMobile, lblMobileError);
            ok &= UserValidator.ValidatePassword(txtPassword, lblPasswordError);
            ok &= UserValidator.ValidateNID(txtNid, lblNidError);
            ok &= UserValidator.ValidateAddress(txtAddress, lblAddressError);
            return ok;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!AllFieldsValid())
            {
                Msg.Warning("Please fill all fields correctly before saving.", "Validation Error");
                return;
            }

            try
            {
                string username = txtUsername.Text.Trim();
                object taken = Db.Scalar("SELECT COUNT(*) FROM UserInfo WHERE USERNAME = @U AND USER_ID <> @ID",
                                         Db.P("@U", username), Db.P("@ID", userId));
                if (Convert.ToInt32(taken) > 0)
                {
                    Msg.Warning("This username is already taken. Please choose another one.", "Validation Error");
                    return;
                }

                int result = Db.Execute(
                    "UPDATE UserInfo SET FULL_NAME = @FullName, EMAIL = @Email, USERNAME = @Username, PASSWORD = @Password, " +
                    "MOBILE = @Mobile, NID = @NID, ADDRESS = @Address, STATUS = @Status WHERE USER_ID = @ID",
                    Db.P("@FullName", txtFullName.Text.Trim()),
                    Db.P("@Email", txtEmail.Text.Trim()),
                    Db.P("@Username", username),
                    Db.P("@Password", txtPassword.Text),
                    Db.P("@Mobile", txtMobile.Text.Trim()),
                    Db.P("@NID", txtNid.Text.Trim()),
                    Db.P("@Address", txtAddress.Text.Trim()),
                    Db.P("@Status", chkIsActive.Checked),
                    Db.P("@ID", userId));

                if (result > 0)
                {
                    Msg.Info("User updated successfully!", "Success");
                    adminView.LoadUsers();
                    GoTo(adminView);
                }
                else
                {
                    Msg.Error("Update failed!");
                }
            }
            catch (Exception ex)
            {
                Msg.DatabaseError(ex);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtFullName.Clear();
            txtEmail.Clear();
            txtUsername.Clear();
            txtMobile.Clear();
            txtPassword.Clear();
            txtNid.Clear();
            txtAddress.Clear();
            chkIsActive.Checked = false;
            ValidationLabel.Reset(lblFullNameError, lblEmailError, lblUsernameError, lblMobileError,
                                  lblNidError, lblAddressError, lblPasswordError);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            GoTo(adminView);
        }
    }
}
