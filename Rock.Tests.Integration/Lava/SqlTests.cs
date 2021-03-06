﻿using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Tests.Shared;

namespace Rock.Tests.Integration.Lava
{
    [TestClass]
    public class SqlTests
    {
        //private static LavaIntegrationTestHelper _helper;

        //[ClassInitialize]
        //public static void ClassInitialize( TestContext testContext )
        //{
        //    _helper = LavaIntegrationTestHelper.New();
        //}

        [TestMethod]
        [TestProperty( "Execution Time", "Long" )]
        public void SqlSelectShortTimeoutShouldFail()
        {
            var lavaScript = @"{% sql timeout:'10' %}

            WAITFOR DELAY '00:00:20';
            SELECT TOP 5 * 
            FROM Person
            {% endsql %}

            [
            {%- for item in results -%}
                {
                        ""CreatedDateTime"": {{ item.CreatedDateTime | ToJSON }},
                        ""LastName"": {{ item.LastName | ToJSON }},
                }{% unless forloop.last -%},{% endunless %}
            {%- endfor -%}
            ]";

            var output = lavaScript.ResolveMergeFields( new Dictionary<string, object>(), null, "Sql" );

            Assert.That.Contains( output, "Lava Error: Execution Timeout Expired." );
        }

        [TestMethod]
        [TestProperty( "Execution Time", "Long" )]
        public void SqlSelectLongTimeoutShouldPass()
        {
            var lavaScript = @"{% sql timeout:'40' %}

            WAITFOR DELAY '00:00:35';
            SELECT TOP 5 * 
            FROM Person
            {% endsql %}

            [
            {%- for item in results -%}
                {
                        ""CreatedDateTime"": {{ item.CreatedDateTime | ToJSON }},
                        ""LastName"": {{ item.LastName | ToJSON }},
                }{% unless forloop.last -%},{% endunless %}
            {%- endfor -%}
            ]";

            var output = lavaScript.ResolveMergeFields( new Dictionary<string, object>(), null, "Sql" );
            Assert.That.IsFalse( output.Contains( "Liquid error" ) );
        }

        [TestMethod]
        public void SqlSelectNoTimeoutShouldPass()
        {
            var lavaScript = @"{% sql %}

            SELECT TOP 5 * 
            FROM Person
            {% endsql %}

            [
            {%- for item in results -%}
                {
                        ""CreatedDateTime"": {{ item.CreatedDateTime | ToJSON }},
                        ""LastName"": {{ item.LastName | ToJSON }},
                }{% unless forloop.last -%},{% endunless %}
            {%- endfor -%}
            ]";

            var output = lavaScript.ResolveMergeFields( new Dictionary<string, object>(), null, "Sql" );
            Assert.That.IsFalse( output.Contains( "Liquid error" ) );
        }

        [TestMethod]
        [TestProperty( "Execution Time", "Long" )]
        public void SqlSelectNoTimeoutButQueryLongerThen30SecondsShouldFail()
        {
            var lavaScript = @"{% sql %}

            WAITFOR DELAY '00:00:35';
            SELECT TOP 5 * 
            FROM Person
            {% endsql %}

            [
            {%- for item in results -%}
                {
                        ""CreatedDateTime"": {{ item.CreatedDateTime | ToJSON }},
                        ""LastName"": {{ item.LastName | ToJSON }},
                }{% unless forloop.last -%},{% endunless %}
            {%- endfor -%}
            ]";

            var output = lavaScript.ResolveMergeFields( new Dictionary<string, object>(), null, "Sql" );
            Assert.That.Contains( output, "Lava Error: Execution Timeout Expired." );
        }

        [TestMethod]
        [TestProperty( "Execution Time", "Long" )]
        public void SqlCommandShortTimeoutShouldFail()
        {
            var lavaScript = @"{% sql statement:'command' timeout:'10' %}
                WAITFOR DELAY '00:00:20';
                DELETE FROM [DefinedValue] WHERE 1 != 1
            {% endsql %}

            {{ results }} {{ 'record' | PluralizeForQuantity:results }} were deleted.";

            var output = lavaScript.ResolveMergeFields( new Dictionary<string, object>(), null, "Sql" );
            Assert.That.Contains( output, "Lava Error: Execution Timeout Expired." );
        }

        [TestMethod]
        [TestProperty( "Execution Time", "Long" )]
        public void SqlCommandLongTimeoutShouldPass()
        {
            var lavaScript = @"{% sql statement:'command' timeout:'40' %}
                WAITFOR DELAY '00:00:35';
                DELETE FROM [DefinedValue] WHERE 1 != 1
            {% endsql %}

            {{ results }} {{ 'record' | PluralizeForQuantity:results }} were deleted.";

            var output = lavaScript.ResolveMergeFields( new Dictionary<string, object>(), null, "Sql" );
            Assert.That.IsFalse( output.Contains( "Liquid error" ) );
        }

        [TestMethod]
        public void SqlCommandNoTimeoutShouldPass()
        {
            var lavaScript = @"{% sql statement:'command' %}
                DELETE FROM [DefinedValue] WHERE 1 != 1
            {% endsql %}

            {{ results }} {{ 'record' | PluralizeForQuantity:results }} were deleted.";

            var output = lavaScript.ResolveMergeFields( new Dictionary<string, object>(), null, "Sql" );
            Assert.That.IsFalse( output.Contains( "Liquid error" ) );
        }

        [TestMethod]
        [TestProperty( "Execution Time", "Long" )]
        public void SqlCommandNoTimeoutButQueryLongerThen30SecondsShouldFail()
        {
            var lavaScript = @"{% sql statement:'command' %}
                WAITFOR DELAY '00:00:35';
                DELETE FROM [DefinedValue] WHERE 1 != 1
            {% endsql %}

            {{ results }} {{ 'record' | PluralizeForQuantity:results }} were deleted.";

            var output = lavaScript.ResolveMergeFields( new Dictionary<string, object>(), null, "Sql" );
            Assert.That.Contains( output, "Lava Error: Execution Timeout Expired." );
        }
    }
}
