using System;
using BanglaProject.Data;
using BanglaProject.Helpers;
using BanglaProject.Validation;

namespace BanglaProject
{
    // Admin adds a new product. The product ID (FIM-001, FIM-002 ...) is made automatically.
    public partial class FormAddProduct : BaseForm
    {
        private readonly FormManageProducts manageProducts;
        private readonly string adminUserId;

        public FormAddProduct()
        {
            InitializeComponent();
        }

        public FormAddProduct(string adminUserId, FormManageProducts manageProducts) : this()
        {
            this.adminUserId = adminUserId;
            this.manageProducts = manageProducts;
        }

        private void FormAddProduct_Load(object sender, EventArgs e)
        {
            ResetErrors();
        }

        private void ResetErrors()
        {
            ValidationLabel.Reset(lblNameError, lblQuantityError, lblCategoryError, lblPriceError, lblDateError, lblStatusError);
        }

        // live validation while typing
        private void txtProductName_TextChanged(object sender, EventArgs e) => ProductValidator.ValidateName(txtProductName, lblNameError);
        private void txtQunatity_TextChanged(object sender, EventArgs e) => ProductValidator.ValidateQuantity(txtQunatity, lblQuantityError);
        private void txtCategory_TextChanged(object sender, EventArgs e) => ProductValidator.ValidateCategory(txtCategory, lblCategoryError);
        private void txtPrice_TextChanged(object sender, EventArgs e) => ProductValidator.ValidatePrice(txtPrice, lblPriceError);
        private void dtpDate_ValueChanged(object sender, EventArgs e) => ProductValidator.ValidateDate(dtpDate, lblDateError);
        private void chkIsActive_CheckedChanged(object sender, EventArgs e) => ProductValidator.ValidateStatus(chkIsActive, lblStatusError);

        private bool AllFieldsValid()
        {
            bool ok = true;
            ok &= ProductValidator.ValidateName(txtProductName, lblNameError);
            ok &= ProductValidator.ValidateQuantity(txtQunatity, lblQuantityError);
            ok &= ProductValidator.ValidateCategory(txtCategory, lblCategoryError);
            ok &= ProductValidator.ValidatePrice(txtPrice, lblPriceError);
            ok &= ProductValidator.ValidateDate(dtpDate, lblDateError);
            ok &= ProductValidator.ValidateStatus(chkIsActive, lblStatusError);
            return ok;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!AllFieldsValid())
            {
                Msg.Warning("Please fix the highlighted errors before saving.", "Validation Error");
                return;
            }

            try
            {
                string productId = Db.NextId("ProductInfo", "PRODUCT_ID", "FIM-");

                Db.Execute(
                    "INSERT INTO ProductInfo (PRODUCT_ID, PRODUCT_NAME, QUANTITY, CATEGORY_NAME, UNITPRICE, CREATED_DATE, IS_AVAILABLE, CREATED_BY_USER_ID) " +
                    "VALUES (@ID, @Name, @Qty, @Category, @Price, @Date, @Available, @CreatedBy)",
                    Db.P("@ID", productId),
                    Db.P("@Name", txtProductName.Text.Trim()),
                    Db.P("@Qty", int.Parse(txtQunatity.Text.Trim())),
                    Db.P("@Category", txtCategory.Text.Trim()),
                    Db.P("@Price", int.Parse(txtPrice.Text.Trim())),
                    Db.P("@Date", dtpDate.Value.Date),
                    Db.P("@Available", chkIsActive.Checked),
                    Db.P("@CreatedBy", adminUserId));   // the admin who is logged in

                Msg.Info("Product saved successfully! Product ID: " + productId, "Success");
                ClearForm();
                manageProducts.LoadProducts();
                GoTo(manageProducts);
            }
            catch (Exception ex)
            {
                Msg.DatabaseError(ex);
            }
        }

        private void ClearForm()
        {
            txtProductName.Clear();
            txtQunatity.Clear();
            txtCategory.Clear();
            txtPrice.Clear();
            dtpDate.Value = DateTime.Today;
            chkIsActive.Checked = false;
            ResetErrors();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void btnCancle_Click(object sender, EventArgs e)
        {
            GoTo(manageProducts);
        }
    }
}
