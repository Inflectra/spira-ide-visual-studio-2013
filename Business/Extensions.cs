using System;
using System.Drawing;
using System.Reflection;
using System.Resources;
using System.ServiceModel;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Business.SpiraTeam_Client;
using Inflectra.Global;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Linq;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Business.Properties;
using System.Collections.Generic;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Business
{
	public static partial class StaticFuncs
	{
		/// <summary>Checks the given properties on the artifact the user is modifying against the given artifact to report wether the fields can be merged or not.</summary>
		/// <param name="originalTask">The original artifact before any edits.</param>
		/// <param name="serverModdedTask">The new artifact form the server - edited by someone else.</param>
		/// <param name="userEnteredTask">The modified artifact from our user.</param>
		/// <returns>True if fields can be merged. False otherwise.</returns>
		public static bool CanBeMergedWith(this RemoteRequirement userEnteredReq, RemoteRequirement originalReq, RemoteRequirement serverModdedReq)
		{
			bool retValue = false;

			//Check the standard fields, first..
			if (userEnteredReq.AuthorId != originalReq.AuthorId)
				retValue = (originalReq.AuthorId == serverModdedReq.AuthorId);
			if (!retValue)
			{
				if (StripTagsCharArray(userEnteredReq.Description).ToLowerInvariant().Trim() != StripTagsCharArray(originalReq.Description).ToLowerInvariant().Trim())
					retValue = (StripTagsCharArray(originalReq.Description).ToLowerInvariant().Trim() == StripTagsCharArray(serverModdedReq.Description).ToLowerInvariant().Trim());
			}
			if (!retValue && userEnteredReq.ImportanceId != originalReq.ImportanceId)
				retValue = (originalReq.ImportanceId == serverModdedReq.ImportanceId);
			if (!retValue && userEnteredReq.Name.Equals(originalReq.Name))
				retValue = (originalReq.Name.Equals(serverModdedReq.ImportanceId));
			if (!retValue && userEnteredReq.OwnerId != originalReq.OwnerId)
				retValue = (originalReq.OwnerId == serverModdedReq.OwnerId);
			if (!retValue && userEnteredReq.PlannedEffort != originalReq.PlannedEffort)
				retValue = (originalReq.PlannedEffort == serverModdedReq.PlannedEffort);
			if (!retValue && userEnteredReq.ReleaseId != originalReq.ReleaseId)
				retValue = (originalReq.ReleaseId == serverModdedReq.ReleaseId);
			if (!retValue && userEnteredReq.StatusId != originalReq.StatusId)
				retValue = (originalReq.StatusId == serverModdedReq.StatusId);

			//Check custom values.
			if (!retValue)
				retValue = CanCustomPropertiesBeMergedWith(userEnteredReq, originalReq, serverModdedReq);

			return retValue;
		}

		/// <summary>Checks the given properties on the artifact the user is modifying against the given artifact to report wether the fields can be merged or not.</summary>
		/// <param name="originalTask">The original artifact before any edits.</param>
		/// <param name="serverModdedTask">The new artifact form the server - edited by someone else.</param>
		/// <param name="userEnteredTask">The modified artifact from our user.</param>
		/// <returns>True if fields can be merged. False otherwise.</returns>
		public static bool CanBeMergedWith(this RemoteTask userEnteredTask, RemoteTask originalTask, RemoteTask serverModdedTask)
		{
			bool retValue = false;

			//Check standard fields.
			if (userEnteredTask.ActualEffort != originalTask.ActualEffort)
				retValue = (originalTask.ActualEffort == serverModdedTask.ActualEffort);
			if (!retValue && userEnteredTask.CreationDate != originalTask.CreationDate)
				retValue = (originalTask.CreationDate == serverModdedTask.CreationDate);
			if (!retValue && userEnteredTask.CreatorId != originalTask.CreatorId)
				retValue = (originalTask.CreatorId == serverModdedTask.CreatorId);
			if (!retValue)
			{
				if (StripTagsCharArray(userEnteredTask.Description).ToLowerInvariant().Trim() != StripTagsCharArray(originalTask.Description).ToLowerInvariant().Trim())
					retValue = (StripTagsCharArray(originalTask.Description).ToLowerInvariant().Trim() == StripTagsCharArray(serverModdedTask.Description).ToLowerInvariant().Trim());
			}
			if (!retValue && userEnteredTask.EndDate != originalTask.EndDate)
				retValue = (originalTask.EndDate == serverModdedTask.EndDate);
			if (!retValue && userEnteredTask.EstimatedEffort != originalTask.EstimatedEffort)
				retValue = (originalTask.EstimatedEffort == serverModdedTask.EstimatedEffort);
			if (!retValue && userEnteredTask.Name.TrimEquals(originalTask.Name))
				retValue = (originalTask.Name.TrimEquals(serverModdedTask.Name));
			if (!retValue && userEnteredTask.OwnerId != originalTask.OwnerId)
				retValue = (originalTask.OwnerId == serverModdedTask.OwnerId);
			if (!retValue && userEnteredTask.ReleaseId != originalTask.ReleaseId)
				retValue = (originalTask.ReleaseId == serverModdedTask.ReleaseId);
			if (!retValue && userEnteredTask.RemainingEffort != originalTask.RemainingEffort)
				retValue = (originalTask.RemainingEffort == serverModdedTask.RemainingEffort);
			if (!retValue && userEnteredTask.RequirementId != originalTask.RequirementId)
				retValue = (originalTask.RequirementId == serverModdedTask.RequirementId);
			if (!retValue && userEnteredTask.StartDate != originalTask.StartDate)
				retValue = (originalTask.StartDate == serverModdedTask.StartDate);
			if (!retValue && userEnteredTask.TaskPriorityId != originalTask.TaskPriorityId)
				retValue = (originalTask.TaskPriorityId == serverModdedTask.TaskPriorityId);
			if (!retValue && userEnteredTask.TaskStatusId != originalTask.TaskStatusId)
				retValue = (originalTask.TaskStatusId == serverModdedTask.TaskStatusId);

			//Check custom fields.
			if (!retValue)
				retValue = CanCustomPropertiesBeMergedWith(userEnteredTask, originalTask, serverModdedTask);

			return retValue;
		}

		/// <summary>Checks the given properties on the artifact the user is modifying against the given artifact to report wether the fields can be merged or not.</summary>
		/// <param name="originalInc">The original artifact before any edits.</param>
		/// <param name="serverModdedInc">The new artifact form the server - edited by someone else.</param>
		/// <param name="userEnteredInc">The modified artifact from our user.</param>
		/// <returns>True if fields can be merged. False otherwise.</returns>
		public static bool CanBeMergedWith(this RemoteIncident userEnteredInc, RemoteIncident originalInc, RemoteIncident serverModdedInc)
		{
			bool retValue = false;

			if (userEnteredInc != null && serverModdedInc != null)
			{
				//Okay, check all fields. We want to see if a user-changed field (userTask) was also
				//   changed by someone else. If it was, we return false (they cannot be merged). Otherwise,
				//   we return true (they can be merged).
				//So we check the user-entered field against the original field. If they are different,
				//   check the original field against the concurrent field. If they are different, return
				//   false. Otherwise to both if's, return true.
				//We just loop through all available fields. The fielNum here has no reference to workflow
				//   field ID, _WorkflowFields is just used to get the count of fields to check against.
				int fieldNum = 1;
				if (userEnteredInc.ActualEffort != originalInc.ActualEffort)
					retValue = (originalInc.ActualEffort == serverModdedInc.ActualEffort);
				if (!retValue && userEnteredInc.ClosedDate != originalInc.ClosedDate)
					retValue = (originalInc.ClosedDate == serverModdedInc.ClosedDate);
				if (!retValue && userEnteredInc.CreationDate != originalInc.CreationDate)
					retValue = (originalInc.CreationDate == serverModdedInc.CreationDate);
				if (!retValue)
				{
					if (StaticFuncs.StripTagsCharArray(userEnteredInc.Description).ToLowerInvariant().Trim() != StaticFuncs.StripTagsCharArray(originalInc.Description).ToLowerInvariant().Trim())
						retValue = (StaticFuncs.StripTagsCharArray(originalInc.Description).ToLowerInvariant().Trim() == StaticFuncs.StripTagsCharArray(serverModdedInc.Description).ToLowerInvariant().Trim());
				}
				if (!retValue && userEnteredInc.DetectedReleaseId != originalInc.DetectedReleaseId)
					retValue = (originalInc.DetectedReleaseId == serverModdedInc.DetectedReleaseId);
				if (!retValue && userEnteredInc.EstimatedEffort != originalInc.EstimatedEffort)
					retValue = (originalInc.EstimatedEffort == serverModdedInc.EstimatedEffort);
				if (!retValue && userEnteredInc.IncidentStatusId != originalInc.IncidentStatusId)
					retValue = (originalInc.IncidentStatusId == serverModdedInc.IncidentStatusId);
				if (!retValue && userEnteredInc.IncidentTypeId != originalInc.IncidentTypeId)
					retValue = (originalInc.IncidentTypeId == serverModdedInc.IncidentTypeId);
				if (!retValue && userEnteredInc.Name.TrimEquals(originalInc.Name))
					retValue = (originalInc.Name.TrimEquals(serverModdedInc.Name));
				if (!retValue && userEnteredInc.OpenerId != originalInc.OpenerId)
					retValue = (originalInc.OpenerId == serverModdedInc.OpenerId);
				if (!retValue && userEnteredInc.OwnerId != originalInc.OwnerId)
					retValue = (originalInc.OwnerId == serverModdedInc.OwnerId);
				if (!retValue && userEnteredInc.PriorityId != originalInc.PriorityId)
					retValue = (originalInc.PriorityId == serverModdedInc.PriorityId);
				if (!retValue && userEnteredInc.RemainingEffort != originalInc.RemainingEffort)
					retValue = (originalInc.RemainingEffort == serverModdedInc.RemainingEffort);
				if (!retValue && userEnteredInc.ResolvedReleaseId != originalInc.ResolvedReleaseId)
					retValue = (originalInc.ResolvedReleaseId == serverModdedInc.ResolvedReleaseId);
				if (!retValue && userEnteredInc.SeverityId != originalInc.SeverityId)
					retValue = (originalInc.SeverityId == serverModdedInc.SeverityId);
				if (!retValue && userEnteredInc.StartDate != originalInc.StartDate)
					retValue = (originalInc.StartDate == serverModdedInc.StartDate);
				if (!retValue && userEnteredInc.TestRunStepId != originalInc.TestRunStepId)
					retValue = (originalInc.TestRunStepId == serverModdedInc.TestRunStepId);
				if (!retValue && userEnteredInc.VerifiedReleaseId != originalInc.VerifiedReleaseId)
					retValue = (originalInc.VerifiedReleaseId == serverModdedInc.VerifiedReleaseId);

				//Check custom fields.
				if (!retValue)
					retValue = CanCustomPropertiesBeMergedWith(userEnteredInc, originalInc, serverModdedInc);

			}
			return retValue;
		}

		/// <summary>Checks the given properties on the artifact the user is modifying against the given artifact to report wether the fields can be merged or not.</summary>
		/// <param name="original">The original artifact before any edits.</param>
		/// <param name="serverModded">The new artifact form the server - edited by someone else.</param>
		/// <param name="userEntered">The modified artifact from our user.</param>
		/// <returns>True if fields can be merged. False otherwise.</returns>
		private static bool CanCustomPropertiesBeMergedWith(this RemoteArtifact userEntered, RemoteArtifact original, RemoteArtifact serverModded)
		{
			bool retValue = false;

			//Loop through custom fields..
			foreach (RemoteArtifactCustomProperty newProp in userEntered.CustomProperties)
			{
				//See if the user changed this property.
				RemoteArtifactCustomProperty origProp = original.CustomProperties.FirstOrDefault(cp => cp.PropertyNumber == newProp.PropertyNumber);

				bool userChanged = false;
				bool otherChanged = false;
				if (origProp == null) userChanged = true;
				else
				{
					//Pull the original value..
					switch (newProp.Definition.CustomPropertyTypeId)
					{
						case 1: // String (String)
						case 9: // URL (String)
							userChanged = (newProp.StringValue.Equals(origProp.StringValue));
							break;
						case 2: // Integer (Int)
						case 6: // List (Int)
						case 8: // User (Int)
							userChanged = (newProp.IntegerValue.Equals(origProp.IntegerValue));
							break;
						case 3: // Decimal (Decimal)
							userChanged = (newProp.DecimalValue.Equals(origProp.DecimalValue));
							break;
						case 4: // Boolean (Bool)
							userChanged = (newProp.BooleanValue.Equals(origProp.BooleanValue));
							break;
						case 5: // DateTime (DateTime)
							userChanged = (newProp.DateTimeValue.Equals(origProp.DateTimeValue));
							break;
						case 7: // Multilist (List<int>)
							newProp.IntegerListValue.Sort();
							origProp.IntegerListValue.Sort();
							userChanged = (newProp.IntegerListValue.SequenceEqual(origProp.IntegerListValue));
							break;
					}
				}

				//If the user changed the field, check to see if the remote user also changed the field.
				// If so, then we can't merge.
				if (userChanged)
				{
					RemoteArtifactCustomProperty remProp = serverModded.CustomProperties.FirstOrDefault(cp => cp.PropertyNumber == newProp.PropertyNumber);

					if (remProp == null)
					{
						//If the original was null, (unset) and this one isn't, we don't care at checking values.
						if (origProp != null) otherChanged = true;
					}
					else
					{
						//Pull the original value..
						switch (remProp.Definition.CustomPropertyTypeId)
						{
							case 1: // String (String)
							case 9: // URL (String)
								otherChanged = (remProp.StringValue.Equals(origProp.StringValue));
								break;
							case 2: // Integer (Int)
							case 6: // List (Int)
							case 8: // User (Int)
								otherChanged = (remProp.IntegerValue.Equals(origProp.IntegerValue));
								break;
							case 3: // Decimal (Decimal)
								otherChanged = (remProp.DecimalValue.Equals(origProp.DecimalValue));
								break;
							case 4: // Boolean (Bool)
								otherChanged = (remProp.BooleanValue.Equals(origProp.BooleanValue));
								break;
							case 5: // DateTime (DateTime)
								otherChanged = (remProp.DateTimeValue.Equals(origProp.DateTimeValue));
								break;
							case 7: // Multilist (List<int>)
								remProp.IntegerListValue.Sort();
								origProp.IntegerListValue.Sort();
								otherChanged = (remProp.IntegerListValue.SequenceEqual(origProp.IntegerListValue));
								break;
						}
					}
				}

				//Now set our final value.
				if (userChanged && otherChanged)
				{
					retValue = false;
					//Escape early..
					break;
				}
				else
					retValue = true;
			}

			return retValue;

		}

		public static RemoteRequirement MergeWithConcurrency(RemoteRequirement userSaved, RemoteRequirement original, RemoteRequirement serverModded)
		{
			//If the field was not changed by the user (reqUserSaved == reqOriginal), then use the reqConcurrent. (Assuming that the
			// reqConcurrent has a possible updated value.
			//Otherwise, use the reqUserSaved value.
			try
			{
				RemoteRequirement retRequirement = new RemoteRequirement();

				//Let do fixed fields first.
				retRequirement.CoverageCountBlocked = serverModded.CoverageCountBlocked;
				retRequirement.CoverageCountCaution = serverModded.CoverageCountCaution;
				retRequirement.CoverageCountFailed = serverModded.CoverageCountFailed;
				retRequirement.CoverageCountPassed = serverModded.CoverageCountPassed;
				retRequirement.CoverageCountTotal = serverModded.CoverageCountTotal;
				retRequirement.CreationDate = serverModded.CreationDate;
				retRequirement.IndentLevel = serverModded.IndentLevel;
				retRequirement.LastUpdateDate = serverModded.LastUpdateDate;
				retRequirement.ProjectId = serverModded.ProjectId;
				retRequirement.RequirementId = serverModded.RequirementId;
				retRequirement.Summary = serverModded.Summary;
				retRequirement.TaskActualEffort = serverModded.TaskActualEffort;
				retRequirement.TaskCount = serverModded.TaskCount;
				retRequirement.TaskEstimatedEffort = serverModded.TaskEstimatedEffort;

				//Now the user fields..
				retRequirement.AuthorId = ((userSaved.AuthorId == original.AuthorId) ? serverModded.AuthorId : userSaved.AuthorId);
				string strDescUser = StaticFuncs.StripTagsCharArray(userSaved.Description);
				string strDescOrig = StaticFuncs.StripTagsCharArray(original.Description);
				retRequirement.Description = ((strDescOrig.TrimEquals(strDescOrig)) ? serverModded.Description : userSaved.Description);
				retRequirement.ImportanceId = ((userSaved.ImportanceId == original.ImportanceId) ? serverModded.ImportanceId : userSaved.ImportanceId);
				retRequirement.Name = ((userSaved.Name == original.Name) ? serverModded.Name : userSaved.Name);
				retRequirement.OwnerId = ((userSaved.OwnerId == original.OwnerId) ? serverModded.OwnerId : userSaved.OwnerId);
				retRequirement.PlannedEffort = ((userSaved.PlannedEffort == original.PlannedEffort) ? serverModded.PlannedEffort : userSaved.PlannedEffort);
				retRequirement.ReleaseId = ((userSaved.ReleaseId == original.ReleaseId) ? serverModded.ReleaseId : userSaved.ReleaseId);
				retRequirement.StatusId = ((userSaved.StatusId == original.StatusId) ? serverModded.StatusId : userSaved.StatusId);

				//Custom Properties
				retRequirement.CustomProperties = MergeCustomFieldsWithConcurrency(userSaved, original, serverModded);

				//Return our new task.
				return retRequirement;
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "save_MergeConcurrency()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
				return null;
			}
		}

		public static RemoteTask MergeWithConcurrency(RemoteTask userSaved, RemoteTask original, RemoteTask serverModded)
		{
			//If the field was not changed by the user (reqUserSaved == reqOriginal), then use the reqConcurrent. (Assuming that the
			// reqConcurrent has a possible updated value.
			//Otherwise, use the reqUserSaved value.
			RemoteTask retTask = new RemoteTask();

			try
			{
				retTask.ActualEffort = ((userSaved.ActualEffort == original.ActualEffort) ? serverModded.ActualEffort : userSaved.ActualEffort);
				retTask.CreationDate = original.CreationDate;
				retTask.CreatorId = ((userSaved.CreatorId == original.CreatorId) ? serverModded.CreatorId : userSaved.CreatorId);
				string strDescUser = StaticFuncs.StripTagsCharArray(userSaved.Description);
				string strDescOrig = StaticFuncs.StripTagsCharArray(original.Description);
				retTask.Description = ((strDescOrig.TrimEquals(strDescOrig)) ? serverModded.Description : userSaved.Description);
				retTask.EndDate = ((userSaved.EndDate == original.EndDate) ? serverModded.EndDate : userSaved.EndDate);
				retTask.EstimatedEffort = ((userSaved.EstimatedEffort == original.EstimatedEffort) ? serverModded.EstimatedEffort : userSaved.EstimatedEffort);
				retTask.LastUpdateDate = serverModded.LastUpdateDate;
				retTask.Name = ((userSaved.Name.TrimEquals(original.Name)) ? serverModded.Name : userSaved.Name);
				retTask.OwnerId = ((userSaved.OwnerId == original.OwnerId) ? serverModded.OwnerId : userSaved.OwnerId);
				retTask.ProjectId = original.ProjectId;
				retTask.ReleaseId = ((userSaved.ReleaseId == original.ReleaseId) ? serverModded.ReleaseId : userSaved.ReleaseId);
				retTask.RemainingEffort = ((userSaved.RemainingEffort == original.RemainingEffort) ? serverModded.RemainingEffort : userSaved.RemainingEffort);
				retTask.RequirementId = ((userSaved.RequirementId == original.RequirementId) ? serverModded.RequirementId : userSaved.RequirementId);
				retTask.StartDate = ((userSaved.StartDate == original.StartDate) ? serverModded.StartDate : userSaved.StartDate);
				retTask.TaskId = original.TaskId;
				retTask.TaskPriorityId = ((userSaved.TaskPriorityId == original.TaskPriorityId) ? serverModded.TaskPriorityId : userSaved.TaskPriorityId);
				retTask.TaskStatusId = ((userSaved.TaskStatusId == original.TaskStatusId) ? serverModded.TaskStatusId : userSaved.TaskStatusId);

				//Custom Properties
				retTask.CustomProperties = MergeCustomFieldsWithConcurrency(userSaved, original, serverModded);

			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "MergeWithConcurrency()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
				retTask = null;
			}

			//Return our new requirement.
			return retTask;
		}

		public static RemoteIncident MergeWithConcurrency(RemoteIncident userSaved, RemoteIncident original, RemoteIncident serverModded)
		{
			//If the field was not changed by the user (tskUserSaved == tskOriginal), then use the tskConcurrent. (Assuming that the
			// tskConcurrent has a possible updated value.
			//Otherwise, use the tskUserSaved value.
			RemoteIncident retIncident = new RemoteIncident();

			try
			{

				retIncident.ActualEffort = ((userSaved.ActualEffort == original.ActualEffort) ? serverModded.ActualEffort : userSaved.ActualEffort);
				retIncident.ClosedDate = ((userSaved.ClosedDate == original.ClosedDate) ? serverModded.ClosedDate : userSaved.ClosedDate);
				retIncident.CreationDate = original.CreationDate;
				string strDescUser = StaticFuncs.StripTagsCharArray(userSaved.Description);
				string strDescOrig = StaticFuncs.StripTagsCharArray(original.Description);
				retIncident.Description = ((strDescOrig.TrimEquals(strDescOrig)) ? serverModded.Description : userSaved.Description);
				retIncident.DetectedReleaseId = ((userSaved.DetectedReleaseId == original.DetectedReleaseId) ? serverModded.DetectedReleaseId : userSaved.DetectedReleaseId);
				retIncident.EstimatedEffort = ((userSaved.EstimatedEffort == original.EstimatedEffort) ? serverModded.EstimatedEffort : userSaved.EstimatedEffort);
				retIncident.IncidentId = original.IncidentId;
				retIncident.IncidentStatusId = ((userSaved.IncidentStatusId == original.IncidentStatusId) ? serverModded.IncidentStatusId : userSaved.IncidentStatusId);
				retIncident.IncidentTypeId = ((userSaved.IncidentTypeId == original.IncidentTypeId) ? serverModded.IncidentTypeId : userSaved.IncidentTypeId);
				retIncident.LastUpdateDate = serverModded.LastUpdateDate;
				retIncident.Name = ((userSaved.Name.TrimEquals(original.Name)) ? serverModded.Name : userSaved.Name);
				retIncident.OpenerId = ((userSaved.OpenerId == original.OpenerId) ? serverModded.OpenerId : userSaved.OpenerId);
				retIncident.OwnerId = ((userSaved.OwnerId == original.OwnerId) ? serverModded.OwnerId : userSaved.OwnerId);
				retIncident.PriorityId = ((userSaved.PriorityId == original.PriorityId) ? serverModded.PriorityId : userSaved.PriorityId);
				retIncident.ProjectId = original.ProjectId;
				retIncident.RemainingEffort = ((userSaved.RemainingEffort == original.RemainingEffort) ? serverModded.RemainingEffort : userSaved.RemainingEffort);
				retIncident.ResolvedReleaseId = ((userSaved.ResolvedReleaseId == original.ResolvedReleaseId) ? serverModded.ResolvedReleaseId : userSaved.ResolvedReleaseId);
				retIncident.SeverityId = ((userSaved.SeverityId == original.SeverityId) ? serverModded.SeverityId : userSaved.SeverityId);
				retIncident.StartDate = ((userSaved.StartDate == original.StartDate) ? serverModded.StartDate : userSaved.StartDate);
				retIncident.TestRunStepId = ((userSaved.TestRunStepId == original.TestRunStepId) ? serverModded.TestRunStepId : userSaved.TestRunStepId);
				retIncident.VerifiedReleaseId = ((userSaved.VerifiedReleaseId == original.VerifiedReleaseId) ? serverModded.VerifiedReleaseId : userSaved.VerifiedReleaseId);

				//Custom Properties
				retIncident.CustomProperties = MergeCustomFieldsWithConcurrency(userSaved, original, serverModded);

			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "clientSave_Connection_ConnectToProjectCompleted()");
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_UnexpectedError"), StaticFuncs.getCultureResource.GetString("app_General_ApplicationShortName"), MessageBoxButton.OK, MessageBoxImage.Error);
				retIncident = null;
			}

			//Return our new task.
			return retIncident;
		}

		private static List<RemoteArtifactCustomProperty> MergeCustomFieldsWithConcurrency(RemoteArtifact userSaved, RemoteArtifact original, RemoteArtifact serverModded)
		{
			List<RemoteArtifactCustomProperty> retList = new List<RemoteArtifactCustomProperty>();

			//First get all the ones our user entered in.
			foreach (RemoteArtifactCustomProperty prop in userSaved.CustomProperties)
			{
				if (((original.CustomProperties.Count(cp => cp.PropertyNumber == prop.PropertyNumber) == 0) &&	//If the property does not exist in the original
					serverModded.CustomProperties.Count(cp => cp.PropertyNumber == prop.PropertyNumber) == 0))	// or the concurrent artifact.
				{
					retList.Add(prop);
				}
				else  //There is either an original one or a concurrent one,
				{
					//If the user one is different than the original, use the user one.
					// Otherwise, use the one from the concurrent.
					bool useUser = false;

					RemoteArtifactCustomProperty orig = original.CustomProperties.SingleOrDefault(cp => cp.PropertyNumber == prop.PropertyNumber);

					if (orig != null)
					{
						RemoteArtifactCustomProperty concur = serverModded.CustomProperties.SingleOrDefault(cp => cp.PropertyNumber == prop.PropertyNumber);

						switch (prop.Definition.CustomPropertyTypeId)
						{
							case 1: // String (String)
							case 9: // URL (String)
								if (prop.StringValue.Equals(orig.StringValue))
								{
									retList.Add(concur);
								}
								else
								{
									retList.Add(prop);
								}
								break;

							case 2: // Integer (Int)
							case 6: // List (Int)
							case 8: // User (Int)
								if (prop.IntegerValue.Equals(orig.IntegerValue))
								{
									retList.Add(concur);
								}
								else
								{
									retList.Add(prop);
								}
								break;

							case 3: // Decimal (Decimal)
								if (prop.DecimalValue.Equals(orig.DecimalValue))
								{
									retList.Add(concur);
								}
								else
								{
									retList.Add(prop);
								}
								break;

							case 4: // Boolean (Bool)
								if (prop.BooleanValue.Equals(orig.BooleanValue))
								{
									retList.Add(concur);
								}
								else
								{
									retList.Add(prop);
								}
								break;

							case 5: // DateTime (DateTime)
								if (prop.DateTimeValue.Equals(orig.DateTimeValue))
								{
									retList.Add(concur);
								}
								else
								{
									retList.Add(prop);
								}
								break;

							case 7: // Multilist (List<int>)
								prop.IntegerListValue.Sort();
								orig.IntegerListValue.Sort();
								if (prop.IntegerListValue.SequenceEqual(orig.IntegerListValue))
								{
									retList.Add(concur);
								}
								else
								{
									retList.Add(prop);
								}
								break;
						}

					}
					else
					{
						useUser = true;
					}
				}
			}

			return retList;
		}

		/// <summary>The CreationDate of the comment, converted to Local Time.</summary>
		/// <param name="comment">The comment.</param>
		/// <returns>Nullable DateTime</returns>
		public static DateTime? getDisplayDate(this RemoteComment comment)
		{
			if (comment.CreationDate.HasValue)
				return comment.CreationDate.Value.ToLocalTime();
			else
				return null;
		}
	}
}
