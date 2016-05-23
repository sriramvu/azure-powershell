﻿// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using Microsoft.Azure.Commands.DevTestLabs.Models;
using Microsoft.Azure.Management.DevTestLabs;
using Microsoft.Azure.Management.DevTestLabs.Models;
using Microsoft.Rest.Azure;
using System;
using System.Management.Automation;

namespace Microsoft.Azure.Commands.DevTestLabs
{
    [Cmdlet(VerbsCommon.Set, "AzureRmDtlAutoShutdownPolicy", HelpUri = Constants.DevTestLabsHelpUri, DefaultParameterSetName = ParameterSetEnable)]
    [OutputType(typeof(PSSchedule))]
    public class SetAzureRmDtlAutoShutdownPolicy : DtlPolicyCmdletBase
    {
        protected override string PolicyName
        {
            get
            {
                return WellKnownPolicyNames.LabVmsShutdown;
            }
        }

        #region Input Parameter Definitions

        /// <summary>
        /// Time of day for shutting down the virtual machine.
        /// </summary>
        [Parameter(Mandatory = false,
            Position = 4,
            HelpMessage = "Time of day for shutting down the virtual machine.")]
        public DateTime? Time { get; set; }

        #endregion Input Parameter Definitions

        public override void ExecuteCmdlet()
        {
            Schedule inputSchedule = null;

            try
            {
                inputSchedule = DataServiceClient.Schedule.GetResource(
                                ResourceGroupName,
                                LabName,
                                PolicyName);
            }
            catch (CloudException ex)
            {
                if (ex.Response.StatusCode != System.Net.HttpStatusCode.NotFound
                    || Time == null)
                {
                    throw;
                }
            }

            if (inputSchedule == null)
            {
                inputSchedule = new Schedule
                {
                    TimeZoneId = TimeZoneInfo.Local.Id,
                    TaskType = TaskType.LabVmsShutdownTask,
                    DailyRecurrence = new DayDetails
                    {
                        Time = Time.Value.ToString("HHmm")
                    },
                    Status = Disable ? PolicyStatus.Disabled : PolicyStatus.Enabled
                };
            }
            else
            {
                if (Time.HasValue)
                {
                    inputSchedule.DailyRecurrence = new DayDetails
                    {
                        Time = Time.Value.ToString("HHmm")
                    };
                }

                if (Disable)
                {
                    inputSchedule.Status = PolicyStatus.Disabled;
                }

                if (Enable)
                {
                    inputSchedule.Status = PolicyStatus.Enabled;
                }
            }

            var outputSchedule = DataServiceClient.Schedule.CreateOrUpdateResource(
                ResourceGroupName,
                LabName,
                PolicyName,
                inputSchedule);

            WriteObject(outputSchedule.DuckType<PSSchedule>());
        }
    }
}
