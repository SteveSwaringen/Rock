//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the Rock.CodeGeneration project
//     Changes to this file will be lost when the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Data Transfer Object for FinancialTransaction object
    /// </summary>
    [Serializable]
    [DataContract]
    public partial class FinancialTransactionDto : DtoSecured<FinancialTransactionDto>
    {
        /// <summary />
        [DataMember]
        public string Description { get; set; }

        /// <summary />
        [DataMember]
        public DateTime? TransactionDateTime { get; set; }

        /// <summary />
        [DataMember]
        public string Entity { get; set; }

        /// <summary />
        [DataMember]
        public int? EntityId { get; set; }

        /// <summary />
        [DataMember]
        public int? BatchId { get; set; }

        /// <summary />
        [DataMember]
        public int? CurrencyTypeValueId { get; set; }

        /// <summary />
        [DataMember]
        public int? CreditCardTypeValueId { get; set; }

        /// <summary />
        [DataMember]
        public decimal Amount { get; set; }

        /// <summary />
        [DataMember]
        public int? RefundTransactionId { get; set; }

        /// <summary />
        [DataMember]
        public int? TransactionImageId { get; set; }

        /// <summary />
        [DataMember]
        public string TransactionCode { get; set; }

        /// <summary />
        [DataMember]
        public int? PaymentGatewayId { get; set; }

        /// <summary />
        [DataMember]
        public int? SourceTypeValueId { get; set; }

        /// <summary />
        [DataMember]
        public string Summary { get; set; }

        /// <summary>
        /// Instantiates a new DTO object
        /// </summary>
        public FinancialTransactionDto ()
        {
        }

        /// <summary>
        /// Instantiates a new DTO object from the entity
        /// </summary>
        /// <param name="financialTransaction"></param>
        public FinancialTransactionDto ( FinancialTransaction financialTransaction )
        {
            CopyFromModel( financialTransaction );
        }

        /// <summary>
        /// Creates a dictionary object.
        /// </summary>
        /// <returns></returns>
        public override Dictionary<string, object> ToDictionary()
        {
            var dictionary = base.ToDictionary();
            dictionary.Add( "Description", this.Description );
            dictionary.Add( "TransactionDateTime", this.TransactionDateTime );
            dictionary.Add( "Entity", this.Entity );
            dictionary.Add( "EntityId", this.EntityId );
            dictionary.Add( "BatchId", this.BatchId );
            dictionary.Add( "CurrencyTypeValueId", this.CurrencyTypeValueId );
            dictionary.Add( "CreditCardTypeValueId", this.CreditCardTypeValueId );
            dictionary.Add( "Amount", this.Amount );
            dictionary.Add( "RefundTransactionId", this.RefundTransactionId );
            dictionary.Add( "TransactionImageId", this.TransactionImageId );
            dictionary.Add( "TransactionCode", this.TransactionCode );
            dictionary.Add( "PaymentGatewayId", this.PaymentGatewayId );
            dictionary.Add( "SourceTypeValueId", this.SourceTypeValueId );
            dictionary.Add( "Summary", this.Summary );
            return dictionary;
        }

        /// <summary>
        /// Creates a dynamic object.
        /// </summary>
        /// <returns></returns>
        public override dynamic ToDynamic()
        {
            dynamic expando = base.ToDynamic();
            expando.Description = this.Description;
            expando.TransactionDateTime = this.TransactionDateTime;
            expando.Entity = this.Entity;
            expando.EntityId = this.EntityId;
            expando.BatchId = this.BatchId;
            expando.CurrencyTypeValueId = this.CurrencyTypeValueId;
            expando.CreditCardTypeValueId = this.CreditCardTypeValueId;
            expando.Amount = this.Amount;
            expando.RefundTransactionId = this.RefundTransactionId;
            expando.TransactionImageId = this.TransactionImageId;
            expando.TransactionCode = this.TransactionCode;
            expando.PaymentGatewayId = this.PaymentGatewayId;
            expando.SourceTypeValueId = this.SourceTypeValueId;
            expando.Summary = this.Summary;
            return expando;
        }

        /// <summary>
        /// Copies the model property values to the DTO properties
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( IEntity model )
        {
            base.CopyFromModel( model );

            if ( model is FinancialTransaction )
            {
                var financialTransaction = (FinancialTransaction)model;
                this.Description = financialTransaction.Description;
                this.TransactionDateTime = financialTransaction.TransactionDateTime;
                this.Entity = financialTransaction.Entity;
                this.EntityId = financialTransaction.EntityId;
                this.BatchId = financialTransaction.BatchId;
                this.CurrencyTypeValueId = financialTransaction.CurrencyTypeValueId;
                this.CreditCardTypeValueId = financialTransaction.CreditCardTypeValueId;
                this.Amount = financialTransaction.Amount;
                this.RefundTransactionId = financialTransaction.RefundTransactionId;
                this.TransactionImageId = financialTransaction.TransactionImageId;
                this.TransactionCode = financialTransaction.TransactionCode;
                this.PaymentGatewayId = financialTransaction.PaymentGatewayId;
                this.SourceTypeValueId = financialTransaction.SourceTypeValueId;
                this.Summary = financialTransaction.Summary;
            }
        }

        /// <summary>
        /// Copies the DTO property values to the entity properties
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyToModel ( IEntity model )
        {
            base.CopyToModel( model );

            if ( model is FinancialTransaction )
            {
                var financialTransaction = (FinancialTransaction)model;
                financialTransaction.Description = this.Description;
                financialTransaction.TransactionDateTime = this.TransactionDateTime;
                financialTransaction.Entity = this.Entity;
                financialTransaction.EntityId = this.EntityId;
                financialTransaction.BatchId = this.BatchId;
                financialTransaction.CurrencyTypeValueId = this.CurrencyTypeValueId;
                financialTransaction.CreditCardTypeValueId = this.CreditCardTypeValueId;
                financialTransaction.Amount = this.Amount;
                financialTransaction.RefundTransactionId = this.RefundTransactionId;
                financialTransaction.TransactionImageId = this.TransactionImageId;
                financialTransaction.TransactionCode = this.TransactionCode;
                financialTransaction.PaymentGatewayId = this.PaymentGatewayId;
                financialTransaction.SourceTypeValueId = this.SourceTypeValueId;
                financialTransaction.Summary = this.Summary;
            }
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public static class FinancialTransactionDtoExtension
    {
        /// <summary>
        /// To the model.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static FinancialTransaction ToModel( this FinancialTransactionDto value )
        {
            FinancialTransaction result = new FinancialTransaction();
            value.CopyToModel( result );
            return result;
        }

        /// <summary>
        /// To the model.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static List<FinancialTransaction> ToModel( this List<FinancialTransactionDto> value )
        {
            List<FinancialTransaction> result = new List<FinancialTransaction>();
            value.ForEach( a => result.Add( a.ToModel() ) );
            return result;
        }

        /// <summary>
        /// To the dto.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static List<FinancialTransactionDto> ToDto( this List<FinancialTransaction> value )
        {
            List<FinancialTransactionDto> result = new List<FinancialTransactionDto>();
            value.ForEach( a => result.Add( a.ToDto() ) );
            return result;
        }

        /// <summary>
        /// To the dto.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static FinancialTransactionDto ToDto( this FinancialTransaction value )
        {
            return new FinancialTransactionDto( value );
        }

    }
}