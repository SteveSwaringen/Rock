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
using System.Text.RegularExpressions;
using Rock.Lava.Shortcodes;

namespace Rock.Lava
{

    /// <summary>
    /// Provides base functionality for an engine that can parse and render Lava Templates.
    /// </summary>
    //TODO: Implement IRockStartup.
    public abstract class LavaEngineBase : ILavaEngine // ,IRockStartup
    {
        public abstract void Initialize( ILavaFileSystem fileSystem, IList<Type> filterImplementationTypes = null );

        public abstract string EngineName { get; }

        public abstract ILavaContext NewContext();

        public abstract LavaEngineTypeSpecifier EngineType { get; }

        public abstract Type GetShortcodeType( string name );

        public abstract void RegisterSafeType( Type type, string[] allowedMembers = null );

        /// <summary>
        /// Register a shortcode that is defined and implemented in code.
        /// </summary>
        /// <param name="shortcodeFactoryMethod"></param>
        public void RegisterStaticShortcode( Func<string, IRockShortcode> shortcodeFactoryMethod )
        {
            var instance = shortcodeFactoryMethod( "default" );

            if ( instance == null )
            {
                throw new Exception( "Shortcode factory could not provide a valid instance for \"default\"." );
            }

            RegisterStaticShortcode( instance.SourceElementName, shortcodeFactoryMethod );
        }

        /// <summary>
        /// Register a shortcode that is defined and implemented in code.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="shortcodeFactoryMethod"></param>
        public void RegisterStaticShortcode( string name, Func<string, IRockShortcode> shortcodeFactoryMethod )
        {
            var instance = shortcodeFactoryMethod( name );

            if ( instance == null )
            {
                throw new Exception( $"Shortcode factory could not provide a valid instance for \"{name}\" ." );
            }

            // Get a decorated name for the shortcode that will not collide with a regular tag name.
            var registrationKey = LavaUtilityHelper.GetLiquidElementNameFromShortcodeName( name );

            if ( instance.ElementType == LavaShortcodeTypeSpecifier.Inline )
            {
                var tagFactoryMethod = shortcodeFactoryMethod as Func<string, IRockLavaTag>;

                RegisterTag( registrationKey, tagFactoryMethod );
            }
            else
            {
                RegisterBlock( registrationKey, ( blockName ) =>
               {
                    // Get a shortcode instance using the provided shortcut factory.
                    var shortcode = shortcodeFactoryMethod( registrationKey );

                    // Return the shortcode instance as a RockLavaBlock
                    return shortcode as IRockLavaBlock;
               } );
                ;
            }
        }

        /// <summary>
        /// Register a shortcode that is defined in the data store.
        /// The definition of a dynamic shortcode can be changed at runtime.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="shortcodeFactoryMethod"></param>
        public void RegisterDynamicShortcode( string name, Func<string, DynamicShortcodeDefinition> shortcodeFactoryMethod )
        {
            // Create a default instance so we can retrieve the properties of the shortcode.
            var instance = shortcodeFactoryMethod( name );

            if ( instance == null )
            {
                throw new Exception( $"Shortcode factory could not provide a valid instance for \"{name}\" ." );
            }

            if ( instance.ElementType == LavaShortcodeTypeSpecifier.Inline )
            {
                // Create a new factory method that returns an initialized Shortcode Tag element.
                Func<string, IRockLavaTag> tagFactoryMethod = ( tagName ) =>
                {
                    var shortcodeInstance = GetShortcodeFromFactory<DynamicShortcodeTag>( tagName, shortcodeFactoryMethod );

                    return shortcodeInstance;
                };

                // Register the shortcode as a custom tag, but use a decorated registration name that will not collide with a regular element name.
                var registrationKey = LavaUtilityHelper.GetLiquidElementNameFromShortcodeName( name );

                RegisterTag( registrationKey, tagFactoryMethod );
            }
            else
            {
                // Create a new factory method that returns an initialized Shortcode Block element.
                Func<string, IRockLavaBlock> blockFactoryMethod = ( blockName ) =>
                {
                    // Call the factory method we have been passed to retrieve the definition of the shortcode.
                    // The definition may change at runtime, so we need to execute the factory method for each new shortcode instance.
                    var shortCodeName = LavaUtilityHelper.GetShortcodeNameFromLiquidElementName( blockName );

                    var shortcodeDefinition = shortcodeFactoryMethod( shortCodeName );

                    var shortcodeInstance = new DynamicShortcodeBlock( shortcodeDefinition );

                    return shortcodeInstance;
                };

                // Register the shortcode as a custom block, but use a decorated registration name that will not collide with a regular element name.
                var registrationKey = LavaUtilityHelper.GetLiquidElementNameFromShortcodeName( name );

                RegisterBlock( registrationKey, blockFactoryMethod );
            }
        }

        private T GetShortcodeFromFactory<T>( string shortcodeInternalName, Func<string, DynamicShortcodeDefinition> shortcodeFactoryMethod )
            where T : DynamicShortcode, new()
        {
            // Call the factory method we have been passed to retrieve the definition of the shortcode.
            // The definition may change at runtime, so we need to execute the factory method every time we create a new shortcode instance.
            var shortCodeName = LavaUtilityHelper.GetShortcodeNameFromLiquidElementName( shortcodeInternalName );

            var shortcodeDefinition = shortcodeFactoryMethod( shortCodeName );

            var shortcodeInstance = new T();

            shortcodeInstance.Initialize( shortcodeDefinition );

            return shortcodeInstance;
        }

        public bool TryRender( string inputTemplate, out string output )
        {
            return TryRender( inputTemplate, out output, context: null );
        }

        public bool TryRender( string inputTemplate, out string output, LavaDictionary mergeValues )
        {
            var context = NewContext();

            context.SetMergeFieldValues( mergeValues );

            return TryRender( inputTemplate, out output, context );
        }

        public abstract bool TryRender( string inputTemplate, out string output, ILavaContext context );

        public abstract void UnregisterShortcode( string name );

        public abstract bool AreEqualValue( object left, object right );

        public bool TryParseTemplate( string inputTemplate, out ILavaTemplate template )
        {
            try
            {
                template = ParseTemplate( inputTemplate );
                return true;
            }
            catch
            {
                template = null;
                return false;
            }
        }

        public abstract ILavaTemplate ParseTemplate( string inputTemplate );

        public Dictionary<string, ILavaElementInfo> GetRegisteredElements()
        {
            var tags = new Dictionary<string, ILavaElementInfo>();

            foreach ( var tagWrapper in _lavaElements )
            {
                var info = new LavaTagInfo();

                info.Name = tagWrapper.Key;

                info.SystemTypeName = tagWrapper.Value.SystemTypeName;

                tags.Add( info.Name, info );
            }

            return tags;
        }

        #region Tags

        private static Dictionary<string, ILavaElementInfo> _lavaElements = new Dictionary<string, ILavaElementInfo>( StringComparer.OrdinalIgnoreCase );

        public virtual void RegisterTag( string name, Func<string, IRockLavaTag> factoryMethod )
        {
            if ( string.IsNullOrWhiteSpace( name ) )
            {
                throw new ArgumentException( "Name must be specified." );
            }

            name = name.Trim().ToLower();

            var tagInstance = factoryMethod( name );

            var tagInfo = new LavaTagInfo();

            tagInfo.Name = name;
            tagInfo.FactoryMethod = factoryMethod;

            tagInfo.IsAvailable = ( tagInstance != null );

            if ( tagInstance != null )
            {
                tagInfo.SystemTypeName = tagInstance.GetType().FullName;
            }

            _lavaElements[name] = tagInfo;
        }

        public virtual void RegisterBlock( string name, Func<string, IRockLavaBlock> factoryMethod )
        {
            if ( string.IsNullOrWhiteSpace( name ) )
            {
                throw new ArgumentException( "Name must be specified." );
            }

            name = name.Trim().ToLower();

            var blockInstance = factoryMethod( name );

            var blockInfo = new LavaBlockInfo();

            blockInfo.Name = name;
            blockInfo.FactoryMethod = factoryMethod;

            blockInfo.IsAvailable = ( blockInstance != null );

            if ( blockInstance != null )
            {
                blockInfo.SystemTypeName = blockInstance.GetType().FullName;
            }

            _lavaElements[name] = blockInfo;
        }

        public bool TryGetTagInstance( string tagName, out IRockLavaTag tagInstance )
        {
            tagInstance = null;

            if ( !_lavaElements.ContainsKey( tagName ) )
            {
                return false;
            }

            var tag = _lavaElements[tagName] as LavaTagInfo;

            if ( tag == null )
            {
                return false;
            }

            var factoryMethod = tag.FactoryMethod;

            tagInstance = factoryMethod( tagName );

            return true;
        }

        #endregion

        protected void ProcessException( Exception ex )
        {
            string discardedOutput;

            ProcessException( ex, out discardedOutput );
        }

        protected void ProcessException( Exception ex, out string message )
        {
            if ( this.ExceptionHandlingStrategy == ExceptionHandlingStrategySpecifier.RenderToOutput )
            {
                message = ex.Message;
            }
            else if ( this.ExceptionHandlingStrategy == ExceptionHandlingStrategySpecifier.Ignore )
            {
                // We should probably log the message here rather than failing silently.
                message = null;
            }
            else
            {
                throw ex;
            }
        }

        public ExceptionHandlingStrategySpecifier ExceptionHandlingStrategy { get; set; } = ExceptionHandlingStrategySpecifier.RenderToOutput;

        /// <summary>
        /// Convert a Lava template to a Liquid-compatible template by replacing Lava-specific syntax and keywords.
        /// </summary>
        /// <param name="lavaTemplateText"></param>
        /// <returns></returns>
        public string ConvertToLiquid( string lavaTemplateText )
        {
            var converter = new LavaToLiquidTemplateConverter();

            return converter.ConvertToLiquid( lavaTemplateText );
        }
    }

    public class LavaToLiquidTemplateConverter
    {
        /// <summary>
        /// Convert a Lava template to a Liquid-compatible template by replacing Lava-specific syntax and keywords.
        /// </summary>
        /// <param name="lavaTemplateText"></param>
        /// <returns></returns>
        public string ConvertToLiquid( string lavaTemplateText )
        {
            string liquidTemplateText;

            liquidTemplateText = ReplaceTemplateShortcodes( lavaTemplateText );
            liquidTemplateText = ReplaceElseIfKeyword( liquidTemplateText );

            return liquidTemplateText;
        }

        internal static readonly Regex FullShortCodeToken = new Regex( @"{\[\s*(\w+)\s*([^\]}]*)?\]}", RegexOptions.Compiled );

        public string ReplaceTemplateShortcodes( string inputTemplate )
        {
            /* The Lava shortcode syntax is not recognized as a document element by Fluid, and at present there is no way to intercept or replace the Fluid parser.
             * As a workaround, we pre-process the template to replace the Lava shortcode token "{[ ]}" with the Liquid tag token "{% %}" and add a prefix to avoid naming collisions with existing standard tags.
             * The shortcode can then be processed as a regular custom block by the Fluid templating engine.
             * As a future improvement, we could look at submitting a pull request to the Fluid project to add support for custom parsers.
             */
            var newBlockName = "{% $1<suffix> $2 %}".Replace( "<suffix>", LavaEngine.ShortcodeInternalNameSuffix );

            inputTemplate = FullShortCodeToken.Replace( inputTemplate, newBlockName );

            return inputTemplate;
        }

        internal static readonly Regex ElseIfToken = new Regex( @"{\%(.*?\s?)elseif(\s?.*?)\%}", RegexOptions.Compiled );

        public string ReplaceElseIfKeyword( string inputTemplate )
        {
            // "elseif" is not a recognized keyword, because Liquid implements the less obvious variant "elsif".
            // This keyword forms part of a stateful construct (if/then/else) that is processed internally by the Liquid engine,
            // so the most portable method of processing this alternative is to replace it with the recognized Liquid keyword.            
            inputTemplate = ElseIfToken.Replace( inputTemplate, "{%$1elsif$2%}" );

            return inputTemplate;
        }

    }
}