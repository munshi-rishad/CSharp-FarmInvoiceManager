using System;
using BanglaProject.Data;
using BanglaProject.Helpers;
using BanglaProject.Validation;

namespace BanglaProject
{
    // Adds a premium customer (ID PC-001, PC-002 ...) from the Add Sales form
    public partial class FormAddCustomer : BaseForm
    {
        private readonly FormAddSales addSales;

        public FormAddCustomer()
        {
            InitializeComponent();
        }

        public FormAddCustomer(FormAddSales addSales) : this()
        {
            this.addSales = addSales;
        }

        private void FormAddCustomer_Load(object sender, EventArgs e)
        {
            ValidationLabel.Reset(lblFullNameError, lblMobileError);
        }

        private void txtCustomerName_TextChanged(object sender, EventArgs e) => UserValidator.ValidateFullName(txtCustomerName, lblFullNameError);
        private void txtMobile_TextChanged(object sender, EventArgs e) => UserValidator.ValidateMobile(txtMobile, lblMobileError);

        private void btnSave_Click(object sender, EventArgs e)
        {
            bool nameOk = UserValidator.ValidateFullName(txtCustomerName, lblFullNameError);
            bool mobileOk = UserValidator.ValidateMobile(txtMobile, lblMobileError);
            if (!nameOk || !mobileOk)
            {
                Msg.Warning("Please fill the customer name and mobile number correctly.", "Validation Error");
                return;
            }

            string name = txtCustomerName.Text.Trim();
            string mobile = txtMobile.Text.Trim();

            try
            {
                object exists = Db.Scalar("SELECT COUNT(*) FROM CustomerInfo WHERE CUSTOMER_MOBILE = @Mobile", Db.P("@Mobile", mobile));
                if (Convert.ToInt32(exists) > 0)
                {
                    Msg.Warning("A customer with this mobile number already exists.", "Duplicate Customer");
                    return;
                }

                string customerId = Db.NextId("CustomerInfo", "CUSTOMER_ID", "PC-");
                Db.Execute("INSERT INTO CustomerInfo (CUSTOMER_ID, CUSTOMER_NAME, CUSTOMER_MOBILE) VALUES (@ID, @Name, @Mobile)",
                           Db.P("@ID", customerId), Db.P("@Name", name), Db.P("@Mobile", mobile));

                Msg.Info("Customer saved! Customer ID: " + customerId, "Success");
                ClearForm();

                // the new customer is selected on the sales form straight away
                addSales.ReloadCustomers(mobile);
                GoTo(addSales);
            }
            catch (Exception ex)
            {
                Msg.DatabaseError(ex);
            }
        }

        private void ClearForm()
        {
            txtCustomerName.Clear();
            txtMobile.Clear();
            ValidationLabel.Reset(lblFullNameError, lblMobileError);
        }

        private void btnClear_Click_1(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            GoTo(addSales);
        }
    }
}
