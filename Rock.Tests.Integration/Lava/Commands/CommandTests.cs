﻿// <copyright>
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
//

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Tests.Shared;

namespace Rock.Tests.Integration.Lava
{
    /// <summary>
    /// Tests for Lava-specific commands implemented as Liquid custom blocks and tags.
    /// </summary>
    [TestClass]
    public class CommandTests : LavaIntegrationTestBase
    {
        #region Command Internals

        [TestMethod]
        public void Command_MultipleInstancesOfCustomBlock_ResolvesAllInstances()
        {
            var input = @"
{% javascript %}
    alert('Message 1');
{% endjavascript %}
{% javascript %}
    alert('Message 2');
{% endjavascript %}
";

            var expectedOutput = @"
<script>
    (function(){
        alert('Message 1');    
    })();
</script>
<script>
    (function(){
        alert('Message 2');
    })();
</script>

";

            _helper.AssertTemplateOutput( expectedOutput, input, ignoreWhitespace: true );
        }

        #endregion

        #region Cache

        [TestMethod]
        public void CacheBlock_CommandNotEnabled_ReturnsConfigurationErrorMessage()
        {
            var input = @"
{% cache key:'decker-page-list' duration:'3600' %}
    {% person where:'LastName == ""Decker""' %}
        {% for person in personItems %}
            {{ person.FullName }} < br />
        {% endfor %}
    {% endperson %}
{% endcache %}
";

            var expectedOutput = @"\s*The Lava command 'cache' is not configured for this template\.\s*";

            _helper.AssertTemplateOutputRegex( expectedOutput, input );
        }

        [TestMethod]
        public void CacheBlock_ForEntityCommandResult_IsCached()
        {
            var input = @"
{% cache key:'decker-page-list' duration:'3600' %}
    {% person where:'LastName == ""Decker"" && NickName == ""Ted""' %}
        {% for person in personItems %}
            {{ person.FullName }} <br/>
        {% endfor %}
    {% endperson %}
{% endcache %}
";

            var expectedOutput = @"
TedDecker<br/>
";

            var context = _helper.LavaEngine.NewContext();

            context.SetEnabledCommands( "Cache,RockEntity" );

            _helper.AssertTemplateOutput( expectedOutput, input, context, ignoreWhiteSpace: true );
        }

        /// <summary>
        /// Verifies the variable scoping behavior of the Cache block.
        /// Within the scope of a Cache block, an Assign statement should not affect the value of a same-named variable in the outer scope.
        /// This behavior differs from the standard scoping behavior for Liquid blocks.
        /// </summary>
        [TestMethod]
        public void Cache_InnerScopeAssign_DoesNotModifyOuterVariable()
        {
            var input = @"
{% assign color = 'blue' %}
Color 1: {{ color }}

{% cache key:'fav-color' duration:'1200' %}
    Color 2: {{ color }}
    {% assign color = 'red' %}
    Color 3: {{color }}
{% endcache %}

Color 4: {{ color }}
";

            var expectedOutput = @"
Color 1: blue
Color 2: blue
Color 3: red
Color 4: blue
";

            var context = _helper.LavaEngine.NewContext();

            context.SetEnabledCommands( "Cache" );

            _helper.AssertTemplateOutput( expectedOutput, input, context, ignoreWhiteSpace: true );
        }

        #endregion

        #region Entity

        [TestMethod]
        public void EntityBlock_CommandNotEnabled_ReturnsConfigurationErrorMessage()
        {
            var input = @"
{% person where: 'LastName == ""Decker""' %}
    {% for person in personItems %}
        {{ person.FullName }} < br />
    {% endfor %}
{% endperson %}
            ";

            var expectedOutput = @"\s*The Lava command 'rockentity' is not configured for this template\.\s*";

            _helper.AssertTemplateOutputRegex( expectedOutput, input );
        }

        [TestMethod]
        public void EntityBlock_PersonWhereLastNameIsDecker_ReturnsDeckers()
        {
            var input = @"
{% person where:'LastName == ""Decker""' %}
    {% for person in personItems %}
        {{ person.FullName }} <br/>
    {% endfor %}
{% endperson %}
            ";

            var context = _helper.LavaEngine.NewContext();

            context.SetEnabledCommands( "RockEntity" );

            var output = _helper.GetTemplateOutput( input, context );

            _helper.WriteTemplateRenderToDebug( input, output );

            Assert.IsTrue( output.Contains( "Ted Decker" ), "Expected person not found." );
            Assert.IsTrue( output.Contains( "Cindy Decker" ), "Expected person not found." );
        }

        #endregion

        #region Execute

        [TestMethod]
        public void ExecuteBlock_CommandNotEnabled_ReturnsConfigurationErrorMessage()
        {
            var input = @"
{% execute %}
    return ""Hello World!"";
{% endexecute %}
            ";

            var expectedOutput = @"\s*The Lava command 'execute' is not configured for this template\.\s*";

            _helper.AssertTemplateOutputRegex( expectedOutput, input );
        }

        [TestMethod]
        public void ExecuteBlock_HelloWorld_ReturnsExpectedOutput()
        {
            var input = @"
{% execute %}
    return ""Hello World!"";
{% endexecute %}
            ";

            var expectedOutput = @"Hello World!";

            var context = _helper.LavaEngine.NewContext();

            context.SetEnabledCommands( "execute" );

            _helper.AssertTemplateOutput( expectedOutput, input, context );
        }

        [TestMethod]
        public void ExecuteBlock_WithImports_ReturnsExpectedOutput()
        {
            var input = @"
{% execute import:'Newtonsoft.Json,Newtonsoft.Json.Linq' %}

    JArray itemArray = JArray.Parse( ``['Banana','Orange','Apple']`` );

    return ``Fruit: `` + itemArray[1];

{% endexecute %}
    ";

            input = input.Replace( "``", @"""" );

            var expectedOutput = @"Fruit: Orange";

            var context = _helper.LavaEngine.NewContext();

            context.SetEnabledCommands( "execute" );

            _helper.AssertTemplateOutput( expectedOutput, input, context );
        }

        [TestMethod]
        public void ExecuteBlock_ClassType_ReturnsExpectedOutput()
        {
            var input = @"
{% execute type:'class' %}
    using Rock;
    using Rock.Data;
    using Rock.Model;
    
    public class MyScript 
    {
        public string Execute() {
            using(RockContext rockContext = new RockContext()) {
                var person = new PersonService(rockContext).Get(""<PersonGuid>"".AsGuid());
                
                return person.FullName;
            }
        }
    }
{% endexecute %}
";

            input = input.Replace( "<PersonGuid>", _helper.GetTestPersonTedDecker().Guid );

            var expectedOutput = @"Ted Decker";

            var context = _helper.LavaEngine.NewContext();

            context.SetEnabledCommands( "execute" );

            _helper.AssertTemplateOutput( expectedOutput, input, context );
        }

        [TestMethod]
        public void ExecuteBlock_WithContextValues_ResolvesContextValuesCorrectly()
        {
            var input = @"
{% execute type:'class' %}
    using Rock;
    using Rock.Data;
    using Rock.Model;
    
    public class MyScript 
    {
        public string Execute() {
            using(RockContext rockContext = new RockContext()) {
                var person = new PersonService(rockContext).Get(""{{ Person | Property: 'Guid' }}"".AsGuid());
                
                return person.FullName;
            }
        }
    }
{% endexecute %}
";

            var expectedOutput = @"Ted Decker";

            var context = _helper.LavaEngine.NewContext();

            context.SetEnabledCommands( "execute" );

            context.SetMergeFieldValue( "Person", _helper.GetTestPersonTedDecker() );

            _helper.AssertTemplateOutput( expectedOutput, input, context );
        }
        #endregion

        #region InteractionWrite

        [TestMethod]
        public void InteractionWriteBlock_CommandNotEnabled_ReturnsConfigurationErrorMessage()
        {
            var input = @"
{% interactionwrite channeltypemediumvalueid:'1' channelentityid:'1' channelname:'Some Channel' componententitytypeid:'1' interactionentitytypeid:'1' componententityid:'1' componentname:'Some Component' entityid:'1' operation:'View' summary:'Viewed Some Page' relatedentitytypeid:'1' relatedentityid:'1' channelcustom1:'Some Custom Value' channelcustom2:'Another Custom Value' channelcustomindexed1:'Some Indexed Custom Value'  personaliasid:'10' %}
    Here is the interaction data.
{% endinteractionwrite %}
";

            var expectedOutput = @"\s*The Lava command 'interactionwrite' is not configured for this template\.\s*";

            _helper.AssertTemplateOutputRegex( expectedOutput, input );
        }

        [TestMethod]
        public void InteractionWriteBlock_ForEntityCommandResult_IsCached()
        {
            var input = @"
{% interactionwrite channeltypemediumvalueid:'1' channelentityid:'1' channelname:'Some Channel' componententitytypeid:'1' interactionentitytypeid:'1' componententityid:'1' componentname:'Some Component' entityid:'1' operation:'View' summary:'Viewed Some Page' relatedentitytypeid:'1' relatedentityid:'1' channelcustom1:'Some Custom Value' channelcustom2:'Another Custom Value' channelcustomindexed1:'Some Indexed Custom Value'  personaliasid:'10' %}
    Here is the interaction data.
{% endinteractionwrite %}
";

            var expectedOutput = @"
";

            var context = _helper.LavaEngine.NewContext();

            context.SetEnabledCommands( "InteractionWrite" );

            _helper.AssertTemplateOutput( expectedOutput, input, context, ignoreWhiteSpace: true );
        }

        #endregion

        #region InteractionContentChannelItemWrite

        [TestMethod]
        public void InteractionContentChannelItemWriteTag_CommandNotEnabled_ReturnsConfigurationErrorMessage()
        {
            var input = @"
{% interactioncontentchannelitemwrite contentchannelitemid:'1' operation:'View' summary:'Viewed content channel item #1' personaliasid:'10' %}
";

            var expectedOutput = @"\s*The Lava command 'interactioncontentchannelitemwrite' is not configured for this template\.\s*";

            _helper.AssertTemplateOutputRegex( expectedOutput, input );
        }

        [TestMethod]
        public void InteractionContentChannelItemWriteTag_ForEntityCommandResult_IsCached()
        {
            var input = @"
{% interactioncontentchannelitemwrite contentchannelitemid:'1' operation:'View' summary:'Viewed content channel item #1' personaliasid:'10' %}
";

            var expectedOutput = @"
";

            var context = _helper.LavaEngine.NewContext();

            context.SetEnabledCommands( "InteractionContentChannelItemWrite" );

            _helper.AssertTemplateOutput( expectedOutput, input, context, ignoreWhiteSpace: true );
        }

        #endregion

        #region Javascript

        [TestMethod]
        public void JavascriptBlock_HelloWorld_ReturnsJavascriptScript()
        {
            var input = @"
{% javascript %}
    alert('Hello world!');
{% endjavascript %}
";

            var expectedOutput = @"
<script>
    (function(){
        alert('Hello world!');    
    })();
</script>
";

            _helper.AssertTemplateOutput( expectedOutput, input, ignoreWhitespace: true );
        }

        #endregion

        #region Search

        [TestMethod]
        public void SearchBlock_CommandNotEnabled_ReturnsConfigurationErrorMessage()
        {
            var input = @"
{% search query: 'ted decker' %}
    {% for result in results %}
        {{ result.DocumentName }}
    {% endfor %}
{% endsearch %}
";

            var expectedOutput = @"\s*The Lava command 'search' is not configured for this template\.\s*";

            _helper.AssertTemplateOutputRegex( expectedOutput, input );
        }

        [TestMethod]
        public void SearchBlock_UniversalSearchNotEnabled_ReturnsConfigurationErrorMessage()
        {
            var input = @"
{% search query:'ted decker' %}
    {% for result in results %}
        {{ result.DocumentName }}
    {% endfor %}
{% endsearch %}
";

            var expectedOutput = @"(.*)Search results not available. Universal search is not enabled for this Rock instance.";

            var context = _helper.LavaEngine.NewContext();

            context.SetEnabledCommands( "Search" );

            _helper.AssertTemplateOutputRegex( expectedOutput, input, context );
        }

        #endregion Search

        #region SQL

        [TestMethod]
        public void SqlBlock_CommandNotEnabled_ReturnsConfigurationErrorMessage()
        {
            var input = @"
{% sql %}
    SELECT   [NickName], [LastName]
    FROM     [Person] 
    WHERE    [LastName] = 'Decker'
    AND      [NickName] IN ('Ted', 'Alex')
    ORDER BY [NickName]
{% endsql %}
";

            var expectedOutput = @"\s*The Lava command 'sql' is not configured for this template\.\s*";

            _helper.AssertTemplateOutputRegex( expectedOutput, input );
        }

        [TestMethod]
        public void SqlBlock_PersonWhereLastNameIsDecker_ReturnsDeckers()
        {
            var input = @"
{% sql %}
    SELECT   [NickName], [LastName]
    FROM     [Person] 
    WHERE    [LastName] = 'Decker'
    AND      [NickName] IN ('Ted', 'Alex')
    ORDER BY [NickName]
{% endsql %}

{% for item in results %}{{ item.NickName }}_{{ item.LastName }};{% endfor %}
";

            var expectedOutput = @"Alex_Decker;Ted_Decker;";

            var context = _helper.LavaEngine.NewContext();

            context.SetEnabledCommands( "Sql" );

            _helper.AssertTemplateOutput( expectedOutput, input, context, ignoreWhiteSpace: true );
        }

        #endregion

        #region Stylesheet

        [TestMethod]
        public void StylesheetBlock_HelloWorld_ReturnsJavascriptScript()
        {
            var input = @"
{% stylesheet %}
#content-wrapper {
    background-color: red !important;
    color: #fff;
}
{% endstylesheet %}
";

            var expectedOutput = @"
<style>
    #content-wrapper {background-color:red!important;color:#fff;}
</style> 
";

            _helper.AssertTemplateOutput( expectedOutput, input, ignoreWhitespace: true );
        }

        #endregion

        #region TagList

        [TestMethod]
        public void TagListTag_InTemplate_ReturnsListOfTags()
        {
            var input = @"
{% taglist %}
";

            var output = _helper.GetTemplateOutput( input );

            output = output.Replace( " ", string.Empty );

            Assert.IsTrue( output.Contains( "person-Rock.Lava.Blocks.RockEntity" ), "Expected Entity Tag not found." );
            Assert.IsTrue( output.Contains( "cache-Rock.Lava.Blocks.Cache" ), "Expected Command Block not found." );
            Assert.IsTrue( output.Contains( "interactionwrite-Rock.Lava.Blocks.InteractionWrite" ), "Expected Command Tag not found." );
        }

        #endregion

        #region Web Request

        [TestMethod]
        public void WebRequestBlock_CommandNotEnabled_ReturnsConfigurationErrorMessage()
        {
            var input = @"
{% webrequest url:'https://api.github.com/repos/SparkDevNetwork/Rock/git/commits/88b33817b02b798679d75f237970649f25332fe1' return:'commit' %}
    {{ commit.message }}
{% endwebrequest %}  
";

            var expectedOutput = @"\s*The Lava command 'webrequest' is not configured for this template\.\s*";

            _helper.AssertTemplateOutputRegex( expectedOutput, input );
        }

        [TestMethod]
        public void WebRequestBlock_GetRockRepoCommits_ReturnsValidResponse()
        {
            var input = @"
{% webrequest url:'https://api.github.com/repos/SparkDevNetwork/Rock/git/commits/88b33817b02b798679d75f237970649f25332fe1' return:'commit' %}
    {{ commit.message }}
{% endwebrequest %}  
";

            var context = _helper.LavaEngine.NewContext();

            context.SetEnabledCommands( "WebRequest" );

            _helper.AssertTemplateOutput( "readme", input, context, ignoreWhiteSpace: true );
        }

        #endregion

        #region WorkflowActivate

        [TestMethod]
        public void WorkflowActivateBlock_CommandNotEnabled_ReturnsConfigurationErrorMessage()
        {
            var input = @"
{% workflowactivate workflowtype:'8fedc6ee-8630-41ed-9fc5-c7157fd1eaa4' %}
  Activated new workflow with the id of #{{ Workflow.Id }}.
{% endworkflowactivate %}
";

            // TODO: If the security check fails, the content of the block is still returned with the erro message.
            // Is this correct behavior, or should the content of the block be hidden?
            var expectedOutput = @"\s*The Lava command 'workflowactivate' is not configured for this template\.\s*.*";

            _helper.AssertTemplateOutputRegex( expectedOutput, input );
        }

        [TestMethod]
        public void WorkflowActivateBlock_ActivateSupportWorkflow_CreatesNewWorkflow()
        {
            // Activate Workflow: IT Support
            var input = @"
{% workflowactivate workflowtype:'51FE9641-FB8F-41BF-B09E-235900C3E53E' %}
  Activated new workflow with the name '{{ Workflow.Name }}'.
{% endworkflowactivate %}
";

            var expectedOutput = @"Activated new workflow with the name 'IT Support'.";

            var context = _helper.LavaEngine.NewContext();

            context.SetEnabledCommands( "WorkflowActivate" );

            _helper.AssertTemplateOutput( expectedOutput, input, context, true );
        }

        #endregion



    }
}