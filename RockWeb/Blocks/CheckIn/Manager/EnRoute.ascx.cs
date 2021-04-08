// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.CheckIn.Manager
{
    /// <summary>
    /// </summary>
    [DisplayName( "En Route" )]
    [Category( "Check-in > Manager" )]
    [Description( "Lists the people who are checked-in but not yet marked present." )]

    [CustomDropdownListField(
        "Filter By",
        Key = AttributeKey.FilterBy,
        DefaultValue = "2",
        ListSource = "2^Checked-In,3^Present,4^Checked-Out",
        IsRequired = true,
        Order = 1
        )]

    [BooleanField(
        "Show Only Parent Group",
        "When enabled, the parent group and path for each check-in, instead of the actual group.",
        Key = AttributeKey.ShowOnlyParentGroup,
        DefaultBooleanValue = false,
        Order = 2 )]
    public partial class EnRoute : RockBlock
    {
        #region Keys

        /// <summary>
        /// Keys for attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string FilterBy = "FilterBy";

            public const string ShowOnlyParentGroup = "ShowOnlyParentGroup";
        }

        #endregion

        #region Custom Settings Keys

        /// <summary>
        /// Keys to use for settings stored in the checkin-manager cookie
        /// </summary>
        private class CustomSettingKey
        {
            public const string EnRouteScheduleIdsFilter = "EnRouteScheduleIdsFilter";
            public const string EnRouteGroupIdsFilter = "EnRouteGroupIdsFilter";
        }

        #endregion Custom Settings Keys

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                BindFilter();
                BindGrid();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            NavigateToCurrentPageReference();
        }

        /// <summary>
        /// Handles the Click event of the btnShowFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnShowFilter_Click( object sender, EventArgs e )
        {
            pnlFilterCriteria.Visible = !pnlFilterCriteria.Visible;
        }

        /// <summary>
        /// Shows the filters.
        /// </summary>
        private void BindFilter()
        {
            ScheduleService scheduleService = new ScheduleService( new RockContext() );
            var customSettings = CheckinManagerHelper.GetCheckinManagerConfigurationFromCookie().CustomSettings;

            var scheduleIdsFilter = customSettings.GetValueOrNull( CustomSettingKey.EnRouteScheduleIdsFilter );
            var groupIdsFilter = customSettings.GetValueOrNull( CustomSettingKey.EnRouteGroupIdsFilter );

            var selectedScheduleIds = scheduleIdsFilter.SplitDelimitedValues().AsIntegerList();

            // limit Schedules to ones that are Active, have a CheckInStartOffsetMinutes, and are Named schedules
            var scheduleQry = scheduleService.Queryable().Where( a => a.IsActive && a.CheckInStartOffsetMinutes != null && a.Name != null && a.Name != string.Empty );

            var scheduleList = scheduleQry.ToList().OrderBy( a => a.Name ).ToList();

            var sortedScheduleList = scheduleList.OrderByOrderAndNextScheduledDateTime();
            lbSchedules.Items.Clear();

            foreach ( var schedule in sortedScheduleList )
            {
                var listItem = new ListItem();
                if ( schedule.Name.IsNotNullOrWhiteSpace() )
                {
                    listItem.Text = schedule.Name;
                }
                else
                {
                    listItem.Text = schedule.FriendlyScheduleText;
                }

                listItem.Value = schedule.Id.ToString();
                listItem.Selected = selectedScheduleIds.Contains( schedule.Id );
                lbSchedules.Items.Add( listItem );
            }

            IEnumerable<CheckinAreaPath> checkinAreasPaths;
            var checkinAreaFilter = CheckinManagerHelper.GetCheckinAreaFilter( this );
            if ( checkinAreaFilter != null )
            {
                checkinAreasPaths = new GroupTypeService( new RockContext() ).GetCheckinAreaDescendantsPath( checkinAreaFilter.Id );
            }
            else
            {
                checkinAreasPaths = new GroupTypeService( new RockContext() ).GetAllCheckinAreaPaths();
            }

            var checkinGroupTypeIdsToShow = checkinAreasPaths.Select( a => a.GroupTypeId ).Distinct().ToList();

            var groupQry = new GroupService( new RockContext() ).Queryable().Where( a => checkinGroupTypeIdsToShow.Contains( a.GroupTypeId ) );

            // get groups in order by GroupType.Order, then Group.Order (same order they would be in CheckIn Configuration UI)
            var groupList = groupQry.OrderBy( a => a.GroupType.Order ).ThenBy( a => a.Order )
                .ToList();

            cblGroups.Items.Clear();
            foreach ( var group in groupList )
            {
                cblGroups.Items.Add( new ListItem( group.Name, group.Id.ToString() ) );
            }

            var selectedGroupIds = groupIdsFilter.SplitDelimitedValues().AsIntegerList();
            cblGroups.SetValues( selectedGroupIds );
        }

        /// <summary>
        /// Handles the Click event of the btnApplyFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnApplyFilter_Click( object sender, EventArgs e )
        {
            pnlFilterCriteria.Visible = false;
            BindGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnClearFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnClearFilter_Click( object sender, EventArgs e )
        {
            lbSchedules.SetValues( new int[0] );
            cblGroups.SetValues( new int[0] );
        }

        /// <summary>
        /// Gets the row attendance ids.
        /// </summary>
        /// <param name="rowEventArgs">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        /// <returns></returns>
        private int[] GetRowAttendanceIds( RowEventArgs rowEventArgs )
        {
            // the attendance grid's DataKeyNames="PersonGuid,AttendanceIds". So each row is a PersonGuid, with a list of attendanceIds (usually one attendance, but could be more)
            var attendanceIds = rowEventArgs.RowKeyValues[1] as int[];
            return attendanceIds;
        }

        /// <summary>
        /// Handles the Click event of the btnChangeRoom control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void btnChangeRoom_Click( object sender, RowEventArgs e )
        {
            var attendanceIds = GetRowAttendanceIds( e ).ToList();
            var mostRecentAttendance = new AttendanceService( new RockContext() ).GetByIds( attendanceIds ).OrderByDescending( a => a.StartDateTime ).AsNoTracking().Select( a => new
            {
                a.Id,
                a.Occurrence.LocationId,
                a.Occurrence.ScheduleId,
                a.Occurrence.Group.GroupLocations
            } ).FirstOrDefault();

            if ( mostRecentAttendance == null )
            {
                return;
            }

            hfChangeRoomAttendanceId.Value = mostRecentAttendance.Id.ToString();

            // limit to Locations that are available for the selected attendence's occurrence's Group and Schedule.
            var groupLocations = mostRecentAttendance.GroupLocations.ToList().ToList();
            var availableGroupLocations = groupLocations.Where( a => a.Schedules.Any( s => s.Id == mostRecentAttendance.ScheduleId.Value ) );
            var sortedLocations = availableGroupLocations.OrderBy( a => a.Order ).ThenBy( a => a.Location.Name ).Select( a => a.Location ).DistinctBy( a => a.Id );

            ddlChangeRoomLocation.Items.Clear();

            foreach ( var location in sortedLocations )
            {
                ddlChangeRoomLocation.Items.Add( new ListItem( location.Name, location.Id.ToString() ) );
            }

            ddlChangeRoomLocation.SetValue( mostRecentAttendance.LocationId );
            mdChangeRoom.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdChangeRoom control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdChangeRoom_SaveClick( object sender, EventArgs e )
        {
            mdChangeRoom.Hide();

            var attendanceId = hfChangeRoomAttendanceId.Value.AsIntegerOrNull();
            if ( attendanceId == null )
            {
                return;
            }

            var rockContext = new RockContext();
            var attendanceService = new AttendanceService( rockContext );
            var attendanceOccurrenceService = new AttendanceOccurrenceService( rockContext );
            var attendance = new AttendanceService( rockContext ).Get( attendanceId.Value );
            if ( attendance == null )
            {
                return;
            }

            var currentOccurrence = attendance.Occurrence;

            var selectedLocationId = ddlChangeRoomLocation.SelectedValueAsId();
            var newRoomsOccurrence = attendanceOccurrenceService.GetOrAdd( currentOccurrence.OccurrenceDate, currentOccurrence.GroupId, selectedLocationId, currentOccurrence.ScheduleId );
            attendance.OccurrenceId = newRoomsOccurrence.Id;
            rockContext.SaveChanges();
            BindGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the campus from context.
        /// </summary>
        /// <returns></returns>
        private CampusCache GetCampusFromContext()
        {
            CampusCache campus = null;

            var campusEntityType = EntityTypeCache.Get( "Rock.Model.Campus" );
            if ( campusEntityType != null )
            {
                var campusContext = RockPage.GetCurrentContext( campusEntityType ) as Campus;
                if ( campusContext != null )
                {
                    campus = CampusCache.Get( campusContext.Id );
                }
            }

            return campus;
        }

        /// <summary>
        /// Handles the RowDataBound event of the gAttendees control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gAttendees_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType != DataControlRowType.DataRow )
            {
                return;
            }

            RosterAttendee attendee = e.Row.DataItem as RosterAttendee;

            var lPhoto = e.Row.FindControl( "lPhoto" ) as Literal;
            lPhoto.Text = attendee.GetPersonPhotoImageHtmlTag();
            var lName = e.Row.FindControl( "lName" ) as Literal;
            lName.Text = attendee.GetAttendeeNameHtml();

            var lGroupNameAndPath = e.Row.FindControl( "lGroupNameAndPath" ) as Literal;
            if ( lGroupNameAndPath != null && lGroupNameAndPath.Visible )
            {
                if ( _showOnlyParentGroup )
                {
                    lGroupNameAndPath.Text = lGroupNameAndPath.Text = attendee.GetParentGroupNameAndPathHtml();
                }
                else
                {
                    lGroupNameAndPath.Text = lGroupNameAndPath.Text = attendee.GetGroupNameAndPathHtml();
                }
            }

            var lLocation = e.Row.FindControl( "lLocation" ) as Literal;
            lLocation.Text = attendee.RoomName;
        }

        private bool _showOnlyParentGroup;

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            CampusCache campus = GetCampusFromContext();

            var selectedScheduleIds = lbSchedules.SelectedValues.AsIntegerList();
            var selectedGroupIds = cblGroups.SelectedValues.AsIntegerList();
            if ( selectedScheduleIds.Any() || selectedGroupIds.Any() )
            {
                btnShowFilter.AddCssClass( "criteria-exists bg-warning" );
            }
            else
            {
                btnShowFilter.RemoveCssClass( "criteria-exists bg-warning" );
            }

            var customSettings = CheckinManagerHelper.GetCheckinManagerConfigurationFromCookie().CustomSettings;

            customSettings.AddOrReplace( CustomSettingKey.EnRouteScheduleIdsFilter, selectedScheduleIds.AsDelimited( "," ) );
            customSettings.AddOrReplace( CustomSettingKey.EnRouteGroupIdsFilter, selectedGroupIds.AsDelimited( "," ) );

            CheckinManagerHelper.SaveCustomSettingsToCookie( customSettings );

            IList<RosterAttendee> attendees = null;

            using ( var rockContext = new RockContext() )
            {
                attendees = GetAttendees( rockContext );
            }

            // sort by Name, and also by PersonGuid (so they stay in a consistent order in cases where Names are identical
            var attendeesSorted = attendees
                .OrderBy( a => a.NickName )
                .ThenBy( a => a.LastName )
                .ThenBy( a => a.PersonGuid )
                .ToList();

            _showOnlyParentGroup = this.GetAttributeValue( AttributeKey.ShowOnlyParentGroup ).AsBoolean();

            gAttendees.DataSource = attendeesSorted;
            gAttendees.DataBind();
        }

        /// <summary>
        /// Gets the attendees.
        /// </summary>
        private IList<RosterAttendee> GetAttendees( RockContext rockContext )
        {
            var startDateTime = RockDateTime.Today;

            CampusCache campusCache = GetCampusFromContext();
            DateTime currentDateTime;
            if ( campusCache != null )
            {
                currentDateTime = campusCache.CurrentDateTime;
            }
            else
            {
                currentDateTime = RockDateTime.Now;
            }

            // Get all Attendance records for the current day and location
            var attendanceQuery = new AttendanceService( rockContext ).Queryable().Where( a =>
                a.StartDateTime >= startDateTime
                && a.DidAttend == true
                && a.StartDateTime <= currentDateTime
                && a.PersonAliasId.HasValue
                && a.Occurrence.GroupId.HasValue
                && a.Occurrence.ScheduleId.HasValue
                && a.Occurrence.LocationId.HasValue
                && a.Occurrence.ScheduleId.HasValue );
            
            if ( campusCache != null )
            {
                // limit locations (rooms) to locations within the selected campus
                var campusLocationIds = new LocationService( rockContext ).GetAllDescendentIds( campusCache.LocationId.Value ).ToList();
                attendanceQuery = attendanceQuery.Where( a => campusLocationIds.Contains( a.Occurrence.LocationId.Value ) );
            }

            var selectedScheduleIds = lbSchedules.SelectedValues.AsIntegerList();
            if ( selectedScheduleIds.Any() )
            {
                attendanceQuery = attendanceQuery.Where( a => selectedScheduleIds.Contains( a.Occurrence.ScheduleId.Value ) );
            }

            var selectedGroupIds = cblGroups.SelectedValues.AsIntegerList();
            if ( selectedGroupIds.Any() )
            {
                attendanceQuery = attendanceQuery.Where( a => selectedGroupIds.Contains( a.Occurrence.GroupId.Value ) );
            }
            else
            {
                var checkinAreaFilter = CheckinManagerHelper.GetCheckinAreaFilter( this );

                if ( checkinAreaFilter != null )
                {
                    // if there is a checkin area filter, limit to groups within the selected check-in area
                    var checkinAreaGroupTypeIds = new GroupTypeService( rockContext ).GetCheckinAreaDescendants( checkinAreaFilter.Id ).Select( a => a.Id ).ToList();
                    selectedGroupIds = new GroupService( rockContext ).Queryable().Where( a => checkinAreaGroupTypeIds.Contains( a.GroupTypeId ) ).Select(a => a.Id).ToList();
                    attendanceQuery = attendanceQuery.Where( a => selectedGroupIds.Contains( a.Occurrence.GroupId.Value ) );
                }
            }

            RosterStatusFilter rosterStatusFilter = this.GetAttributeValue( AttributeKey.FilterBy ).ConvertToEnumOrNull<RosterStatusFilter>() ?? RosterStatusFilter.CheckedIn;

            attendanceQuery = CheckinManagerHelper.FilterByRosterStatusFilter( attendanceQuery, rosterStatusFilter );

            var attendanceList = RosterAttendeeAttendance.Select( attendanceQuery ).ToList();

            attendanceList = CheckinManagerHelper.FilterByActiveCheckins( currentDateTime, attendanceList );

            attendanceList = attendanceList.Where( a => a.Person != null ).ToList();
            var attendees = RosterAttendee.GetFromAttendanceList( attendanceList );

            return attendees;
        }

        #endregion
    }
}