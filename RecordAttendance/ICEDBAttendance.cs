using System;
using System.Collections.Generic;

using System.Text;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Collections;

using RecordAttendance;

namespace ICEPDDatabaseModule
{
    public class ICEDBAttendance
    {
        SqlConnection Con = new SqlConnection();
        public ICEDBAttendance()
        {
            Con.ConnectionString = DBInfo.DBConnectionString;
        }

        public String IsServerUp()
        {
            try
            {
                if (Con.State != ConnectionState.Open)
                    Con.Open();
                Con.Close();
                return "Connected";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private Int32 GenerateInOutID(String EmployeeCode)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = Con;
            cmd.CommandText = "Select IsNull(Max(InOutID),0) + 1 from AttendanceDetails where EmployeeID = (Select EmployeeID from Employee Where EmployeeCode = @EmployeeCode)";
            cmd.Parameters.AddWithValue("@EmployeeCode", (String)EmployeeCode);
            if (Con.State != ConnectionState.Open)
                Con.Open();
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public DataSet RecordAttendance(ICEPDCommonModule.CommonAttendanceDetails objAD)
        {
            try
            {
                SqlCommand cmdRA = new SqlCommand();
                SqlDataAdapter adpRA = new SqlDataAdapter();
                DataSet dsRA = new DataSet();
                #region "Make Return State"
                DataSet dsFinal = new DataSet();
                dsFinal.Tables.Add("ReturnContents");
                dsFinal.Tables["ReturnContents"].Columns.Add("strReturn", Type.GetType("System.String"));
                String blnIsEmpPresent;
                #endregion

                //String strReturn = "";
                //ArrayList strReturn = new ArrayList();
                cmdRA.Connection = Con;
                cmdRA.CommandText = "SELECT EmployeeID, TimeFrom, TimeTo from ShiftDetails where SDID = (Select Max(SDID) from ShiftDetails where EmployeeID = (Select EmployeeID from Employee where EmployeeCode = @EmployeeCode))";
                cmdRA.Parameters.AddWithValue("@EmployeeCode", (String)objAD.EmployeeCode);
                if (Con.State != ConnectionState.Open)
                    Con.Open();
                adpRA.SelectCommand = cmdRA;
                adpRA.Fill(dsRA, "EmployeeDetails");

                if (dsRA.Tables.Count != 0)
                    if (dsRA.Tables["EmployeeDetails"].Rows.Count != 0)
                    {
                        Int32 EmployeeID = 0;
                        EmployeeID = Convert.ToInt32(dsRA.Tables["EmployeeDetails"].Rows[0]["EmployeeID"]);
                        if (EmployeeID != 0)
                        {
                            cmdRA.CommandText = "Select InOutID, TrackDate, InOutTime, InOutStatus from AttendanceDetails where ADID = (Select IsNull(Max(ADID),0) from AttendanceDetails where EmployeeID = @EmployeeID)";
                            cmdRA.Parameters.AddWithValue("@EmployeeID", (Int32)EmployeeID);
                            adpRA.SelectCommand = cmdRA;
                            adpRA.Fill(dsRA, "LastTransaction");

                            if (dsRA.Tables["LastTransaction"].Rows.Count != 0)
                            {
                                if (dsRA.Tables["LastTransaction"].Rows[0]["InOutStatus"].ToString() != "In")
                                {
                                    if (dsRA.Tables["LastTransaction"].Rows[0]["InOutStatus"].ToString() == "Out")
                                    {
                                        DateTime OutDate = Convert.ToDateTime(dsRA.Tables["LastTransaction"].Rows[0]["TrackDate"]);
                                        if (!(OutDate.Date >= objAD.AttendanceDate.Date))
                                        {
                                            if (Convert.ToDateTime(Convert.ToDateTime(dsRA.Tables["LastTransaction"].Rows[0]["InOutTime"]).ToShortTimeString()) >= Convert.ToDateTime(Convert.ToDateTime(dsRA.Tables["EmployeeDetails"].Rows[0]["TimeFrom"]).AddHours(4).ToShortTimeString()))
                                                OutDate = OutDate.AddDays(1);
                                            while (Convert.ToDateTime(OutDate.ToShortDateString()) < Convert.ToDateTime(objAD.AttendanceDate.ToShortDateString()))
                                            {
                                                blnIsEmpPresent = IsEmpPresent(objAD.EmployeeCode, OutDate.Date);
                                                if (blnIsEmpPresent == "")
                                                {
                                                    //if (OutDate.Date < objAD.AttendanceDate.Date)
                                                    //{
                                                    SqlCommand cmdnRA = new SqlCommand();
                                                    cmdnRA.Connection = Con;
                                                    cmdnRA.CommandText = "Insert INTO EmployeeAttendance(AttendanceDate, EmployeeID, AttendanceStatus) Values(@AttendanceDate, @EmployeeID, @AttendanceStatus)";
                                                    cmdnRA.Parameters.AddWithValue("@AttendanceDate", (DateTime)OutDate.Date);
                                                    cmdnRA.Parameters.AddWithValue("@EmployeeID", (Int32)EmployeeID);
                                                    if (IsNormalDay(OutDate))
                                                        cmdnRA.Parameters.AddWithValue("@AttendanceStatus", (String)"A");
                                                    else
                                                        cmdnRA.Parameters.AddWithValue("@AttendanceStatus", (String)"Holiday");
                                                    //cmdnRA.Parameters.AddWithValue("@TimeStatus", (String)"Absent");
                                                    if (Con.State != ConnectionState.Open)
                                                        Con.Open();
                                                    cmdnRA.ExecuteNonQuery();
                                                    //}
                                                }
                                                OutDate = OutDate.AddDays(1);
                                            }
                                            DataTable dtTemp = new DataTable();
                                            dtTemp = GetEmployeeMonthlyAttendanceStatus(objAD.EmployeeCode, OutDate);
                                        }
                                    }

                                    DateTime dtTimeFrom = Convert.ToDateTime(Convert.ToDateTime(dsRA.Tables["EmployeeDetails"].Rows[0]["TimeFrom"]).AddMinutes(30).ToShortTimeString());
                                    blnIsEmpPresent = IsEmpPresent(objAD.EmployeeCode, objAD.AttendanceDate.Date);
                                    if (blnIsEmpPresent == "")
                                    {
                                        cmdRA.CommandText = "Insert INTO EmployeeAttendance(AttendanceDate, EmployeeID, AttendanceStatus, TimeStatus) Values(@AttendanceDate, @EmployeeID, @AttendanceStatus, @TimeStatus) \n";

                                        cmdRA.Parameters.AddWithValue("@AttendanceDate", (DateTime)objAD.AttendanceDate.Date);
                                        //cmdRA.Parameters.AddWithValue("@EmployeeID", (Int32)EmployeeID);
                                        cmdRA.Parameters.AddWithValue("@AttendanceStatus", (String)"P");
                                        if (IsNormalDay(objAD.AttendanceDate) && blnIsEmpPresent != "P" && Convert.ToDateTime(objAD.AttendanceDate.ToShortTimeString()) > dtTimeFrom)
                                            cmdRA.Parameters.AddWithValue("@TimeStatus", (String)"Late");
                                        else if (IsNormalDay(objAD.AttendanceDate.Date))
                                            cmdRA.Parameters.AddWithValue("@TimeStatus", (String)"InTime");
                                        else
                                            cmdRA.Parameters.AddWithValue("@TimeStatus", (String)"OverTime");
                                    }
                                    else if (blnIsEmpPresent != "P")
                                    {
                                        cmdRA.CommandText = "Update EmployeeAttendance Set AttendanceStatus = 'P', TimeStatus = @TimeStatus Where EmployeeID = (Select EmployeeID from Employee Where EmployeeCode = @EmployeeCode) AND AttendanceDate = @Today \n";
                                        cmdRA.Parameters.AddWithValue("@Today", (DateTime)objAD.AttendanceDate.Date);

                                        cmdRA.Parameters.AddWithValue("@AttendanceStatus", (String)"P");
                                        if (IsNormalDay(objAD.AttendanceDate) && Convert.ToDateTime(objAD.AttendanceDate.ToShortTimeString()) > dtTimeFrom)
                                            cmdRA.Parameters.AddWithValue("@TimeStatus", (String)"Late");
                                        else if (IsNormalDay(objAD.AttendanceDate.Date))
                                            cmdRA.Parameters.AddWithValue("@TimeStatus", (String)"InTime");
                                        else
                                            cmdRA.Parameters.AddWithValue("@TimeStatus", (String)"OverTime");
                                    }

                                    cmdRA.CommandText += "Insert INTO AttendanceDetails(InOutID, EmployeeID, TrackDate, InOutTime, InOutStatus) Values(@InOutID, @EmployeeID, @TrackDate, @InOutTime, @InOutStatus)";
                                    cmdRA.Parameters.AddWithValue("@InOutID", (Int32)GenerateInOutID(objAD.EmployeeCode));

                                    cmdRA.Parameters.AddWithValue("@TrackDate", (DateTime)objAD.AttendanceDate.Date);
                                    cmdRA.Parameters.AddWithValue("@InOutTime", Convert.ToDateTime(objAD.InOutTime.ToShortTimeString()));
                                    cmdRA.Parameters.AddWithValue("@InOutStatus", (String)"In");

                                    cmdRA.ExecuteNonQuery();
                                    dsFinal.Tables["ReturnContents"].Rows.Add("In");
                                }
                                else if (dsRA.Tables["LastTransaction"].Rows[0]["InOutStatus"].ToString() == "In")
                                {
                                    DateTime InDate = Convert.ToDateTime(dsRA.Tables["LastTransaction"].Rows[0]["TrackDate"]);
                                    if (!(InDate >= objAD.AttendanceDate))
                                    {
                                        InDate = InDate.AddDays(1);
                                        while (Convert.ToDateTime(InDate.ToShortDateString()) <= Convert.ToDateTime(objAD.AttendanceDate.ToShortDateString()))
                                        {
                                            if (InDate.Date == objAD.AttendanceDate.Date && (Convert.ToDateTime(objAD.InOutTime.ToShortTimeString()) >= Convert.ToDateTime(Convert.ToDateTime(dsRA.Tables["EmployeeDetails"].Rows[0]["TimeFrom"]).AddHours(4).ToShortTimeString())))
                                            {
                                                SqlCommand cmdnRA = new SqlCommand();
                                                cmdnRA.Connection = Con;
                                                cmdnRA.CommandText = "Insert INTO EmployeeAttendance(AttendanceDate, EmployeeID, AttendanceStatus, TimeStatus) Values(@AttendanceDate, @EmployeeID, @AttendanceStatus, @TimeStatus)";
                                                cmdnRA.Parameters.AddWithValue("@AttendanceDate", (DateTime)InDate.Date);
                                                cmdnRA.Parameters.AddWithValue("@EmployeeID", (Int32)EmployeeID);
                                                cmdnRA.Parameters.AddWithValue("@AttendanceStatus", (String)"P");
                                                if (IsNormalDay(InDate.Date))
                                                    cmdnRA.Parameters.AddWithValue("@TimeStatus", (String)"InTime");
                                                else
                                                    cmdnRA.Parameters.AddWithValue("@TimeStatus", (String)"OverTime");
                                                if (Con.State != ConnectionState.Open)
                                                    Con.Open();
                                                cmdnRA.ExecuteNonQuery();

                                            }
                                            if (InDate.Date < objAD.AttendanceDate.Date)
                                            {
                                                SqlCommand cmdnRA = new SqlCommand();
                                                cmdnRA.Connection = Con;
                                                cmdnRA.CommandText = "Insert INTO EmployeeAttendance(AttendanceDate, EmployeeID, AttendanceStatus, TimeStatus) Values(@AttendanceDate, @EmployeeID, @AttendanceStatus, @TimeStatus)";
                                                cmdnRA.Parameters.AddWithValue("@AttendanceDate", (DateTime)InDate.Date);
                                                cmdnRA.Parameters.AddWithValue("@EmployeeID", (Int32)EmployeeID);
                                                cmdnRA.Parameters.AddWithValue("@AttendanceStatus", (String)"P");
                                                if (IsNormalDay(InDate.Date))
                                                    cmdnRA.Parameters.AddWithValue("@TimeStatus", (String)"InTime");
                                                else
                                                    cmdnRA.Parameters.AddWithValue("@TimeStatus", (String)"OverTime");
                                                if (Con.State != ConnectionState.Open)
                                                    Con.Open();
                                                cmdnRA.ExecuteNonQuery();

                                            }
                                            InDate = InDate.AddDays(1);
                                        }
                                    }

                                    cmdRA.CommandText += "Insert INTO AttendanceDetails(InOutID, EmployeeID, TrackDate, InOutTime, InOutStatus) Values(@InOutID, @EmployeeID, @TrackDate, @InOutTime, @InOutStatus)";

                                    cmdRA.Parameters.AddWithValue("@InOutID", Convert.ToInt32(dsRA.Tables["LastTransaction"].Rows[0]["InOutID"]));
                                    //cmdRA.Parameters.AddWithValue("@EmployeeID", (Int32)EmployeeID);
                                    cmdRA.Parameters.AddWithValue("@TrackDate", (DateTime)objAD.AttendanceDate.Date);
                                    cmdRA.Parameters.AddWithValue("@InOutTime", Convert.ToDateTime(objAD.InOutTime.ToShortTimeString()));
                                    cmdRA.Parameters.AddWithValue("@InOutStatus", (String)"Out");
                                    cmdRA.ExecuteNonQuery();
                                    dsFinal.Tables["ReturnContents"].Rows.Add("Out");
                                    ICEDBEmployee objEmp = new ICEDBEmployee();
                                    //strReturn.Add(objEmp.GetBusinessHours(Convert.ToDateTime(dsRA.Tables["EmployeeDetails"].Rows[0]["TimeFrom"]), Convert.ToDateTime(dsRA.Tables["EmployeeDetails"].Rows[0]["TimeTo"]), Convert.ToDateTime(dsRA.Tables["LastTransaction"].Rows[0]["TrackDate"]), objAD.AttendanceDate));
                                    DateTime dtFrom = Convert.ToDateTime(Convert.ToDateTime(dsRA.Tables["LastTransaction"].Rows[0]["TrackDate"]).ToShortDateString() + " " + Convert.ToDateTime(dsRA.Tables["LastTransaction"].Rows[0]["InOutTime"]).ToShortTimeString());
                                    cmdRA.CommandText = "Select DateDiff(Minute, @DateFrom, @DateTo) AS TotalMinuts";
                                    cmdRA.Parameters.AddWithValue("@DateFrom", (DateTime)dtFrom);
                                    cmdRA.Parameters.AddWithValue("@DateTo", (DateTime)objAD.AttendanceDate);
                                    dsFinal.Tables["ReturnContents"].Rows.Add(cmdRA.ExecuteScalar().ToString());
                                }
                            }
                            else
                            {
                                DateTime dtTimeFrom = Convert.ToDateTime(Convert.ToDateTime(dsRA.Tables["EmployeeDetails"].Rows[0]["TimeFrom"]).AddMinutes(30).ToShortTimeString());
                                blnIsEmpPresent = IsEmpPresent(objAD.EmployeeCode, objAD.AttendanceDate.Date);
                                if (blnIsEmpPresent == "")
                                {
                                    cmdRA.CommandText = "Insert INTO EmployeeAttendance(AttendanceDate, EmployeeID, AttendanceStatus, TimeStatus) Values(@AttendanceDate, @EmployeeID, @AttendanceStatus, @TimeStatus) \n";

                                    cmdRA.Parameters.AddWithValue("@AttendanceDate", (DateTime)objAD.AttendanceDate.Date);
                                    //cmdRA.Parameters.AddWithValue("@EmployeeID", (Int32)EmployeeID);
                                    cmdRA.Parameters.AddWithValue("@AttendanceStatus", (String)"P");
                                    if (IsNormalDay(objAD.AttendanceDate) && blnIsEmpPresent != "P" && Convert.ToDateTime(objAD.AttendanceDate.ToShortTimeString()) > dtTimeFrom)
                                        cmdRA.Parameters.AddWithValue("@TimeStatus", (String)"Late");
                                    else if (IsNormalDay(objAD.AttendanceDate.Date))
                                        cmdRA.Parameters.AddWithValue("@TimeStatus", (String)"InTime");
                                    else
                                        cmdRA.Parameters.AddWithValue("@TimeStatus", (String)"OverTime");
                                }
                                else if (blnIsEmpPresent != "P")
                                {
                                    cmdRA.CommandText = "Update EmployeeAttendance Set AttendanceStatus = 'P', TimeStatus = @TimeStatus Where EmployeeID = (Select EmployeeID from Employee Where EmployeeCode = @EmployeeCode) AND AttendanceDate = @Today \n";
                                    cmdRA.Parameters.AddWithValue("@Today", (DateTime)objAD.AttendanceDate.Date);


                                    cmdRA.Parameters.AddWithValue("@AttendanceStatus", (String)"P");
                                    if (IsNormalDay(objAD.AttendanceDate) && Convert.ToDateTime(objAD.AttendanceDate.ToShortTimeString()) > dtTimeFrom)
                                        cmdRA.Parameters.AddWithValue("@TimeStatus", (String)"Late");
                                    else if (IsNormalDay(objAD.AttendanceDate.Date))
                                        cmdRA.Parameters.AddWithValue("@TimeStatus", (String)"InTime");
                                    else
                                        cmdRA.Parameters.AddWithValue("@TimeStatus", (String)"OverTime");
                                }

                                cmdRA.CommandText += "Insert INTO AttendanceDetails(InOutID, EmployeeID, TrackDate, InOutTime, InOutStatus) Values(@InOutID, @EmployeeID, @TrackDate, @InOutTime, @InOutStatus)";
                                cmdRA.Parameters.AddWithValue("@InOutID", (Int32)GenerateInOutID(objAD.EmployeeCode));

                                cmdRA.Parameters.AddWithValue("@TrackDate", (DateTime)objAD.AttendanceDate.Date);
                                cmdRA.Parameters.AddWithValue("@InOutTime", Convert.ToDateTime(objAD.InOutTime.ToShortTimeString()));
                                cmdRA.Parameters.AddWithValue("@InOutStatus", (String)"In");


                                cmdRA.ExecuteNonQuery();
                                dsFinal.Tables["ReturnContents"].Rows.Add("In");
                            }
                        }
                        //----

                        DataTable dtTempED = new DataTable();
                        DataTable EmployeeAttendance = new DataTable();

                        dtTempED = GetEmpAttendanceData(objAD.EmployeeCode);
                        EmployeeAttendance = dtTempED.Copy();
                        dsFinal.Tables.Add(EmployeeAttendance);

                        DataTable dtMDetails = new DataTable();
                        DataTable EmployeeMonthlyDetails = new DataTable();

                        dtMDetails = GetEmployeeMonthlyAttendanceStatus(objAD.EmployeeCode, objAD.AttendanceDate.Date);
                        EmployeeMonthlyDetails = dtMDetails.Copy();
                        dsFinal.Tables.Add(EmployeeMonthlyDetails);

                    }
                Con.Close();
                cmdRA.Dispose();
                return dsFinal;
            }
            catch (Exception ex)
            {
                DataSet dsFinal = new DataSet();
                //ArrayList strReturn = new ArrayList();
                dsFinal.Tables.Add("ReturnContents");
                dsFinal.Tables["ReturnContents"].Rows.Clear();
                dsFinal.Tables["ReturnContents"].Rows.Add(ex.Message);
                return dsFinal;
            }
        }

        private String IsEmpPresent(String EmployeeCode, DateTime TodayDate)
        {
            SqlCommand cmdCommand = new SqlCommand();
            SqlDataAdapter adpAdapter = new SqlDataAdapter();
            DataSet dsDataSet = new DataSet();

            cmdCommand.Connection = Con;
            adpAdapter.SelectCommand = cmdCommand;

            cmdCommand.CommandText = "Select AttendanceStatus from EmployeeAttendance EA INNER JOIN Employee Emp On EA.EmployeeID = Emp.EmployeeID Where Emp.EmployeeCode = @EmployeeCode AND AttendanceDate = @Today";
            cmdCommand.Parameters.AddWithValue("@EmployeeCode", (String)EmployeeCode);
            cmdCommand.Parameters.AddWithValue("@Today", (DateTime)TodayDate.Date);
            if (Con.State != ConnectionState.Open)
                Con.Open();

            return Convert.ToString(cmdCommand.ExecuteScalar());
        }

        public String UpdateServerData(DataSet dsNewRecords)
        {
            SqlCommand cmdCommand = new SqlCommand();

            cmdCommand.Connection = Con;

            if (dsNewRecords.Tables.Count != 0)
            {
                if (dsNewRecords.Tables["EmployeeAttendance"].Rows.Count != 0)
                {
                    SqlTransaction trans;
                    if (Con.State != ConnectionState.Open)
                        Con.Open();
                    trans = Con.BeginTransaction();
                    for (Int32 i = 0; i <= dsNewRecords.Tables["EmployeeAttendance"].Rows.Count - 1; i++)
                    {
                        cmdCommand.CommandText = "Insert INTO EmployeeAttendance(AttendanceDate, EmployeeID, AttendanceStatus, Description) Values(@AttendanceDate, @EmployeeID, @AttendanceStatus, @Description)";
                        cmdCommand.Parameters.AddWithValue("@AttendanceDate", Convert.ToDateTime(dsNewRecords.Tables["EmployeeAttendance"].Rows[i]["AttendanceDate"]).Date);
                        cmdCommand.Parameters.AddWithValue("@EmployeeID", (Int32)dsNewRecords.Tables["EmployeeAttendance"].Rows[i]["EmployeeID"]);
                        cmdCommand.Parameters.AddWithValue("@AttendanceStatus", (String)dsNewRecords.Tables["EmployeeAttendance"].Rows[i]["AttendanceStatus"]);
                        cmdCommand.Parameters.AddWithValue("@Description", (String)dsNewRecords.Tables["EmployeeAttendance"].Rows[i]["Description"]);
                        cmdCommand.ExecuteNonQuery();
                        cmdCommand.Parameters.RemoveAt("@AttendanceDate");
                        cmdCommand.Parameters.RemoveAt("@EmployeeID");
                        cmdCommand.Parameters.RemoveAt("@AttendanceStatus");
                        cmdCommand.Parameters.RemoveAt("@Description");
                    }

                    try
                    {
                        trans.Commit();
                        cmdCommand.Dispose();
                        trans.Dispose();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        return ex.Message;
                    }


                }

                if (dsNewRecords.Tables["AttendanceDetails"].Rows.Count != 0)
                {
                    SqlTransaction trans;
                    if (Con.State != ConnectionState.Open)
                        Con.Open();
                    trans = Con.BeginTransaction();
                    for (Int32 i = 0; i <= dsNewRecords.Tables["AttendanceDetails"].Rows.Count - 1; i++)
                    {
                        cmdCommand.CommandText = "Insert INTO AttendanceDetails(EmployeeID, TrackDate, InOutTime, InOutStatus) Values(@EmployeeID, @TrackDate, @InOutTime, @InOutStatus)";

                        cmdCommand.Parameters.AddWithValue("@EmployeeID", (Int32)dsNewRecords.Tables["AttendanceDetails"].Rows[i]["EmployeeID"]);
                        cmdCommand.Parameters.AddWithValue("@TrackDate", Convert.ToDateTime(dsNewRecords.Tables["AttendanceDetails"].Rows[i]["TrackDate"]).Date);
                        cmdCommand.Parameters.AddWithValue("@InOutTime", Convert.ToDateTime(Convert.ToDateTime(dsNewRecords.Tables["AttendanceDetails"].Rows[i]["InOutTime"]).ToShortTimeString()));
                        cmdCommand.Parameters.AddWithValue("@InOutStatus", (String)dsNewRecords.Tables["AttendanceDetails"].Rows[i]["InOutStatus"]);
                        cmdCommand.ExecuteNonQuery();

                        cmdCommand.Parameters.RemoveAt("@EmployeeID");
                        cmdCommand.Parameters.RemoveAt("@TrackDate");
                        cmdCommand.Parameters.RemoveAt("@InOutTime");
                        cmdCommand.Parameters.RemoveAt("@InOutStatus");
                    }

                    try
                    {
                        trans.Commit();
                        return "Updated Successfully";
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        return ex.Message;
                    }
                }
            }

            return "Updated Successfully";
        }

        public DataSet GetServerAttendanceData()
        {
            SqlCommand cmdCommand = new SqlCommand();
            SqlDataAdapter adpAdapter = new SqlDataAdapter();
            DataSet dsDataSet = new DataSet();

            cmdCommand.Connection = Con;
            adpAdapter.SelectCommand = cmdCommand;

            cmdCommand.CommandText = "Select DesignationID, Designation, Description from Designation";
            if (Con.State != ConnectionState.Open)
                Con.Open();
            adpAdapter.Fill(dsDataSet, "Designation");

            cmdCommand.CommandText = "Select DepartmentID, Department, Description from Department";

            adpAdapter.Fill(dsDataSet, "Department");

            cmdCommand.CommandText = "SELECT EmployeeID, DesignationID, DepartmentID, EmployeeName, FatherName, ContactNo, Address, DOB, JoiningDate, Description, EmployeeLogin, EmployeeImage, EmployeeCode FROM Employee";

            adpAdapter.Fill(dsDataSet, "Designation");

            return dsDataSet;
        }

        public DataTable GetEmpAttendanceData(String EmployeeCode)
        {
            SqlCommand cmdCommand = new SqlCommand();
            SqlDataAdapter adpAdapter = new SqlDataAdapter();
            DataSet dsDataSet = new DataSet();

            cmdCommand.Connection = Con;
            adpAdapter.SelectCommand = cmdCommand;

            cmdCommand.CommandText = "Select EmployeeName, JoiningDate, Des.Designation, EmployeeImage, SD.TimeFrom, SD.TimeTo from Employee Emp INNER JOIN Designation Des ON Emp.DesignationID = Des.DesignationID INNER JOIN ShiftDetails SD ON SD.EmployeeID = Emp.EmployeeID Where SD.SDID = (Select Max(SDID) from ShiftDetails where EmployeeID = (Select EmployeeID from Employee Where EmployeeCode = @EmployeeCode))";
            cmdCommand.Parameters.AddWithValue("@EmployeeCode", (String)EmployeeCode);
            if (Con.State != ConnectionState.Open)
                Con.Open();
            adpAdapter.Fill(dsDataSet, "EmployeeInfo");

            //DataTable dtTemp = new DataTable();
            //DataTable dtNew = new DataTable();

            //dtTemp = GetEmployeeMonthlyAttendanceStatus(EmployeeCode, RequiredDate);
            //dtNew = dtTemp.Copy();

            //dsDataSet.Tables.Add(dtNew);
            return dsDataSet.Tables["EmployeeInfo"];
        }

        //This function is used for Generating data for txt file
        public DataSet GetAttendanceByDate(DateTime StartDate, DateTime EndDate) // Method to get attendance data for exporting.
        {
            SqlCommand cmdCommand = new SqlCommand();
            SqlDataAdapter adpAdapter = new SqlDataAdapter();
            DataSet dsDataSet = new DataSet();

            cmdCommand.Connection = Con;
            adpAdapter.SelectCommand = cmdCommand;

            cmdCommand.CommandText = "Select Row_Number() over(order by TrackDate) , Emp.EmployeeCode, TrackDate, InOutTime, Status = case AD.InOutStatus when 'In' then 'I' when 'Out' then 'O' Else 'Invalid' End from AttendanceDetails AD INNER JOIN Employee Emp ON Emp.EmployeeID = AD.EmployeeID Where TrackDate >= CONVERT(DATETIME, @StartDate, 102) AND TrackDate <= CONVERT(DATETIME, @EndDate, 102)";
            cmdCommand.Parameters.AddWithValue("@StartDate", (DateTime)StartDate.Date);
            cmdCommand.Parameters.AddWithValue("@EndDate", (DateTime)EndDate.Date);
            if (Con.State != ConnectionState.Open)
                Con.Open();
            adpAdapter.Fill(dsDataSet, "EmployeeAttendance");
            return dsDataSet;
        }

        public DataTable GetEmployeeMonthlyAttendanceStatus(String EmployeeCode, DateTime RequiredDate)
        {
            SqlCommand cmdCommand = new SqlCommand();
            SqlDataAdapter adpAdapter = new SqlDataAdapter();
            DataSet dsDataSet = new DataSet();

            cmdCommand.Connection = Con;
            adpAdapter.SelectCommand = cmdCommand;

            cmdCommand.CommandText = "Select EAID, EA.EmployeeID, AttendanceDate, AttendanceStatus from EmployeeAttendance EA INNER JOIN Employee Emp ON EA.EmployeeID = Emp.EmployeeID where EmployeeCode = @EmployeeCode AND Year(AttendanceDate) = @Year AND Month(AttendanceDate) = @Month";

            cmdCommand.Parameters.AddWithValue("@EmployeeCode", (String)EmployeeCode);
            cmdCommand.Parameters.AddWithValue("@Year", (Int32)RequiredDate.Year);
            cmdCommand.Parameters.AddWithValue("@Month", (Int32)RequiredDate.Month);
            if (Con.State != ConnectionState.Open)
                Con.Open();

            adpAdapter.Fill(dsDataSet, "EmployeeAttendance");

            cmdCommand.CommandText = "Select Emp.JoiningDate from Employee Emp where EmployeeCode = @EmployeeCode";
            adpAdapter.Fill(dsDataSet, "EmployeeDetails");

            cmdCommand.CommandText = "Select Count(TimeStatus) AS TotalLate from EmployeeAttendance EA INNER JOIN Employee Emp ON EA.EmployeeID = Emp.EmployeeID Where EA.TimeStatus = 'Late' AND EmployeeCode = @EmployeeCode AND Year(AttendanceDate) = @Year AND Month(AttendanceDate) = @Month";
            int TotalLate = Convert.ToInt16(cmdCommand.ExecuteScalar());

            DateTime RunningDate = new DateTime(RequiredDate.Year, RequiredDate.Month, 1);
            int PresentCount = 0;
            int AbsentCount = 0;
            int SickLeaveCount = 0;
            int CasualLeaveCount = 0;
            int AnnualLeaveCount = 0;

            if (RunningDate.Date.Month < DateTime.Now.Month)
            {
                while (RunningDate.Date.Month == RequiredDate.Date.Month)
                {
                    //Boolean FoundDate = false;
                    if (/* IsNormalDay(RunningDate.Date) && */ RunningDate.Date >= Convert.ToDateTime(dsDataSet.Tables["EmployeeDetails"].Rows[0]["JoiningDate"]).Date)
                    {
                        for (int i = 0; i <= dsDataSet.Tables["EmployeeAttendance"].Rows.Count - 1; i++)
                        {
                            if (Convert.ToDateTime(dsDataSet.Tables["EmployeeAttendance"].Rows[i]["AttendanceDate"]) == RunningDate.Date)
                            {
                                //FoundDate = true;
                                //row = dsDataSet.Tables["EmployeeAttendance"].Rows.Find(dsDataSet.Tables["EmployeeAttendance"].Rows[i]["EAID"].ToString());
                                if (dsDataSet.Tables["EmployeeAttendance"].Rows[i]["AttendanceStatus"].ToString() == "A")
                                    AbsentCount++;
                                else if (dsDataSet.Tables["EmployeeAttendance"].Rows[i]["AttendanceStatus"].ToString() == "P")
                                    PresentCount++;
                                else if (dsDataSet.Tables["EmployeeAttendance"].Rows[i]["AttendanceStatus"].ToString() == "SL")
                                    SickLeaveCount++;
                                else if (dsDataSet.Tables["EmployeeAttendance"].Rows[i]["AttendanceStatus"].ToString() == "AL")
                                    AnnualLeaveCount++;
                                else if (dsDataSet.Tables["EmployeeAttendance"].Rows[i]["AttendanceStatus"].ToString() == "CL")
                                    CasualLeaveCount++;
                                break;
                            }
                        }
                    }
                    RunningDate = RunningDate.AddDays(1);
                }

            }
            else if (RunningDate.Date.Month == DateTime.Now.Month)
            {
                while (RunningDate.Date <= DateTime.Now.Date)
                {
                    //Boolean FoundDate = false;
                    if (/* IsNormalDay(RunningDate.Date) && */ RunningDate.Date >= Convert.ToDateTime(dsDataSet.Tables["EmployeeDetails"].Rows[0]["JoiningDate"]).Date)
                    {
                        for (int i = 0; i <= dsDataSet.Tables["EmployeeAttendance"].Rows.Count - 1; i++)
                        {
                            if (Convert.ToDateTime(dsDataSet.Tables["EmployeeAttendance"].Rows[i]["AttendanceDate"]) == RunningDate.Date)
                            {
                                //FoundDate = true;
                                //row = dsDataSet.Tables["EmployeeAttendance"].Rows.Find(dsDataSet.Tables["EmployeeAttendance"].Rows[i]["EAID"].ToString());
                                if (dsDataSet.Tables["EmployeeAttendance"].Rows[i]["AttendanceStatus"].ToString() == "A")
                                    AbsentCount++;
                                else if (dsDataSet.Tables["EmployeeAttendance"].Rows[i]["AttendanceStatus"].ToString() == "P")
                                    PresentCount++;
                                else if (dsDataSet.Tables["EmployeeAttendance"].Rows[i]["AttendanceStatus"].ToString() == "SL")
                                    SickLeaveCount++;
                                else if (dsDataSet.Tables["EmployeeAttendance"].Rows[i]["AttendanceStatus"].ToString() == "AL")
                                    AnnualLeaveCount++;
                                else if (dsDataSet.Tables["EmployeeAttendance"].Rows[i]["AttendanceStatus"].ToString() == "CL")
                                    CasualLeaveCount++;
                                break;
                            }
                        }
                        //if (FoundDate == false)
                        //{
                        //    AbsentCount++;
                        //}
                    }
                    RunningDate = RunningDate.AddDays(1);
                }
            }

            UpdateMonthlyDetails(EmployeeCode, RequiredDate.Date.Year.ToString(), RequiredDate.Date.Month.ToString(), SickLeaveCount, AnnualLeaveCount, CasualLeaveCount, TotalLate, AbsentCount);

            cmdCommand.CommandText = "Select (Select Sum(SickLeave) from EmployeeMonthlyDetails Where EmployeeID = (Select EmployeeID from Employee Where EmployeeCode = @EmployeeCode) AND MDYear = @Year) AS SickLeave,";
            cmdCommand.CommandText += "(Select Sum(AnnualLeave) from EmployeeMonthlyDetails Where EmployeeID = (Select EmployeeID from Employee Where EmployeeCode = @EmployeeCode) AND MDYear = @Year) AS AnnualLeave,";
            cmdCommand.CommandText += "(Select Sum(CasualLeave) from EmployeeMonthlyDetails Where EmployeeID = (Select EmployeeID from Employee Where EmployeeCode = @EmployeeCode) AND MDYear = @Year) AS CasualLeave,";
            cmdCommand.CommandText += "(Select Sum(Absent) from EmployeeMonthlyDetails Where EmployeeID = (Select EmployeeID from Employee Where EmployeeCode = @EmployeeCode) AND MDYear = @Year) AS Absent,";
            cmdCommand.CommandText += "(Select Sum(Late) from EmployeeMonthlyDetails Where EmployeeID = (Select EmployeeID from Employee Where EmployeeCode = @EmployeeCode) AND MDYear = @Year AND MDMonth = @Month) AS Late";

            if (Con.State != ConnectionState.Open)
                Con.Open();
            adpAdapter.Fill(dsDataSet, "MonthlyDetails");

            dsDataSet.Tables.Add("AttendanceMonthlyStatus");
            dsDataSet.Tables["AttendanceMonthlyStatus"].Columns.Add("TotalPresent", Type.GetType("System.Int32"));
            dsDataSet.Tables["AttendanceMonthlyStatus"].Columns.Add("TotalAbsent", Type.GetType("System.Int32"));
            dsDataSet.Tables["AttendanceMonthlyStatus"].Columns.Add("TotalSickLeave", Type.GetType("System.Int32"));
            dsDataSet.Tables["AttendanceMonthlyStatus"].Columns.Add("TotalAnnualLeave", Type.GetType("System.Int32"));
            dsDataSet.Tables["AttendanceMonthlyStatus"].Columns.Add("TotalCasualLeave", Type.GetType("System.Int32"));
            dsDataSet.Tables["AttendanceMonthlyStatus"].Columns.Add("TotalLate", Type.GetType("System.Int32"));
            dsDataSet.Tables["AttendanceMonthlyStatus"].Rows.Add(PresentCount, Convert.ToInt32(dsDataSet.Tables["MonthlyDetails"].Rows[0]["Absent"]), Convert.ToInt32(dsDataSet.Tables["MonthlyDetails"].Rows[0]["SickLeave"]), Convert.ToInt32(dsDataSet.Tables["MonthlyDetails"].Rows[0]["AnnualLeave"]), Convert.ToInt32(dsDataSet.Tables["MonthlyDetails"].Rows[0]["CasualLeave"]), Convert.ToInt32(dsDataSet.Tables["MonthlyDetails"].Rows[0]["Late"]));
            return dsDataSet.Tables["AttendanceMonthlyStatus"];
            //return dsDataSet.Tables["MonthlyDetails"];
        }

        public Boolean CalculateMonthlyAttendance(DateTime FromMonth, DateTime ToMonth)
        {
            SqlDataAdapter adpEmployees = new SqlDataAdapter();
            SqlCommand cmdEmployees = new SqlCommand();
            DataSet dsEmployees = new DataSet();

            cmdEmployees.Connection = Con;
            adpEmployees.SelectCommand = cmdEmployees;

            cmdEmployees.CommandText = "Select EmployeeCode from Employee Where EmployeeStatus = 1 And EmployeeCode <> 'None'";
            if (Con.State != ConnectionState.Open)
                Con.Open();
            adpEmployees.Fill(dsEmployees, "Employees");

            for (int i = 0; i <= dsEmployees.Tables["Employees"].Rows.Count - 1; i++)
            {
                for (int m = FromMonth.Month; m <= ToMonth.Month; m++)
                {
                    DateTime d = FromMonth.Date;
                    d = new DateTime(d.Year, d.Month, DateTime.DaysInMonth(d.Year, d.Month));
                    DataTable dt = new DataTable();
                    dt = GetEmployeeMonthlyAttendanceStatus(dsEmployees.Tables["Employees"].Rows[i]["EmployeeCode"].ToString(), d);
                }
            }
            return true;
        }

        public Boolean IsNormalDay(DateTime dtDate)
        {
            SqlCommand cmdCommand = new SqlCommand();
            SqlDataAdapter adpAdapter = new SqlDataAdapter();
            DataSet dsDataSet = new DataSet();

            cmdCommand.Connection = Con;
            adpAdapter.SelectCommand = cmdCommand;

            cmdCommand.CommandText = "SELECT WEID, Weekend from Weekend";
            if (Con.State != ConnectionState.Open)
                Con.Open();
            adpAdapter.Fill(dsDataSet, "Weekend");

            cmdCommand.CommandText = "SELECT OfficialStatus From DayDetails Where TodayDate = @Today";
            cmdCommand.Parameters.AddWithValue("@Today", (DateTime)dtDate.Date);
            if (Con.State != ConnectionState.Open)
                Con.Open();
            String strStatus = Convert.ToString(cmdCommand.ExecuteScalar());

            foreach (DataRow row in dsDataSet.Tables["Weekend"].Rows)
            {
                if (dtDate.Date.DayOfWeek.ToString() == row["Weekend"].ToString())
                    return false;
            }

            if (strStatus == "Off")
                return false;
            else
                return true;
        }


        public Boolean UpdateMonthlyDetails(String EmployeeCode, String Year, String Month, Int32 SickLeave, Int32 AnnualLeave, Int32 CasualLeave, Int32 Late, Int32 Absent)
        {
            SqlCommand cmdCommand = new SqlCommand();
            //SqlDataAdapter adpAdapter = new SqlDataAdapter();
            //DataSet dsDataSet = new DataSet();

            cmdCommand.Connection = Con;
            //adpAdapter.SelectCommand = cmdCommand;

            Int32 EmpID = 0;
            cmdCommand.CommandText = "(Select EmployeeID from Employee where EmployeeCode = @EmployeeCode)";
            cmdCommand.Parameters.AddWithValue("@EmployeeCode", (String)EmployeeCode);
            if (Con.State != ConnectionState.Open)
                Con.Open();
            EmpID = Convert.ToInt32(cmdCommand.ExecuteScalar());

            Int32 EMDID = 0;
            cmdCommand.CommandText = "Select EMDID from EmployeeMonthlyDetails where EmployeeID = @EmployeeID AND MDYear = @Year AND MDMonth = @Month";
            cmdCommand.Parameters.AddWithValue("@EmployeeID", (Int32)EmpID);
            cmdCommand.Parameters.AddWithValue("@Month", (String)Month);
            cmdCommand.Parameters.AddWithValue("@Year", (String)Year);
            EMDID = Convert.ToInt32(cmdCommand.ExecuteScalar());

            if (EMDID == 0)
            {
                cmdCommand.CommandText = "Insert INTO EmployeeMonthlyDetails(EmployeeID, MDYear, MDMonth, SickLeave, AnnualLeave, CasualLeave, Late, Absent) Values(@EmployeeID, @Year, @Month, @SickLeave, @AnnualLeave, @CasualLeave, @Late, @Absent)";
                cmdCommand.Parameters.AddWithValue("@SickLeave", (Int32)SickLeave);
                cmdCommand.Parameters.AddWithValue("@AnnualLeave", (Int32)AnnualLeave);
                cmdCommand.Parameters.AddWithValue("@CasualLeave", (Int32)CasualLeave);
                cmdCommand.Parameters.AddWithValue("@Late", (Int32)Late);
                cmdCommand.Parameters.AddWithValue("@Absent", (Int32)Absent);
            }
            else
            {
                cmdCommand.CommandText = "Update EmployeeMonthlyDetails Set SickLeave = @SickLeave, AnnualLeave = @AnnualLeave, CasualLeave = @CasualLeave, Late = @Late, Absent = @Absent Where EmployeeID = @EmployeeID AND MDYear = @Year AND MDMonth = @Month";
                cmdCommand.Parameters.AddWithValue("@SickLeave", (Int32)SickLeave);
                cmdCommand.Parameters.AddWithValue("@AnnualLeave", (Int32)AnnualLeave);
                cmdCommand.Parameters.AddWithValue("@CasualLeave", (Int32)CasualLeave);
                cmdCommand.Parameters.AddWithValue("@Late", (Int32)Late);
                cmdCommand.Parameters.AddWithValue("@Absent", (Int32)Absent);
            }

            cmdCommand.ExecuteNonQuery();
            return true;
        }

        public DataSet GetMonthlyAttendance(DateTime dtForDate)
        {
            SqlCommand cmdCommand = new SqlCommand();
            SqlDataAdapter adpAdapter = new SqlDataAdapter();
            DataSet dsDataSet = new DataSet();

            cmdCommand.Connection = Con;
            adpAdapter.SelectCommand = cmdCommand;

            cmdCommand.CommandText = "SELECT Att_Merge.EmployeeCode, Att_Merge.EmployeeName, Att_Merge.Designation, Att_Merge.Week_Day,";
            cmdCommand.CommandText += "Att_Merge.AttendanceDate, Att_Merge.InTime, Att_Merge.OutTime, DATEDIFF(Minute, Att_Merge.InTime, ";
            cmdCommand.CommandText += "Att_Merge.OutTime) AS BusinessMinutes, Att_Merge.Remarks, Att_Merge.SickLeave, Att_Merge.AnnualLeave, ";
            cmdCommand.CommandText += "Att_Merge.CasualLeave, Att_Merge.Absent, Att_Merge.Late, Att_Merge.LastOutDate, Att_Merge.LastOutTime, TimeStatus, Att_Merge.Description ";
            cmdCommand.CommandText += "FROM Att_Merge WHERE (YEAR(Att_Merge.AttendanceDate) = @Year) ";
            cmdCommand.CommandText += "AND (MONTH(Att_Merge.AttendanceDate) = @Month)";
            cmdCommand.Parameters.AddWithValue("@Year", (Int32)dtForDate.Date.Year);
            cmdCommand.Parameters.AddWithValue("@Month", (Int32)dtForDate.Date.Month);
            if (Con.State != ConnectionState.Open)
                Con.Open();
            adpAdapter.Fill(dsDataSet);

            return dsDataSet;
        }

        public DataSet GetMonthlyAttendanceByRange(DateTime dtFromDate, DateTime dtToDate)
        {
            SqlCommand cmdCommand = new SqlCommand();
            SqlDataAdapter adpAdapter = new SqlDataAdapter();
            DataSet dsDataSet = new DataSet();

            cmdCommand.Connection = Con;
            cmdCommand.CommandTimeout = 300;
            adpAdapter.SelectCommand = cmdCommand;

            cmdCommand.CommandText = "SELECT EmployeeCode, EmployeeName, Designation, Week_Day,";
            cmdCommand.CommandText += "AttendanceDate, InTime, OutTime, DATEDIFF(Minute, InTime, ";
            cmdCommand.CommandText += "OutTime) AS BusinessMinutes, Remarks, SickLeave, AnnualLeave, ";
            cmdCommand.CommandText += "CasualLeave, Absent, Late, LastOutDate, LastOutTime, TimeStatus, Description ";
            cmdCommand.CommandText += "FROM Att_Merge WHERE (AttendanceDate >= @FromDate) ";
            cmdCommand.CommandText += "AND (AttendanceDate <= @ToDate)";
            //cmdCommand.CommandText += " AND (EmployeeCode = '9600021') AND (Remarks <> 'P') AND (Remarks <> 'Holiday')";
            cmdCommand.Parameters.AddWithValue("@FromDate", (DateTime)dtFromDate.Date);
            cmdCommand.Parameters.AddWithValue("@ToDate", (DateTime)dtToDate.Date);
            if (Con.State != ConnectionState.Open)
                Con.Open();
            adpAdapter.Fill(dsDataSet);

            return dsDataSet;
        }

        public DataSet GetEmployeeMonthlyAttendance(String EmployeeCode, DateTime dtForDate)
        {
            SqlCommand cmdCommand = new SqlCommand();
            SqlDataAdapter adpAdapter = new SqlDataAdapter();
            DataSet dsDataSet = new DataSet();

            cmdCommand.Connection = Con;
            adpAdapter.SelectCommand = cmdCommand;

            cmdCommand.CommandText = "SELECT Att_Merge.EmployeeCode, Att_Merge.EmployeeName, Att_Merge.Designation, Att_Merge.Week_Day,";
            cmdCommand.CommandText += "Att_Merge.AttendanceDate, Att_Merge.InTime, Att_Merge.OutTime, DATEDIFF(Minute, Att_Merge.InTime, ";
            cmdCommand.CommandText += "Att_Merge.OutTime) AS BusinessMinutes, Att_Merge.Remarks, Att_Merge.SickLeave, Att_Merge.AnnualLeave, ";
            cmdCommand.CommandText += "Att_Merge.CasualLeave, Att_Merge.Absent, Att_Merge.Late, Att_Merge.LastOutDate, Att_Merge.LastOutTime, TimeStatus, Description ";
            cmdCommand.CommandText += "FROM Att_Merge WHERE (YEAR(Att_Merge.AttendanceDate) = @Year) AND (MONTH(Att_Merge.AttendanceDate) = @Month) AND Att_Merge.EmployeeCode = @EmployeeCode";

            cmdCommand.Parameters.AddWithValue("@Year", (Int32)dtForDate.Date.Year);
            cmdCommand.Parameters.AddWithValue("@Month", (Int32)dtForDate.Date.Month);
            cmdCommand.Parameters.AddWithValue("@EmployeeCode", (String)EmployeeCode);

            if (Con.State != ConnectionState.Open)
                Con.Open();
            adpAdapter.Fill(dsDataSet);

            return dsDataSet;
        }

        public DataSet GetSelectedEmployeeMonthlyAttendance(ArrayList arrEmployeeID, DateTime dtForDate)
        {
            SqlCommand cmdCommand = new SqlCommand();
            SqlDataAdapter adpAdapter = new SqlDataAdapter();
            DataSet dsDataSet = new DataSet();

            cmdCommand.Connection = Con;
            adpAdapter.SelectCommand = cmdCommand;

            String EmpIDs = String.Join(",", (String[])arrEmployeeID.ToArray(Type.GetType("System.String")));
            cmdCommand.CommandText = "SELECT Att_Merge.EmployeeCode, Att_Merge.EmployeeName, Att_Merge.Designation, Att_Merge.Week_Day,";
            cmdCommand.CommandText += "Att_Merge.AttendanceDate, Att_Merge.InTime, Att_Merge.OutTime, DATEDIFF(Minute, Att_Merge.InTime, ";
            cmdCommand.CommandText += "Att_Merge.OutTime) AS BusinessMinutes, Att_Merge.Remarks, Att_Merge.SickLeave, Att_Merge.AnnualLeave, ";
            cmdCommand.CommandText += "Att_Merge.CasualLeave, Att_Merge.Absent, Att_Merge.Late, Att_Merge.LastOutDate, Att_Merge.LastOutTime, TimeStatus, Description ";
            cmdCommand.CommandText += "FROM Att_Merge WHERE (YEAR(Att_Merge.AttendanceDate) = @Year) AND (MONTH(Att_Merge.AttendanceDate) = @Month) AND (Att_Merge.EmployeeID IN (" + EmpIDs + "))";

            cmdCommand.Parameters.AddWithValue("@Year", (Int32)dtForDate.Date.Year);
            cmdCommand.Parameters.AddWithValue("@Month", (Int32)dtForDate.Date.Month);
            //cmdCommand.Parameters.AddWithValue("@EmployeeIDs", "67,131");
            //cmdCommand.Parameters.AddWithValue("@EmployeeIDs", (String)String.Join(",", (String[])arrEmployeeID.ToArray(Type.GetType("System.String"))));

            if (Con.State != ConnectionState.Open)
                Con.Open();
            adpAdapter.Fill(dsDataSet);

            return dsDataSet;
        }

        public DataSet GetEmployeeAttendanceByRange(String EmployeeCode, DateTime dtFromDate, DateTime dtToDate)
        {
            SqlCommand cmdCommand = new SqlCommand();
            SqlDataAdapter adpAdapter = new SqlDataAdapter();
            DataSet dsDataSet = new DataSet();

            
            cmdCommand.Connection = Con;
            adpAdapter.SelectCommand = cmdCommand;

            cmdCommand.CommandText = "Select EmployeeID from Employee where EmployeeCode = @EmployeeCode";
            cmdCommand.Parameters.AddWithValue("@EmployeeCode", (String)EmployeeCode);

            if (Con.State != ConnectionState.Open)
                Con.Open();
            Int32 EmpID = Convert.ToInt32(cmdCommand.ExecuteScalar());

            cmdCommand.CommandText = "SELECT EmployeeCode, EmployeeName, Designation, Week_Day,";
            cmdCommand.CommandText += "AttendanceDate, InTime, OutTime, DATEDIFF(Minute, InTime, ";
            cmdCommand.CommandText += "OutTime) AS BusinessMinutes, Remarks, SickLeave, AnnualLeave, ";
            cmdCommand.CommandText += "CasualLeave, Absent, Late, LastOutDate, LastOutTime, TimeStatus, Description ";
            cmdCommand.CommandText += "FROM Att_Merge WHERE (AttendanceDate >= @FromDate) AND (AttendanceDate <= @ToDate) AND (EmployeeID = @EmpID)";

            cmdCommand.Parameters.AddWithValue("@FromDate", (DateTime)dtFromDate.Date);
            cmdCommand.Parameters.AddWithValue("@ToDate", (DateTime)dtToDate.Date);
            cmdCommand.Parameters.AddWithValue("@EmpID", (Int32)EmpID);
            

            adpAdapter.Fill(dsDataSet, "dtMonthlyAttendance");

            return dsDataSet;
        }

        public DataSet GetSelectedEmployeeAttendanceByRange(ArrayList arrSelectedEmployeeIDs, DateTime dtFromDate, DateTime dtToDate)
        {
            SqlCommand cmdCommand = new SqlCommand();
            SqlDataAdapter adpAdapter = new SqlDataAdapter();
            DataSet dsDataSet = new DataSet();


            cmdCommand.Connection = Con;
            adpAdapter.SelectCommand = cmdCommand;

            String EmpIDs = String.Join(",", (String[])arrSelectedEmployeeIDs.ToArray(Type.GetType("System.String")));
            cmdCommand.CommandText = "SELECT EmployeeCode, EmployeeName, Designation, Week_Day,";
            cmdCommand.CommandText += "AttendanceDate, InTime, OutTime, DATEDIFF(Minute, InTime, ";
            cmdCommand.CommandText += "OutTime) AS BusinessMinutes, Remarks, SickLeave, AnnualLeave, ";
            cmdCommand.CommandText += "CasualLeave, Absent, Late, LastOutDate, LastOutTime, TimeStatus, Description ";
            cmdCommand.CommandText += "FROM Att_Merge WHERE (AttendanceDate >= @FromDate) AND (AttendanceDate <= @ToDate) AND (EmployeeID IN (" + EmpIDs + "))";

            cmdCommand.Parameters.AddWithValue("@FromDate", (DateTime)dtFromDate.Date);
            cmdCommand.Parameters.AddWithValue("@ToDate", (DateTime)dtToDate.Date);


            if (Con.State != ConnectionState.Open)
                Con.Open();
            adpAdapter.Fill(dsDataSet, "dtMonthlyAttendance");

            return dsDataSet;
        }

        public DataSet GetMonthlyLeaves(DateTime dtForDate)
        {
            SqlCommand cmdCommand = new SqlCommand();
            SqlDataAdapter adpAdapter = new SqlDataAdapter();
            DataSet dsDataSet = new DataSet();

            cmdCommand.Connection = Con;
            adpAdapter.SelectCommand = cmdCommand;

            cmdCommand.CommandText = "SELECT Att_Merge.EmployeeCode, Att_Merge.EmployeeName, Att_Merge.Designation, Att_Merge.Week_Day,";
            cmdCommand.CommandText += "Att_Merge.AttendanceDate, Att_Merge.InTime, Att_Merge.OutTime, DATEDIFF(Minute, Att_Merge.InTime, ";
            cmdCommand.CommandText += "Att_Merge.OutTime) AS BusinessMinutes, Att_Merge.Remarks, Att_Merge.SickLeave, Att_Merge.AnnualLeave, ";
            cmdCommand.CommandText += "Att_Merge.CasualLeave, Att_Merge.Absent, Att_Merge.Late, Att_Merge.LastOutDate, Att_Merge.LastOutTime, TimeStatus, Description ";
            cmdCommand.CommandText += "FROM Att_Merge WHERE (YEAR(Att_Merge.AttendanceDate) = @Year) ";
            cmdCommand.CommandText += "AND (MONTH(Att_Merge.AttendanceDate) = @Month) AND Remarks Not IN ('P','Holiday') OR ((YEAR(Att_Merge.AttendanceDate) = @Year) ";
            cmdCommand.CommandText += "AND (MONTH(Att_Merge.AttendanceDate) = @Month) AND (Remarks = 'P') AND (TimeStatus = 'Late'))";
            cmdCommand.Parameters.AddWithValue("@Year", (Int32)dtForDate.Date.Year);
            cmdCommand.Parameters.AddWithValue("@Month", (Int32)dtForDate.Date.Month);
            if (Con.State != ConnectionState.Open)
                Con.Open();
            adpAdapter.Fill(dsDataSet, "dtMonthlyAttendance");

            return dsDataSet;
        }

        public DataSet GetLeavesByRange(DateTime dtFromDate, DateTime dtToDate)
        {
            SqlCommand cmdCommand = new SqlCommand();
            SqlDataAdapter adpAdapter = new SqlDataAdapter();
            DataSet dsDataSet = new DataSet();

            cmdCommand.Connection = Con;
            adpAdapter.SelectCommand = cmdCommand;

            cmdCommand.CommandText = "SELECT Att_Merge.EmployeeCode, Att_Merge.EmployeeName, Att_Merge.Designation, Att_Merge.Week_Day,";
            cmdCommand.CommandText += "Att_Merge.AttendanceDate, Att_Merge.InTime, Att_Merge.OutTime, DATEDIFF(Minute, Att_Merge.InTime, ";
            cmdCommand.CommandText += "Att_Merge.OutTime) AS BusinessMinutes, Att_Merge.Remarks, Att_Merge.SickLeave, Att_Merge.AnnualLeave, ";
            cmdCommand.CommandText += "Att_Merge.CasualLeave, Att_Merge.Absent, Att_Merge.Late, Att_Merge.LastOutDate, Att_Merge.LastOutTime, TimeStatus, Description ";
            cmdCommand.CommandText += "FROM Att_Merge WHERE (Att_Merge.AttendanceDate >= @FromDate) ";
            cmdCommand.CommandText += "AND (Att_Merge.AttendanceDate <= @ToDate) AND Remarks Not IN ('P','Holiday') OR ((Att_Merge.AttendanceDate >= @FromDate) ";
            cmdCommand.CommandText += "AND (Att_Merge.AttendanceDate <= @ToDate) AND (Remarks = 'P') AND (TimeStatus = 'Late'))";
            //cmdCommand.CommandText += " AND (Att_Merge.EmployeeCode = '9600021') AND (Att_Merge.Remarks <> 'P') AND (Att_Merge.Remarks <> 'Holiday')";
            cmdCommand.Parameters.AddWithValue("@FromDate", (DateTime)dtFromDate.Date);
            cmdCommand.Parameters.AddWithValue("@ToDate", (DateTime)dtToDate.Date);
            if (Con.State != ConnectionState.Open)
                Con.Open();
            adpAdapter.Fill(dsDataSet);

            return dsDataSet;
        }

        public DataSet GetEmployeeMonthlyLeaves(String EmployeeCode, DateTime dtForDate)
        {
            SqlCommand cmdCommand = new SqlCommand();
            SqlDataAdapter adpAdapter = new SqlDataAdapter();
            DataSet dsDataSet = new DataSet();

            cmdCommand.Connection = Con;
            adpAdapter.SelectCommand = cmdCommand;

            cmdCommand.CommandText = "SELECT Att_Merge.EmployeeCode, Att_Merge.EmployeeName, Att_Merge.Designation, Att_Merge.Week_Day,";
            cmdCommand.CommandText += "Att_Merge.AttendanceDate, Att_Merge.InTime, Att_Merge.OutTime, DATEDIFF(Minute, Att_Merge.InTime, ";
            cmdCommand.CommandText += "Att_Merge.OutTime) AS BusinessMinutes, Att_Merge.Remarks, Att_Merge.SickLeave, Att_Merge.AnnualLeave, ";
            cmdCommand.CommandText += "Att_Merge.CasualLeave, Att_Merge.Absent, Att_Merge.Late, Att_Merge.LastOutDate, Att_Merge.LastOutTime, TimeStatus, Description ";
            cmdCommand.CommandText += "FROM Att_Merge WHERE (YEAR(Att_Merge.AttendanceDate) = @Year) AND (MONTH(Att_Merge.AttendanceDate) = @Month) AND Att_Merge.EmployeeCode = @EmployeeCode AND Remarks Not IN ('P','Holiday') OR ((YEAR(Att_Merge.AttendanceDate) = @Year) ";
            cmdCommand.CommandText += "AND (MONTH(Att_Merge.AttendanceDate) = @Month) AND Att_Merge.EmployeeCode = @EmployeeCode AND (Remarks = 'P') AND (TimeStatus = 'Late'))";

            cmdCommand.Parameters.AddWithValue("@Year", (Int32)dtForDate.Date.Year);
            cmdCommand.Parameters.AddWithValue("@Month", (Int32)dtForDate.Date.Month);
            cmdCommand.Parameters.AddWithValue("@EmployeeCode", (String)EmployeeCode);

            if (Con.State != ConnectionState.Open)
                Con.Open();
            adpAdapter.Fill(dsDataSet);

            return dsDataSet;
        }

        public DataSet GetEmployeeLeavesByRange(String EmployeeCode, DateTime dtFromDate, DateTime dtToDate)
        {
            SqlCommand cmdCommand = new SqlCommand();
            SqlDataAdapter adpAdapter = new SqlDataAdapter();
            DataSet dsDataSet = new DataSet();

            cmdCommand.Connection = Con;
            adpAdapter.SelectCommand = cmdCommand;

            cmdCommand.CommandText = "SELECT Att_Merge.EmployeeCode, Att_Merge.EmployeeName, Att_Merge.Designation, Att_Merge.Week_Day,";
            cmdCommand.CommandText += "Att_Merge.AttendanceDate, Att_Merge.InTime, Att_Merge.OutTime, DATEDIFF(Minute, Att_Merge.InTime, ";
            cmdCommand.CommandText += "Att_Merge.OutTime) AS BusinessMinutes, Att_Merge.Remarks, Att_Merge.SickLeave, Att_Merge.AnnualLeave, ";
            cmdCommand.CommandText += "Att_Merge.CasualLeave, Att_Merge.Absent, Att_Merge.Late, Att_Merge.LastOutDate, Att_Merge.LastOutTime, TimeStatus, Description ";
            cmdCommand.CommandText += "FROM Att_Merge WHERE (Att_Merge.AttendanceDate >= @FromDate) AND (Att_Merge.AttendanceDate <= @ToDate) AND ";
            cmdCommand.CommandText += "Att_Merge.EmployeeCode = @EmployeeCode AND Remarks Not IN ('P','Holiday') OR ((Att_Merge.AttendanceDate >= @FromDate) ";
            cmdCommand.CommandText += "AND (Att_Merge.AttendanceDate <= @ToDate) AND Att_Merge.EmployeeCode = @EmployeeCode AND (Remarks = 'P') AND (TimeStatus = 'Late'))";

            cmdCommand.Parameters.AddWithValue("@FromDate", (DateTime)dtFromDate.Date);
            cmdCommand.Parameters.AddWithValue("@ToDate", (DateTime)dtToDate.Date);
            cmdCommand.Parameters.AddWithValue("@EmployeeCode", (String)EmployeeCode);

            if (Con.State != ConnectionState.Open)
                Con.Open();
            adpAdapter.Fill(dsDataSet);

            return dsDataSet;
        }

        //Select all the official leaves for specific year from daydetails table
        public DataSet GetAttendanceLeaves(Int32 Year)
        {
            SqlCommand cmdCommand = new SqlCommand();
            SqlDataAdapter adpAdapter = new SqlDataAdapter();
            DataSet dsDataSet = new DataSet();

            cmdCommand.Connection = Con;
            adpAdapter.SelectCommand = cmdCommand;

            cmdCommand.CommandText = "Select DDID, TodayDate, Description, OfficialStatus from DayDetails where Year(TodayDate) = @Year Order By TodayDate";

            cmdCommand.Parameters.AddWithValue("@Year", (Int32)Year);
            //cmdCommand.Parameters.AddWithValue("@Month", (Int32)dtForDate.Date.Month);
            if (Con.State != ConnectionState.Open)
                Con.Open();
            adpAdapter.Fill(dsDataSet, "DayDetails");

            cmdCommand.CommandText = "Select WEID, Weekend from Weekend";
            adpAdapter.Fill(dsDataSet, "Weekend");

            return dsDataSet;
        }

        public DataSet GetEmployeeAttendanceLeaves(Int32 EmployeeID, Int32 Year)
        {
            SqlCommand cmdCommand = new SqlCommand();
            SqlDataAdapter adpAdapter = new SqlDataAdapter();
            DataSet dsDataSet = new DataSet();

            cmdCommand.Connection = Con;
            adpAdapter.SelectCommand = cmdCommand;

            cmdCommand.CommandText = "Select AttendanceDate, Description, AttendanceStatus from EmployeeAttendance where Year(AttendanceDate) = @Year AND EmployeeID = @EmployeeID AND AttendanceStatus = 'A' Order By AttendanceDate";

            cmdCommand.Parameters.AddWithValue("@EmployeeID", (Int32)EmployeeID);
            cmdCommand.Parameters.AddWithValue("@Year", (Int32)Year);
            //cmdCommand.Parameters.AddWithValue("@Month", (Int32)dtForDate.Date.Month);
            if (Con.State != ConnectionState.Open)
                Con.Open();
            adpAdapter.Fill(dsDataSet, "Absents");

            cmdCommand.CommandText = "Select AttendanceDate, Description, AttendanceStatus from EmployeeAttendance where Year(AttendanceDate) = @Year AND EmployeeID = @EmployeeID AND AttendanceStatus = 'SL' Order By AttendanceDate";
            adpAdapter.Fill(dsDataSet, "SickLeaves");

            cmdCommand.CommandText = "Select AttendanceDate, Description, AttendanceStatus from EmployeeAttendance where Year(AttendanceDate) = @Year AND EmployeeID = @EmployeeID AND AttendanceStatus = 'AL' Order By AttendanceDate";
            adpAdapter.Fill(dsDataSet, "AnnualLeaves");

            cmdCommand.CommandText = "Select AttendanceDate, Description, AttendanceStatus from EmployeeAttendance where Year(AttendanceDate) = @Year AND EmployeeID = @EmployeeID AND AttendanceStatus = 'CL' Order By AttendanceDate";
            adpAdapter.Fill(dsDataSet, "CasualLeaves");

            cmdCommand.CommandText = "Select AttendanceDate, Description, AttendanceStatus from EmployeeAttendance where Year(AttendanceDate) = @Year AND EmployeeID = @EmployeeID AND AttendanceStatus = 'C' Order By AttendanceDate";
            adpAdapter.Fill(dsDataSet, "Compensation");

            cmdCommand.CommandText = "Select AttendanceDate, Description, AttendanceStatus from EmployeeAttendance where Year(AttendanceDate) = @Year AND EmployeeID = @EmployeeID AND AttendanceStatus Not IN('C', 'A', 'SL', 'AL', 'CL', 'Holiday', 'P') Order By AttendanceDate";
            adpAdapter.Fill(dsDataSet, "OtherLeaves");


            cmdCommand.CommandText = "SELECT  sum(AnnualLeave) as Total FROM [Ice_Project_Directory].[dbo].[EmployeeMonthlyDetails] where EmployeeID=@EmployeeID AND MDYear >=2012  AND MDYear=(@Year-1) Group By EmployeeID ";
            adpAdapter.Fill(dsDataSet, "AL-Total");





            return dsDataSet;
        }

        public DataSet LoadEmployeeAbsents(Int32 EmployeeID, Int32 Year)
        {
            SqlCommand cmdCommand = new SqlCommand();
            SqlDataAdapter adpAdapter = new SqlDataAdapter();
            DataSet dsDataSet = new DataSet();
            

            cmdCommand.Connection = Con;
            adpAdapter.SelectCommand = cmdCommand;

            cmdCommand.CommandText = "Select AttendanceDate, Description, AttendanceStatus from EmployeeAttendance where Year(AttendanceDate) = @Year AND EmployeeID = @EmployeeID AND AttendanceStatus = 'A' AND (ApprovalStatus = 0 OR ApprovalStatus IS NULL) AND EAID Not IN(Select LR.EAID from LeaveRequest LR INNER JOIN EmployeeAttendance EA ON EA.EAID = LR.EAID Where Year(EA.AttendanceDate) = @Year AND EA.EmployeeID = @EmployeeID AND EA.AttendanceStatus = 'A' AND (EA.ApprovalStatus = 0 OR EA.ApprovalStatus IS NULL) AND (LR.ApprovalStatus = 1 OR LR.ApprovalStatus IS NULL)) Order By AttendanceDate";

            cmdCommand.Parameters.AddWithValue("@EmployeeID", (Int32)EmployeeID);
            cmdCommand.Parameters.AddWithValue("@Year", (Int32)Year);
            //cmdCommand.Parameters.AddWithValue("@Month", (Int32)dtForDate.Date.Month);            
            if (Con.State != ConnectionState.Open)
                Con.Open();
            adpAdapter.Fill(dsDataSet, "Absents");

            cmdCommand.CommandText = "Select LeaveDate, LR.Description, RequestedStatus from LeaveRequest LR where Year(LeaveDate) = @Year AND LR.EmployeeID = @EmployeeID AND (LR.ApprovalStatus IS NULL) Order By LeaveDate";

            //cmdCommand.Parameters.AddWithValue("@EmployeeID", (Int32)EmployeeID);
            //cmdCommand.Parameters.AddWithValue("@Year", (Int32)Year);
            //cmdCommand.Parameters.AddWithValue("@Month", (Int32)dtForDate.Date.Month);            
            if (Con.State != ConnectionState.Open)
                Con.Open();
            adpAdapter.Fill(dsDataSet, "RequestedAbsents");
            return dsDataSet;
        }

        //public Boolean EmployeeAbsentsExists(String Login, Int32 Year)
        //{
        //    ICEDBEmployee objDBEmployee = new ICEDBEmployee();

        //    Int32 EmployeeID = Commons.GetEmployeeID(Login);
        //    SqlCommand cmdCommand = new SqlCommand();
        //    SqlDataAdapter adpAdapter = new SqlDataAdapter();
        //    DataSet dsDataSet = new DataSet();


        //    cmdCommand.Connection = Con;
        //    adpAdapter.SelectCommand = cmdCommand;

        //    cmdCommand.CommandText = "Select AttendanceDate, Description, AttendanceStatus from EmployeeAttendance where Year(AttendanceDate) = @Year AND EmployeeID = @EmployeeID AND AttendanceStatus = 'A' AND ApprovalStatus = 0 AND EAID Not IN(Select EAID from LeaveRequest LR INNER JOIN EmployeeAttendance EA ON EA.EAID = LR.EAID Where Year(EA.AttendanceDate) = @Year AND EA.EmployeeID = @EmployeeID AND EA.AttendanceStatus = 'A' AND EA.ApprovalStatus = 0) Order By AttendanceDate";

        //    cmdCommand.Parameters.AddWithValue("@EmployeeID", (Int32)EmployeeID);
        //    cmdCommand.Parameters.AddWithValue("@Year", (Int32)Year);
        //    //cmdCommand.Parameters.AddWithValue("@Month", (Int32)dtForDate.Date.Month);            
        //    if (Con.State != ConnectionState.Open)
        //        Con.Open();
        //    adpAdapter.Fill(dsDataSet, "Absents");

        //    if (dsDataSet.Tables["Absents"].Rows.Count == 0)
        //        return false;
        //    else
        //        return true;
        //}

        public bool CTCY(ArrayList Todate) // Copy to current year.
        {
            //SqlCommand cmdCommand = new SqlCommand();
            //cmdCommand.Connection = Con;

            //cmdCommand.CommandText = "Delete from Weekend";
            ////cmdCommand.Parameters.AddWithValue("@TodayDate", (DateTime)StartDate.Date);
            //if (Con.State != ConnectionState.Open)
            //    Con.Open();
            //cmdCommand.ExecuteNonQuery();

            for (int i = 0; i <= Todate.Count - 1; i++)
            {
                SqlCommand cmdnCommand = new SqlCommand();
                cmdnCommand.Connection = Con;
                DateTime dt = new DateTime();
                dt = Convert.ToDateTime(Todate[i]);

                cmdnCommand.CommandText = "Select DDID from DayDetails Where TodayDate = @newDate";
                cmdnCommand.Parameters.AddWithValue("@newDate", new DateTime(DateTime.Now.Year, dt.Month, dt.Day));
                if (Con.State != ConnectionState.Open)
                    Con.Open();
                Int32 DDID = Convert.ToInt32(cmdnCommand.ExecuteScalar());
                if (DDID == 0)
                {

                    cmdnCommand.CommandText = "Insert INTO DayDetails(TodayDate, Description, OfficialStatus) ";
                    cmdnCommand.CommandText += "Select @newDate, Description, OfficialStatus From DayDetails Where TodayDate = @TodayDate";
                }
                else
                {
                    cmdnCommand.CommandText = "Update DayDetails Set Description = t.Description, OfficialStatus = t.OfficialStatus from";
                    cmdnCommand.CommandText = " (Select Description, OfficialStatus From DayDetails Where TodayDate = @TodayDate) AS t where TodayDate = @newDate";
                }
                //cmdnCommand.Parameters.AddWithValue("@newDate", new DateTime(DateTime.Now.Year, dt.Month, dt.Day));
                cmdnCommand.Parameters.AddWithValue("@TodayDate", (DateTime)dt.Date);
                if (Con.State != ConnectionState.Open)
                    Con.Open();
                cmdnCommand.ExecuteNonQuery();
            }
            return true;
        }

        public bool DeleteLeaveDays(ArrayList ArrDate)  ///Delete Official off days from the database.
        {
            for (int i = 0; i <= ArrDate.Count - 1; i++)
            {
                SqlCommand cmdnCommand = new SqlCommand();
                cmdnCommand.Connection = Con;
                cmdnCommand.CommandText = "Delete from DayDetails Where TodayDate = @TodayDate";
                cmdnCommand.Parameters.AddWithValue("@TodayDate", Convert.ToDateTime(ArrDate[i]));
                if (Con.State != ConnectionState.Open)
                    Con.Open();
                cmdnCommand.ExecuteNonQuery();
            }
            return true;
        }

        public bool DeleteEmployeeLeaves(Int32 EmployeeID, ArrayList ArrDate)  ///Delete Employee Sick, Annual Or Casual leave and add it to absents.
        {
            SqlCommand cmdCommand = new SqlCommand();
            cmdCommand.Connection = Con;
            cmdCommand.CommandText = "Select EmployeeCode from Employee where EmployeeID = @EmployeeID";
            cmdCommand.Parameters.AddWithValue("@EmployeeID", (Int32)EmployeeID);
            if (Con.State != ConnectionState.Open)
                Con.Open();
            String strEmployeeCode = cmdCommand.ExecuteScalar().ToString();

            for (int i = 0; i <= ArrDate.Count - 1; i++)
            {
                SqlCommand cmdnCommand = new SqlCommand();
                cmdnCommand.Connection = Con;
                cmdnCommand.CommandText = "Update EmployeeAttendance Set AttendanceStatus = 'A', ApprovalStatus = 0, ApprovedBy = NULL Where EmployeeID = @EmployeeID AND AttendanceDate = @AttendanceDate";
                cmdnCommand.Parameters.AddWithValue("@AttendanceDate", Convert.ToDateTime(ArrDate[i]));
                cmdnCommand.Parameters.AddWithValue("@EmployeeID", (Int32)EmployeeID);
                if (Con.State != ConnectionState.Open)
                    Con.Open();
                cmdnCommand.ExecuteNonQuery();

                cmdnCommand.CommandText = "Update EmployeeAttendance Set TimeStatus = 'OverTime' Where EmployeeID = @EmployeeID AND AttendanceDate = (Select CompensationDate from Compensate Where EmployeeID = @EmployeeID AND ToDate = @AttendanceDate) AND TimeStatus = 'Compensate'";
                cmdnCommand.ExecuteNonQuery();

                cmdnCommand.CommandText = "Delete from Compensate where EAID = (Select EAID from EmployeeAttendance Where AttendanceDate = @AttendanceDate And EmployeeID = @EmployeeID)";
                cmdnCommand.ExecuteNonQuery();

                DataTable dt = GetEmployeeMonthlyAttendanceStatus(strEmployeeCode, Convert.ToDateTime(ArrDate[i]));
            }
            return true;
        }

        public bool DeleteEmployeeLeavesDays(Int32 EmployeeID, ArrayList ArrDate)  ///Delete Employee Sick, Annual Or Casual leave permanently from the database. ///
        {
            SqlCommand cmdCommand = new SqlCommand();
            cmdCommand.Connection = Con;
            cmdCommand.CommandText = "Select EmployeeCode from Employee where EmployeeID = @EmployeeID";
            cmdCommand.Parameters.AddWithValue("@EmployeeID", (Int32)EmployeeID);
            if (Con.State != ConnectionState.Open)
                Con.Open();
            String strEmployeeCode = cmdCommand.ExecuteScalar().ToString();

            for (int i = 0; i <= ArrDate.Count - 1; i++)
            {
                SqlCommand cmdnCommand = new SqlCommand();
                cmdnCommand.Connection = Con;
                cmdnCommand.CommandText = "Delete from EmployeeAttendance Where EmployeeID = @EmployeeID AND AttendanceDate = @AttendanceDate";
                cmdnCommand.Parameters.AddWithValue("@AttendanceDate", Convert.ToDateTime(ArrDate[i]));
                cmdnCommand.Parameters.AddWithValue("@EmployeeID", (Int32)EmployeeID);
                if (Con.State != ConnectionState.Open)
                    Con.Open();
                cmdnCommand.ExecuteNonQuery();

                cmdnCommand.CommandText = "Update EmployeeAttendance Set TimeStatus = 'OverTime' Where EmployeeID = @EmployeeID AND AttendanceDate = (Select CompensationDate from Compensate Where EmployeeID = @EmployeeID AND ToDate = @AttendanceDate) AND TimeStatus = 'Compensate'";
                cmdnCommand.ExecuteNonQuery();

                cmdnCommand.CommandText = "Delete from Compensate where EAID = (Select EAID from EmployeeAttendance Where AttendanceDate = @AttendanceDate And EmployeeID = @EmployeeID)";
                cmdnCommand.ExecuteNonQuery();

                DataTable dt = GetEmployeeMonthlyAttendanceStatus(strEmployeeCode, Convert.ToDateTime(ArrDate[i]));
            }
            return true;
        }

        //public bool AddToSickLeaves(Int32 EmployeeID, ArrayList ArrDate, ArrayList ArrDesc)  ///Add Sick Leaves.
        //{
        //    SqlCommand cmdCommand = new SqlCommand();
        //    cmdCommand.Connection = Con;
        //    cmdCommand.CommandText = "Select EmployeeCode from Employee where EmployeeID = @EmployeeID";
        //    cmdCommand.Parameters.AddWithValue("@EmployeeID", (Int32)EmployeeID);
        //    if (Con.State != ConnectionState.Open)
        //        Con.Open();
        //    String strEmployeeCode = cmdCommand.ExecuteScalar().ToString();

        //    for (int i = 0; i <= ArrDate.Count - 1; i++)
        //    {
        //        SqlCommand cmdnCommand = new SqlCommand();
        //        cmdnCommand.Connection = Con;
        //        cmdnCommand.CommandText = "Update EmployeeAttendance Set AttendanceStatus = 'SL', Description = @Description Where EmployeeID = @EmployeeID AND AttendanceDate = @AttendanceDate";
        //        cmdnCommand.Parameters.AddWithValue("@AttendanceDate", Convert.ToDateTime(ArrDate[i]));
        //        cmdnCommand.Parameters.AddWithValue("@Description", Convert.ToString(ArrDesc[i]));
        //        cmdnCommand.Parameters.AddWithValue("@EmployeeID", (Int32)EmployeeID);
        //        if (Con.State != ConnectionState.Open)
        //            Con.Open();
        //        cmdnCommand.ExecuteNonQuery();

        //        DataTable dt = GetEmployeeMonthlyAttendanceStatus(strEmployeeCode, Convert.ToDateTime(ArrDate[i]));
        //    }
        //    return true;
        //}


        public bool AddToLeavesRequest(Int32 EmployeeID, ArrayList ArrDate, ArrayList ArrDesc, String LeaveStatus)  ///Add Leaves Request.
        {
            SqlCommand cmdCommand = new SqlCommand();
            cmdCommand.Connection = Con;
            cmdCommand.CommandText = "Select EmployeeCode from Employee where EmployeeID = @EmployeeID";
            cmdCommand.Parameters.AddWithValue("@EmployeeID", (Int32)EmployeeID);
            if (Con.State != ConnectionState.Open)
                Con.Open();
            String strEmployeeCode = cmdCommand.ExecuteScalar().ToString();

            for (int i = 0; i <= ArrDate.Count - 1; i++)
            {
                SqlCommand cmdnCommand = new SqlCommand();
                cmdnCommand.Connection = Con;

                cmdnCommand.CommandText = "Select EAID from EmployeeAttendance Where EmployeeID = @EmployeeID AND AttendanceDate = @AttendanceDate";
                cmdnCommand.Parameters.AddWithValue("@EmployeeID", (Int32)EmployeeID);
                cmdnCommand.Parameters.AddWithValue("@AttendanceDate", Convert.ToDateTime(ArrDate[i]));
                if (Con.State != ConnectionState.Open)
                    Con.Open();
                Int32 EAID = Convert.ToInt32(cmdnCommand.ExecuteScalar());

                cmdnCommand.CommandText = "Insert INTO LeaveRequest(EAID, RequestedStatus, Description, LeaveDate, EmployeeID) Values(@EAID, @RequestedStatus, @Description, @LeaveDate, @EmployeeID)";
                cmdnCommand.Parameters.AddWithValue("@LeaveDate", Convert.ToDateTime(ArrDate[i]));
                cmdnCommand.Parameters.AddWithValue("@Description", Convert.ToString(ArrDesc[i]));
                cmdnCommand.Parameters.AddWithValue("@RequestedStatus", LeaveStatus);                
                cmdnCommand.Parameters.AddWithValue("@EAID", (Int32)EAID);
                cmdnCommand.ExecuteNonQuery();

                //DataTable dt = GetEmployeeMonthlyAttendanceStatus(strEmployeeCode, Convert.ToDateTime(ArrDate[i]));
            }
            return true;
        }

        public bool AddCompensateToLeavesRequest(Int32 EmployeeID, ArrayList ArrDate, ArrayList ToDate, ArrayList ArrDesc, String LeaveStatus)  ///Add Leaves Request.
        {
            SqlCommand cmdCommand = new SqlCommand();
            cmdCommand.Connection = Con;
            cmdCommand.CommandText = "Select EmployeeCode from Employee where EmployeeID = @EmployeeID";
            cmdCommand.Parameters.AddWithValue("@EmployeeID", (Int32)EmployeeID);
            if (Con.State != ConnectionState.Open)
                Con.Open();
            String strEmployeeCode = cmdCommand.ExecuteScalar().ToString();

            for (int i = 0; i <= ArrDate.Count - 1; i++)
            {
                SqlCommand cmdnCommand = new SqlCommand();
                cmdnCommand.Connection = Con;

                cmdnCommand.CommandText = "Select EAID from EmployeeAttendance Where EmployeeID = @EmployeeID AND AttendanceDate = @AttendanceDate";
                cmdnCommand.Parameters.AddWithValue("@EmployeeID", (Int32)EmployeeID);
                cmdnCommand.Parameters.AddWithValue("@AttendanceDate", Convert.ToDateTime(ArrDate[i]));
                if (Con.State != ConnectionState.Open)
                    Con.Open();
                Int32 EAID = Convert.ToInt32(cmdnCommand.ExecuteScalar());

                cmdnCommand.CommandText = "Insert INTO LeaveRequest(EAID, RequestedStatus, Description, LeaveDate, ToDate, EmployeeID) Values(@EAID, @RequestedStatus, @Description, @LeaveDate, @ToDate, @EmployeeID)";
                cmdnCommand.Parameters.AddWithValue("@LeaveDate", Convert.ToDateTime(ArrDate[i]));
                cmdnCommand.Parameters.AddWithValue("@ToDate", Convert.ToDateTime(ToDate[i]));
                cmdnCommand.Parameters.AddWithValue("@Description", Convert.ToString(ArrDesc[i]));
                cmdnCommand.Parameters.AddWithValue("@RequestedStatus", LeaveStatus);
                cmdnCommand.Parameters.AddWithValue("@EAID", (Int32)EAID);
                cmdnCommand.ExecuteNonQuery();

                //DataTable dt = GetEmployeeMonthlyAttendanceStatus(strEmployeeCode, Convert.ToDateTime(ArrDate[i]));
            }
            return true;
        }

        /// <summary>
        /// This method adds absent to leave, Providing leave type SL for sick leave, AL for annual leave, CL for casual leave and C for compensation
        /// </summary>
        /// <param name="EmployeeID"></param>
        /// <param name="ArrDate"></param>
        /// <param name="ArrDesc"></param>
        /// <param name="Leavetype"></param>
        /// <returns></returns>
        public bool AddToLeaves(Int32 EmployeeID, ArrayList ArrDate, ArrayList ArrDesc, String Leavetype)  ///Add Sick Leaves.
        {
            SqlCommand cmdCommand = new SqlCommand();
            cmdCommand.Connection = Con;
            cmdCommand.CommandText = "Select EmployeeCode from Employee where EmployeeID = @EmployeeID";
            cmdCommand.Parameters.AddWithValue("@EmployeeID", (Int32)EmployeeID);
            if (Con.State != ConnectionState.Open)
                Con.Open();
            String strEmployeeCode = cmdCommand.ExecuteScalar().ToString();

            for (int i = 0; i <= ArrDate.Count - 1; i++)
            {
                SqlCommand cmdnCommand = new SqlCommand();
                cmdnCommand.Connection = Con;
                cmdnCommand.CommandText = "Update EmployeeAttendance Set AttendanceStatus = @AttendanceStatus, Description = @Description Where EmployeeID = @EmployeeID AND AttendanceDate = @AttendanceDate";
                cmdnCommand.Parameters.AddWithValue("@AttendanceDate", Convert.ToDateTime(ArrDate[i]));
                cmdnCommand.Parameters.AddWithValue("@Description", Convert.ToString(ArrDesc[i]));
                cmdnCommand.Parameters.AddWithValue("@EmployeeID", (Int32)EmployeeID);
                cmdnCommand.Parameters.AddWithValue("@AttendanceStatus", (String)Leavetype);
                if (Con.State != ConnectionState.Open)
                    Con.Open();
                cmdnCommand.ExecuteNonQuery();

                DataTable dt = GetEmployeeMonthlyAttendanceStatus(strEmployeeCode, Convert.ToDateTime(ArrDate[i]));
            }
            return true;
        }

        public bool AddNewSickLeaves(Int32 EmployeeID, ArrayList ArrDate, ArrayList ArrDesc)  ///Add Sick Leaves.
        {
            SqlCommand cmdCommand = new SqlCommand();
            cmdCommand.Connection = Con;
            cmdCommand.CommandText = "Select EmployeeCode from Employee where EmployeeID = @EmployeeID";
            cmdCommand.Parameters.AddWithValue("@EmployeeID", (Int32)EmployeeID);
            if (Con.State != ConnectionState.Open)
                Con.Open();
            String strEmployeeCode = cmdCommand.ExecuteScalar().ToString();

            for (int i = 0; i <= ArrDate.Count - 1; i++)
            {
                SqlCommand cmdnCommand = new SqlCommand();
                cmdnCommand.Connection = Con;
                cmdnCommand.CommandText = "Insert INTO EmployeeAttendance(AttendanceDate, EmployeeID, AttendanceStatus, Description) Values(@AttendanceDate, @EmployeeID, 'SL', @Description)";
                cmdnCommand.Parameters.AddWithValue("@AttendanceDate", Convert.ToDateTime(ArrDate[i]));
                cmdnCommand.Parameters.AddWithValue("@Description", Convert.ToString(ArrDesc[i]));
                cmdnCommand.Parameters.AddWithValue("@EmployeeID", (Int32)EmployeeID);
                if (Con.State != ConnectionState.Open)
                    Con.Open();
                cmdnCommand.ExecuteNonQuery();

                DataTable dt = GetEmployeeMonthlyAttendanceStatus(strEmployeeCode, Convert.ToDateTime(ArrDate[i]));
            }
            return true;
        }

        public bool AddNewAnnualLeaves(Int32 EmployeeID, ArrayList ArrDate, ArrayList ArrDesc)  ///Add Annual Leaves.
        {
            SqlCommand cmdCommand = new SqlCommand();
            cmdCommand.Connection = Con;
            cmdCommand.CommandText = "Select EmployeeCode from Employee where EmployeeID = @EmployeeID";
            cmdCommand.Parameters.AddWithValue("@EmployeeID", (Int32)EmployeeID);
            if (Con.State != ConnectionState.Open)
                Con.Open();
            String strEmployeeCode = cmdCommand.ExecuteScalar().ToString();

            for (int i = 0; i <= ArrDate.Count - 1; i++)
            {
                SqlCommand cmdnCommand = new SqlCommand();
                cmdnCommand.Connection = Con;
                cmdnCommand.CommandText = "Insert INTO EmployeeAttendance(AttendanceDate, EmployeeID, AttendanceStatus, Description) Values(@AttendanceDate, @EmployeeID, 'AL', @Description)";
                cmdnCommand.Parameters.AddWithValue("@AttendanceDate", Convert.ToDateTime(ArrDate[i]));
                cmdnCommand.Parameters.AddWithValue("@Description", Convert.ToString(ArrDesc[i]));
                cmdnCommand.Parameters.AddWithValue("@EmployeeID", (Int32)EmployeeID);
                if (Con.State != ConnectionState.Open)
                    Con.Open();
                cmdnCommand.ExecuteNonQuery();

                DataTable dt = GetEmployeeMonthlyAttendanceStatus(strEmployeeCode, Convert.ToDateTime(ArrDate[i]));
            }
            return true;
        }

        public bool AddNewCasualLeaves(Int32 EmployeeID, ArrayList ArrDate, ArrayList ArrDesc)  ///Add Casual Leaves.
        {
            SqlCommand cmdCommand = new SqlCommand();
            cmdCommand.Connection = Con;
            cmdCommand.CommandText = "Select EmployeeCode from Employee where EmployeeID = @EmployeeID";
            cmdCommand.Parameters.AddWithValue("@EmployeeID", (Int32)EmployeeID);
            if (Con.State != ConnectionState.Open)
                Con.Open();
            String strEmployeeCode = cmdCommand.ExecuteScalar().ToString();

            for (int i = 0; i <= ArrDate.Count - 1; i++)
            {
                SqlCommand cmdnCommand = new SqlCommand();
                cmdnCommand.Connection = Con;
                cmdnCommand.CommandText = "Insert INTO EmployeeAttendance(AttendanceDate, EmployeeID, AttendanceStatus, Description) Values(@AttendanceDate, @EmployeeID, 'CL', @Description)";
                cmdnCommand.Parameters.AddWithValue("@AttendanceDate", Convert.ToDateTime(ArrDate[i]));
                cmdnCommand.Parameters.AddWithValue("@Description", Convert.ToString(ArrDesc[i]));
                cmdnCommand.Parameters.AddWithValue("@EmployeeID", (Int32)EmployeeID);
                if (Con.State != ConnectionState.Open)
                    Con.Open();
                cmdnCommand.ExecuteNonQuery();

                DataTable dt = GetEmployeeMonthlyAttendanceStatus(strEmployeeCode, Convert.ToDateTime(ArrDate[i]));
            }
            return true;
        }

        public DataSet GetPresentEmployees()
        {
            SqlCommand cmdCommand = new SqlCommand();
            SqlDataAdapter adpAdapter = new SqlDataAdapter();
            DataSet dsDataSet = new DataSet();

            cmdCommand.Connection = Con;
            adpAdapter.SelectCommand = cmdCommand;

            cmdCommand.CommandText = "SELECT TOP (100) PERCENT dbo.Employee.EmployeeCode, dbo.Employee.EmployeeName, dbo.Designation.Designation, VwAttendanceInOutStatus.InOutStatus, dbo.VwAttendanceInOutStatus.InOutTime, dbo.VwAttendanceInOutStatus.TrackDate, ";
            cmdCommand.CommandText += " VwAttendanceInOutStatus.ADID, dbo.VwAttendanceInOutStatus.EmployeeID, dbo.VwAttendanceInOutStatus.InOutID FROM Employee INNER JOIN Designation ON dbo.Employee.DesignationID = dbo.Designation.DesignationID INNER JOIN ";
            cmdCommand.CommandText += " VwAttendanceInOutStatus ON dbo.Employee.EmployeeID = dbo.VwAttendanceInOutStatus.EmployeeID Where VwAttendanceInOutStatus.InOutStatus = 'In' ORDER BY dbo.Employee.EmployeeCode";

            //cmdCommand.Parameters.AddWithValue("@Year", (Int32)dtForDate.Date.Year);
            //cmdCommand.Parameters.AddWithValue("@Month", (Int32)dtForDate.Date.Month);
            //cmdCommand.Parameters.AddWithValue("@EmployeeCode", (String)EmployeeCode);

            if (Con.State != ConnectionState.Open)
                Con.Open();
            adpAdapter.Fill(dsDataSet, "PresentEmployees");

            cmdCommand.CommandText = "SELECT TOP (100) PERCENT dbo.Employee.EmployeeCode, dbo.Employee.EmployeeName, dbo.Designation.Designation, VwAttendanceInOutStatus.InOutStatus, dbo.VwAttendanceInOutStatus.InOutTime, dbo.VwAttendanceInOutStatus.TrackDate, ";
            cmdCommand.CommandText += " VwAttendanceInOutStatus.ADID, dbo.VwAttendanceInOutStatus.EmployeeID, dbo.VwAttendanceInOutStatus.InOutID FROM Employee INNER JOIN Designation ON dbo.Employee.DesignationID = dbo.Designation.DesignationID INNER JOIN ";
            cmdCommand.CommandText += " VwAttendanceInOutStatus ON dbo.Employee.EmployeeID = dbo.VwAttendanceInOutStatus.EmployeeID Where VwAttendanceInOutStatus.InOutStatus = 'Out' AND Employee.EmployeeStatus = 1 ORDER BY dbo.Employee.EmployeeCode";

            //cmdCommand.Parameters.AddWithValue("@Year", (Int32)dtForDate.Date.Year);
            //cmdCommand.Parameters.AddWithValue("@Month", (Int32)dtForDate.Date.Month);
            //cmdCommand.Parameters.AddWithValue("@EmployeeCode", (String)EmployeeCode);

            if (Con.State != ConnectionState.Open)
                Con.Open();
            adpAdapter.Fill(dsDataSet, "AbsentEmployees");

            return dsDataSet;
        }

        public Boolean SetPresentEmployeesOut(DataTable dtPE)
        {
            SqlTransaction trans;
            SqlCommand cmdCommand = new SqlCommand();
            cmdCommand.Connection = Con;
            if (Con.State != ConnectionState.Open)
                Con.Open();
            trans = Con.BeginTransaction();

            try
            {

                cmdCommand.Transaction = trans;
                for (int i = 0; i <= dtPE.Rows.Count - 1; i++)
                {
                    cmdCommand.CommandText = "Select InOutStatus from AttendanceDetails where ADID = (Select IsNull(Max(ADID),0) from AttendanceDetails where EmployeeID = @EmployeeID)";
                    cmdCommand.Parameters.AddWithValue("@EmployeeID", (Int32)dtPE.Rows[i]["EmployeeID"]);
                    String strEmployeeStatus = cmdCommand.ExecuteScalar().ToString();
                    if (strEmployeeStatus == "In")
                    {
                        cmdCommand.CommandText = "Insert INTO AttendanceDetails(InOutID, EmployeeID, TrackDate, InOutTime, InOutStatus) Values(@InOutID, @EmployeeID, @TrackDate, @InOutTime, 'Out')";
                        cmdCommand.Parameters.AddWithValue("@InOutID", (Int32)dtPE.Rows[i]["InOutID"]);
                        //cmdCommand.Parameters.AddWithValue("@EmployeeID", (Int32)dtPE.Rows[i]["EmployeeID"]);
                        cmdCommand.Parameters.AddWithValue("@TrackDate", (DateTime)dtPE.Rows[i]["TrackDate"]);
                        cmdCommand.Parameters.AddWithValue("@InOutTime", Convert.ToDateTime(Convert.ToDateTime(dtPE.Rows[i]["TrackDate"]).ToShortDateString() + " " + Convert.ToDateTime(dtPE.Rows[i]["InOutTime"]).ToShortTimeString()).AddHours(9));

                        cmdCommand.ExecuteNonQuery();

                        cmdCommand.Parameters.RemoveAt("@InOutID");
                        cmdCommand.Parameters.RemoveAt("@EmployeeID");
                        cmdCommand.Parameters.RemoveAt("@TrackDate");
                        cmdCommand.Parameters.RemoveAt("@InOutTime");
                    }
                }
                trans.Commit();
                return true;
            }
            catch
            {
                trans.Rollback();
                return false;
            }
        }



        public DataSet GetAbsentList(int Month, int Year)
        {
            SqlCommand cmdCommand = new SqlCommand();
            SqlDataAdapter adpAdapter = new SqlDataAdapter();
            DataSet dsDataSet = new DataSet();

            cmdCommand.Connection = Con;
            adpAdapter.SelectCommand = cmdCommand;

            cmdCommand.CommandText = "SELECT EmployeeID, EmployeeName, EmployeeCode, AttendanceDate, AttendanceStatus, EAID FROM VwAbsentList WHERE (Month(AttendanceDate) = @Month) AND (Year(AttendanceDate) = @Year) ORDER BY EmployeeName";
            cmdCommand.Parameters.AddWithValue("@Month", (int)Month);
            cmdCommand.Parameters.AddWithValue("@Year", (int)Year);
            //cmdCommand.Parameters.AddWithValue("@EmployeeCode", (String)EmployeeCode);

            if (Con.State != ConnectionState.Open)
                Con.Open();
            adpAdapter.Fill(dsDataSet, "AbsentList");
            return dsDataSet;
        }

        public DataSet GetEmployeeAbsentList(String EmployeeCode, int Month, int Year)
        {
            SqlCommand cmdCommand = new SqlCommand();
            SqlDataAdapter adpAdapter = new SqlDataAdapter();
            DataSet dsDataSet = new DataSet();

            cmdCommand.Connection = Con;
            adpAdapter.SelectCommand = cmdCommand;

            cmdCommand.CommandText = "SELECT EmployeeID, EmployeeName, EmployeeCode, AttendanceDate, AttendanceStatus, EAID FROM VwAbsentList WHERE (Month(AttendanceDate) = @Month) AND (Year(AttendanceDate) = @Year) AND (EmployeeCode = @EmployeeCode) ORDER BY EmployeeName";
            cmdCommand.Parameters.AddWithValue("@Month", (int)Month);
            cmdCommand.Parameters.AddWithValue("@Year", (int)Year);
            cmdCommand.Parameters.AddWithValue("@EmployeeCode", (String)EmployeeCode);

            if (Con.State != ConnectionState.Open)
                Con.Open();
            adpAdapter.Fill(dsDataSet, "AbsentList");
            return dsDataSet;
        }


        public DataTable GetEmployeeNightShift(string Year, string Month,string EmpCode,Int32 Signal,string DtStart,string DtEnd)  /// signal 0=Filter Emp  signal 1=MonthWise  ///
        {
            

                string Query = string.Empty;
                if (Signal == 0) { Query = "SELECT TOP (100) PERCENT CONVERT(int, AD.EmployeeID) AS EmployeeID, dbo.Employee.EmployeeCode, dbo.Employee.EmployeeName, dbo.Designation.Designation, CONVERT(varchar, AD.TrackDate, 103) AS InDate,Right((SELECT InOutTime FROM dbo.AttendanceDetails AS ADS WHERE (InOutID = AD.InOutID) AND (InOutStatus = 'In') AND (EmployeeID = AD.EmployeeID)),7) AS InTime,CONVERT(varchar, (SELECT TrackDate FROM dbo.AttendanceDetails AS ADS WHERE (InOutID = AD.InOutID) AND (InOutStatus = 'Out') AND (EmployeeID = AD.EmployeeID) AND (TrackDate <> AD.TrackDate)) ,103)AS OutDate,Right((SELECT InOutTime FROM dbo.AttendanceDetails AS ADS WHERE (InOutID = AD.InOutID) AND (InOutStatus = 'Out') AND (EmployeeID = AD.EmployeeID) AND (TrackDate <> AD.TrackDate)),7) AS OutTime, AD.ADID, AD.InOutID, YEAR(AD.TrackDate) AS Year, MONTH(AD.TrackDate) AS Month FROM dbo.AttendanceDetails AS AD INNER JOIN dbo.Employee ON AD.EmployeeID = dbo.Employee.EmployeeID INNER JOIN dbo.Designation ON dbo.Employee.DesignationID = dbo.Designation.DesignationID WHERE (YEAR(AD.TrackDate) = @Year) AND (MONTH(AD.TrackDate) = @Month) AND (dbo.Employee.EmployeeCode = @EmpCode) AND ((SELECT TrackDate FROM dbo.AttendanceDetails AS ADS WHERE (InOutID = AD.InOutID) AND (InOutStatus = 'Out') AND (EmployeeID = AD.EmployeeID) AND (TrackDate <> AD.TrackDate)) IS NOT NULL) ORDER BY AD.EmployeeID, AD.ADID, InDate, AD.InOutID"; }
                if (Signal == 1) { Query = "SELECT TOP (100) PERCENT CONVERT(int, AD.EmployeeID) AS EmployeeID, dbo.Employee.EmployeeCode, dbo.Employee.EmployeeName, dbo.Designation.Designation, CONVERT(varchar, AD.TrackDate, 103) AS InDate,Right((SELECT InOutTime FROM dbo.AttendanceDetails AS ADS WHERE (InOutID = AD.InOutID) AND (InOutStatus = 'In') AND (EmployeeID = AD.EmployeeID)),7) AS InTime,CONVERT(varchar, (SELECT TrackDate FROM dbo.AttendanceDetails AS ADS WHERE (InOutID = AD.InOutID) AND (InOutStatus = 'Out') AND (EmployeeID = AD.EmployeeID) AND (TrackDate <> AD.TrackDate)) ,103)AS OutDate,Right((SELECT InOutTime FROM dbo.AttendanceDetails AS ADS WHERE (InOutID = AD.InOutID) AND (InOutStatus = 'Out') AND (EmployeeID = AD.EmployeeID) AND (TrackDate <> AD.TrackDate)),7) AS OutTime, AD.ADID, AD.InOutID, YEAR(AD.TrackDate) AS Year, MONTH(AD.TrackDate) AS Month FROM dbo.AttendanceDetails AS AD INNER JOIN dbo.Employee ON AD.EmployeeID = dbo.Employee.EmployeeID INNER JOIN dbo.Designation ON dbo.Employee.DesignationID = dbo.Designation.DesignationID WHERE (YEAR(AD.TrackDate) = @Year) AND (MONTH(AD.TrackDate) = @Month)  AND ((SELECT TrackDate FROM dbo.AttendanceDetails AS ADS WHERE (InOutID = AD.InOutID) AND (InOutStatus = 'Out') AND (EmployeeID = AD.EmployeeID) AND (TrackDate <> AD.TrackDate)) IS NOT NULL) ORDER BY AD.EmployeeID, AD.ADID, InDate, AD.InOutID"; }
                if (Signal == 2) { Query = "SELECT TOP (100) PERCENT CONVERT(int, AD.EmployeeID) AS EmployeeID, dbo.Employee.EmployeeCode, dbo.Employee.EmployeeName, dbo.Designation.Designation, CONVERT(varchar, AD.TrackDate, 103) AS InDate,Right((SELECT InOutTime FROM dbo.AttendanceDetails AS ADS WHERE (InOutID = AD.InOutID) AND (InOutStatus = 'In') AND (EmployeeID = AD.EmployeeID)),7) AS InTime,CONVERT(varchar, (SELECT TrackDate FROM dbo.AttendanceDetails AS ADS WHERE (InOutID = AD.InOutID) AND (InOutStatus = 'Out') AND (EmployeeID = AD.EmployeeID) AND (TrackDate <> AD.TrackDate)) ,103)AS OutDate,Right((SELECT InOutTime FROM dbo.AttendanceDetails AS ADS WHERE (InOutID = AD.InOutID) AND (InOutStatus = 'Out') AND (EmployeeID = AD.EmployeeID) AND (TrackDate <> AD.TrackDate)),7) AS OutTime, AD.ADID, AD.InOutID, YEAR(AD.TrackDate) AS Year, MONTH(AD.TrackDate) AS Month FROM dbo.AttendanceDetails AS AD INNER JOIN dbo.Employee ON AD.EmployeeID = dbo.Employee.EmployeeID INNER JOIN dbo.Designation ON dbo.Employee.DesignationID = dbo.Designation.DesignationID WHERE (CAST(CONVERT(varchar(10), AD.TrackDate, 112) AS datetime) BETWEEN CAST(CONVERT(varchar(10), @DateStart, 112) AS datetime) AND CAST(CONVERT(varchar(10), @DateEnd, 112) AS datetime))  AND ((SELECT TrackDate FROM dbo.AttendanceDetails AS ADS WHERE (InOutID = AD.InOutID) AND (InOutStatus = 'Out') AND (EmployeeID = AD.EmployeeID) AND (TrackDate <> AD.TrackDate)) IS NOT NULL) ORDER BY AD.EmployeeID, AD.ADID, InDate, AD.InOutID"; }
                
                
                SqlCommand Cmd = new SqlCommand();
                Cmd.Connection = Con;
                Cmd.CommandText = Query;
                Cmd.Parameters.AddWithValue("@Year", Year);
                Cmd.Parameters.AddWithValue("@Month", Month);
                Cmd.Parameters.AddWithValue("@EmpCode", EmpCode);
                Cmd.Parameters.AddWithValue("@DateStart", DtStart);
                Cmd.Parameters.AddWithValue("@DateEnd", DtEnd);




                SqlDataAdapter Adp = new SqlDataAdapter(Cmd);

                DataTable Dt = new DataTable("DtEmpNightSitting");
                if (Con.State != ConnectionState.Open) { Con.Open(); }
                Adp.Fill(Dt);
                Cmd.Dispose(); Con.Dispose();
                return Dt;

            







        }








         
















    }
}
