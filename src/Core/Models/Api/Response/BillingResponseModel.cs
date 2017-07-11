﻿using System;
using System.Linq;
using System.Collections.Generic;
using Bit.Core.Models.Business;
using Stripe;
using Bit.Core.Models.Table;

namespace Bit.Core.Models.Api
{
    public class BillingResponseModel : ResponseModel
    {
        public BillingResponseModel(IStorable storable, BillingInfo billing)
            : base("billing")
        {
            PaymentSource = billing.PaymentSource != null ? new BillingSource(billing.PaymentSource) : null;
            Subscription = billing.Subscription != null ? new BillingSubscription(billing.Subscription) : null;
            Charges = billing.Charges.Select(c => new BillingCharge(c));
            UpcomingInvoice = billing.UpcomingInvoice != null ? new BillingInvoice(billing.UpcomingInvoice) : null;
            StorageName = storable.Storage.HasValue ? Utilities.CoreHelpers.ReadableBytesSize(storable.Storage.Value) : null;
            StorageGb = storable.Storage.HasValue ? Math.Round(storable.Storage.Value / 1073741824D, 2) : 0; // 1 GB
            MaxStorageGb = storable.MaxStorageGb;
        }

        public string StorageName { get; set; }
        public double? StorageGb { get; set; }
        public short? MaxStorageGb { get; set; }
        public BillingSource PaymentSource { get; set; }
        public BillingSubscription Subscription { get; set; }
        public BillingInvoice UpcomingInvoice { get; set; }
        public IEnumerable<BillingCharge> Charges { get; set; }
    }

    public class BillingSource
    {
        public BillingSource(Source source)
        {
            Type = source.Type;

            switch(source.Type)
            {
                case SourceType.Card:
                    Description = $"{source.Card.Brand}, *{source.Card.Last4}, " +
                        string.Format("{0}/{1}",
                            string.Concat(source.Card.ExpirationMonth.Length == 1 ?
                                "0" : string.Empty, source.Card.ExpirationMonth),
                            source.Card.ExpirationYear);
                    CardBrand = source.Card.Brand;
                    break;
                case SourceType.BankAccount:
                    Description = $"{source.BankAccount.BankName}, *{source.BankAccount.Last4}";
                    break;
                // bitcoin/alipay?
                default:
                    break;
            }
        }

        public SourceType Type { get; set; }
        public string CardBrand { get; set; }
        public string Description { get; set; }
    }

    public class BillingSubscription
    {
        public BillingSubscription(StripeSubscription sub)
        {
            Status = sub.Status;
            TrialStartDate = sub.TrialStart;
            TrialEndDate = sub.TrialEnd;
            EndDate = sub.CurrentPeriodEnd;
            CancelledDate = sub.CanceledAt;
            CancelAtEndDate = sub.CancelAtPeriodEnd;
            if(sub.Items?.Data != null)
            {
                Items = sub.Items.Data.Select(i => new BillingSubscriptionItem(i));
            }
        }

        public DateTime? TrialStartDate { get; set; }
        public DateTime? TrialEndDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? CancelledDate { get; set; }
        public bool CancelAtEndDate { get; set; }
        public string Status { get; set; }
        public IEnumerable<BillingSubscriptionItem> Items { get; set; } = new List<BillingSubscriptionItem>();

        public class BillingSubscriptionItem
        {
            public BillingSubscriptionItem(StripeSubscriptionItem item)
            {
                if(item.Plan != null)
                {
                    Name = item.Plan.Name;
                    Amount = item.Plan.Amount / 100M;
                    Interval = item.Plan.Interval;
                }

                Quantity = item.Quantity;
            }

            public string Name { get; set; }
            public decimal Amount { get; set; }
            public int Quantity { get; set; }
            public string Interval { get; set; }
        }
    }

    public class BillingInvoice
    {
        public BillingInvoice(StripeInvoice inv)
        {
            Amount = inv.AmountDue / 100M;
            Date = inv.Date.Value;
        }

        public decimal Amount { get; set; }
        public DateTime? Date { get; set; }
    }

    public class BillingCharge
    {
        public BillingCharge(StripeCharge charge)
        {
            Amount = charge.Amount / 100M;
            RefundedAmount = charge.AmountRefunded / 100M;
            PaymentSource = charge.Source != null ? new BillingSource(charge.Source) : null;
            CreatedDate = charge.Created;
            FailureMessage = charge.FailureMessage;
            Refunded = charge.Refunded;
            Status = charge.Status;
            InvoiceId = charge.InvoiceId;
        }

        public DateTime CreatedDate { get; set; }
        public decimal Amount { get; set; }
        public BillingSource PaymentSource { get; set; }
        public string Status { get; set; }
        public string FailureMessage { get; set; }
        public bool Refunded { get; set; }
        public bool PartiallyRefunded => !Refunded && RefundedAmount > 0;
        public decimal RefundedAmount { get; set; }
        public string InvoiceId { get; set; }
    }
}
