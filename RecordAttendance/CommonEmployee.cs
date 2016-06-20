using System;
using System.Collections.Generic;
using System.Text;

namespace ICEPDCommonModule
{
    [Serializable]
    public class CommonEmployee
    {        

        private Int32 mEmployeeID;
        public Int32 EmployeeID
        {
            get { return mEmployeeID; }
            set { mEmployeeID = value; }
        }

        private Int32 mDepartmentID;
        public Int32 DepartmentID
        {
            get { return mDepartmentID; }
            set { mDepartmentID = value; }
        }

        private String mEmployeeName;
        public String EmployeeName
        {
            get { return mEmployeeName; }
            set { mEmployeeName = value; }
        }

        private Int32 mCompanyID;
        public Int32 CompanyID
        {
            get { return mCompanyID; }
            set { mCompanyID = value; }
        }

        private String mFatherName;
        public String FatherName
        {
            get { return mFatherName; }
            set { mFatherName = value; }
        }

        private String mContactNo;
        public String ContactNo
        {
            get { return mContactNo; }
            set { mContactNo = value; }
        }

        private String mMobile;
        public String Mobile
        {
            get { return mMobile; }
            set { mMobile = value; }
        }





        private String mAddress;
        public String Address
        {
            get { return mAddress; }
            set { mAddress = value; }
        }

        private DateTime mDOB;
        public DateTime DOB
        {
            get { return mDOB; }
            set { mDOB = value; }
        }

        private Int32 mDesignationID;
        public Int32 DesignationID
        {
            get { return mDesignationID; }
            set { mDesignationID = value; }
        }

        private DateTime mJoiningDate;
        public DateTime JoiningDate
        {
            get { return mJoiningDate; }
            set { mJoiningDate = value; }
        }

        private String mEmployeeLogin;
        public String EmployeeLogin
        {
            get { return mEmployeeLogin; }
            set { mEmployeeLogin = value; }
        }

        private String mDescription;
        public String Description
        {
            get { return mDescription; }
            set { mDescription = value; }
        }

        private Int32 mSDID;
        public Int32 SDID
        {
            get { return mSDID; }
            set { mSDID = value; }
        }

        private DateTime mChangeDate;
        public DateTime ChangeDate
        {
            get { return mChangeDate; }
            set { mChangeDate = value; }
        }

        private DateTime mTimeTo;
        public DateTime TimeTo
        {
            get { return mTimeTo; }
            set { mTimeTo = value; }
        }

        private DateTime mTimeFrom;
        public DateTime TimeFrom
        {
            get { return mTimeFrom; }
            set { mTimeFrom = value; }
        }

        private byte[] mEmpImage;
        public byte[] EmpImage
        {
            get { return mEmpImage; }
            set { mEmpImage = value; }
        }

        private String mEmployeeCode;
        public String EmployeeCode
        {
            get { return mEmployeeCode; }
            set { mEmployeeCode = value; }
        }

        private Boolean mEmployeeStatus;
        public Boolean EmployeeStatus
        {
            get { return mEmployeeStatus; }
            set { mEmployeeStatus = value; }
        }

        private String mCNIC;
        public String CNIC
        {
            get { return mCNIC; }
            set { mCNIC = value; }
        }

        private String mMobileNetwork;
        public String MobileNetwork
        {
            get { return mMobileNetwork; }
            set { mMobileNetwork = value; }
        }

        private byte[] mSignatureImage;
        public byte[] SignatureImage
        {
            get { return mSignatureImage; }
            set { mSignatureImage = value; }
        }
    }
}
