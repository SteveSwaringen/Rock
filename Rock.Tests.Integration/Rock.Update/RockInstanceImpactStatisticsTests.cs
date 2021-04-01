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
using System.Data.Entity;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Rock.Data;
using Rock.Model;
using Rock.Update;
using Rock.Update.Interfaces;
using Rock.Update.Models;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.RockUpdate
{
    [TestClass]
    public class RockInstanceImpactStatisticsTests : BaseRockTest
    {
        private Mock<RockContext> _mockRockContext = null;
        private List<Person> _people = new List<Person>();

        [TestInitialize]
        public void TestInitialize()
        {
            var data = _people.AsQueryable();
            var mockSet = new Mock<DbSet<Person>>();
            mockSet.As<IQueryable<Person>>().Setup( m => m.Provider ).Returns( () => { return data.Provider; } );
            mockSet.As<IQueryable<Person>>().Setup( m => m.Expression ).Returns( () => { return data.Expression; } );
            mockSet.As<IQueryable<Person>>().Setup( m => m.ElementType ).Returns( () => { return data.ElementType; } );
            mockSet.As<IQueryable<Person>>().Setup( m => m.GetEnumerator() ).Returns( () => { return _people.GetEnumerator(); } );

            var globalAttributes = GlobalAttributesCache.Get();
            var locations = new List<Location>
            {
                new Location
                {
                    Guid = globalAttributes.GetValue( "OrganizationAddress" ).AsGuid(),
                    Name = "Test Organization Location"
                }
            };

            var locationQueryable = locations.AsQueryable();
            var mockLocationSet = new Mock<DbSet<Location>>();
            mockLocationSet.As<IQueryable<Location>>().Setup( m => m.Provider ).Returns( () => { return locationQueryable.Provider; } );
            mockLocationSet.As<IQueryable<Location>>().Setup( m => m.Expression ).Returns( () => { return locationQueryable.Expression; } );
            mockLocationSet.As<IQueryable<Location>>().Setup( m => m.ElementType ).Returns( () => { return locationQueryable.ElementType; } );
            mockLocationSet.As<IQueryable<Location>>().Setup( m => m.GetEnumerator() ).Returns( () => { return locations.GetEnumerator(); } );

            _mockRockContext = new Mock<RockContext>();
            _mockRockContext.Setup( m => m.People ).Returns( mockSet.Object );
            _mockRockContext.Setup( m => m.Set<Person>() ).Returns( mockSet.Object );

            _mockRockContext.Setup( m => m.Locations ).Returns( mockLocationSet.Object );
            _mockRockContext.Setup( m => m.Set<Location>() ).Returns( mockLocationSet.Object );
        }

        [TestMethod]
        public void SendImpactStatisticsToSpark_ShouldNotSendDataToServiceWhenSampleDataInUse()
        {
            EnsureMoreThen100Records();
            SystemSettings.SetValue( SystemKey.SystemSetting.SAMPLEDATA_DATE, DateTime.Now.ToString() );
            Thread.Sleep( 500 );

            var rockImpactService = new Mock<IRockImpactService>();
            var rockInstanceImpactStatistics = new RockInstanceImpactStatistics( rockImpactService.Object, _mockRockContext.Object );

            rockInstanceImpactStatistics.SendImpactStatisticsToSpark( false, "1.13.0", "0.0.0.0", "data" );
            rockImpactService.Verify( x => x.SendImpactStatisticsToSpark( It.IsAny<ImpactStatistic>() ), Times.Never );
        }

        [TestMethod]
        public void SendImpactStatisticsToSpark_ShouldNotSendDataToServiceWhenFewerThen100Records()
        {
            SystemSettings.SetValue( SystemKey.SystemSetting.SAMPLEDATA_DATE, string.Empty );

            var rockImpactService = new Mock<IRockImpactService>();
            var rockInstanceImpactStatistics = new RockInstanceImpactStatistics( rockImpactService.Object, _mockRockContext.Object );

            rockInstanceImpactStatistics.SendImpactStatisticsToSpark( false, "1.13.0", "0.0.0.0", "data" );
            rockImpactService.Verify( x => x.SendImpactStatisticsToSpark( It.IsAny<ImpactStatistic>() ), Times.Never );
        }

        [TestMethod]
        public void SendImpactStatisticsToSpark_ShouldSendDataToService()
        {
            SystemSettings.SetValue( SystemKey.SystemSetting.SAMPLEDATA_DATE, string.Empty );

            EnsureMoreThen100Records();

            var rockImpactService = new Mock<IRockImpactService>();
            var rockInstanceImpactStatistics = new RockInstanceImpactStatistics( rockImpactService.Object, _mockRockContext.Object );

            rockInstanceImpactStatistics.SendImpactStatisticsToSpark( false, "1.13.0", "0.0.0.0", "data" );
            rockImpactService.Verify( x => x.SendImpactStatisticsToSpark( It.IsAny<ImpactStatistic>() ), Times.Once );
        }

        [TestMethod]
        public void SendImpactStatisticsToSpark_ShouldNotIncludeOrganizationData()
        {
            var expectedInstanceId = SystemSettings.GetRockInstanceId();
            var expectedVersion = "1.13.0";
            var expectedIpAddress = "192.168.1.0";
            var expectedEnvironmentData = "data";

            SystemSettings.SetValue( SystemKey.SystemSetting.SAMPLEDATA_DATE, string.Empty );

            EnsureMoreThen100Records();

            var rockImpactService = new Mock<IRockImpactService>();
            var rockInstanceImpactStatistics = new RockInstanceImpactStatistics( rockImpactService.Object, _mockRockContext.Object );

            rockInstanceImpactStatistics.SendImpactStatisticsToSpark( false, expectedVersion, expectedIpAddress, "data" );
            rockImpactService.Verify(
                x => x.SendImpactStatisticsToSpark( It.Is<ImpactStatistic>( i =>
                    i.RockInstanceId == expectedInstanceId
                    && i.Version == expectedVersion
                    && i.IpAddress == expectedIpAddress
                    && i.PublicUrl.IsNullOrWhiteSpace()
                    && i.OrganizationName.IsNullOrWhiteSpace()
                    && i.OrganizationLocation == null
                    && i.NumberOfActiveRecords == 0
                    && i.EnvironmentData == expectedEnvironmentData ) ),
                Times.Once );
        }

        /// <summary>
        /// Sends the impact statistics to spark should include organization data.
        /// </summary>
        [TestMethod]
        public void SendImpactStatisticsToSpark_ShouldIncludeOrganizationData()
        {
            var expectedInstanceId = SystemSettings.GetRockInstanceId();
            var expectedVersion = "1.13.0";
            var expectedIpAddress = "192.168.1.0";
            var expectedEnvironmentData = "data";

            var globalAttributes = GlobalAttributesCache.Get();
            var expectedOrganizationName = globalAttributes.GetValue( "OrganizationName" );
            var expectedPublicUrl = globalAttributes.GetValue( "PublicApplicationRoot" );

            SystemSettings.SetValue( SystemKey.SystemSetting.SAMPLEDATA_DATE, string.Empty );

            EnsureMoreThen100Records();
            var expectedNumberOfRecords = new PersonService( _mockRockContext.Object ).Queryable( includeDeceased: false, includeBusinesses: false ).Count();

            var rockImpactService = new Mock<IRockImpactService>();
            var rockInstanceImpactStatistics = new RockInstanceImpactStatistics( rockImpactService.Object, _mockRockContext.Object );

            rockInstanceImpactStatistics.SendImpactStatisticsToSpark( true, expectedVersion, expectedIpAddress, "data" );
            rockImpactService.Verify(
                x => x.SendImpactStatisticsToSpark( It.Is<ImpactStatistic>( i =>
                    i.RockInstanceId == expectedInstanceId
                    && i.Version == expectedVersion
                    && i.IpAddress == expectedIpAddress
                    && i.PublicUrl == expectedPublicUrl
                    && i.OrganizationName == expectedOrganizationName
                    && i.OrganizationLocation != null
                    && i.NumberOfActiveRecords == expectedNumberOfRecords
                    && i.EnvironmentData == expectedEnvironmentData ) ),
                Times.Once );
        }

        private void EnsureMoreThen100Records()
        {
            var numberOfActiveRecords = _people.Count;
            var numberOfRecordsToCreate = 101 - numberOfActiveRecords;
            var recordTypePersonId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() );

            while ( numberOfRecordsToCreate > 0 )
            {
                var person = new Person
                {
                    FirstName = Guid.NewGuid().ToString(),
                    LastName = Guid.NewGuid().ToString(),
                    Email = $"{Guid.NewGuid()}@test.com",
                    RecordTypeValueId = recordTypePersonId
                };
                _people.Add( person );
                numberOfRecordsToCreate--;
            }
        }
    }
}
