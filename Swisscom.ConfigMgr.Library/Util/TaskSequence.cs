namespace Swisscom.ConfigMgr.Library.Util
{
    using System;

    /// <summary>
    /// Access to variables in the running task sequence.
    /// </summary>
    public class TaskSequence
    {
        /// <summary>
        /// The configuration MGR variable client unique identifier
        /// </summary>
        public const string ConfigMgrVarClientGuid = "_SMSTSClientGUID";

        /// <summary>
        /// The configuration MGR variable computer name
        /// </summary>
        public const string ConfigMgrVarComputerName = "OSDComputerName";

        /// <summary>
        /// The configuration MGR variable for the SMSHTTP connection
        /// </summary>
        public const string ConfigMgrVarHttp = "_SMSTSHTTP";

        /// <summary>
        /// The configuration MGR variable management point
        /// </summary>
        public const string ConfigMgrVarManagementPoint = "_SMSTSMP";

        /// <summary>
        /// The configuration MGR variable org name
        /// </summary>
        public const string ConfigMgrVarOrgName = "_SMSTSOrgName";

        /// <summary>
        /// The configuration MGR variable site code
        /// </summary>
        public const string ConfigMgrVarSiteCode = "_SMSTSSiteCode";

        /// <summary>
        /// The configuration MGR variable assigned site code
        /// </summary>
        public const string ConfigMgrVarAssignedSiteCode = "_SMSTSAssignedSiteCode";

        /// <summary>
        /// The configuration MGR variable for the machine name.
        /// If the host already exists in SCCM, the name recieved from SCCM will be used.
        /// </summary>
        public const string ConfigMgrVarMachineName = "_SMSTSMachineName";

        /// <summary>
        /// The configuration MGR variable for the ID of the current running Package.
        /// </summary>
        public const string ConfigMgrVarPackageId = "_SMSTSPackageID";

        /// <summary>
        /// The configuration MGR variable for the name of the current running Package.
        /// </summary>
        public const string ConfigMgrVarPackageName = "_SMSTSPackageName";

        /// <summary>
        /// The configuration MGR variable primary user
        /// </summary>
        public const string ConfigMgrVarPrimaryUser = "SMSTSUDAUsers";

        /// <summary>
        /// The _tasksequence environment
        /// </summary>
        private readonly dynamic _tasksequenceEnvironment;

        /// <summary>
        /// The _progress unique identifier
        /// </summary>
        private readonly dynamic _progressUi;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskSequence"/> class.
        /// </summary>
        public TaskSequence()
        {
            try
            {
                this._tasksequenceEnvironment = Activator.CreateInstance(Type.GetTypeFromProgID("Microsoft.SMS.TSEnvironment"));
                this._progressUi = Activator.CreateInstance(Type.GetTypeFromProgID("Microsoft.SMS.TsProgressUI"));
                this.IsRunningInTasksequence = true;
            }
            catch (Exception ex)
            {
                this.IsRunningInTasksequence = false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether [running in tasksequence].
        /// </summary>
        /// <value>
        /// <c>true</c> if [running in tasksequence]; otherwise, <c>false</c>.
        /// </value>
        public bool IsRunningInTasksequence { get; private set; }

        /// <summary>
        /// Closes the ConfigMgr dialog.
        /// </summary>
        public void CloseDialog()
        {
            this._progressUi.CloseProgressDialog();
        }

        /// <summary>
        /// Gets the tasksequence variable.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <returns></returns>
        public string GetTasksequenceVariable(string variableName)
        {
            return this._tasksequenceEnvironment[variableName];
        }

        /// <summary>
        /// Sets the tasksequence variable.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <param name="variableValue">The variable value.</param>
        /// <returns></returns>
        public void SetTasksequenceVariable(string variableName, string variableValue)
        {
            this._tasksequenceEnvironment[variableName] = variableValue;
        }

        /// <summary>
        /// Gets the distribution point name.
        /// </summary>
        /// <returns></returns>
        public string GetDistributionPoint()
        {
            var dp = string.Empty;

            try
            {
                var siteCode = this.GetTasksequenceVariable(ConfigMgrVarSiteCode);
                var smsTsHttp = string.Format("{0}{1}", ConfigMgrVarHttp, siteCode);
                var allVariables = this._tasksequenceEnvironment.GetVariables();
                foreach (var variable in allVariables)
                {
                    if (variable.ToLower().Contains(smsTsHttp.ToLower()))
                    {
                        var connection = this.GetTasksequenceVariable(variable);
                        // dp = connection;
                        var arrUrl = connection.Split('/');
                        dp = arrUrl[2];
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                // in any case of an error, display the message
                // instead of the distribution point (not realy nice, but
                // can be used for debugging issues)
                dp = ex.Message;
            }

            return dp;
        }
    }
}
