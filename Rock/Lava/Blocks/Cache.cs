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
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;

using Rock.Web.Cache;

namespace Rock.Lava.Blocks
{
    /// <summary>
    /// Cache allows you to cache the results of a Lava template.
    ///
    /// {% cache key:'my-content' %}
    ///     My Lava is now fast!
    /// {% endcache %}
    /// </summary>
    public class Cache : RockLavaBlockBase
    {
        private static readonly Regex Syntax = new Regex( @"(\w+)" );

        string _markup = string.Empty;
        string _tagName = string.Empty;

        string _blockMarkup = null; // = new StringBuilder();

        const int _maxRecursionDepth = 10;

        /// <summary>
        /// Method that will be run at Rock startup
        /// </summary>
        //public override void OnStartup()
        //{
        //    Template.RegisterTag<Cache>( "cache" );
        //}

        /// <summary>
        /// Initializes the specified tag name.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="markup">The markup.</param>
        /// <param name="tokens">The tokens.</param>
        /// <exception cref="System.Exception">Could not find the variable to place results in.</exception>
        public override void OnInitialize( string tagName, string markup, List<string> tokens )
        {
            _markup = markup;
            _tagName = tagName;

            _blockMarkup = tokens.JoinStrings( string.Empty );

            base.OnInitialize( tagName, markup, tokens );
        }

        /*
        /// <summary>
        /// Parses the specified tokens.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        public override void OnParse( List<string> tokens, out List<object> nodeList )
        {
            // Get the block markup. The list of tokens contains all of the lava from the start tag to
            // the end of the template. This will pull out just the internals of the block.

            // We must take into consideration nested tags of the same type

            // This is similar logic to the Shortcodes, but the tag regex are different. Attempted to refactor to a reusable helper, but it needs
            // access to a lot of the internals of the command.

            var endTagFound = false;

            var startTag = $@"{{\%\s*{ _tagName }\s*\%}}";
            var endTag = $@"{{\%\s*end{ _tagName }\s*\%}}";

            var childTags = 0;

            Regex regExStart = new Regex( startTag );
            Regex regExEnd = new Regex( endTag );

            nodeList = new List<object>();

            string token;
            while ( ( token = tokens.Shift() ) != null )
            {

                Match startTagMatch = regExStart.Match( token );
                if ( startTagMatch.Success )
                {
                    childTags++; // increment the child tag counter
                    _blockMarkup.Append( token );
                }
                else
                {
                    Match endTagMatch = regExEnd.Match( token );

                    if ( endTagMatch.Success )
                    {
                        if ( childTags > 0 )
                        {
                            childTags--; // decrement the child tag counter
                            _blockMarkup.Append( token );
                        }
                        else
                        {
                            endTagFound = true;
                            break;
                        }
                    }
                    else
                    {
                        _blockMarkup.Append( token );
                    }
                }
            }

            if ( !endTagFound )
            {
                AssertMissingDelimitation();
            }
        }

        */

        /// <summary>
        /// Renders the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        public override void OnRender( ILavaContext context, TextWriter result )
        {
            // First ensure that cached commands are allowed in the context
            if ( !this.IsAuthorized( context ) )
            {
                result.Write( string.Format( RockLavaBlockBase.NotAuthorizedMessage, this.SourceElementName ) );
                base.OnRender( context, result );
                return;
            }

            var parms = ParseMarkup( _markup, context );

            var twoPassEnabled = parms["twopass"].AsBoolean();

            var cacheKey = "lavacache-" + parms["key"];
            if ( cacheKey == string.Empty )
            {
                result.Write( "* No cache key provided. *" );
                base.OnRender( context, result );
                return;
            }

            // Get content from cache
            var cachedResult = RockCache.Get( cacheKey, true ) as CacheLavaTag;

            // Check that the cached value is current
            if ( cachedResult != null )
            {
                var currentHash = CalculateContentHash( _blockMarkup.ToString() );
                if ( currentHash != cachedResult.Hash )
                {
                    cachedResult = null;
                }
            }

            // Use the cached value
            if ( cachedResult != null )
            {
                if ( twoPassEnabled )
                {
                    result.Write( MergeLava( cachedResult.Content, context ) );
                }
                else
                {
                    result.Write( cachedResult.Content );
                }

                base.OnRender( context, result );
                return;
            }

            // Cached value not available so render the template and cache it
            var lavaResults = MergeLava( _blockMarkup.ToString(), context );

            var cacheDuration = parms["duration"].AsInteger();

            if ( cacheDuration > 0 )
            {
                // Don't cache if it's too large
                var maxCacheSize = parms["maxcachesize"].AsInteger();

                if ( lavaResults.Length < maxCacheSize )
                {
                    var expiration = RockDateTime.Now.AddSeconds( cacheDuration );
                    var cachedHash = CalculateContentHash( _blockMarkup.ToString() );
                    RockCache.AddOrUpdate( cacheKey, string.Empty, new CacheLavaTag { Hash = cachedHash, Content = lavaResults }, expiration, parms["tags"] );
                }
            }

            // If twopass is enabled run the lava again
            if ( twoPassEnabled )
            {
                lavaResults = MergeLava( lavaResults, context );
            }

            result.Write( lavaResults );


            // TODO: Removed to fix for DotLiquid implementation - if present, output prints twice.
            //base.OnRender( context, result );
        }

        /// <summary>
        /// Calculates the content hash.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        private int CalculateContentHash( string content )
        {
            return ( content + _markup ).GetHashCode();
        }

        /// <summary>
        /// Cache Tag POCO
        /// </summary>
		[Serializable]
        [DataContract]
        private class CacheLavaTag
        {
            /// <summary>
            /// Gets or sets the hash.
            /// </summary>
            /// <value>
            /// The hash.
            /// </value>
			[DataMember]
            public int Hash { get; set; }

            /// <summary>
            /// Gets or sets the lava.
            /// </summary>
            /// <value>
            /// The lava.
            /// </value>
			[DataMember]
            public string Content { get; set; }
        }

        /// <summary>
        /// Merges the lava.
        /// </summary>
        /// <param name="lavaTemplate">The lava template.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private string MergeLava( string lavaTemplate, ILavaContext context )
        {
            // Get enabled commands
            //var enabledCommands = context.GetEnabledCommands();

            //var lavaMergeFields = context.GetMergeFieldsForLocalScope();

            /*
            // Get mergefields from lava context
            var lavaMergeFields = new Dictionary<string, object>();
            if ( context.GetEnvironments?.Count > 0 )
            {
                foreach ( var item in context.GetEnvironments[0] )
                {
                    lavaMergeFields.Add( item.Key, item.Value );
                }
            }

            // Add variables in the scope (defined in the lava itself via assign)
            if ( context.GetScopes?.Count > 0 )
            {
                foreach ( var item in context.GetScopes[0] )
                {
                    lavaMergeFields.Add( item.Key, item.Value );
                }
            }
            */
            string output = null;

            // Resolve the Lava template contained in this block in a new context.
            var innerContext = LavaEngine.Instance.NewContext();

            


            // Resolve the inner template using a new scope.
            context.Stack( () => LavaEngine.Instance.TryRender( lavaTemplate, out output, context ) );
            //LavaEngine.Instance.TryRender( lavaTemplate, out output, context );

            return output;


            //return lavaTemplate.ResolveMergeFields( lavaMergeFields, enabledCommands, throwExceptionOnErrors:true );
        }

        /// <summary>
        /// Parses the markup.
        /// </summary>
        /// <param name="markup">The markup.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private Dictionary<string, string> ParseMarkup( string markup, ILavaContext context )
        {
            // first run lava across the inputted markup
            var internalMergeFields = context.GetMergeFieldsInLocalScope();

            var parms = new Dictionary<string, string>();
            parms.Add( "key", string.Empty );
            parms.Add( "tags", string.Empty );
            parms.Add( "twopass", "false" );
            parms.Add( "duration", "3600" );
            parms.Add( "maxcachesize", "200000" );

            var markupItems = Regex.Matches( markup, @"(\S*?:'[^']+')" )
                .Cast<Match>()
                .Select( m => m.Value )
                .ToList();

            foreach ( var item in markupItems )
            {
                var itemParts = item.ToString().Split( new char[] { ':' }, 2 );
                if ( itemParts.Length > 1 )
                {
                    var value = itemParts[1];

                    if ( value.HasMergeFields() )
                    {
                        value = value.ResolveMergeFields( internalMergeFields );
                    }

                    parms.AddOrReplace( itemParts[0].Trim().ToLower(), value.Substring( 1, value.Length - 2 ).Trim() );
                }
            }
            return parms;
        }

    }
}