using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

using RecordAttendance;

namespace ICEPDDatabaseModule
{
    public class ICEDBEmployee
    {
        SqlConnection Con = new SqlConnection();

        public ICEDBEmployee()
        {
            Con.ConnectionString = DBInfo.DBConnectionString;
        }

        private Int32 GenerateEmployeeID()
        {
            SqlCommand cmdEmployeeID = new SqlCommand();
            cmdEmployeeID.Connection = Con;
            cmdEmployeeID.CommandText = "Select ISNULL(MAX(EmployeeID),0) + 1 from Employee";
            if (Con.State != ConnectionState.Open)
                Con.Open();
            Int32 EmployeeID = Convert.ToInt32(cmdEmployeeID.ExecuteScalar());
            Con.Close();
            
            return EmployeeID;
        }

        public String GetEmployeeCode(Int32 EmployeeID)
        {
            SqlCommand cmdEmployeeID = new SqlCommand();
            cmdEmployeeID.Connection = Con;
            cmdEmployeeID.CommandText = "Select EmployeeCode from Employee Where EmployeeID = @EmployeeID";
            cmdEmployeeID.Parameters.AddWithValue("@EmployeeID", (Int32)EmployeeID);
            if (Con.State != ConnectionState.Open)
                Con.Open();
            String EmployeeCode = cmdEmployeeID.ExecuteScalar().ToString();
            Con.Close();

            return EmployeeCode;
        }

        public Int32 SaveEmployee(ICEPDCommonModule.CommonEmployee objCommonEmployee)
        {
            SqlCommand cmdEmployee = new SqlCommand();
            cmdEmployee.Connection = Con;
            Int32 EmployeeID = GenerateEmployeeID();
            cmdEmployee.CommandText = "Insert Into Employee(EmployeeID, DepartmentID, CompanyID, EmployeeName, FatherName, Address, DOB, ContactNo, DesignationID, JoiningDate, EmployeeLogin, EmployeeImage, Description, EmployeeCode, EmployeeStatus, CNIC, MobileNetwork) Values(@EmployeeID, @DepartmentID, @CompanyID, @EmployeeName, @FatherName, @Address, @DOB, @ContactNo, @DesignationID, @JoiningDate, @EmployeeLogin, @EmployeeImage, @Description, @EmployeeCode, @EmployeeStatus, @CNIC, @MobileNetwork)";
            if (objCommonEmployee.SignatureImage != null)
            {
                cmdEmployee.CommandText = "Insert Into Employee(EmployeeID, DepartmentID, CompanyID, EmployeeName, FatherName, Address, DOB, ContactNo, DesignationID, JoiningDate, EmployeeLogin, EmployeeImage, Description, EmployeeCode, EmployeeStatus, CNIC, MobileNetwork, EmployeeSignature) Values(@EmployeeID, @DepartmentID, @CompanyID, @EmployeeName, @FatherName, @Address, @DOB, @ContactNo, @DesignationID, @JoiningDate, @EmployeeLogin, @EmployeeImage, @Description, @EmployeeCode, @EmployeeStatus, @CNIC, @MobileNetwork, @EmployeeSignature)";
                cmdEmployee.Parameters.AddWithValue("@EmployeeSignature", (object)objCommonEmployee.SignatureImage);
            }
            cmdEmployee.Parameters.AddWithValue("@EmployeeID", (Int32)EmployeeID);
            cmdEmployee.Parameters.AddWithValue("@DepartmentID", (Int32)objCommonEmployee.DepartmentID);
            cmdEmployee.Parameters.AddWithValue("@DesignationID", (Int32)objCommonEmployee.DesignationID);
            cmdEmployee.Parameters.AddWithValue("@CompanyID", (Int32)objCommonEmployee.CompanyID);
            cmdEmployee.Parameters.AddWithValue("@EmployeeName", (String)objCommonEmployee.EmployeeName);
            cmdEmployee.Parameters.AddWithValue("@FatherName", (String)objCommonEmployee.FatherName);
            cmdEmployee.Parameters.AddWithValue("@Address", (String)objCommonEmployee.Address);
            cmdEmployee.Parameters.AddWithValue("@DOB", (DateTime)objCommonEmployee.DOB.Date);
            cmdEmployee.Parameters.AddWithValue("@ContactNo", (String)objCommonEmployee.ContactNo);
            cmdEmployee.Parameters.AddWithValue("@JoiningDate", (DateTime)objCommonEmployee.JoiningDate.Date);
            cmdEmployee.Parameters.AddWithValue("@EmployeeLogin", (String)objCommonEmployee.EmployeeLogin);
            cmdEmployee.Parameters.AddWithValue("@EmployeeImage", (object)objCommonEmployee.EmpImage);
            cmdEmployee.Parameters.AddWithValue("@Description", (String)objCommonEmployee.Description);
            cmdEmployee.Parameters.AddWithValue("@EmployeeCode", (String)objCommonEmployee.EmployeeCode);
            cmdEmployee.Parameters.AddWithValue("@EmployeeStatus", (Boolean)objCommonEmployee.EmployeeStatus);
            cmdEmployee.Parameters.AddWithValue("@CNIC", (String)objCommonEmployee.CNIC);
            cmdEmployee.Parameters.AddWithValue("@MobileNetwork", (String)objCommonEmployee.MobileNetwork);


            if (Con.State != ConnectionState.Open)
                Con.Open();

            cmdEmployee.ExecuteNonQuery();
            Con.Close();

            objCommonEmployee.EmployeeID = EmployeeID;
            SaveShiftDetails(objCommonEmployee);
            return EmployeeID;
        }

        public bool SaveShiftDetails(ICEPDCommonModule.CommonEmployee objCommonEmployee)
        {
            SqlCommand cmdEmployee = new SqlCommand();
            cmdEmployee.Connection = Con;
            cmdEmployee.CommandText = "Insert Into ShiftDetails(EmployeeID, ChangeDate, TimeFrom, TimeTo) Values(@EmployeeID, @ChangeDate, @TimeFrom, @TimeTo)";
            cmdEmployee.Parameters.AddWithValue("@EmployeeID", (Int32)objCommonEmployee.EmployeeID);            
            cmdEmployee.Parameters.AddWithValue("@ChangeDate", (DateTime)objCommonEmployee.ChangeDate.Date);
            cmdEmployee.Parameters.AddWithValue("@TimeFrom", Convert.ToDateTime(Convert.ToDateTime(objCommonEmployee.TimeFrom).ToShortTimeString()));
            cmdEmployee.Parameters.AddWithValue("@TimeTo", Convert.ToDateTime(Convert.ToDateTime(objCommonEmployee.TimeTo).ToShortTimeString()));

            if (Con.State != ConnectionState.Open)
                Con.Open();

            cmdEmployee.ExecuteNonQuery();
            Con.Close();
            
            return true;      
        }

        public bool UpdateShiftDetails(ICEPDCommonModule.CommonEmployee objCommonEmployee)
        {
            SqlCommand cmdEmployee = new SqlCommand();
            cmdEmployee.Connection = Con;
            cmdEmployee.CommandText = "Update ShiftDetails Set ChangeDate = @ChangeDate, TimeFrom = @TimeFrom, TimeTo = @TimeTo where SDID = @SDID";
            cmdEmployee.Parameters.AddWithValue("SDID", (Int32)objCommonEmployee.SDID);
            cmdEmployee.Parameters.AddWithValue("@ChangeDate", (DateTime)objCommonEmployee.ChangeDate.Date);
            cmdEmployee.Parameters.AddWithValue("@TimeFrom", Convert.ToDateTime(Convert.ToDateTime(objCommonEmployee.TimeFrom).ToShortTimeString()));
            cmdEmployee.Parameters.AddWithValue("@TimeTo", Convert.ToDateTime(Convert.ToDateTime(objCommonEmployee.TimeTo).ToShortTimeString()));

            if (Con.State != ConnectionState.Open)
                Con.Open();

            cmdEmployee.ExecuteNonQuery();
            Con.Close();
            
            return true;
        }

        public Boolean UpdateEmployee(ICEPDCommonModule.CommonEmployee objCommonEmployee)
        {
            try
            {
                SqlCommand cmdEmployee = new SqlCommand();
                cmdEmployee.Connection = Con;
                cmdEmployee.CommandText = "Update Employee Set DepartmentID = @DepartmentID, DesignationID = @DesignationID, CompanyID = @CompanyID, EmployeeName = @EmployeeName, FatherName = @FatherName, Address = @Address, DOB = @DOB, ContactNo = @ContactNo, JoiningDate = @JoiningDate, Description = @Description, EmployeeLogin = @EmployeeLogin, EmployeeImage = @EmployeeImage, EmployeeCode = @EmployeeCode, EmployeeStatus = @EmployeeStatus, CNIC = @CNIC, MobileNetwork = @MobileNetwork";
                if (objCommonEmployee.SignatureImage != null)
                {
                    cmdEmployee.CommandText += ", EmployeeSignature = @EmployeeSignature";
                    cmdEmployee.Parameters.AddWithValue("@EmployeeSignature", (object)objCommonEmployee.SignatureImage);
                }
                cmdEmployee.CommandText += " where EmployeeID = @EmployeeID";
                cmdEmployee.Parameters.AddWithValue("@EmployeeID", (Int32)objCommonEmployee.EmployeeID);
                cmdEmployee.Parameters.AddWithValue("@DepartmentID", (Int32)objCommonEmployee.DepartmentID);
                cmdEmployee.Parameters.AddWithValue("@DesignationID", (Int32)objCommonEmployee.DesignationID);
                cmdEmployee.Parameters.AddWithValue("@CompanyID", (Int32)objCommonEmployee.CompanyID);
                cmdEmployee.Parameters.AddWithValue("@EmployeeName", (String)objCommonEmployee.EmployeeName);
                cmdEmployee.Parameters.AddWithValue("@FatherName", (String)objCommonEmployee.FatherName);
                cmdEmployee.Parameters.AddWithValue("@Address", (String)objCommonEmployee.Address);
                cmdEmployee.Parameters.AddWithValue("@DOB", (DateTime)objCommonEmployee.DOB.Date);
                cmdEmployee.Parameters.AddWithValue("@ContactNo", (String)objCommonEmployee.ContactNo);
                cmdEmployee.Parameters.AddWithValue("@JoiningDate", (DateTime)objCommonEmployee.JoiningDate.Date);
                cmdEmployee.Parameters.AddWithValue("@Description", (String)objCommonEmployee.Description);
                cmdEmployee.Parameters.AddWithValue("@EmployeeLogin", (String)objCommonEmployee.EmployeeLogin);
                cmdEmployee.Parameters.AddWithValue("@EmployeeImage", (object)objCommonEmployee.EmpImage);
                cmdEmployee.Parameters.AddWithValue("@EmployeeCode", (String)objCommonEmployee.EmployeeCode);
                cmdEmployee.Parameters.AddWithValue("@EmployeeStatus", (Boolean)objCommonEmployee.EmployeeStatus);
                cmdEmployee.Parameters.AddWithValue("@CNIC", (String)objCommonEmployee.CNIC);
                cmdEmployee.Parameters.AddWithValue("@MobileNetwork", (String)objCommonEmployee.MobileNetwork);
                

                if (Con.State != ConnectionState.Open)
                    Con.Open();

                cmdEmployee.ExecuteNonQuery();
                Con.Close();
                

                UpdateShiftDetails(objCommonEmployee);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public DataSet LoadEmployees()
        {
            SqlCommand cmdEmployees = new SqlCommand();
            SqlDataAdapter adpEmployees = new SqlDataAdapter();
            DataSet dsEmployees = new DataSet();

            cmdEmployees.Connection = Con;
            //cmdEmployees.CommandText = "Select EmployeeID, Department.Department, EmployeeName, FatherName, Address, DOB, ContactNo, Designation.Designation, JoiningDate, Employee.Description from Employee INNER JOIN Department ON Employee.DepartmentID = Department.DepartmentID INNER JOIN Designation ON Employee.DesignationID = Designation.DesignationID";            
            cmdEmployees.CommandText = "SELECT Employee.EmployeeID, Designation.Designation, Department.Department, Company.CompanyName, Employee.EmployeeName, Employee.FatherName, Employee.ContactNo, Employee.Address, Employee.DOB, Employee.JoiningDate, Employee.Description, Employee.EmployeeLogin, EmployeeImage, EmployeeCode, ShiftDetails.SDID, ShiftDetails.ChangeDate, ShiftDetails.TimeFrom, ShiftDetails.TimeTo, Employee.EmployeeStatus";
            cmdEmployees.CommandText += ", CNIC, MobileNetwork, EmployeeSignature FROM Employee Left JOIN ShiftDetails ON Employee.EmployeeID = ShiftDetails.EmployeeID INNER JOIN Department ON Employee.DepartmentID = Department.DepartmentID INNER JOIN Designation ON Employee.DesignationID = Designation.DesignationID INNER JOIN Company On Company.CompanyID = Employee.CompanyID WHERE (ShiftDetails.SDID = (SELECT MAX(SDID) FROM ShiftDetails WHERE (EmployeeID = Employee.EmployeeID))) Order By Employee.EmployeeID";
            adpEmployees.SelectCommand = cmdEmployees;
            if (Con.State != ConnectionState.Open)
                Con.Open();
            adpEmployees.Fill(dsEmployees);
            Con.Close();
            
            return dsEmployees;
        }

        public DataSet LoadSelectedEmployee(Int32 EmployeeID)
        {
            SqlCommand cmdEmployees = new SqlCommand();
            SqlDataAdapter adpEmployees = new SqlDataAdapter();
            DataSet dsEmployees = new DataSet();

            cmdEmployees.Connection = Con;            
            cmdEmployees.CommandText = "SELECT Employee.EmployeeID, Designation.Designation, Department.Department, Company.CompanyName, Employee.EmployeeName, Employee.FatherName, Employee.ContactNo, Employee.Address, Employee.DOB, Employee.JoiningDate, Employee.Description, Employee.EmployeeLogin, EmployeeImage, EmployeeCode, ShiftDetails.SDID, ShiftDetails.ChangeDate, ShiftDetails.TimeFrom, ShiftDetails.TimeTo, Employee.EmployeeStatus";
            cmdEmployees.CommandText += ", CNIC, MobileNetwork, EmployeeSignature FROM Employee Left JOIN ShiftDetails ON Employee.EmployeeID = ShiftDetails.EmployeeID INNER JOIN Department ON Employee.DepartmentID = Department.DepartmentID INNER JOIN Designation ON Employee.DesignationID = Designation.DesignationID INNER JOIN Company On Company.CompanyID = Employee.CompanyID WHERE (ShiftDetails.SDID = (SELECT MAX(SDID) FROM ShiftDetails WHERE (EmployeeID = Employee.EmployeeID))) AND (Employee.EmployeeID = @EmployeeID) Order By Employee.EmployeeID";
            cmdEmployees.Parameters.AddWithValue("@EmployeeID", (Int32)EmployeeID);
            adpEmployees.SelectCommand = cmdEmployees;
            if (Con.State != ConnectionState.Open)
                Con.Open();
            adpEmployees.Fill(dsEmployees);
            Con.Close();

            return dsEmployees;
        }

        public Boolean EmployeeCodeExist(string code)
        {
            SqlCommand cmdCode = new SqlCommand();
            cmdCode.Connection = Con;
            cmdCode.CommandText = "Select EmployeeID from Employee where EmployeeCode = @EmployeeCode";
            cmdCode.Parameters.AddWithValue("@EmployeeCode", (String)code);
            if (Con.State != ConnectionState.Open)
                Con.Open();
            Int32 EmpID = 0;

            EmpID = Convert.ToInt32(cmdCode.ExecuteScalar());
            Con.Close();
            if (EmpID == 0)
                return false;
            else
                return true;
        }

        

        

        public bool DeleteEmployee(Int32 EmpID)
        {
            try
            {
            SqlCommand cmdDeleteEmployee = new SqlCommand();

            cmdDeleteEmployee.Connection = Con;
            cmdDeleteEmployee.CommandText = "Delete from Employee where EmployeeID = @EmployeeID";
            cmdDeleteEmployee.Parameters.AddWithValue("@EmployeeID", EmpID);
            if (Con.State != ConnectionState.Open)
                Con.Open();
            cmdDeleteEmployee.ExecuteNonQuery();
            Con.Close();
            
            return true;
            }
            catch
            {
                return false;
            }
        }


        public DataSet GetShiftTiming(Int32 EmpID)
        {

            SqlDataAdapter adpEmployee = new SqlDataAdapter();
            SqlCommand cmdEmployee = new SqlCommand();
            DataSet dsTiming = new DataSet();

            cmdEmployee.Connection = Con;
            cmdEmployee.CommandText = "Select Convert(varchar(10), TimeFrom, 108) as TimeFrom, Convert(varchar(10), TimeTo, 108) as TimeTo, EmployeeID from ShiftDetails where SDID = (Select Max(SDID) from ShiftDetails where EmployeeID = @EmployeeID)";
            cmdEmployee.Parameters.AddWithValue("@EmployeeID", EmpID);
            adpEmployee.SelectCommand = cmdEmployee;
            if (Con.State != ConnectionState.Open)
                Con.Open();
            adpEmployee.Fill(dsTiming, "ShiftDetails");

            cmdEmployee.CommandText = "Select EmployeeID, EmployeeCode, EmployeeName, EmployeeImage, (Select TrackDate from AttendanceDetails ";
            cmdEmployee.CommandText += "where ADID = (Select IsNull(Max(ADID),0) from AttendanceDetails where EmployeeID = E.EmployeeID)) AS LastInOutDate,";
            cmdEmployee.CommandText += "(Select InOutTime from AttendanceDetails ";
            cmdEmployee.CommandText += "where ADID = (Select IsNull(Max(ADID),0) from AttendanceDetails where EmployeeID = E.EmployeeID)) AS LastInOutTime,";
            cmdEmployee.CommandText += "(Select InOutStatus from AttendanceDetails ";
            cmdEmployee.CommandText += "where ADID = (Select IsNull(Max(ADID),0) from AttendanceDetails where EmployeeID = E.EmployeeID))AS LastInOutStatus from Employee E Where EmployeeID = @EmployeeID";

            adpEmployee.Fill(dsTiming, "EmployeeDetails");
            Con.Close();
            
            return dsTiming;

        }

        public DataSet LoadAvailableEmployees()
        {
            SqlCommand cmdEmployees = new SqlCommand();
            SqlDataAdapter adpEmployees = new SqlDataAdapter();
            DataSet dsEmployees = new DataSet();
            DataSet dsAvailableEmployees = new DataSet();

            cmdEmployees.Connection = Con;
            cmdEmployees.CommandText = "Select EmployeeID, EmployeeName, Designation, LastTaskAssigned, LastTaskCompleted, Department from VwEmployeeDetails Group By  EmployeeID, EmployeeName, Designation, LastTaskAssigned, LastTaskCompleted, Department";
            adpEmployees.SelectCommand = cmdEmployees;
            if (Con.State != ConnectionState.Open)
                Con.Open();
            adpEmployees.Fill(dsEmployees, "Employees");

            cmdEmployees.Connection = Con;
            cmdEmployees.CommandText = "SELECT Task.Status, TaskAssignment.EmployeeID FROM Task INNER JOIN TaskAssignment ON Task.TaskID = TaskAssignment.TaskID Group By Task.Status, TaskAssignment.EmployeeID";
            adpEmployees.SelectCommand = cmdEmployees;
            if (Con.State != ConnectionState.Open)
                Con.Open();
            adpEmployees.Fill(dsEmployees, "Status");

            Con.Close();
            

            dsAvailableEmployees.Tables.Add("AvailableEmployees");
            dsAvailableEmployees.Tables["AvailableEmployees"].Columns.Add("EmployeeID");
            dsAvailableEmployees.Tables["AvailableEmployees"].Columns.Add("EmployeeName");
            dsAvailableEmployees.Tables["AvailableEmployees"].Columns.Add("Designation");
            dsAvailableEmployees.Tables["AvailableEmployees"].Columns.Add("LastTaskAssigned", Type.GetType("System.DateTime"));
            dsAvailableEmployees.Tables["AvailableEmployees"].Columns.Add("LastTaskCompleted", Type.GetType("System.DateTime"));
            dsAvailableEmployees.Tables["AvailableEmployees"].Columns.Add("Department");

            //Int32 dsIndex = 0;

            for (int i = 0; i <= dsEmployees.Tables["Employees"].Rows.Count - 1; i++)
            {
                if (Convert.ToInt32(dsEmployees.Tables["Employees"].Rows[i][0]) != 0)
                {
                    Boolean Found = false;
                    for (int j = 0; j <= dsEmployees.Tables["Status"].Rows.Count - 1; j++)
                    {
                        if (dsEmployees.Tables["Employees"].Rows[i][0].ToString() == dsEmployees.Tables["Status"].Rows[j][1].ToString() && Convert.ToInt32(dsEmployees.Tables["Status"].Rows[j][0]) == 1)
                        {
                            Found = true;
                            break;
                        }
                    }
                    if (Found == false)
                    {
                        dsAvailableEmployees.Tables["AvailableEmployees"].Rows.Add(dsEmployees.Tables["Employees"].Rows[i][0],
                            dsEmployees.Tables["Employees"].Rows[i][1],
                            dsEmployees.Tables["Employees"].Rows[i][2],
                            dsEmployees.Tables["Employees"].Rows[i][3],
                            dsEmployees.Tables["Employees"].Rows[i][4],
                            dsEmployees.Tables["Employees"].Rows[i][5]);
                    }
                }
            }

            return dsAvailableEmployees;
        }

        public DataSet TaskTimingReport(Int32 EmpID)
        {
            SqlCommand cmdEmpTask = new SqlCommand();
            SqlDataAdapter adpEmpTask = new SqlDataAdapter();
            DataSet dsEmpTask = new DataSet();

            cmdEmpTask.Connection = Con;
            cmdEmpTask.CommandText = "SELECT Employee.EmployeeID, Employee.EmployeeName, Designation.Designation, Task.TaskID, Project.ProjectName, TaskType.TaskType, TaskAssignment.PartialCompletionDate, TaskAssignment.CompletedAs, Task.CreditHours ";
            cmdEmpTask.CommandText += "FROM Task INNER JOIN Project ON Task.ProjectID = Project.ProjectID INNER JOIN TaskType ON Task.TaskTypeID = TaskType.TaskTypeID INNER JOIN TaskAssignment ON Task.TaskID = TaskAssignment.TaskID INNER JOIN ";
            cmdEmpTask.CommandText += "Employee ON TaskAssignment.EmployeeID = Employee.EmployeeID INNER JOIN Designation ON Employee.DesignationID = Designation.DesignationID WHERE (TaskAssignment.EmployeeID = @EmployeeID) AND (TaskAssignment.PartialStatus = 2)";

            cmdEmpTask.Parameters.AddWithValue("@EmployeeID", (Int32)EmpID);
            adpEmpTask.SelectCommand = cmdEmpTask;

            if (Con.State != ConnectionState.Open)
                Con.Open();

            adpEmpTask.Fill(dsEmpTask, "EmployeeTasks");

            cmdEmpTask.CommandText = "SELECT TimeFrom, TimeTo from ShiftDetails where SDID = (Select Max(SDID) from ShiftDetails where EmployeeID = @EmployeeID)";

            //cmdEmpTask.Parameters.AddWithValue("@EmployeeID", (Int32)EmpID);
            adpEmpTask.SelectCommand = cmdEmpTask;
            adpEmpTask.Fill(dsEmpTask, "ShiftDetails");

            cmdEmpTask.CommandText = "Select WEID, Weekend from Weekend";
            adpEmpTask.SelectCommand = cmdEmpTask;
            adpEmpTask.Fill(dsEmpTask, "Weekend");

            DataColumn[] col = new DataColumn[1];
            col[0] = dsEmpTask.Tables["Weekend"].Columns[1];
            dsEmpTask.Tables["Weekend"].PrimaryKey = col;

            DataSet dsFinalReturn = new DataSet();
            dsFinalReturn.Tables.Add("EmployeeTaskDetails");

            dsFinalReturn.Tables["EmployeeTaskDetails"].Columns.Add("EmployeeID", Type.GetType("System.Int32"));
            dsFinalReturn.Tables["EmployeeTaskDetails"].Columns.Add("EmployeeName", Type.GetType("System.String"));
            dsFinalReturn.Tables["EmployeeTaskDetails"].Columns.Add("Designation", Type.GetType("System.String"));
            dsFinalReturn.Tables["EmployeeTaskDetails"].Columns.Add("TaskID",Type.GetType("System.Int32"));
            dsFinalReturn.Tables["EmployeeTaskDetails"].Columns.Add("ProjectName",Type.GetType("System.String"));
            dsFinalReturn.Tables["EmployeeTaskDetails"].Columns.Add("TaskType", Type.GetType("System.String"));
            dsFinalReturn.Tables["EmployeeTaskDetails"].Columns.Add("CompletionDate", Type.GetType("System.DateTime"));
            dsFinalReturn.Tables["EmployeeTaskDetails"].Columns.Add("CompletedAs", Type.GetType("System.String"));
            dsFinalReturn.Tables["EmployeeTaskDetails"].Columns.Add("CreditHours", Type.GetType("System.Double"));
            dsFinalReturn.Tables["EmployeeTaskDetails"].Columns.Add("WorkingHours", Type.GetType("System.Int32"));
            //dsFinalReturn.Tables["EmployeeTaskDetails"].Columns.Add("WorkingMinutes");

            for (int i = 0; i <= dsEmpTask.Tables["EmployeeTasks"].Rows.Count - 1; i++)
            {
                SqlCommand cmdnEmpTask = new SqlCommand();
                cmdnEmpTask.Connection = Con;
                if (dsEmpTask.Tables.Contains("TaskTiming")) 
                dsEmpTask.Tables["TaskTiming"].Rows.Clear();
                cmdnEmpTask.CommandText = "SELECT AssignmentDate, AssignedTime, SubmissionDate, SubmissionTime from TaskTiming where TaskID = @TaskID AND EmployeeID = @EmployeeID";
                cmdnEmpTask.Parameters.AddWithValue("@EmployeeID", (Int32)Convert.ToInt32(dsEmpTask.Tables["EmployeeTasks"].Rows[i]["EmployeeID"]));
                cmdnEmpTask.Parameters.AddWithValue("@TaskID", (Int32)Convert.ToInt32(dsEmpTask.Tables["EmployeeTasks"].Rows[i]["TaskID"]));
                adpEmpTask.SelectCommand = cmdnEmpTask;

                if (Con.State != ConnectionState.Open)
                    Con.Open();

                adpEmpTask.Fill(dsEmpTask, "TaskTiming");

                Double HourTaken = 0;
                //Double MinutesTaken = 0;

                string strStartTime = " " + Convert.ToDateTime(dsEmpTask.Tables["ShiftDetails"].Rows[0]["TimeFrom"]).ToShortTimeString();
                string strEndTime = " " + Convert.ToDateTime(dsEmpTask.Tables["ShiftDetails"].Rows[0]["TimeTo"]).ToShortTimeString();

                for (int j = 0; j <= dsEmpTask.Tables["TaskTiming"].Rows.Count - 1; j++)
                {
                    DateTime AssignmentDate = Convert.ToDateTime(dsEmpTask.Tables["TaskTiming"].Rows[j]["AssignmentDate"]);
                    DateTime AssignedTime = Convert.ToDateTime(dsEmpTask.Tables["TaskTiming"].Rows[j]["AssignedTime"]);
                    DateTime SubmissionDate = Convert.ToDateTime(dsEmpTask.Tables["TaskTiming"].Rows[j]["SubmissionDate"]);
                    DateTime SubmissionTime = Convert.ToDateTime(dsEmpTask.Tables["TaskTiming"].Rows[j]["SubmissionTime"]);

                    //put conditions here                    

                    DateTime AssignedDateTime = Convert.ToDateTime(AssignmentDate.Date.ToShortDateString() + " " + AssignedTime.ToShortTimeString());
                    DateTime SubmissionDateTime = Convert.ToDateTime(SubmissionDate.Date.ToShortDateString() + " " + SubmissionTime.ToShortTimeString());
                                        
                    DateTime StartDate;
                    DateTime EndDate;
                    //DateTime AssignedDateTime = Convert.ToDateTime("10/20/2010 02:00 PM");
                    //DateTime SubmissionDateTime = Convert.ToDateTime("10/25/2010 06:00 PM");
                    StartDate = AssignedDateTime;
                    string strTempDate;

                    if (dsEmpTask.Tables["Weekend"].Rows.Contains(AssignedDateTime.DayOfWeek))
                    {
                        AssignedDateTime = AssignedDateTime.AddDays(1);
                        strTempDate = AssignedDateTime.ToShortDateString() + strStartTime;
                        StartDate = Convert.ToDateTime(strTempDate);
                    }

                    if (dsEmpTask.Tables["Weekend"].Rows.Contains(SubmissionDateTime.DayOfWeek) || (!dsEmpTask.Tables["Weekend"].Rows.Contains(SubmissionDateTime.DayOfWeek) && Convert.ToDateTime(SubmissionDateTime.ToShortTimeString()) < Convert.ToDateTime(strStartTime)))
                    {
                        SubmissionDateTime = SubmissionDateTime.AddDays(-1);
                        strTempDate = SubmissionDateTime.ToShortDateString() + strEndTime;
                        EndDate = Convert.ToDateTime(strTempDate);
                        SubmissionDateTime = Convert.ToDateTime(strTempDate);
                    }

                    while (AssignedDateTime.Date <= SubmissionDateTime.Date)
                    {
                        if (dsEmpTask.Tables["Weekend"].Rows.Contains(AssignedDateTime.DayOfWeek))
                        {
                            strTempDate = AssignedDateTime.AddDays(-1).ToShortDateString() + strEndTime;
                            EndDate = Convert.ToDateTime(strTempDate);
                            HourTaken += GetBusinessHours(Convert.ToDateTime(strStartTime), Convert.ToDateTime(strEndTime), StartDate, EndDate);
                            strTempDate = AssignedDateTime.AddDays(1).ToShortDateString() + strStartTime;
                            StartDate = Convert.ToDateTime(strTempDate);
                        }
                        else if (AssignedDateTime.Date == SubmissionDateTime.Date)                       
                        {
                            EndDate = SubmissionDateTime;
                            HourTaken += GetBusinessHours(Convert.ToDateTime(strStartTime), Convert.ToDateTime(strEndTime), StartDate, EndDate);
                        }
                        AssignedDateTime = AssignedDateTime.AddDays(1);
                    }
                }

                dsFinalReturn.Tables["EmployeeTaskDetails"].Rows.Add(dsEmpTask.Tables["EmployeeTasks"].Rows[i]["EmployeeID"],
                    dsEmpTask.Tables["EmployeeTasks"].Rows[i]["EmployeeName"],
                    dsEmpTask.Tables["EmployeeTasks"].Rows[i]["Designation"],
                    dsEmpTask.Tables["EmployeeTasks"].Rows[i]["TaskID"],
                    dsEmpTask.Tables["EmployeeTasks"].Rows[i]["ProjectName"],
                    dsEmpTask.Tables["EmployeeTasks"].Rows[i]["TaskType"],
                    dsEmpTask.Tables["EmployeeTasks"].Rows[i]["PartialCompletionDate"],
                    dsEmpTask.Tables["EmployeeTasks"].Rows[i]["CompletedAs"],
                    dsEmpTask.Tables["EmployeeTasks"].Rows[i]["CreditHours"],
                    HourTaken);

                cmdnEmpTask.Dispose();

            }

            Con.Close();

            return dsFinalReturn;
        }

        public DataSet TaskTimingReport()
        {
            SqlCommand cmdEmpTask = new SqlCommand();
            SqlDataAdapter adpEmpTask = new SqlDataAdapter();
            DataSet dsEmpTask = new DataSet();

            cmdEmpTask.Connection = Con;
            cmdEmpTask.CommandText = "SELECT Employee.EmployeeID, Employee.EmployeeName, Designation.Designation, Task.TaskID, Project.ProjectName, Project.StartingDate, Project.EndDate, TaskType.TaskType, TaskAssignment.PartialCompletionDate, TaskAssignment.CompletedAs, Task.CreditHours, ShiftDetails.TimeFrom, ShiftDetails.TimeTo ";
            cmdEmpTask.CommandText += "FROM Task INNER JOIN Project ON Task.ProjectID = Project.ProjectID INNER JOIN TaskType ON Task.TaskTypeID = TaskType.TaskTypeID INNER JOIN TaskAssignment ON Task.TaskID = TaskAssignment.TaskID INNER JOIN ";
            cmdEmpTask.CommandText += "Employee ON TaskAssignment.EmployeeID = Employee.EmployeeID INNER JOIN ShiftDetails On ShiftDetails.EmployeeID = Employee.EmployeeID INNER JOIN Designation ON Employee.DesignationID = Designation.DesignationID WHERE (TaskAssignment.PartialStatus = 2)";

            //cmdEmpTask.Parameters.AddWithValue("@EmployeeID", (Int32)EmpID);
            adpEmpTask.SelectCommand = cmdEmpTask;

            if (Con.State != ConnectionState.Open)
                Con.Open();

            adpEmpTask.Fill(dsEmpTask, "EmployeeTasks");

            //cmdEmpTask.CommandText = "SELECT TimeFrom, TimeTo from ShiftDetails where SDID = (Select Max(SDID) from ShiftDetails where EmployeeID = @EmployeeID)";

            ////cmdEmpTask.Parameters.AddWithValue("@EmployeeID", (Int32)EmpID);
            //adpEmpTask.SelectCommand = cmdEmpTask;

            //if (Con.State != ConnectionState.Open)
            //    Con.Open();

            //adpEmpTask.Fill(dsEmpTask, "ShiftDetails");

            cmdEmpTask.CommandText = "Select WEID, Weekend from Weekend";
            adpEmpTask.SelectCommand = cmdEmpTask;
            adpEmpTask.Fill(dsEmpTask, "Weekend");

            DataColumn[] col = new DataColumn[1];
            col[0] = dsEmpTask.Tables["Weekend"].Columns[1];
            dsEmpTask.Tables["Weekend"].PrimaryKey = col;

            DataSet dsFinalReturn = new DataSet();
            dsFinalReturn.Tables.Add("EmployeeTaskDetails");

            dsFinalReturn.Tables["EmployeeTaskDetails"].Columns.Add("EmployeeID", Type.GetType("System.Int32"));
            dsFinalReturn.Tables["EmployeeTaskDetails"].Columns.Add("EmployeeName", Type.GetType("System.String"));
            dsFinalReturn.Tables["EmployeeTaskDetails"].Columns.Add("Designation", Type.GetType("System.String"));
            dsFinalReturn.Tables["EmployeeTaskDetails"].Columns.Add("TaskID", Type.GetType("System.Int32"));
            dsFinalReturn.Tables["EmployeeTaskDetails"].Columns.Add("ProjectName", Type.GetType("System.String"));
            dsFinalReturn.Tables["EmployeeTaskDetails"].Columns.Add("StartingDate", Type.GetType("System.DateTime"));
            dsFinalReturn.Tables["EmployeeTaskDetails"].Columns.Add("EndDate", Type.GetType("System.DateTime"));
            dsFinalReturn.Tables["EmployeeTaskDetails"].Columns.Add("TaskType", Type.GetType("System.String"));
            dsFinalReturn.Tables["EmployeeTaskDetails"].Columns.Add("CompletionDate", Type.GetType("System.DateTime"));
            dsFinalReturn.Tables["EmployeeTaskDetails"].Columns.Add("CompletedAs", Type.GetType("System.String"));
            dsFinalReturn.Tables["EmployeeTaskDetails"].Columns.Add("CreditHours", Type.GetType("System.Double"));
            dsFinalReturn.Tables["EmployeeTaskDetails"].Columns.Add("WorkingHours", Type.GetType("System.Int32"));
            //dsFinalReturn.Tables["EmployeeTaskDetails"].Columns.Add("WorkingMinutes");

            for (int i = 0; i <= dsEmpTask.Tables["EmployeeTasks"].Rows.Count - 1; i++)
            {
                SqlCommand cmdnEmpTask = new SqlCommand();
                cmdnEmpTask.Connection = Con;
                if (dsEmpTask.Tables.Contains("TaskTiming"))
                    dsEmpTask.Tables["TaskTiming"].Rows.Clear();
                cmdnEmpTask.CommandText = "SELECT AssignmentDate, AssignedTime, SubmissionDate, SubmissionTime from TaskTiming where TaskID = @TaskID AND EmployeeID = @EmployeeID";
                cmdnEmpTask.Parameters.AddWithValue("@EmployeeID", (Int32)Convert.ToInt32(dsEmpTask.Tables["EmployeeTasks"].Rows[i]["EmployeeID"]));
                cmdnEmpTask.Parameters.AddWithValue("@TaskID", (Int32)Convert.ToInt32(dsEmpTask.Tables["EmployeeTasks"].Rows[i]["TaskID"]));
                adpEmpTask.SelectCommand = cmdnEmpTask;

                if (Con.State != ConnectionState.Open)
                    Con.Open();

                adpEmpTask.Fill(dsEmpTask, "TaskTiming");

                Double HourTaken = 0;
                //Double MinutesTaken = 0;

                string strStartTime = " " + Convert.ToDateTime(dsEmpTask.Tables["EmployeeTasks"].Rows[i]["TimeFrom"]).ToShortTimeString();
                string strEndTime = " " + Convert.ToDateTime(dsEmpTask.Tables["EmployeeTasks"].Rows[i]["TimeTo"]).ToShortTimeString();

                for (int j = 0; j <= dsEmpTask.Tables["TaskTiming"].Rows.Count - 1; j++)
                {
                    DateTime AssignmentDate = Convert.ToDateTime(dsEmpTask.Tables["TaskTiming"].Rows[j]["AssignmentDate"]);
                    DateTime AssignedTime = Convert.ToDateTime(dsEmpTask.Tables["TaskTiming"].Rows[j]["AssignedTime"]);
                    DateTime SubmissionDate = Convert.ToDateTime(dsEmpTask.Tables["TaskTiming"].Rows[j]["SubmissionDate"]);
                    DateTime SubmissionTime = Convert.ToDateTime(dsEmpTask.Tables["TaskTiming"].Rows[j]["SubmissionTime"]);

                    //put conditions here                    

                    DateTime AssignedDateTime = Convert.ToDateTime(AssignmentDate.Date.ToShortDateString() + " " + AssignedTime.ToShortTimeString());
                    DateTime SubmissionDateTime = Convert.ToDateTime(SubmissionDate.Date.ToShortDateString() + " " + SubmissionTime.ToShortTimeString());

                    DateTime StartDate;
                    DateTime EndDate;
                    //DateTime AssignedDateTime = Convert.ToDateTime("10/20/2010 02:00 PM");
                    //DateTime SubmissionDateTime = Convert.ToDateTime("10/25/2010 06:00 PM");
                    StartDate = AssignedDateTime;
                    string strTempDate;

                    if (dsEmpTask.Tables["Weekend"].Rows.Contains(AssignedDateTime.DayOfWeek))
                    {
                        AssignedDateTime = AssignedDateTime.AddDays(1);
                        strTempDate = AssignedDateTime.ToShortDateString() + strStartTime;
                        StartDate = Convert.ToDateTime(strTempDate);
                    }

                    if (dsEmpTask.Tables["Weekend"].Rows.Contains(SubmissionDateTime.DayOfWeek) || (!dsEmpTask.Tables["Weekend"].Rows.Contains(SubmissionDateTime.DayOfWeek) && Convert.ToDateTime(SubmissionDateTime.ToShortTimeString()) < Convert.ToDateTime(strStartTime)))
                    {
                        SubmissionDateTime = SubmissionDateTime.AddDays(-1);
                        strTempDate = SubmissionDateTime.ToShortDateString() + strEndTime;
                        EndDate = Convert.ToDateTime(strTempDate);
                        SubmissionDateTime = Convert.ToDateTime(strTempDate);
                    }

                    while (AssignedDateTime.Date <= SubmissionDateTime.Date)
                    {
                        if (dsEmpTask.Tables["Weekend"].Rows.Contains(AssignedDateTime.DayOfWeek))
                        {
                            strTempDate = AssignedDateTime.AddDays(-1).ToShortDateString() + strEndTime;
                            EndDate = Convert.ToDateTime(strTempDate);
                            HourTaken += GetBusinessHours(Convert.ToDateTime(strStartTime), Convert.ToDateTime(strEndTime), StartDate, EndDate);
                            strTempDate = AssignedDateTime.AddDays(1).ToShortDateString() + strStartTime;
                            StartDate = Convert.ToDateTime(strTempDate);
                        }
                        else if (AssignedDateTime.Date == SubmissionDateTime.Date)
                        {
                            EndDate = SubmissionDateTime;
                            HourTaken += GetBusinessHours(Convert.ToDateTime(strStartTime), Convert.ToDateTime(strEndTime), StartDate, EndDate);
                        }
                        AssignedDateTime = AssignedDateTime.AddDays(1);
                    }
                }

                dsFinalReturn.Tables["EmployeeTaskDetails"].Rows.Add(dsEmpTask.Tables["EmployeeTasks"].Rows[i]["EmployeeID"],
                    dsEmpTask.Tables["EmployeeTasks"].Rows[i]["EmployeeName"],
                    dsEmpTask.Tables["EmployeeTasks"].Rows[i]["Designation"],
                    dsEmpTask.Tables["EmployeeTasks"].Rows[i]["TaskID"],
                    dsEmpTask.Tables["EmployeeTasks"].Rows[i]["ProjectName"],
                    dsEmpTask.Tables["EmployeeTasks"].Rows[i]["StartingDate"],
                    dsEmpTask.Tables["EmployeeTasks"].Rows[i]["EndDate"],
                    dsEmpTask.Tables["EmployeeTasks"].Rows[i]["TaskType"],
                    dsEmpTask.Tables["EmployeeTasks"].Rows[i]["PartialCompletionDate"],
                    dsEmpTask.Tables["EmployeeTasks"].Rows[i]["CompletedAs"],
                    dsEmpTask.Tables["EmployeeTasks"].Rows[i]["CreditHours"],
                    HourTaken);

                cmdnEmpTask.Dispose();

            }

            Con.Close();

            return dsFinalReturn;
        }

        public DataSet LoadEmployeesByName(String EmployeeName)
        {
            DataSet dsAP = new DataSet();
            SqlCommand cmdAP = new SqlCommand();
            SqlDataAdapter adpAP = new SqlDataAdapter();

            cmdAP.Connection = Con;
            cmdAP.CommandText = "Select EmployeeID, EmployeeName from Employee where EmployeeName Like @EmployeeName";
            cmdAP.Parameters.AddWithValue("@EmployeeName", Convert.ToString("%" + EmployeeName + "%"));
            adpAP.SelectCommand = cmdAP;

            if (Con.State != ConnectionState.Open)
                Con.Open();

            adpAP.Fill(dsAP, "Employee");
            Con.Close();

            return dsAP;
        }

        public DataSet LoadEmployeesByCode(String EmployeeCode)
        {
            DataSet dsAP = new DataSet();
            SqlCommand cmdAP = new SqlCommand();
            SqlDataAdapter adpAP = new SqlDataAdapter();

            cmdAP.Connection = Con;
            cmdAP.CommandText = "Select EmployeeID, EmployeeName from Employee where EmployeeCode Like @EmployeeCode";
            cmdAP.Parameters.AddWithValue("@EmployeeCode", Convert.ToString("%" + EmployeeCode + "%"));
            adpAP.SelectCommand = cmdAP;

            if (Con.State != ConnectionState.Open)
                Con.Open();

            adpAP.Fill(dsAP, "Employee");
            Con.Close();

            return dsAP;
        }

        /// <summary>
        /// Calulates business hours between two times provided,
        /// Maintaining employee shift timing.
        /// </summary>
        /// <param name="ShiftStartTime"></param>
        /// <param name="ShiftEndTime"></param>
        /// <param name="StartDateTime"></param>
        /// <param name="EndDateTime"></param>
        /// <returns></returns>
        public Double GetBusinessHours(DateTime ShiftStartTime, DateTime ShiftEndTime, DateTime StartDateTime, DateTime EndDateTime)
        {
            SqlCommand cmdhour = new SqlCommand();
            cmdhour.Connection = Con;

            cmdhour.CommandText = "drop table week_calendar \n";

            cmdhour.CommandText += "CREATE TABLE [dbo].[week_calendar] (\n";
            cmdhour.CommandText += "	[day_number] [varchar] (50) NOT NULL ,\n";
            cmdhour.CommandText += "	[day_name] [varchar] (50) NULL ,\n";
            cmdhour.CommandText += "	[begin_time] [datetime] NULL ,\n";
            cmdhour.CommandText += "	[end_time] [datetime] NULL ,\n";
            cmdhour.CommandText += "	[duration] [real] NULL \n";
            cmdhour.CommandText += ") ON [PRIMARY]\n";

            cmdhour.CommandText += "insert into week_calendar \n";
            cmdhour.CommandText += "select 1,             'Monday',      '" + ShiftStartTime.ToShortTimeString() + "',    '" + ShiftEndTime.ToShortTimeString() + "',   DateDiff(Second, '" + ShiftStartTime.ToShortTimeString() + "', '" + ShiftEndTime.ToShortTimeString() + "') union all \n";
            cmdhour.CommandText += "select 2,             'Tuesday',     '" + ShiftStartTime.ToShortTimeString() + "',    '" + ShiftEndTime.ToShortTimeString() + "',   DateDiff(Second, '" + ShiftStartTime.ToShortTimeString() + "', '" + ShiftEndTime.ToShortTimeString() + "') union all \n";
            cmdhour.CommandText += "select 3,             'Wednesday',   '" + ShiftStartTime.ToShortTimeString() + "',    '" + ShiftEndTime.ToShortTimeString() + "',   DateDiff(Second, '" + ShiftStartTime.ToShortTimeString() + "', '" + ShiftEndTime.ToShortTimeString() + "') union all \n";
            cmdhour.CommandText += "select 4,             'Thursday',    '" + ShiftStartTime.ToShortTimeString() + "',    '" + ShiftEndTime.ToShortTimeString() + "',   DateDiff(Second, '" + ShiftStartTime.ToShortTimeString() + "', '" + ShiftEndTime.ToShortTimeString() + "') union all \n";
            cmdhour.CommandText += "select 5,             'Friday',      '" + ShiftStartTime.ToShortTimeString() + "',    '" + ShiftEndTime.ToShortTimeString() + "',   DateDiff(Second, '" + ShiftStartTime.ToShortTimeString() + "', '" + ShiftEndTime.ToShortTimeString() + "') union all \n";
            cmdhour.CommandText += "select 6,             'Saturday',    '" + ShiftStartTime.ToShortTimeString() + "',    '" + ShiftEndTime.ToShortTimeString() + "',   DateDiff(Second, '" + ShiftStartTime.ToShortTimeString() + "', '" + ShiftEndTime.ToShortTimeString() + "') union all \n";
            cmdhour.CommandText += "select 7,             'Sunday',      '" + ShiftStartTime.ToShortTimeString() + "',    '" + ShiftEndTime.ToShortTimeString() + "',   DateDiff(Second, '" + ShiftStartTime.ToShortTimeString() + "', '" + ShiftEndTime.ToShortTimeString() + "') \n";

            cmdhour.CommandText += "declare	@start_date	datetime, \n";
            cmdhour.CommandText += "	@end_date	datetime \n";

            cmdhour.CommandText += "select	@start_date = '" + Convert.ToDateTime(StartDateTime) + "',\n";
            cmdhour.CommandText += "	@end_date = '" + Convert.ToDateTime(EndDateTime) +"' \n";

            cmdhour.CommandText += "select	total_hours = sum(case 	\n";
            cmdhour.CommandText += "			when dateadd(day, datediff(day, 0, @start_date), 0) = dateadd(day, datediff(day, 0, @end_date), 0) then \n";
            cmdhour.CommandText += "			datediff(second, @start_date, @end_date) \n";
            cmdhour.CommandText += "		when [DATE] = dateadd(day, datediff(day, 0, @start_date), 0) then \n";
            cmdhour.CommandText += "			case 	\n";
            cmdhour.CommandText += "			when @start_date > [DATE] + begin_time then datediff(second, @start_date, [DATE] + end_time) \n";
            cmdhour.CommandText += "			else duration \n";
            cmdhour.CommandText += "			end \n";
            cmdhour.CommandText += "		when [DATE] = dateadd(day, datediff(day, 0, @end_date), 0) then \n";
            cmdhour.CommandText += "			case	\n";
            cmdhour.CommandText += "			when @end_date	<  [DATE] + end_time then datediff(second, [DATE] + begin_time, @end_date) \n";
            cmdhour.CommandText += "			else duration \n";
            cmdhour.CommandText += "			end \n";
            cmdhour.CommandText += "else duration \n";
            cmdhour.CommandText += "		end \n";
            cmdhour.CommandText += "		  ) \n";
            cmdhour.CommandText += "		/ 60.0\n";
            cmdhour.CommandText += "from	F_TABLE_DATE(@start_date, @end_date) d inner join week_calendar c \n";
            cmdhour.CommandText += "on	d.WEEKDAY_NAME_LONG = c.day_name \n";

            if (Con.State != ConnectionState.Open)
                Con.Open();
            return Convert.ToDouble(cmdhour.ExecuteScalar());
        }

        /// <summary>
        /// Calculates the time taken by employee
        /// for specific task. Excluding off time and leaves.
        /// </summary>
        /// <param name="EmpID"></param>
        /// <param name="TaskID"></param>
        /// <returns></returns>
        public double EmployeeCurrentTaskTime(Int32 EmpID, Int32 TaskID)
        {
            SqlCommand cmdEmpTask = new SqlCommand();
            SqlDataAdapter adpEmpTask = new SqlDataAdapter();
            DataSet dsEmpTask = new DataSet();
            DataTable dtTemp = new DataTable();
            dtTemp.Columns.Add("TimeTaken",Type.GetType("System.Double"));
            dtTemp.Columns.Add("ShiftTime", Type.GetType("System.Double"));

            cmdEmpTask.Connection = Con;           

            cmdEmpTask.CommandText = "SELECT TimeFrom, TimeTo from ShiftDetails where SDID = (Select Max(SDID) from ShiftDetails where EmployeeID = @EmployeeID)";
            cmdEmpTask.Parameters.AddWithValue("@EmployeeID", (Int32)EmpID);
            adpEmpTask.SelectCommand = cmdEmpTask;

            if (Con.State != ConnectionState.Open)
                Con.Open();

            adpEmpTask.Fill(dsEmpTask, "ShiftDetails");

            cmdEmpTask.CommandText = "Select WEID, Weekend from Weekend";
            adpEmpTask.SelectCommand = cmdEmpTask;
            adpEmpTask.Fill(dsEmpTask, "Weekend");

            DataColumn[] col = new DataColumn[1];
            col[0] = dsEmpTask.Tables["Weekend"].Columns[1];
            dsEmpTask.Tables["Weekend"].PrimaryKey = col;
                                 
                        
            SqlCommand cmdnEmpTask = new SqlCommand();
            cmdnEmpTask.Connection = Con;            
            cmdnEmpTask.CommandText = "SELECT AssignmentDate, AssignedTime, SubmissionDate, SubmissionTime from TaskTiming where TaskID = @TaskID AND EmployeeID = @EmployeeID";
            cmdnEmpTask.Parameters.AddWithValue("@EmployeeID", (Int32)EmpID);
            cmdnEmpTask.Parameters.AddWithValue("@TaskID", (Int32)TaskID);
            adpEmpTask.SelectCommand = cmdnEmpTask;            
            adpEmpTask.Fill(dsEmpTask, "TaskTiming");

            Double HourTaken = 0;
            
            string strStartTime = " " + Convert.ToDateTime(dsEmpTask.Tables["ShiftDetails"].Rows[0]["TimeFrom"]).ToShortTimeString();
            string strEndTime = " " + Convert.ToDateTime(dsEmpTask.Tables["ShiftDetails"].Rows[0]["TimeTo"]).ToShortTimeString();

            for (int j = 0; j <= dsEmpTask.Tables["TaskTiming"].Rows.Count - 1; j++)
            {
                DateTime AssignmentDate = Convert.ToDateTime(dsEmpTask.Tables["TaskTiming"].Rows[j]["AssignmentDate"]);
                DateTime AssignedTime = Convert.ToDateTime(dsEmpTask.Tables["TaskTiming"].Rows[j]["AssignedTime"]);
                DateTime SubmissionDate;
                DateTime SubmissionTime;

                if (j != dsEmpTask.Tables["TaskTiming"].Rows.Count - 1)
                {
                    if (dsEmpTask.Tables["TaskTiming"].Rows[j]["SubmissionDate"].ToString() != "")
                        SubmissionDate = Convert.ToDateTime(dsEmpTask.Tables["TaskTiming"].Rows[j]["SubmissionDate"]);
                    else
                        SubmissionDate = DateTime.Today.Date;

                    
                    if (dsEmpTask.Tables["TaskTiming"].Rows[j]["SubmissionTime"].ToString() != "")
                        SubmissionTime = Convert.ToDateTime(dsEmpTask.Tables["TaskTiming"].Rows[j]["SubmissionTime"]);
                    else
                        SubmissionTime = DateTime.Now;
                }
                else
                {
                    SubmissionDate = DateTime.Now;
                    SubmissionTime = DateTime.Now;
                }

                //put conditions here                    

                DateTime AssignedDateTime = Convert.ToDateTime(AssignmentDate.Date.ToShortDateString() + " " + AssignedTime.ToShortTimeString());
                DateTime SubmissionDateTime = Convert.ToDateTime(SubmissionDate.Date.ToShortDateString() + " " + SubmissionTime.ToShortTimeString());
                //DateTime SubmissionDateTime = DateTime.Now;

                DateTime StartDate;
                DateTime EndDate;
                
                StartDate = AssignedDateTime;
                string strTempDate;

                if (dsEmpTask.Tables["Weekend"].Rows.Contains(AssignedDateTime.DayOfWeek)) //AssignedDateTime.DayOfWeek == DayOfWeek.Sunday)
                {
                    AssignedDateTime = AssignedDateTime.AddDays(1);
                    strTempDate = AssignedDateTime.ToShortDateString() + strStartTime;
                    StartDate = Convert.ToDateTime(strTempDate);
                }
                
//                if (SubmissionDateTime.DayOfWeek == DayOfWeek.Sunday || (SubmissionDateTime.DayOfWeek != DayOfWeek.Sunday && Convert.ToDateTime(SubmissionDateTime.ToShortTimeString()) < Convert.ToDateTime(strStartTime)))
                if (dsEmpTask.Tables["Weekend"].Rows.Contains(SubmissionDateTime.DayOfWeek) || (!dsEmpTask.Tables["Weekend"].Rows.Contains(SubmissionDateTime.DayOfWeek) && Convert.ToDateTime(SubmissionDateTime.ToShortTimeString()) < Convert.ToDateTime(strStartTime)))
                {
                    SubmissionDateTime = SubmissionDateTime.AddDays(-1);
                    strTempDate = SubmissionDateTime.ToShortDateString() + strEndTime;
                    EndDate = Convert.ToDateTime(strTempDate);
                    SubmissionDateTime = Convert.ToDateTime(strTempDate);
                }
                
                while (AssignedDateTime.Date <= SubmissionDateTime.Date)
                {

//                    if (dsEmpTask.Tables["Weekend"].Rows.Contains(AssignedDateTime.DayOfWeek))
                    if (dsEmpTask.Tables["Weekend"].Rows.Contains(AssignedDateTime.DayOfWeek))
                    {
                        strTempDate = AssignedDateTime.AddDays(-1).ToShortDateString() + strEndTime;
                        EndDate = Convert.ToDateTime(strTempDate);
                        HourTaken += GetBusinessHours(Convert.ToDateTime(strStartTime), Convert.ToDateTime(strEndTime), StartDate, EndDate);
                        strTempDate = AssignedDateTime.AddDays(1).ToShortDateString() + strStartTime;
                        StartDate = Convert.ToDateTime(strTempDate);
                    }
                    else if (AssignedDateTime.Date == SubmissionDateTime.Date)
                    {
                        EndDate = SubmissionDateTime;
                        HourTaken += GetBusinessHours(Convert.ToDateTime(strStartTime), Convert.ToDateTime(strEndTime), StartDate, EndDate);
                    }
                    AssignedDateTime = AssignedDateTime.AddDays(1);
                }

                cmdnEmpTask.Dispose();
            }

            double HoldTime = 0.0;

            HoldTime = EmployeeTaskHoldTime(EmpID, TaskID);
            HourTaken = HourTaken - HoldTime;
            Con.Close();

            return HourTaken;
        }


        public double EmployeeTaskHoldTime(Int32 EmpID, Int32 TaskID)
        {
            SqlCommand cmdEmpTask = new SqlCommand();
            SqlDataAdapter adpEmpTask = new SqlDataAdapter();
            DataSet dsEmpTask = new DataSet();
            DataTable dtTemp = new DataTable();
            dtTemp.Columns.Add("TimeTaken", Type.GetType("System.Double"));
            dtTemp.Columns.Add("ShiftTime", Type.GetType("System.Double"));

            cmdEmpTask.Connection = Con;

            cmdEmpTask.CommandText = "SELECT TimeFrom, TimeTo from ShiftDetails where SDID = (Select Max(SDID) from ShiftDetails where EmployeeID = @EmployeeID)";
            cmdEmpTask.Parameters.AddWithValue("@EmployeeID", (Int32)EmpID);
            adpEmpTask.SelectCommand = cmdEmpTask;

            if (Con.State != ConnectionState.Open)
                Con.Open();

            adpEmpTask.Fill(dsEmpTask, "ShiftDetails");

            cmdEmpTask.CommandText = "Select WEID, Weekend from Weekend";
            adpEmpTask.SelectCommand = cmdEmpTask;
            adpEmpTask.Fill(dsEmpTask, "Weekend");

            DataColumn[] col = new DataColumn[1];
            col[0] = dsEmpTask.Tables["Weekend"].Columns[1];
            dsEmpTask.Tables["Weekend"].PrimaryKey = col;


            SqlCommand cmdnEmpTask = new SqlCommand();
            cmdnEmpTask.Connection = Con;
            cmdnEmpTask.CommandText = "SELECT FromTime, ToTime from TaskHold where TaskID = @TaskID AND EmployeeID = @EmployeeID";
            cmdnEmpTask.Parameters.AddWithValue("@EmployeeID", (Int32)EmpID);
            cmdnEmpTask.Parameters.AddWithValue("@TaskID", (Int32)TaskID);
            adpEmpTask.SelectCommand = cmdnEmpTask;
            adpEmpTask.Fill(dsEmpTask, "TaskHold");

            Double HourTaken = 0;

            string strStartTime = " " + Convert.ToDateTime(dsEmpTask.Tables["ShiftDetails"].Rows[0]["TimeFrom"]).ToShortTimeString();
            string strEndTime = " " + Convert.ToDateTime(dsEmpTask.Tables["ShiftDetails"].Rows[0]["TimeTo"]).ToShortTimeString();

            for (int j = 0; j <= dsEmpTask.Tables["TaskHold"].Rows.Count - 1; j++)
            {
                DateTime HoldDate = Convert.ToDateTime(Convert.ToDateTime(dsEmpTask.Tables["TaskHold"].Rows[j]["FromTime"]).ToShortDateString());
                DateTime HoldTime = Convert.ToDateTime(Convert.ToDateTime(dsEmpTask.Tables["TaskHold"].Rows[j]["FromTime"]).ToShortTimeString());
                DateTime ReleaseDate;
                DateTime ReleaseTime;
                if (dsEmpTask.Tables["TaskHold"].Rows[j]["ToTime"].ToString() != "")
                {
                    ReleaseDate = Convert.ToDateTime(Convert.ToDateTime(dsEmpTask.Tables["TaskHold"].Rows[j]["ToTime"]).ToShortDateString());
                    ReleaseTime = Convert.ToDateTime(Convert.ToDateTime(dsEmpTask.Tables["TaskHold"].Rows[j]["ToTime"]).ToShortTimeString());
                }
                else
                {
                    ReleaseDate = DateTime.Today.Date;
                    ReleaseTime = Convert.ToDateTime(DateTime.Now.ToShortTimeString());
                }
                
                //if (dsEmpTask.Tables["TaskHold"].Rows[j]["SubmissionTime"].ToString() != "")
                //    SubmissionTime = Convert.ToDateTime(dsEmpTask.Tables["TaskHold"].Rows[j]["SubmissionTime"]);
                //else
                //    SubmissionTime = DateTime.Now;

                //put conditions here  

                DateTime HoldDateTime = new DateTime();

                if (Convert.ToDateTime(HoldTime.ToShortTimeString()) > Convert.ToDateTime(DateTime.Now.Date.ToShortDateString() + strEndTime))
                {
                    HoldDateTime = Convert.ToDateTime(HoldDate.Date.ToShortDateString() + strEndTime);
                }
                else
                {
                    HoldDateTime = Convert.ToDateTime(HoldDate.Date.ToShortDateString() + " " + HoldTime.ToShortTimeString());
                }
                DateTime ReleaseDateTime = Convert.ToDateTime(ReleaseDate.Date.ToShortDateString() + " " + ReleaseTime.ToShortTimeString());

                DateTime StartDate;
                DateTime EndDate;

                StartDate = HoldDateTime;
                string strTempDate;

                if (dsEmpTask.Tables["Weekend"].Rows.Contains(HoldDateTime.DayOfWeek)) //AssignedDateTime.DayOfWeek == DayOfWeek.Sunday)
                {
                    HoldDateTime = HoldDateTime.AddDays(1);
                    strTempDate = HoldDateTime.ToShortDateString() + strStartTime;
                    StartDate = Convert.ToDateTime(strTempDate);
                }

                //                if (SubmissionDateTime.DayOfWeek == DayOfWeek.Sunday || (SubmissionDateTime.DayOfWeek != DayOfWeek.Sunday && Convert.ToDateTime(SubmissionDateTime.ToShortTimeString()) < Convert.ToDateTime(strStartTime)))
                if (dsEmpTask.Tables["Weekend"].Rows.Contains(ReleaseDateTime.DayOfWeek) || (!dsEmpTask.Tables["Weekend"].Rows.Contains(ReleaseDateTime.DayOfWeek) && Convert.ToDateTime(ReleaseDateTime.ToShortTimeString()) < Convert.ToDateTime(strStartTime)))
                {
                    ReleaseDateTime = ReleaseDateTime.AddDays(-1);
                    strTempDate = ReleaseDateTime.ToShortDateString() + strEndTime;
                    EndDate = Convert.ToDateTime(strTempDate);
                    ReleaseDateTime = Convert.ToDateTime(strTempDate);
                }

                while (HoldDateTime.Date <= ReleaseDateTime.Date)
                {

                    if (dsEmpTask.Tables["Weekend"].Rows.Contains(HoldDateTime.DayOfWeek))
                    {
                        strTempDate = HoldDateTime.AddDays(-1).ToShortDateString() + strEndTime;
                        EndDate = Convert.ToDateTime(strTempDate);
                        HourTaken += GetBusinessHours(Convert.ToDateTime(strStartTime), Convert.ToDateTime(strEndTime), StartDate, EndDate);
                        strTempDate = HoldDateTime.AddDays(1).ToShortDateString() + strStartTime;
                        StartDate = Convert.ToDateTime(strTempDate);
                    }
                    else if (HoldDateTime.Date == ReleaseDateTime.Date)
                    {
                        EndDate = ReleaseDateTime;
                        HourTaken += GetBusinessHours(Convert.ToDateTime(strStartTime), Convert.ToDateTime(strEndTime), StartDate, EndDate);
                    }
                    HoldDateTime = HoldDateTime.AddDays(1);
                }

                cmdnEmpTask.Dispose();

            }

            Con.Close();

            return HourTaken;
        }
         
    }
}
