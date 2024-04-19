using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NewEventLogDLL;
using NewToolsDLL;
using NewEmployeeDLL;

namespace UpdateCurrentLocation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //setting up the classes
        WPFMessagesClass TheMessagesClass = new WPFMessagesClass();
        ToolsClass TheToolsClass = new ToolsClass();
        EventLogClass TheEventLogClass = new EventLogClass();
        EmployeeClass TheEmployeeClass = new EmployeeClass();

        //setting up the data
        ToolsDataSet TheToolsDataSet = new ToolsDataSet();
        FindEmployeeByEmployeeIDDataSet TheFindEmployeeByEmployeeIDDataSet = new FindEmployeeByEmployeeIDDataSet();
        FindWarehousesDataSet TheFindWarehouseDataSet = new FindWarehousesDataSet();
        NewToolsDataSet TheNewToolsDataSet = new NewToolsDataSet();
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            int intCounter;
            int intNumberOfRecords;
            int intWarehouseID;
            int intEmployeeID;
            string strAssignedOffice;
            
            try
            {
                TheToolsDataSet = TheToolsClass.GetToolsInfo();
                
                TheFindWarehouseDataSet = TheEmployeeClass.FindWarehouses();

                intNumberOfRecords = TheToolsDataSet.tools.Rows.Count - 1;

                for(intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
                {
                    intEmployeeID = TheToolsDataSet.tools[intCounter].EmployeeID;

                    TheFindEmployeeByEmployeeIDDataSet = TheEmployeeClass.FindEmployeeByEmployeeID(intEmployeeID);

                    strAssignedOffice = TheFindEmployeeByEmployeeIDDataSet.FindEmployeeByEmployeeID[0].HomeOffice;

                    intWarehouseID = FindWarehouseID(strAssignedOffice);

                    if(intWarehouseID == 0)
                    {
                        intWarehouseID = 100000;
                        strAssignedOffice = "CLEVELAND";
                    }

                    NewToolsDataSet.toolsRow NewToolRow = TheNewToolsDataSet.tools.NewtoolsRow();

                    NewToolRow.AssignedOffice = strAssignedOffice;
                    NewToolRow.Description = TheToolsDataSet.tools[intCounter].ToolDescription;
                    NewToolRow.ToolID = TheToolsDataSet.tools[intCounter].ToolID;
                    NewToolRow.ToolKey = TheToolsDataSet.tools[intCounter].ToolKey;
                    NewToolRow.WarehouseID = intWarehouseID;

                    TheNewToolsDataSet.tools.Rows.Add(NewToolRow);
                }

                dgrResults.ItemsSource = TheNewToolsDataSet.tools;
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Update Current Location // Grid Loaded " + Ex.Message);

                TheMessagesClass.ErrorMessage(Ex.ToString());
            }            
        }
        private int FindWarehouseID(string strWarehouseName)
        {
            int intWarehouseID = 0;
            int intCounter;
            int intNumberOfRecords;

            intNumberOfRecords = TheFindWarehouseDataSet.FindWarehouses.Rows.Count - 1;

            for(intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
            {
                if(strWarehouseName == TheFindWarehouseDataSet.FindWarehouses[intCounter].FirstName)
                {
                    intWarehouseID = TheFindWarehouseDataSet.FindWarehouses[intCounter].EmployeeID;
                    break;
                }
            }


            return intWarehouseID;
        }

        private void btnProcess_Click(object sender, RoutedEventArgs e)
        {
            int intCounter;
            int intNumberOfRecords;
            int intToolKey;
            int intCurrentLocation;
            bool blnFatalError;

            try
            {
                intNumberOfRecords = TheNewToolsDataSet.tools.Rows.Count - 1;

                for(intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
                {
                    intToolKey = TheNewToolsDataSet.tools[intCounter].ToolKey;
                    intCurrentLocation = TheNewToolsDataSet.tools[intCounter].WarehouseID;

                    blnFatalError = TheToolsClass.UpdateToolCurrentLocation(intToolKey, intCurrentLocation);

                    if (blnFatalError == true)
                        throw new Exception();
                }
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Update Currrent Location // Process Button " + Ex.Message);

                TheMessagesClass.ErrorMessage(Ex.ToString());
            }
        }
    }
}
